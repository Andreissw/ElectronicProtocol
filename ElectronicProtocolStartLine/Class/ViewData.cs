using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class
{
    public class ViewData
    {
        FASEntities fas = new FASEntities();
        public string ClientType { get; set; }
        public string NameClient { get; set; }
        public string NameOrder { get; set; }
        public string NameSpec { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreate { get; set; }

        public void GetView(int LOTID)
        {
            //ViewData viewData = new ViewData();

            var data = fas.FAS_GS_LOTs.Where(c => c.LOTID == LOTID).Select(c => new 

            {
                NameSpec = c.Specification,
                NameOrder = c.FULL_LOT_Code,
                ClientType = "ВЛВ",
                NameClient = "ВЛВ",
                DateCrate = c.CreateDate,
                isActive = c.IsActive,


            }).FirstOrDefault();

            if (data == null)
            {
                data = fas.Contract_LOT.Where(c => c.ID == LOTID).Select(c => new 
                {
                    NameSpec = c.Specification,
                    NameOrder = c.FullLOTCode,
                    ClientType = "Контрактное",
                    NameClient = fas.CT_Сustomers.Where(b => b.ID == c.СustomersID).FirstOrDefault().СustomerName,
                    DateCrate = c.CreateDate,
                    isActive = c.IsActive,

                }).FirstOrDefault();
            }

            this.NameClient = data.NameClient;
            this.NameOrder = data.NameOrder;
            this.NameSpec = data.NameSpec;
            this.ClientType = data.ClientType;
            this.DateCreate = data.DateCrate;
            this.IsActive = data.isActive;
            
        }

    }
}