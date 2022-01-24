using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Protocols
{
    public class LotsInfo
    {
        public int LOTID { get; set; }
        public short LotCode { get; set; }

        public string ModelName { get; set; }

        public string Spec { get; set; }

        public string FullLotCode { get; set; }
    }
}