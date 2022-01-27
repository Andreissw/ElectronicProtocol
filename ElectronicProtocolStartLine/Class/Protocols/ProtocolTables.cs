using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Protocols
{
    public class ProtocolTables
    {
        public int LOTID { get; set; }
        public int ID { get; set; }
        public string Type { get; set; }
        public string NameProtocol { get; set; }
        public string NameOrder { get; set; }
        public string Spec { get; set; }
        public string Manuf { get; set; }
        public List<Programms> Programms { get; set; }
        public string TOPBOT { get; set; }
        public DateTime DateCreate { get; set; }
        public TOBOT InfoTOPBOT { get; set; }
        public bool IsActiveTOP { get; set; }
        public bool IsActiveBOT { get; set; }
        public byte? Line { get; set; }     

    }

    public class TOBOT
    { 
        public string TOPBOT { get; set; }

        public int CountAll { get; set; }

        public int CountTrue { get; set; }

        public int CountProtocolTrue { get; set; }

        public int CountProtocolAll { get; set; }

        public bool IsActive { get; set; }


    }

    public class Programms
    { 
        public string ProtocolName { get; set; }

        public string NameOrder { get; set; }

        public string TOPBOT { get; set; }
        public string Machine { get; set; }
        public string PGName { get; set; }
    }
}