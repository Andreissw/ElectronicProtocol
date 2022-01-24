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
        public ActionResult EditDetails(FormCollection fm)
        {
            if (Session["UsID"] == null)
                return RedirectToAction("Index");

            var id = int.Parse( fm["ID"].ToString());
            var LOTID = int.Parse( fm["LOTID"].ToString());
            if (fm["Result"] == null)
            {
                TempData[$"Er{fm["item.ID"].ToString()}"] = $"Укажите Результат проверки";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = fm["TOPBOT"].ToString() });
            }

            if (fm["item.RFID"] == "")
            {
                TempData[$"Er{fm["item.ID"].ToString()}"] = $"Отсканируйте бейджик";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = fm["TOPBOT"].ToString() });
            }

            var RFID = fm["item.RFID"].ToString();
            var IDPrInf = int.Parse( fm["item.ID"].ToString());
           

            var resultPRID = fas.EP_ProtocolsInfo.Where(b => b.ID == IDPrInf).Select(b =>
            
                new { 
                    
                    IDService = fas.EP_TypeVerification.Where(c => c.ID == b.TypeVerifID).Select(c => c.IDService).FirstOrDefault(),
                    Serive = fas.EP_Service.Where(x=>x.ID == fas.EP_TypeVerification.Where(c => c.ID == b.TypeVerifID).Select(c => c.IDService).FirstOrDefault()).Select(x=>x.Name).FirstOrDefault(),                
                
                } ).ToList();

            var ListUserData = fas.FAS_Users.Where(c => c.RFID == RFID).Select(c => 
            
            new { 
                ServiceID = c.IDService, 
                Name = c.UserName.Replace("."," "), 
                Service = fas.EP_Service.Where(x=>x.ID == c.IDService).Select(x=>x.Name).FirstOrDefault(),
                }).ToList();


            if (ListUserData.Count == 0)
            {
                TempData[$"Er{fm["item.ID"].ToString()}"] = "В базе не найден пользователь";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = fm["TOPBOT"].ToString() });
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

                TempData[$"Er{fm["item.ID"].ToString()}"] = Message;
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = fm["TOPBOT"].ToString() });
            }

            if (Session["RFID"].ToString() != RFID)
            {
                TempData[$"Er{fm["item.ID"].ToString()}"] = $"Учетная запись под которой зашли в систему - {Session["Name"].ToString().Replace("."," ")} не совпадает с отсканированным бейджиком {ListUserData.FirstOrDefault().Name}";
                TempData["Er"] = "Ошибка ввода!";
                return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = fm["TOPBOT"].ToString() });
            }

            var Result = fm["Result"].ToString();
            var desc = fm["item.Description"].ToString();
            desc = desc.Substring(0, desc.IndexOf(','));

            ProtocolEdit(IDPrInf, Result,desc);

            var idveryf = fas.EP_ProtocolsInfo.Where(c => c.ID == IDPrInf).FirstOrDefault().TypeVerifID;
            var TOBOT = fm["TOPBOT"].ToString();


            SetLog((int)resultPRID.FirstOrDefault().IDService, desc, id, idveryf, Result,5, TOBOT, LOTID);

            TempData["Success"] = "Изменении в протоколе прошли успешно!";           
            return RedirectToAction("EditProtocol", "Lot", new { ID = id, LOTID = LOTID, TOPBOT = TOBOT });
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

        void SetLog(int ServiceID, string desc, int IDProtocol, int idVeryf, string Result,int Step,string TOPBOT, int LOTID)
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

            };

            fas.EP_Log.Add(log);
            fas.SaveChanges();
        }


        

      

       

        List<DetailsFrom> SetDetail(List<DetailsFrom> details, string Manuf)
        {
            List<DetailsFrom> d = new List<DetailsFrom>();
            foreach (var item in details.Where(c=>c.Manuf == Manuf))
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

      

        [HttpPost]
        //public ActionResult Create(CreateProtocol createProtocol)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        ModelState.AddModelError("", "Неверно введены данные");
        //        TempData["Er"] = "Неверный тип ввода данных";
        //        return RedirectToAction("CreateProtocol");
        //    }

        //    foreach (var item in ModelState.Values)           
        //        if (item.Value.AttemptedValue  == "")
        //        {
        //            ModelState.AddModelError("", "Неверно введены данные");
        //            TempData["Emp"] = $"Не все поля заполенные";
        //            return RedirectToAction("CreateProtocol");
        //        }

        //    if (Session["RFID"].ToString() != createProtocol.LoginData.RFID)
        //    {
        //        ModelState.AddModelError("", "Неверно введены данные");
        //        TempData["NotPass"] = $"Ошибка создание протокола, отсканированный бейджик(RFID) не совпадает с бейджиком(RFID), который был отсканирован при входе";
        //        return RedirectToAction("CreateProtocol");
        //    }


        //    if (CheckNameSpec(createProtocol))
        //    {
        //        ModelState.AddModelError("", "Неверно введены данные");
        //        TempData["Name"] = $"С таким именем уже создана спецификация";
        //        return RedirectToAction("CreateProtocol");
        //    }

            
        //    //var ID = ADDProtocol(createProtocol);
        //    AddInfo(ID);
        //    var IDService = fas.EP_Service.Where(c => fas.FAS_Users.Where(b => b.RFID == createProtocol.LoginData.RFID).FirstOrDefault().IDService == c.ID).FirstOrDefault().ID;
        //    SetLog(IDService,ID);

        //    return RedirectToAction("WorkForm", "Home", createProtocol.LoginData);
        //}      

        //bool CheckNameSpec(CreateProtocol CP)
        //{
        //   return fas.EP_Protocols.Where(c => c.NameSpec == CP.NameSpec).Select(c => c.NameSpec == c.NameSpec).FirstOrDefault();
        //}

        void AddInfo(int ProtocolID)
        {
            
            var IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c=>c.Num).ToList();

            foreach (var item in IdList)
            {
                EP_ProtocolsInfo INFO = new EP_ProtocolsInfo()
                {
                    TypeVerifID = item.ID,
                    DateCreate = DateTime.UtcNow.AddHours(2),
                    ProtocolID = ProtocolID,
                    Signature = false,
                };

                fas.EP_ProtocolsInfo.Add(INFO);
            }

            fas.SaveChanges();
        }

        //int ADDProtocol(CreateProtocol CP)
        //{
        //    EP_Protocols ep = new EP_Protocols()
        //    {
        //        NameClient = CP.NameClient,
        //        NameOrder = CP.NameOrder,
        //        DateCreate = DateTime.UtcNow.AddHours(2),
        //        NameSpec = CP.NameSpec,
        //        ManufLevel = "Входной контроль",
        //    };

        //    fas.EP_Protocols.Add(ep);
        //    fas.SaveChanges();
        //    var ID = fas.EP_Protocols.OrderByDescending(c=>c.ID).FirstOrDefault().ID;
        //    return ID;
        //}

        List<string> CheckUser(string RFID)
        {
            List<string> list = new List<string>();
            var L = fas.FAS_Users.Where(c => c.RFID == RFID).Select(c => 
            new
            {
                 name = c.UserName,
                 service = fas.EP_Service.Where(b => b.ID == c.IDService).FirstOrDefault().Name

            }).ToList();

            foreach (var item in L)
            {
                list.Add(item.name);
                list.Add(item.service);
            }
            return list;
           
               
          
        }

    }
}