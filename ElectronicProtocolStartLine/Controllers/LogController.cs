using ElectronicProtocolStartLine.Class.Log;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{
    public class LogController : Controller
    {
        // GET: Log
        FASEntities fas = new FASEntities();
        public ActionResult Index(int ProtocolID,bool top)
        {
            LogInfo logInfo = new LogInfo() { IsMany = ProtocolID == 0 ? true : false, IsTop = top };

            logInfo.Logs = fas.EP_Log.OrderByDescending(c => c.Date).Select(c => new Logs
            {
                Line = c.line,
                Date = c.Date,
                Description = c.Description,
                TypeVeryf = c.EP_TypeVerification.Name,
                Result = c.Result,
                Service = c.EP_Service.Name,
                UserName = c.FAS_Users.UserName,
                LotID = c.LOTID,
                NameProtocol = c.EP_Protocols.NameProtocol,
                ProtocolID = c.IDProtocol,
                StepName = c.EP_StepLog.StepName,
                TOPBOT = c.TOPBOT,
                NameDocument = fas.EP_Doc.Where(b => b.ID == c.DocumentID).Select(b => b.NameFile + b.extension).FirstOrDefault(),

            }).ToList();

            if (!logInfo.IsMany)
                logInfo.Logs = logInfo.Logs.Where(c => c.ProtocolID == ProtocolID).ToList();

            Class.ViewData view = new Class.ViewData();

            foreach (var x in logInfo.Logs)
            {
                if (x.LotID != null)
                {
                    if (x.LotID != 0)
                    {                   
                        view.GetView((int)x.LotID);
                        x.OrderName = view.NameOrder;
                    }
                }

            }

            return View(logInfo);
        }
     
    }
}