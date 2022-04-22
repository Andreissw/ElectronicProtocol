using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Monitoring
{
    public class PrtMon
    {
        public int ID { get; set; }
        public int LOTID { get; set; }
        public bool IsStartStatusTOP { get; set; }
        public bool IsStartStatusBOT { get; set; }
        public bool IsActiveTOP { get; set; }
        public bool IsActiveBOT { get; set; }

        public string Manufacter { get; set; }
        public string TOPBOT { get; set; }
        public string NameProtocol { get; set; }

        public short? Iter { get; set; }

        public byte? Line { get;set; }

    }
}