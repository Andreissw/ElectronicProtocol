using ElectronicProtocolStartLine.Class;
using ElectronicProtocolStartLine.Class.SetLine;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{
    public class SetLineController : Controller
    {
        // GET: SetLine
        FASEntities fas = new FASEntities();

        public List<byte?> LineFAS = new List<byte?>() { 1,2,3,4,5,6 };

        public List<byte?> LineSMT = new List<byte?>() { 1, 2, 3, 4};

        public ActionResult Index(int LOTID)
        {
            Lines lines = new Lines() { LOTID = LOTID , Manuf = Session["Manuf"].ToString()};         

            List<byte?> result = lines.Manuf == "Цех поверхностного монтажа"? LineSMT : LineFAS;

            foreach (var item in result) lines.Linelist.Add(new SelectListItem() { Value = item.ToString(), Text = item.ToString() });
          
             lines.TOPList = fas.EP_Protocols.Where(c => c.LOTID == LOTID).Select(c => c.TOPBOT).FirstOrDefault() == true ? 
                  new List<SelectListItem>() { new SelectListItem() { Value = "TOP" , Text = "TOP"}, new SelectListItem() { Value = "BOT", Text = "BOT" } } 
                 : new List<SelectListItem>() { new SelectListItem() { Value = "TOP", Text = "TOP" } };           

            if (lines.Manuf == "Цех Сборки")
            {              
                lines.ProtocolsList = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.LOTID == LOTID & c.EP_TypeVerification.Manufacter != "Цех Сборки")
                    .GroupBy(b => new { b.ProtocolID, b.line }).Select(c => new 
                    {
                        Count = fas.EP_ProtocolsInfo.Where(b=> (c.Select(z=>z.ID).Contains(b.ID) & b.EP_TypeVerification.Name != "Цех Сборки" & b.Signature == false)).Count(),
                        Protocol = c.Where(b=>b.EP_Protocols.ID == c.Key.ProtocolID).Select(b=>b.EP_Protocols.NameProtocol + c.Key.line).FirstOrDefault(),

                    }).Where(b=>b.Count == 0).Select(b=> new SelectListItem() { 
                        Value = b.Protocol,
                        Text = b.Protocol,
                    }).ToList();
            }


            return View(lines);
        }

        public ActionResult SetLine(Lines lines)
        {
            var result = byte.TryParse(lines.Line, out byte _line);
            if (result == false) { TempData["Err"] = "Ошибка"; return RedirectToAction("Index", new { LOTID = lines.LOTID, Manuf = lines.Manuf }); }

            lines.TOPBOT = lines.Manuf == "Цех Сборки" ? "TOP" : lines.TOPBOT;

            var Check = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.LOTID == lines.LOTID & c.EP_TypeVerification.Manufacter == lines.Manuf & c.TOPBOT == lines.TOPBOT & c.line == _line)
                .FirstOrDefault();

            if (Check != null)
            {
                TempData["Err"] = $"Протокол со стороной {lines.TOPBOT} и линией {lines.Line} уже был создан ранее";
                return RedirectToAction("Index", new { LOTID = lines.LOTID, Manuf = lines.Manuf });
            }
                   

            lines.ProttocolID = fas.EP_Protocols.Where(c => c.LOTID == lines.LOTID).Select(c => c.ID).FirstOrDefault();            

         
                foreach (var item in fas.EP_TypeVerification.Where(c=>c.Manufacter == lines.Manuf))
                {
                    EP_ProtocolsInfo INFO = new EP_ProtocolsInfo()
                    {
                        TypeVerifID = item.ID,
                        DateCreate = DateTime.UtcNow.AddHours(2),
                        ProtocolID = lines.ProttocolID,
                        Signature = false,
                        TOPBOT = lines.TOPBOT,
                        Visible = true,
                        Itter = 1,
                        line = _line,
                    };

                    fas.EP_ProtocolsInfo.Add(INFO);
                }

            fas.SaveChanges();

            SetLog(lines,_line);

            var _nameProtocol = fas.EP_Protocols.Where(c => c.LOTID == lines.LOTID).Select(c => c.NameProtocol).FirstOrDefault();
            ViewData viewData = new ViewData();
            viewData.GetView(lines.LOTID);

            Email.SendEmailProtocol($"Создание линии: {lines.Line}. Цех: {lines.Manuf}. Протокол: {_nameProtocol}. Сторона: {lines.TOPBOT}",$"Добрый день! Пользователь {Session["Name"]} Создал линию: {lines.Line}. Цех {lines.Manuf}. Сторона: {lines.TOPBOT}. Протокол {_nameProtocol} Заказ {viewData.NameOrder}");
            return RedirectToAction("Index","Lot", new { LOTID = lines.LOTID });
        }

        void SetLog(Lines lines, byte? line)
        {
            var _userid = short.Parse(Session["UsID"].ToString());
            EP_Log eP_Log = new EP_Log()
            {
                Date = DateTime.UtcNow.AddHours(2),
                Description = $"Создание протокола по линии: {lines.Line}. Цех: {lines.Manuf}. Сторона: {lines.TOPBOT}",
                IDProtocol = lines.ProttocolID,
                IDStep = 14,
                LOTID = lines.LOTID,
                UserID = _userid,     
                line = line,
            };

            fas.EP_Log.Add(eP_Log);
            fas.SaveChanges();
        }
    }
}