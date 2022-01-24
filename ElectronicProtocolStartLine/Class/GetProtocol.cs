using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine
{
    public class GetProtocol       
    {

        public GetProtocol()
        {

            protocolSMT = new Protocol("Цех поверхностного монтажа");
            protocolFAS = new Protocol("Цех Сборки");
            ProtocolOTK = new Protocol("ОТК Выборочный контроль");
            ListProtocol = new List<Protocol>() { protocolSMT , protocolFAS , ProtocolOTK };
        
        }

        public List<Protocol> ListProtocol { get; set; }

        public int LOTID { get; set; }
        public int ID { get; set; } 
        public string NameOrder { get; set; }
        public string NameProtocol { get; set; }
        public string TOPBOT { get; set; }
        public string SelectResult { get; set; }
        public Protocol protocolSMT { get; set; }
        public Protocol protocolFAS { get; set; }
        public Protocol ProtocolOTK { get; set; }
        
    }

    public class Protocol
    {
        public Protocol(string Name)
        {
            this.Name = Name;
            Details = new List<DetailsFrom>();
            Views = new List<DetailsFrom>();
        }

        public string Name { get; set; }
        public List<DetailsFrom> Details { get; set; }

        public List<DetailsFrom> Views { get; set; }
    }

    public class DetailsFrom
    {
        public int ID { get; set; }
        public string BOTTOB { get; set; }

        public string TypeVerification { get; set; }

        public DateTime DateCheck { get; set; }

        public string Result { get; set; }

        public string Service { get; set; }

        public string UserName { get; set; }

        public string DocCheck { get; set; }

        public string Manuf { get; set; }

        public string Description { get; set; }

        public string RFID { get; set; }

        public string LastResult { get; set; }

        public string LastUserResult { get; set; }
       

    }

    

   
}