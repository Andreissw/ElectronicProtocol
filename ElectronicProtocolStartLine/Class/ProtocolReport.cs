using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class
{
    public class ProtocolReport
    {
        public ProtocolReport()
        {
            OTKProtocol = new Infos("ОТК Выборочный контроль");
            ListInfoFAS = new Infos("Цех Сборки");
            ListInfoSMT = new Infos("Цех поверхностного монтажа"); 
            Infos = new List<Infos>() { ListInfoSMT, ListInfoFAS,  };
        }
        public int ID { get; set; }
        public bool TOBOT { get; set; }
        public int LOTID { get; set; }

        public string ProtocolName { get; set; }

        public string Order { get; set; }
        public Infos OTKProtocol { get; set; }
        public Infos ListInfoFAS { get; set; }
        public Infos ListInfoSMT { get; set; }        
        public List<Infos> Infos { get; set; }

    }

    public class Infos
    {
        public Infos(string Name)
        {
            this.Name = Name;
            InfosDetails = new List<InfoProtocol>();
            ListCounts = new List<Counts>();
        }

        public string Name { get;}

        public string InfoCount { get; set; }

        public List<Counts> ListCounts { get; set; }


        public List<InfoProtocol> InfosDetails { get; set; }
   
    }

    public class Counts
    {
        public string TOBBOT { get; set; }
        public int CountOK { get; set; }

        public int CountNOK { get; set; }

        public int CountNA { get; set; }
    }
     

    public class InfoProtocol
    {
        public string BOTTOB { get; set; }

        public string TypeVerif { get; set; }

        public DateTime? Date { get; set; }

        public string Result { get; set; }

        public string Service { get; set; }

        public string UserName { get; set; }
        public bool Signature { get; set; }

        public string Document { get; set; }
        public string Manuf { get; set; }

        public bool TypeTobBot { get; set; }

        public byte? Line { get; set; }
        
    }

    public class OTKInfo
    {
        public string TypeVerif { get; set; }
        public DateTime Date { get; set; }
        public string Result { get; set; }
        public string Service { get; set; }
        public bool Signature { get; set; }

        public string Description { get; set; }

        

    }
}