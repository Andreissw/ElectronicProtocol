using ElectronicProtocolStartLine.Class.Protocols;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine
{
    public class CreateProtocol
    {
        public List<SelectListItem> ListOrders { get; set; }
        public int CountProtocols { get; set; } = 1;
        public List<SelectListItem> TOPBOT { get; set; }
        public int ProtocolID { get; set; }
        public int UserID { get; set; }
        public int LOTID { get; set; }

        public byte? line { get; set; }
        public bool IsTOPBOT { get; set; }
        public string ProtocolName { get; set; }
        [Required]
        public string TOPBOTName { get; set; }
        [Required]
        public string Order { get; set; }
        public CreateProtocol()
        {
            ListOrders = new List<SelectListItem>() { new SelectListItem() { Text = "" } };
        }

        public void GetCountProtocols()
        {
            using (var fas = new FASEntities())
            {
                CountProtocols = fas.EP_Protocols.Where(c => c.LOTID == LOTID & c.Line == line).Count();
            }
        }
        public void GenerateNameProtocol()
        {
            using (var fas = new FASEntities())
            {
                var ModelID = fas.FAS_GS_LOTs.Select(c => new { Name = c.FULL_LOT_Code, modelid = c.ModelID }).Union(fas.Contract_LOT.Select(c => new { Name = c.FullLOTCode, modelid = (short)c.ModelID })).Where(c => c.Name == Order).Select(c => c.modelid).FirstOrDefault();

                ProtocolName = LOTID.ToString() + "_" + fas.FAS_Models.Where(c => c.ModelID == ModelID).FirstOrDefault().ModelName
               + "_Protocol №" + CountProtocols;
            }

        }

        public void GetProtocolname()
        {
            using (var fas = new FASEntities())
            {
                ProtocolName = fas.EP_Protocols.Where(c => c.LOTID == LOTID & c.Line == line).Select(c => c.NameProtocol).FirstOrDefault();
            }           
        }

        public void CrtProtocol()
        {
            using (var fas = new FASEntities())
            {
                var isTOPBOT = GetTopBot(TOPBOTName);
                EP_Protocols ep = new EP_Protocols()
                {
                    DateCreate = DateTime.UtcNow.AddHours(2),
                    LOTID = LOTID,
                    TOPBOT = isTOPBOT,
                    IsActiveTOP = true,
                    IsActiveBOT = isTOPBOT,
                    StartStatusTOP = false,
                    StartStatusBOT = false,
                    NameProtocol = ProtocolName,
                    //ProgrammName = CreateOrder.ProgrammName,
                };

                fas.EP_Protocols.Add(ep);
                fas.SaveChanges();
                ProtocolID = fas.EP_Protocols.OrderByDescending(c => c.DateCreate).FirstOrDefault().ID;
                IsTOPBOT = (bool)ep.TOPBOT;
            }
        }

        public void AddInfo(string TOBBOT)
        {
            using (var fas = new FASEntities())
            {
                //var IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c => c.Manufacter != "Цех поверхностного монтажа").ToList();
                //addinfo(IdList);

                var IdList = fas.EP_TypeVerification.OrderBy(c => c.Manufacter).ThenBy(c => c.Num).Where(c => c.Manufacter != "Цех Сборки").ToList();
                addinfo(IdList, TOBBOT);

                void addinfo(List<EP_TypeVerification> list, string TOP)
                {
                    bool vis = true;

                    if (TOP == "BOT")
                        vis = fas.EP_Protocols.Where(c => c.ID == ProtocolID).Select(c => c.TOPBOT).FirstOrDefault();


                    foreach (var item in list)
                    {
                        EP_ProtocolsInfo INFO = new EP_ProtocolsInfo()
                        {
                            TypeVerifID = item.ID,
                            DateCreate = DateTime.UtcNow.AddHours(2),
                            ProtocolID = ProtocolID,
                            Signature = false,
                            TOPBOT = TOP,
                            Visible = vis,
                            Itter = (short)(CountProtocols + 1),
                        };

                        fas.EP_ProtocolsInfo.Add(INFO);
                    }

                    fas.SaveChanges();
                }
            }
        }

        public void GeneratePGName()
        {
            using (var fas = new FASEntities())
            {
                var listlots = fas.FAS_GS_LOTs.Select(c => new LotsInfo
                {
                    LOTID = c.LOTID,
                    LotCode = c.LOTCode,
                    ModelName = fas.FAS_Models.Where(b => b.ModelID == c.ModelID).Select(b => b.ModelName).FirstOrDefault(),
                    Spec = c.Specification,
                    FullLotCode = c.FULL_LOT_Code,

                }).ToList();

                listlots.AddRange(fas.Contract_LOT.Select(c => new LotsInfo
                {

                    LOTID = c.ID,
                    LotCode = (short)c.LOTCode,
                    ModelName = fas.FAS_Models.Where(b => b.ModelID == c.ModelID).Select(b => b.ModelName).FirstOrDefault(),
                    Spec = c.Specification,
                    FullLotCode = c.FullLOTCode,

                }));

                var name = listlots.Where(c => c.LOTID == LOTID).Select(c => ProtocolID + "_SP_" + c.LotCode + "_" + c.ModelName).FirstOrDefault().Replace(" ", "_");

                var lsitbtobbot = new List<string> { "TOP", "BOT" };

                foreach (var item in lsitbtobbot)
                {
                    bool visible = true;
                    if (item == " BOT")
                        visible = GetTopBot(TOPBOTName);

                     var ListPG = fas.EP_Machine.Select(c => new { Name = name + "_" + c.Name + "_" + item, IDMachine = c.ID, Type = item, IDProtocol = ProtocolID }).ToList();

                    List<EP_PGName> eP_PGName = new List<EP_PGName>();
                    foreach (var i in ListPG)
                    {
                        EP_PGName ep = new EP_PGName()
                        {
                            IDMachine = i.IDMachine,
                            IDProtocol = i.IDProtocol,
                            Name = i.Name,
                            Type = i.Type,
                            Visible = visible,
                        };

                        eP_PGName.Add(ep);
                    }

                    fas.EP_PGName.AddRange(eP_PGName); fas.SaveChanges();
                }

            }
        }

        bool GetTopBot(string name)
        {
            return name == "Одностороняя плата" ? false : true;
        }

        public int GetServiceID()
        {
            using (var fas = new FASEntities())
            {
                return (int)fas.FAS_Users.Where(c => c.UserID == UserID).FirstOrDefault().IDService;
            }

        }

        public void SetLOG(string TOPBOT)
        {
            using (FASEntities fas = new FASEntities())
            {

                EP_Log log = new EP_Log()
                {
                    IDProtocol = ProtocolID,
                    //UserID = (short)UserID,
                    //ServiceID = GetServiceID(),
                    Date = DateTime.UtcNow.AddHours(2),
                    IDStep = 6,
                    LOTID = LOTID,
                    TOPBOT = TOPBOT,                   
                };

                fas.EP_Log.Add(log);
                fas.SaveChanges();
            }
        }



    }
}