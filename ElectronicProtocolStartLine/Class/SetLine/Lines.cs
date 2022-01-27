using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Class.SetLine
{
    public class Lines
    {
        public Lines()
        {
            Linelist = new List<SelectListItem>();
            TOPList = new List<SelectListItem>();
        }

        public string Manuf { get; set; }
        public int LOTID { get; set; }
        public List<SelectListItem> Linelist { get; set; }
        public string Line { get; set; }

        public List<SelectListItem> TOPList { get; set; }
        public string TOPBOT { get; set; }

        public List<SelectListItem> ProtocolsList { get; set; }
        public string Protocol { get; set; }

        public int ProttocolID { get; set;}

    }
}