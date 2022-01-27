using ElectronicProtocolStartLine.Class;
using ElectronicProtocolStartLine.Class.Orders;
using ElectronicProtocolStartLine.Class.Protocols;
using ElectronicProtocolStartLine.Models;
using ElectronicProtocolStartLine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Controllers
{

    public class HomeController : Controller
    {
        FASEntities fas = new FASEntities();
   

        public ActionResult Index()
        {
            HttpContext.Session["userID"] = "1";
            Session["RFID"] = null;
            Session["Name"] = null;
            Session["UsID"] = null;
            Session["Service"] = null;
            Session["Manuf"] = null;

            return View();
        }

        [HttpPost]     
        public ActionResult GetUserID(LoginData login, bool Loggin)
        {
            //if (Session["UsID"] == null)
            //    return RedirectToAction("Index");

            if (!Loggin)
            {
                Session["Service"] = 0;
                Session["RFID"] = 0;
                Session["Name"] = 0;
                Session["UsID"] = 0;
                Session["Manuf"] = 0;
                return RedirectToAction("WorkForm");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Не верный формат пароля";
                return RedirectToAction("Index");
            }
            var ListUser = CheckUser(login.RFID);

           

            if (ListUser.Count == 0)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Не верный пароль";
                return RedirectToAction("Index");
            }

            if (ListUser[1] == null)
            {
                ModelState.AddModelError("", "Не верно введены данные");
                TempData["Er"] = "Недостаточно прав. У вас не указана служба в базе данных, обратитесь к Володину А.А.";
                return RedirectToAction("Index");
            }

            TempData["Er"] = null;
            login.UserName = ListUser.FirstOrDefault();
            login.UserID = fas.FAS_Users.Where(c => c.UserName == ListUser.FirstOrDefault()).Select(c => c.UserID).FirstOrDefault();
            Session["Service"] = ListUser[1];
            Session["RFID"] = login.RFID;
            Session["Name"] = ListUser[0];
            Session["UsID"] = login.UserID;
            Session["Manuf"] = ListUser[2];
            return RedirectToAction("WorkForm");
        }

  
        public ActionResult WorkForm()
        {
            if (Session["UsID"] == null)
                return RedirectToAction("Index");

            //Projects project = new Projects();
            HomePage homePage = new HomePage();
            List<Orders> orders = new List<Orders>();

            var Service = Session["Service"].ToString();
            var ListLOTID = fas.EP_Protocols.OrderByDescending(c=>c.DateCreate).Select(c => c.LOTID).Distinct();

            foreach (var item in ListLOTID)
            {
                ViewData view = new ViewData();               
                view.GetView((int)item);
                Orders orders1 = new Orders()
                {
                    ID = item,
                    NameClient = view.NameClient,
                    DateCreate =view.DateCreate,
                    NameOrder = view.NameOrder,
                    Spec = view.NameSpec,
                    IsActive = view.IsActive,
                    TypeClient = view.ClientType,   
                };

                orders.Add(orders1);                
            }

            var PTS = fas.EP_Protocols.Where(c => c.StartStatusTOP == true || c.StartStatusBOT == true).ToList();
            if (PTS.Count != 0)           
                foreach (var item in PTS)
                {
                    if ((bool)item.StartStatusBOT)
                        homePage.ProtocolsStart.Add(GetTableStart("BOT", item));

                    if ((bool)item.StartStatusTOP)
                            homePage.ProtocolsStart.Add(GetTableStart("TOP", item));
                }            
            

            homePage.orders.AddRange(orders.Where(c => c.IsActive == true).OrderByDescending(c=>c.DateCreate));

            return View(homePage);
        }

        ProtocolTables GetTableStart( string TOPBOT, EP_Protocols pr)
        {
            ViewData viewData = new ViewData();
            viewData.GetView(pr.LOTID);
            ProtocolTables protocolTables = new ProtocolTables() {
                NameProtocol = pr.NameProtocol,               
                DateCreate = fas.EP_Log.Where(b=>b.IDProtocol == pr.ID & b.IDStep == 1 & b.TOPBOT == TOPBOT).OrderByDescending(b=>b.Date).FirstOrDefault().Date,
                TOPBOT = TOPBOT == "TOP"? "Вверх платы" : "Низ платы",
                NameOrder = viewData.NameOrder,            
            };           

            return protocolTables;

        }

        [HttpPost] 
        //public ActionResult EditDetails(FormCollection fm)
        public ActionResult EditDetails(int IDItem, int ProtocolID, string TOPBOT,int LOTID, byte? Line, GetProtocol Det )
        {

            if (Session["UsID"] == null)
                return RedirectToAction("Index");

            //var id = int.Parse(fm["ID"].ToString());
            //var LOTID = int.Parse(fm["LOTID"].ToString());
            //var Line = byte.Parse(fm["Line"].ToString());

            if (Det.Result == null)
            {
                TempData[$"Er{IDItem}"] = $"Укажите Результат проверки";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
            }

            if (Det.RFID == "")
            {
                TempData[$"Er{IDItem}"] = $"Отсканируйте бейджик";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
            }

            //var RFID = fm["item.RFID"].ToString();
            //var IDPrInf = int.Parse(fm["item.ID"].ToString());

            var resultPRID = fas.EP_ProtocolsInfo.Where(b => b.ID == IDItem).Select(b =>   new
                {
                    IDService = fas.EP_TypeVerification.Where(c => c.ID == b.TypeVerifID).Select(c => c.IDService).FirstOrDefault(),
                    Serive = fas.EP_Service.Where(x => x.ID == fas.EP_TypeVerification.Where(c => c.ID == b.TypeVerifID).Select(c => c.IDService).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),

                }).ToList();

            var ListUserData = fas.FAS_Users.Where(c => c.RFID == Det.RFID).Select(c => new
            {
                ServiceID = c.IDService,
                Name = c.UserName.Replace(".", " "),
                Service = fas.EP_Service.Where(x => x.ID == c.IDService).Select(x => x.Name).FirstOrDefault(),

            }).ToList();


            if (ListUserData.Count == 0)
            {
                TempData[$"Er{IDItem}"] = "В базе не найден пользователь";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
            }


            if (resultPRID.FirstOrDefault().IDService != ListUserData.FirstOrDefault().ServiceID)
            {
                string Message = "";

                if (ListUserData.FirstOrDefault().ServiceID == null)
                {
                    Message = $"У пользователя {ListUserData.FirstOrDefault().Name} в базе не установлена служба Обратитесь к разработичку (Володин А А)";
                }
                else
                {
                    Message = $"Пользователь {ListUserData.FirstOrDefault().Name} который находится в службе {ListUserData.FirstOrDefault().Service} Не имеет возможности изменять протоколы службы {resultPRID.FirstOrDefault().Serive}";
                }

                TempData[$"Er{IDItem}"] = Message;
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
            }

            if (Session["RFID"].ToString() != Det.RFID)
            {
                TempData[$"Er{IDItem}"] = $"Учетная запись под которой зашли в систему - {Session["Name"].ToString().Replace(".", " ")} не совпадает с отсканированным бейджиком {ListUserData.FirstOrDefault().Name}";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
            }

            //var Result = fm["Result"].ToString();
            //var desc = fm["item.Description"].ToString();

            //Det.Description = Det.Description.Substring(0, Det.Description.IndexOf(','));

            ProtocolEdit(IDItem, Det.Result, Det.Description);

            var idveryf = fas.EP_ProtocolsInfo.Where(c => c.ID == IDItem).FirstOrDefault().TypeVerifID;           


            SetLog((int)resultPRID.FirstOrDefault().IDService, Det.Description, ProtocolID, idveryf, Det.Result, 5, TOPBOT, LOTID, Line);

            TempData["Success"] = "Изменении в протоколе прошли успешно!";
            return RedirectToAction("EditProtocol", "Lot", new { ID = ProtocolID, LOTID = LOTID, TOPBOT = TOPBOT, line = Line });
        }


        void ProtocolEdit(int IdPrInfo, string Result, string desc )
        {
            var pr = fas.EP_ProtocolsInfo.Where(c => c.ID == IdPrInfo);
            pr.FirstOrDefault().Result = Result;            
            pr.FirstOrDefault().Signature = true;
            pr.FirstOrDefault().UserID = short.Parse(Session["UsID"].ToString());
            pr.FirstOrDefault().Description = desc;
            fas.SaveChanges();
        }

        void SetLog(int ServiceID, string desc, int IDProtocol, int idVeryf, string Result,int Step,string TOPBOT, int LOTID, byte? line)
        {
            EP_Log log = new EP_Log()
            {
                UserID = short.Parse(Session["UsID"].ToString()),
                ServiceID = ServiceID,
                Description = desc,
                IDProtocol = IDProtocol,
                IDVeryf = idVeryf,
                Result = Result,
                Date = DateTime.UtcNow.AddHours(2),
                IDStep = Step,
                TOPBOT = TOPBOT,
                LOTID = LOTID,
                line = line
            };

            fas.EP_Log.Add(log);
            fas.SaveChanges();
        }
        List<string> CheckUser(string RFID)
        {
            List<string> list = new List<string>();
            var L = fas.FAS_Users.Where(c => c.RFID == RFID).Select(c => 
            new
            {
                 name = c.UserName,
                 service = fas.EP_Service.Where(b => b.ID == c.IDService).FirstOrDefault().Name,
                 manufacter = fas.EP_Service.Where(b=> b.ID == c.IDService).FirstOrDefault().Manufacter,

            }).ToList();

            foreach (var item in L)
            {
                list.Add(item.name);
                list.Add(item.service);
                list.Add(item.manufacter);
            }
            return list;
           
               
          
        }

    }
}