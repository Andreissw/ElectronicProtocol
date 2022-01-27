using ElectronicProtocolStartLine.Class;
using ElectronicProtocolStartLine.Class.Monitoring;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{
    public class MonitoringController : Controller
    {
        SMDCOMPONETSEntities smd = new SMDCOMPONETSEntities();
        FASEntities fas = new FASEntities();
        QAEntities1 qa = new QAEntities1();

        // GET: Monitoring
        public ActionResult Index(List<Fuji> fuji)
        {
            //var result = CheckProgramms(32);
            // Проверка Fuji
            fuji = CheckFuji(fuji);

            // Проверка протокола
            CheckingProtocol();

            return View(fuji);
        }


        List<Fuji> CheckFuji(List<Fuji> fuji)
        {
            fuji = GetFujis(); // Список текущей программы и его макимального кол-во модулей на Fuji из таблицы NewFujiScrapSQliteLine1 по линиям 

            foreach (var item in fuji)
            {
                if (!item.IsDataMonitoring())
                { //Если в таблице EP_Monitoring нет данных по линии
                    item.addDataMon(); continue; //Добавляем запись и переход к след. итерации
                }
                //var PG = item.CheckDataMonitoring();
                if (item.GetMonitorProgramm() != item.ProgrammName)
                { //Если программа сменилась, высылаем оповещение
                    HttpStop(); //отключаем линию
                    Email.SendEmailFuji($"Смена программы на Fuji | Линия {item.Line}", $"Добрый день! </br><h1>Смена программы с {item.GetMonitorProgramm()} </h1> <h1> На  {item.ProgrammName} </h1>");
                    //RunEmailFAS($"Смена программы на Fuji | Линия {item.Line}", $"Добрый день! </br><h1>Смена программы с {item.GetMonitorProgramm()} </h1> <h1> На  {item.ProgrammName} </h1>");               
                    item.UpdateDataMon();
                }
                
            }

            return fuji;
        }
        void CheckingProtocol()
        {      
            //Выгружаю список протоколов (ключ), которые потенциально могут запуститься и не запущены в текущее время
            var listPrtotocols= fas.EP_ProtocolsInfo.GroupBy(c=> new { LOTID = c.EP_Protocols.LOTID, Line = c.line, TOPBOT = c.TOPBOT, ProtocolID = c.ProtocolID, Manuf = c.EP_TypeVerification.Manufacter })
                .Where(c=>c.Key.Line != null)
               .Select(c => new PrtMon 
            { 
                Manufacter = c.Key.Manuf,
                ID = c.Key.ProtocolID,
                LOTID = c.Key.LOTID,
                IsStartStatusTOP = c.Select(b=>b.EP_Protocols.StartStatusTOP).FirstOrDefault(),
                IsStartStatusBOT = (bool)c.Select(b => b.EP_Protocols.StartStatusBOT).FirstOrDefault(), 
                IsActiveTOP = c.Select(b => b.EP_Protocols.IsActiveTOP).FirstOrDefault(), 
                IsActiveBOT = c.Select(b => b.EP_Protocols.IsActiveBOT).FirstOrDefault(),
                Line = c.Key.Line,
                TOPBOT = c.Key.TOPBOT,
                NameProtocol = c.Select(b=>b.EP_Protocols.NameProtocol).FirstOrDefault(),
                

            }).Where(c => (c.IsActiveTOP != false || c.IsActiveBOT != false) & c.Manufacter == "Цех поверхностного монтажа").ToList();
          
            foreach (var item in listPrtotocols)
            {
                ViewData viewdata = new ViewData();
                viewdata.GetView(item.LOTID);
                if (!viewdata.IsActive) continue;

                               
                CheckStuckPrt(item);//Проверка запущенного протокола  
                CheckStartPrt(item);   //Проверка не запущенного протокола          
            }

            fas.SaveChanges();

            //CheckStuckPrt(listPrtotocols);//Проверка протоколов, для остановки, которые работают
            //CheckStartPrt(listPrtotocols);//Проверка протоколов, для старта, которые стоят
            //Перечисление списка
        }

        void CheckStuckPrt(PrtMon p)
        {
            //Проверяем протокол на резлуьтат проверки
            var result = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == p.ID & c.line == p.Line & c.Visible == true  & c.EP_TypeVerification.Manufacter == p.Manufacter & c.TOPBOT == p.TOPBOT & c.Start == true)
                .Select(c => c.Result
                 ).Where(c=> c == null  || c == "NOK").ToList();

            //Если данные найдены, значит в протоколе есть не првоеренные модули, останавливаем линию
            if (result.Count() != 0)
            {
                HttpStop(); //Остновка протокола
                SetStartStatus(p);
                SetLog(p,2);

                Class.ViewData viewData = new Class.ViewData();
                viewData.GetView(p.LOTID);
                //var protocolname = fas.EP_Protocols.Where(c => c.ID == ID).FirstOrDefault().NameProtocol;
                //Сообщение в почту о протоколе
                string lineMes = $@"<div> <h2> Добрый день!</h2> </div>
                        <div> <h2> Заказ: {viewData.NameOrder}</h2> </div>
                        <div style=""width: 500px; background-color:lightcoral""> <h2> Электронный протокол | {p.NameProtocol} Сторона {p.TOPBOT} | Линия {p.Line} | - Остановлен! </h2> </div>";

                //var LineFuji = fas.EP_Log.Where(c => c.IDProtocol == p.ID & c.TOPBOT == _topbot).OrderByDescending(c => c.Date).Where(c => c.IDStep == 1).Select(c => c.Description).FirstOrDefault();

                Email.SendEmailProtocol($"Остановка линии SMT - {p.Line}", lineMes);
                return;
            }
           
        }

        void CheckStartPrt(PrtMon p)
        {         
            //Проверяем протокол на результат проверки
            var result = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == p.ID &  c.line == p.Line & c.Visible == true 
            & c.EP_TypeVerification.Manufacter == p.Manufacter & c.TOPBOT == p.TOPBOT & c.Start == false)
                .Select(c => c.Result ).Where(c => c == null || c == "NOK").ToList();

            //Если ничего не найдено, значит проверка проткола прошла успешно, можно запускать линию
            if (result.Count() == 0)
            {
                ClassProtocol protocol = new ClassProtocol(p);
                protocol.Start();
                #region
                ////foreach (var _topbot in fas.EP_ProtocolsInfo.Where(c=>c.ProtocolID == item).Select(c=>c.TOPBOT).Distinct())
                ////{
                //var FujiPG = fas.EP_PGName.Where(c => c.IDProtocol == ID & c.IDMachine == 4 & c.Type == _topbot).Select(c=>c.Name).FirstOrDefault();
                //if (FujiPG == null)            
                //    return;

                //var LineFuji = fas.EP_Monitoring.Where(c => c.ProgrammName == FujiPG.Substring(0,c.ProgrammName.Length)).Select(c => c.Line).FirstOrDefault();

                //if (LineFuji == null)
                //    return;

                //if (LineFuji == "")
                //    return;

                ////LineFuji = fas.EP_Monitoring.Select(c => c.Line).FirstOrDefault();

                //if (!CheckProgramms(ID, LineFuji, _topbot)) // Проверка программ                
                //    return;

                //HttpGo(); //Отправка протокола на старт
                //SetStartStatus(ID, true,x); //Устанавливаем статус протоколу
                //SetLog(ID, 1, _topbot,LineFuji);
                ////Сообщение в почту о протоколе
                //Class.ViewData view = new Class.ViewData();
                //view.GetView(result.FirstOrDefault().LOTID);
                ////var protocolName = fas.EP_Protocols.Where(c => c.ID == ID).FirstOrDefault().NameProtocol;

                //string mes = $@"<div> <h2> Добрый день!</h2> </div>
                //        <div><h2>Заказ: {view.NameOrder}</h2></div>
                //        <div style=""width: 500px; background-color:lightgreen""> <h2> Электронный протокол | {result.FirstOrDefault().ProtocolName} Сторона {x}| - активирован </h2> </div>";

                //Email.RunEmailFAS($"Запуск линии SMT - {LineFuji}", mes);
                #endregion
                return;               

            }
            //необходимо посмотреть сколько времени прошло с последнего действия пользователей, если проткол начали заполнять и прошло 12 часов, значит надо обнулить его

            //Записываем последний элемент записи шага и его даты 
            var ListDateAfter = fas.EP_Log.Where(c => c.IDProtocol == p.ID & c.line == p.Line & c.TOPBOT == p.TOPBOT  & c.EP_TypeVerification.Manufacter == p.Manufacter & new List<int>() { 1,4,2,3,5,14}.Contains(c.IDStep))
                .OrderByDescending(c => c.Date)
                .Select(c => new { date = c.Date, step = c.IDStep, Manuf = c.EP_TypeVerification.Manufacter}).ToList();

            if (ListDateAfter.Count == 0)
                return;

            var DateAfter = ListDateAfter.Where(c => !new List<string>() { "Входной контроль", "Цех Сборки" }.Contains(c.Manuf)).FirstOrDefault();

            if (DateAfter == null)
                return;


            DateTime LastDateAction;

            //Если последний элемент был созданием лота или обнулиением, то берём первую дату после этих этапов

            if (DateAfter.step == 3 || DateAfter.step == 4 || DateAfter.step == 14) //Проткол создан или обновлён, смотрим первую дату после этих этапов
                LastDateAction = fas.EP_Log.Where(b => b.IDProtocol == p.ID & b.line == p.Line & b.IDVeryf != null & b.Date >= DateAfter.date & b.TOPBOT == p.TOPBOT)
                  .OrderBy(b => b.Date).Select(b => b.Date).FirstOrDefault();
            else if (DateAfter.step == 1)//Если протокол был запущен, значит выход
                return;
            else 
                LastDateAction = DateAfter.date;

            //Если дата равно null Выход
            if (LastDateAction == DateTime.MinValue)
                return;

            //Получаем результат часов
            var ResultDateSpan = (DateTime.UtcNow.AddHours(2) - LastDateAction).Duration();

            if (ResultDateSpan.TotalHours >= 12) //Если превышает 12 часов то обнуляем 
            {//Обновление протокола если прошло 12 часов после первых действий
                RefreshProtocol(p);
                SetLog(p, 3);            
                Class.ViewData view = new Class.ViewData();
                view.GetView(p.LOTID);
              
                CreateNewProtocol(p, view);
            }

        }

        void CreateNewProtocol(PrtMon p,ViewData view)
        {
            CreateProtocol _cp = new CreateProtocol() {prtMon = p, Order = view.NameOrder};
            _cp.GetCountProtocols();
            _cp.AddInfo();
            _cp.SetLOG();

            Email.SendEmailProtocol($"Обновление протокола {p.NameProtocol} Сторона {p.TOPBOT}. Заказ: {_cp.Order}", $@"<div><h2> Добрый день!</h2> </div>
                        <div><h2> По Заказу { _cp.Order } обновлён новый протокол {p.NameProtocol}, Сторона {p.TOPBOT}, Линия {p.Line} </h2></div> ");

        }

        void ActiveOff(int ID,string topbot)
        {
            var line = fas.EP_Protocols.Where(c => c.ID == ID);
            if (topbot == "TOP")           
                line.FirstOrDefault().IsActiveTOP = false;
            else
                line.FirstOrDefault().IsActiveBOT = false;

        }

        void RefreshProtocol(PrtMon p)
        {
            var list = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == p.ID & c.line == p.Line & c.EP_TypeVerification.Manufacter == p.Manufacter & c.TOPBOT == p.TOPBOT);
            foreach (var item in list)
            {
                //item.Signature = false;
                item.Visible = false;              
            }
            
        }   

        void SetLog(PrtMon p, int Step, string Desc = "")
        {
            //var lotid = fas.EP_Protocols.Where(c => c.ID == p.ID & c.Line == p.Line).Select(b => b.LOTID).FirstOrDefault();
            EP_Log eP_Log = new EP_Log()
            {
                IDProtocol = p.ID,
                Description = Desc,
                IDStep = Step,
                TOPBOT = p.TOPBOT,
                Date = DateTime.UtcNow.AddHours(2),
                LOTID = p.LOTID,
                line = p.Line,
            };

            fas.EP_Log.Add(eP_Log);
            //fas.SaveChanges();
        }

     
        void HttpStop()
        { 
        
        }       

        void SetStartStatus(PrtMon p)
        {
            var line = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == p.ID & c.line == p.Line & c.EP_TypeVerification.Manufacter == p.Manufacter & c.TOPBOT == p.TOPBOT).ToList();
            foreach (var item in line)  item.Start = true;     
        }      

        List<Fuji> GetFujis()
        {

            var listLines = smd.NewFujiScrapSQliteLine1.Select(c => c.Line).Distinct();
            var pgs = listLines.Select(c => new
            {

                Line = c,
                ProgrammName = smd.NewFujiScrapSQliteLine1.OrderByDescending(b => b.DateTime).Where(b => b.Line == c).FirstOrDefault().Recipe

            }).ToList();

            return pgs.Select(c => new Fuji
            {

                Line = c.Line,
                ProgrammName = c.ProgrammName,
                MaxModule = (int)smd.NewFujiScrapSQliteLine1.Where(b => b.Line == c.Line & c.ProgrammName == b.Recipe).Max(b => b.Module),


            }).ToList();

        }

    }
}