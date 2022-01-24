using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Monitoring
{
    public class Fuji
    {
        SMDCOMPONETSEntities smd = new SMDCOMPONETSEntities();
        FASEntities fas = new FASEntities();
        QAEntities1 qa = new QAEntities1();
        public string Line { get; set; }
        public string ProgrammName { get; set; }
        public int MaxModule { get; set; }    
        public bool IsDataMonitoring()
        {
            return fas.EP_Monitoring.Where(b => b.Line == Line).Select(b => b.id == b.id).FirstOrDefault();
        }

        public string CheckDataMonitoring()
        {
            //return false;
            //Получаю максмальный модуль по линии в таблице EP_Monitoring
            var RModules = fas.EP_Monitoring.Where(c => c.Line == Line).Select(c => c.MaxModules).FirstOrDefault(); 

            //Получаю имя последней программе по линии и максимальному модулю
            var NewPG = qa.FUJI.OrderByDescending(c => c.DateTime).Where(c => c.Line == Line & c.Modules == RModules).FirstOrDefault().PGName;

            //Если программы не соовпадают, значит был переход 
            if (GetMonitorProgramm() != NewPG)       
                return NewPG;
          

            return "";
        }

        public string GetMonitorProgramm()
        {
            return fas.EP_Monitoring.Where(c => c.Line == Line).FirstOrDefault().ProgrammName;
        }

        public void UpdateDataMon()
        {
            var monitorng = fas.EP_Monitoring.Where(c => c.Line == Line).FirstOrDefault();
            //monitorng.MaxModules = MaxModule;
            monitorng.ProgrammName = ProgrammName;
            fas.SaveChanges();
        }

        public void addDataMon()
        {
            EP_Monitoring eP_Monitoring = new EP_Monitoring()
            {
                Line = Line,
                //MaxModules = MaxModule,
                ProgrammName = ProgrammName,
            };
            fas.EP_Monitoring.Add(eP_Monitoring);
            fas.SaveChanges();

        }

    }
}