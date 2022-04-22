using ElectronicProtocolStartLine.Class.Protocols.Interface;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Protocols.ClassIdentify.Classes
{
    public class General : IGetmanufProtocol
    {
        FASEntities Fas = new FASEntities();
        public int LOTID { get; set; }        
        public Redirect Redirect { get; set; }

        public General()
        {
            Redirect = new Redirect() { ActionName = "WorkForm", ControllerName = "Home" };
        }       
    }
}