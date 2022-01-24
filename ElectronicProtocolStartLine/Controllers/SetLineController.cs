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

        //public ActionResult Index()
        //{
        //    Lines lines = new Lines() { LOTID = 20147 };

        //    var listline = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.LOTID == lines.LOTID & c.Visible == false).Select(c => c.EP_Protocols.Line).ToList();

        //    //if (listline.Count == 0) return RedirectToAction("Index", "Home");

        //    foreach (var item in new List<byte>() { 1, 2, 3, 4 }) lines.Linelist.Add(new SelectListItem() { Value = item.ToString(), Text = item.ToString() });



        //    return View(lines);
        //}

        public ActionResult Index(int LOTID)
        {
            Lines lines = new Lines() { LOTID = LOTID };

            var listline = fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.LOTID == LOTID & c.Visible == false).Select(c => c.EP_Protocols.Line).Distinct().ToList();

            if (listline.Count == 0) return RedirectToAction("Index", "Home");

            foreach (var item in listline) lines.Linelist.Add(new SelectListItem() { Value = item.ToString(), Text = item.ToString() });



            return View(lines);
        }

        public ActionResult SetLine(Lines lines)
        {
            byte _line = byte.Parse(lines.Line);
            var info = fas.EP_ProtocolsInfo.Where(c=> c.EP_Protocols.LOTID == lines.LOTID & c.EP_Protocols.Line == _line).ToList();

            if (info.Count == 0) return RedirectToAction("Index", new {LOTID = lines.LOTID });           
            
            foreach (var item in info)  item.Visible = true;
            lines.ProttocolID = info.FirstOrDefault().ProtocolID;

            SetLog(lines);

            var _nameProtocol = fas.EP_Protocols.Where(c => c.LOTID == lines.LOTID).Select(c => c.NameProtocol).FirstOrDefault();
            ViewData viewData = new ViewData();
            viewData.GetView(lines.LOTID);

            Email.SendEmailProtocol($"Создание линии номер {lines.Line} под протокол {_nameProtocol}",$"Добрый день! Пользователь {Session["Name"]} Создал линию номер {lines.Line} под протокол {_nameProtocol} Заказ {viewData.NameOrder}");

            return RedirectToAction("Index","Lot", new { LOTID = lines.LOTID });
        }

        void SetLog(Lines lines)
        {
            var _userid = short.Parse(Session["UsID"].ToString());
            EP_Log eP_Log = new EP_Log()
            {
                Date = DateTime.UtcNow.AddHours(2),
                Description = "Создание протокола по линии",
                IDProtocol = lines.ProttocolID,
                IDStep = 14,
                LOTID = lines.LOTID,
                UserID = _userid,                
            };

            fas.EP_Log.Add(eP_Log);
            fas.SaveChanges();
        }
    }
}