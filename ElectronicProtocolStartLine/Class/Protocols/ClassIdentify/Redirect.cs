using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Protocols.ClassIdentify
{
    public class Redirect
    {
        public Redirect()
        {
            ListProtocols = new List<ProtocolTables>();
        }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public object Routes { get; set; }

        public List<ProtocolTables> ListProtocols { get; set; }
    }
}