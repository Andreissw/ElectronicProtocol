using ElectronicProtocolStartLine.Class;
using ElectronicProtocolStartLine.Class.Monitoring;
using ElectronicProtocolStartLine.Class.Protocols;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{
    public class LotController : Controller
    {
        // GET: Lot
        FASEntities fas = new FASEntities();
        public ActionResult Index(int LOTID)
        {
            List<ProtocolTables> protocols = new List<ProtocolTables>();
            var ListProtocols = new FASEntities().EP_Protocols.Where(c => c.LOTID == LOTID & c.EP_ProtocolsInfo.Select(b=>b.Visible).Contains(true)).OrderByDescending(c => c.DateCreate).Select(c => new 
            {
                ID = c.ID, 
                DateCreate = c.DateCreate, 
                LOTID = (int)c.LOTID,
                TOPBOT = c.TOPBOT, 
                nameprt = c.NameProtocol,
                IsActiveTOP = c.IsActiveTOP,
                IsActiveBOT = c.IsActiveBOT,  
                Line = c.Line,
            
            }).ToList();

            if (ListProtocols.Count == 0)  return RedirectToAction("Index","SetLine",new { LOTID = LOTID });
           

            var ServiceName = Session["Service"].ToString();
            foreach (var item in ListProtocols)
            {
                var view = new ViewData();
                view.GetView(LOTID);
                ProtocolTables protocolTables = new ProtocolTables()
                {
                    LOTID = LOTID,
                    ID = item.ID,
                    NameOrder = view.NameOrder,
                    Type = view.ClientType,
                    NameProtocol = item.nameprt,
                    DateCreate = item.DateCreate,
                    IsActiveBOT = item.IsActiveBOT,
                    IsActiveTOP = item.IsActiveTOP,
                    Line = item.Line,
                    Programms = fas.EP_PGName.Where(b => b.IDProtocol == item.ID).Select(c => new Programms
                    {

                        Machine = c.EP_Machine.Name,
                        PGName = c.Name,

                    }).ToList(),

                    TOPBOT = item.TOPBOT == true ? "Двусторонняя плата" : "Односторонняя плата",
                    TOP = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == item.ID & c.TOPBOT == "TOP" & c.Visible == true).GroupBy(c => c.ProtocolID).Select(c => new TOBOT {

                        TOPBOT = "Вверх платы",
                        CountAll = c.Where(b => b.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountTrue = c.Where(b => b.Signature == true & b.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountProtocolAll = c.Count(),
                        CountProtocolTrue = c.Where(b => b.Signature == true).Count(),
                        IsActive = c.Select(b => b.EP_Protocols.IsActiveTOP).FirstOrDefault()

                    }).FirstOrDefault(),

                    BOT = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == item.ID & c.TOPBOT == "BOT" & c.Visible == true).GroupBy(c => c.ProtocolID).Select(c => new TOBOT
                    {

                        TOPBOT = "Низ платы",
                        CountAll = c.Where(b => b.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountTrue = c.Where(b => b.Signature == true & b.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountProtocolAll = c.Count(),
                        CountProtocolTrue = c.Where(b => b.Signature == true).Count(),
                        IsActive = c.Select(b => b.EP_Protocols.IsActiveBOT).FirstOrDefault()

                    }).FirstOrDefault(),
                };
                protocols.Add(protocolTables);
            }

            return View(protocols);
        }

        public ActionResult GetPGName(int ProtocolID, string name, string Protocolname)
        {
            if (Session["UsID"] == null)
                return RedirectToAction("Index");

            List<Programms> programmNames = new List<Programms>();            
            programmNames = fas.EP_PGName.Where(c => c.IDProtocol == ProtocolID & c.Visible == true).Select(c => new Programms
            {
                ProtocolName = Protocolname,
                NameOrder = name,
                TOPBOT = c.Type,
                Machine = fas.EP_Machine.Where(b => b.ID == c.IDMachine).FirstOrDefault().Name,
                PGName = c.Name,

            }).ToList();

            return View(programmNames);
        }

        public ActionResult GetReportProtocol(int ID,int LOTID, string NameProtocol, string NameOrder)
        {
            if (Session["UsID"] == null)
                return RedirectToAction("Index","Home");

            var Report = new ProtocolReport()
            {
                ID = ID,
                LOTID = LOTID,
                TOBOT = (bool)fas.EP_Protocols.Where(x => x.ID == ID).FirstOrDefault().TOPBOT,
                ProtocolName = NameProtocol,
                Order = NameOrder,
            };

            var Result = fas.EP_ProtocolsInfo.Where(c => c.ProtocolID == ID & c.Visible == true).Select(c => new InfoProtocol()
            {
                BOTTOB = c.TOPBOT,
                Date = fas.EP_Log.Where(b => b.IDProtocol == ID & b.IDVeryf == c.TypeVerifID & b.TOPBOT == c.TOPBOT).OrderByDescending(b => b.Date).FirstOrDefault().Date,
                Document = fas.EP_TypeDocument.Where(b => fas.EP_TypeVerification.Where(x => x.ID == c.TypeVerifID).FirstOrDefault().TypeDocID == b.ID).FirstOrDefault().Name,
                Result = c.Result,
                Service = fas.EP_Service.Where(b => b.ID == fas.EP_TypeVerification.Where(x => x.ID == c.TypeVerifID).FirstOrDefault().IDService).FirstOrDefault().Name,
                Signature = (bool)c.Signature,
                TypeVerif = fas.EP_TypeVerification.Where(b => b.ID == c.TypeVerifID).FirstOrDefault().Name,
                UserName = fas.FAS_Users.Where(b => b.UserID == c.UserID).FirstOrDefault().UserName,
                Manuf = fas.EP_TypeVerification.Where(x => x.ID == c.TypeVerifID).FirstOrDefault().Manufacter,


            }).ToList();

            Report.OTKProtocol.InfosDetails.AddRange(Result.Where(c => c.Manuf == "Входной контроль").OrderBy(c => c.Service));
            Report.OTKProtocol.InfoCount = $"Выполнено проверок {Report.OTKProtocol.InfosDetails.Where(c => c.Signature).Count()} из { Report.OTKProtocol.InfosDetails.Count}";
            Report.OTKProtocol.ListCounts.Add(new Counts()
            {
                CountOK = Report.OTKProtocol.InfosDetails.Where(b => b.Result == "OK").Count(),
                CountNA = Report.OTKProtocol.InfosDetails.Where(b => b.Result == "N/A").Count(),
                CountNOK = Report.OTKProtocol.InfosDetails.Where(b => b.Result == "NOK").Count(),

            });


            Report.ListInfoSMT.InfosDetails.AddRange(Result.Where(c => c.Manuf == "Цех поверхностного монтажа"));
            Report.ListInfoSMT.InfoCount = $"Выполнено проверок {Report.ListInfoSMT.InfosDetails.Where(c => c.Signature).Count()} из { Report.ListInfoSMT.InfosDetails.Count}";
            Report.ListInfoSMT.ListCounts = new List<string>() { "TOP", "BOT" }.Select(c => new Counts()
            {
                CountOK = Report.ListInfoSMT.InfosDetails.Where(b => b.Result == "OK" & b.BOTTOB == c).Count(),
                CountNA = Report.ListInfoSMT.InfosDetails.Where(b => b.Result == "N/A" & b.BOTTOB == c).Count(),
                CountNOK = Report.ListInfoSMT.InfosDetails.Where(b => b.Result == "NOK" & b.BOTTOB == c).Count(),
                TOBBOT = c,

            }).ToList();

            Report.ListInfoFAS.InfosDetails.AddRange(Result.Where(c => c.Manuf == "Цех Сборки"));
            Report.ListInfoFAS.InfoCount = $"Выполнено проверок {Report.ListInfoFAS.InfosDetails.Where(c => c.Signature).Count()} из { Report.ListInfoFAS.InfosDetails.Count}";
            Report.ListInfoFAS.ListCounts.Add(new Counts()
            {
                CountOK = Report.ListInfoFAS.InfosDetails.Where(b => b.Result == "OK").Count(),
                CountNA = Report.ListInfoFAS.InfosDetails.Where(b => b.Result == "N/A").Count(),
                CountNOK = Report.ListInfoFAS.InfosDetails.Where(b => b.Result == "NOK").Count(),

            });


            return View(Report);
        }

        public ActionResult EditProtocol(int ID, int LOTID, string TOPBOT)
        {
            if (Session["UsID"] == null)
                return RedirectToAction("Index");

            if (!int.TryParse(Session["UsID"].ToString(), out int userid))
                return RedirectToAction("Index");

          
            var IDService = GetService(userid);
            GetProtocol protocol = new GetProtocol() { ID = ID, LOTID = LOTID, TOPBOT = TOPBOT };

            var lots = fas.FAS_GS_LOTs.Select(c => new LOTS { FULLOTCODE = c.FULL_LOT_Code, ID = c.LOTID }).ToList();
            lots.AddRange(fas.Contract_LOT.Select(c => new LOTS { FULLOTCODE = c.FullLOTCode, ID = (short)c.ID }));

            var lotid = fas.EP_Protocols.Where(c => c.ID == ID).Select(c => c.LOTID).FirstOrDefault();
            protocol.NameOrder = lots.Where(c => c.ID == lotid).FirstOrDefault().FULLOTCODE;

            protocol.NameProtocol = fas.EP_Protocols.Where(c => c.ID == ID).FirstOrDefault().NameProtocol;



            var prs = (from inf in fas.EP_ProtocolsInfo
                       join ver in fas.EP_TypeVerification on inf.TypeVerifID equals ver.ID
                       where inf.ProtocolID == protocol.ID & ver.IDService == IDService & inf.TOPBOT == TOPBOT & inf.Visible == true
                       orderby ver.Sort & ver.Num
                       select new DetailsFrom()
                       {

                           //TypeVerification = fas.EP_Protocols.Where(c=>c.ID == inf.ProtocolID).Select(b => b.NameClient + " " + b.NameOrder + " " + b.NameSpec).FirstOrDefault(),
                           ID = inf.ID,
                           LastResult = inf.Result,
                           LastUserResult = fas.FAS_Users.Where(b => b.UserID == inf.UserID).Select(b => b.UserName).FirstOrDefault(),
                           TypeVerification = ver.Name,
                           Manuf = ver.Manufacter,
                           DocCheck = fas.EP_TypeDocument.Where(c => c.ID == ver.TypeDocID).Select(c => c.Name).FirstOrDefault(),
                           DateCheck = (DateTime)inf.DateCreate,
                           BOTTOB = ver.TOPBOT,
                           Description = inf.Description,


                       }).ToList();

            protocol.protocolFAS.Details.AddRange(SetDetail(prs, "Цех Сборки"));
            protocol.protocolSMT.Details.AddRange(SetDetail(prs, "Цех поверхностного монтажа"));
            protocol.ProtocolOTK.Details.AddRange(SetDetail(prs, "Входной контроль"));

            return View(protocol);
        }
        int GetService(int UserID)
        {
            return (int)fas.FAS_Users.Where(c => c.UserID == UserID).Select(c => c.IDService).FirstOrDefault();

        }

        List<DetailsFrom> SetDetail(List<DetailsFrom> details, string Manuf)
        {
            List<DetailsFrom> d = new List<DetailsFrom>();
            foreach (var item in details.Where(c => c.Manuf == Manuf))
            {
                var det = new DetailsFrom()
                {
                    ID = item.ID,
                    LastResult = item.LastResult,
                    LastUserResult = item.LastUserResult,
                    DateCheck = item.DateCheck,
                    DocCheck = item.DocCheck,
                    Manuf = item.Manuf,
                    Result = item.Result,
                    Service = item.Service,
                    TypeVerification = item.TypeVerification,
                    UserName = item.UserName,
                    BOTTOB = item.BOTTOB,
                    Description = item.Description,
                };

                d.Add(det);
            }

            return d;
        }

        public JsonResult StartProtocol(int idProtocol,int Line,string TOPBOT, int LOTID )
        {
            ClassProtocol prt = new ClassProtocol(new PrtMon() { ID = idProtocol }, TOPBOT, LOTID);
            var result = prt.Start();
            if (result != "true")
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}