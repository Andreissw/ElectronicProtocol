using ElectronicProtocolStartLine.Class.Protocols.Interface;
using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicProtocolStartLine.Class.Protocols.ClassIdentify.Classes
{
    public class SMT : IGetmanufProtocol
    {
        FASEntities Fas = new FASEntities();
        public int LOTID { get; set; }
        public Redirect Redirect { get; set; }

        public SMT()
        {
            Redirect = new Redirect() { ActionName = "Index", ControllerName = "SetLine", Routes = new { LOTID = LOTID } };
        }     
        
    }
}