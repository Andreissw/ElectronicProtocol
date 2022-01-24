using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Orders
{
    public class HomePage
    {
        public HomePage()
        {
            orders = new List<Orders>();
            ProtocolsStart = new List<Protocols.ProtocolTables>();
        }
          
        public List<Protocols.ProtocolTables> ProtocolsStart { get; set; }
        public List<Orders> orders { get; set; }
    }
    public class Orders
    {
        public int ID { get; set; }

        public string TypeClient { get; set; }

        //public bool IsActive { get; set; }

        //public string ProtocolName { get; set; }

        public string NameClient { get; set; }

        public string NameOrder { get; set; }

        public string Spec { get; set; }          

        public DateTime DateCreate { get; set; }

        public bool IsActive { get; set; }       
        
    }
   
}