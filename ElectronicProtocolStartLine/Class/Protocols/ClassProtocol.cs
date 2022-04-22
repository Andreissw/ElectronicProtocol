using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Monitoring
{
    public class ClassProtocol
    {
        FASEntities fas { get; set; }
        int _protocolID { get; set; }
        string _topbot { get; set; }
        int _lotid { get; set; }

        byte? _line { get; set; }

        public ClassProtocol(PrtMon p)
        {
            fas = new FASEntities();
            _protocolID = p.ID;
            _line = p.Line;
            _topbot = p.TOPBOT;
            _lotid = p.LOTID;
        }

        public string Start()
        {
            var FujiPG = fas.EP_PGName.Where(c => c.IDProtocol == _protocolID & c.IDMachine == 4 & c.Type == _topbot).Select(c => c.Name).FirstOrDefault();
            if (FujiPG == null)
                return $"Не найдена программа в таблице EP_PGName по протоколу с униальным номером ProtocolID {_protocolID}, Линия {_line} , IDMachine {4}, Сторона {_topbot}";

            var LineFuji = fas.EP_Monitoring.Where(c => c.ProgrammName == FujiPG.Substring(0, c.ProgrammName.Length)).Select(c => c.Line).FirstOrDefault();

            if (LineFuji == null)
                return $"Программа {FujiPG} не была найдена в таблице EP_Monitoring";

            if (LineFuji == "")
                return $"Программа {FujiPG} не была найдена в таблице EP_Monitoring, БД вернуло пустое значение";

            var result = CheckProgramms(LineFuji);
            if (result != "true") // Проверка программ                
                return $"Не пройдена проверка! Не совпадает имя программы протокола и имя программы у следующего списка оборудования {result}";

            HttpGo(); //Отправка протокола на старт
            SetStartStatus(true); //Устанавливаем статус протоколу
            SetLog( 1,LineFuji);

            var ProtocolName = fas.EP_Protocols.Where(c => c.ID == _protocolID).FirstOrDefault();

            Class.ViewData view = new Class.ViewData();
            view.GetView(_lotid);
            //var protocolName = fas.EP_Protocols.Where(c => c.ID == ID).FirstOrDefault().NameProtocol;

            string mes = $@"<div> <h2> Добрый день!</h2> </div>
                        <div><h2>Заказ: {view.NameOrder}</h2></div>
                        <div style=""width: 500px; background-color:lightgreen""> <h2> Электронный протокол | {ProtocolName},Линия {_line}, Сторона {_topbot}| - активирован </h2> </div>";

            Email.SendEmailProtocol($"Запуск линии SMT - {LineFuji}", mes);

            return "true";
        }

        public string Stop()
        {
            return "true";
        }

        void HttpGo() //Отправка запроса на запуск линии
        {

        }

        void HttpStop()
        {

        }

        string CheckProgramms(string LineName)
        {
            //Список программ у протокола
            var ListMachine = fas.EP_PGName.Where(c => c.IDProtocol == _protocolID & c.Type == _topbot).Select(c => new { PGName = c.Name, MachineName = fas.EP_Machine.Where(b => b.ID == c.IDMachine).Select(b => b.Name).FirstOrDefault() }).ToList();

            //Записываем в массив классы машин
            List<SMTProgramms> List = new List<SMTProgramms>() { new LazerMarker(), new StencilPrinter(), new PPMachine(), new ReflowOven(), new SPI(), new PreReflowAOI(), new PostReflowAOI() };
            // В каждом классе свойству линия присваиваем линию
            List.ForEach(c => c.LineName = LineName);

            //Записываем в каждый класс имя программы для определённой машины
            foreach (var item in ListMachine)
                List.Where(b => b.GetType().Name == item.MachineName).FirstOrDefault().Name = item.PGName;


            var listResult = List.Select(c => new { Result = c.CheckProgramm() }).ToList().Where(c => c.Result == false);
            return listResult.Count() == 0 ? "true" : string.Join(",", listResult.Select(b=>b.GetType().Name));
        }

        void SetStartStatus(bool isStart)
        {
            var line = fas.EP_Protocols.Where(c => c.ID == _protocolID & c.Line == _line).FirstOrDefault();
            if (_topbot == "TOP")
            {
                line.StartStatusTOP = isStart;
                line.IsActiveTOP = isStart;
            }
            else
            {
                line.StartStatusBOT = isStart;
                line.IsActiveBOT = isStart;
            }           
        }

        void SetLog(int Step, string Desc = "")
        {
            var lotid = fas.EP_Protocols.Where(c => c.ID == _protocolID & c.Line == _line).Select(b => b.LOTID).FirstOrDefault();
            EP_Log eP_Log = new EP_Log()
            {
                IDProtocol = _protocolID,
                Description = Desc,
                IDStep = Step,
                TOPBOT = _topbot,
                Date = DateTime.UtcNow.AddHours(2),
                LOTID = lotid,
            };

            fas.EP_Log.Add(eP_Log);            
        }
    }
}