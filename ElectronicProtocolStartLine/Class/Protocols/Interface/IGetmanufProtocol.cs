using ElectronicProtocolStartLine.Class.Protocols.ClassIdentify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Class.Protocols.Interface
{
    public interface IGetmanufProtocol
    {
        int LOTID { get; set; }       
        Redirect Redirect { get; set; }  

    }
}
