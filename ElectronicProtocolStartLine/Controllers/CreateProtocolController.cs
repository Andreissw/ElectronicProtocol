using ElectronicProtocolStartLine.Class;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{
    public class CreateProtocolController : Controller
    {
        // GET: CreateProtocol
        List<SelectListItem> ListTOPBOT = new List<SelectListItem>() { new SelectListItem() { Text = "" }, new SelectListItem() { Text = "Одностороняя плата" }, new SelectListItem() { Text = "Двустороняя плата" }, };
        public ActionResult Index()
        {
            CreateProtocol create = new CreateProtocol()
            {
                TOPBOT = ListTOPBOT
            };

            List<string> ListOrders = new List<string>();
            using (var fas = new FASEntities())
            {
                var listlots = fas.EP_Protocols.Select(c => c.LOTID).Distinct().ToList();
                var list = fas.FAS_GS_LOTs.Where(c => listlots.Contains(c.LOTID)).Select(c => new { name = c.FULL_LOT_Code, date = c.CreateDate }).ToList().Union(fas.Contract_LOT.Where(c => listlots.Contains(c.ID)).Select(c => new { name = c.FullLOTCode, date = c.CreateDate }).ToList()).OrderByDescending(c => c.date).Select(c => c.name).ToList();
                ListOrders.AddRange(list);
            }

            Parallel.ForEach(ListOrders, x =>
            {
                create.ListOrders.Add(new SelectListItem() { Text = x });
            });

            return View(create);
        }

        public ActionResult Create(CreateProtocol createProtocol)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Ошибка";
                return RedirectToAction("Index");
            }

            using (var fas = new FASEntities())
            {
                createProtocol.LOTID = fas.FAS_GS_LOTs.Select(c => new { name = c.FULL_LOT_Code, ID = c.LOTID }).ToList().Union(fas.Contract_LOT.Select(c => new { name = c.FullLOTCode, ID = (short)c.ID }).ToList()).Where(c => c.name == createProtocol.Order).Select(c => c.ID).FirstOrDefault();
            }

            //createProtocol.UserID = int.Parse(Session["UsID"].ToString()); //Записываем userID в переменную
            createProtocol.GetCountProtocols();
            createProtocol.GenerateNameProtocol();
            createProtocol.CrtProtocol();
            createProtocol.GeneratePGName();
            //createProtocol.AddInfo();
            //createProtocol.SetLOG();
            Email.SendEmailProtocol($"Создание нового протокола {createProtocol.ProtocolName}. Заказ: {createProtocol.Order}", $@"<div><h2> Добрый день!</h2> </div>
                        <div><h2> По Заказу { createProtocol.Order } создан новый протокол {createProtocol.ProtocolName}</h2></div> ");
            return RedirectToAction("WorkForm", "Home");
        }
    }
}