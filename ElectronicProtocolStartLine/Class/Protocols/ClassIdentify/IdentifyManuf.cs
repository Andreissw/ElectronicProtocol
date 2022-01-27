using ElectronicProtocolStartLine.Class.Protocols.ClassIdentify.Classes;
using ElectronicProtocolStartLine.Class.Protocols.Interface;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Class.Protocols.ClassIdentify
{
    public class IdentifyManuf
    {       
        public int LOTID { get; set; }
        public int ProtocolID { get; set; }
        
        string ServiceName { get; set; }

        public IGetmanufProtocol GetProtocol { get; set; }

        FASEntities Fas = new FASEntities();
        public IdentifyManuf(string Manuf, int LOTID, string ServiceName)
        {           
            this.LOTID = LOTID;
            this.ServiceName = ServiceName;
            GetProtocol = getmanufProtocol(Manuf);
            var protocolID = Fas.EP_Protocols.Where(c => c.LOTID == LOTID).Select(c => c.ID).FirstOrDefault();
            GetProtocol.Redirect.Routes = new { ID = protocolID ,LOTID = LOTID};
        }       

        IGetmanufProtocol getmanufProtocol(string manuf)
        {
            switch (manuf)
            {
                case "Цех поверхностного монтажа":
                    return new SMT();
                case "Цех Сборки":
                    return new FAS();
                case "Входной контроль":
                    return new EnterControl();
                default:
                    return new General();
            }

        }

        public List<ProtocolTables> GetListProtocols()
        {
            var view = new ViewData();
            view.GetView(LOTID);

            return Fas.EP_ProtocolsInfo.Where(c => c.EP_Protocols.LOTID == LOTID & c.EP_TypeVerification.Manufacter != "Входной контроль" & c.Visible == true)
                .GroupBy(c => new { ProtocolID = c.ProtocolID, line = c.line, TOPBOT = c.TOPBOT, Manuf = c.EP_TypeVerification.Manufacter }).Select(c => new ProtocolTables
                {
                    LOTID = c.Select(b => b.EP_Protocols).FirstOrDefault().LOTID,
                    ID = c.Key.ProtocolID,
                    NameOrder = view.NameOrder,
                    Type = view.ClientType,
                    NameProtocol = c.Select(b => b.EP_Protocols).FirstOrDefault().NameProtocol,
                    DateCreate = (DateTime)c.Select(b => b.DateCreate).FirstOrDefault(),
                    IsActiveBOT = c.Select(b => b.EP_Protocols).FirstOrDefault().IsActiveBOT,
                    IsActiveTOP = c.Select(b => b.EP_Protocols).FirstOrDefault().IsActiveTOP,
                    Line = c.Key.line,
                    Manuf = c.Key.Manuf,
                    Programms = Fas.EP_PGName.Where(b => b.IDProtocol == c.Key.ProtocolID).Select(b => new Programms
                    {
                        Machine = b.EP_Machine.Name,
                        PGName = b.Name,
                    }).ToList(),
                    TOPBOT = c.Key.TOPBOT == "TOP" ? "Вверх(TOP)" : "Низ(BOT)",

                    InfoTOPBOT = c.Select(b => new TOBOT()
                    {

                        TOPBOT = c.Key.TOPBOT,
                        CountAll = c.Where(x => x.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountTrue = c.Where(x => x.Signature == true & x.EP_TypeVerification.EP_Service.Name == ServiceName).Count(),
                        CountProtocolAll = c.Count(),
                        CountProtocolTrue = c.Where(x => x.Signature == true).Count(),

                    }).FirstOrDefault(),

                }).ToList();
        }
    }
}