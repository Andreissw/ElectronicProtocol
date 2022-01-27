using ElectronicProtocolStartLine.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ElectronicProtocolStartLine.Class.Monitoring
{
   
    public abstract class  SMTProgramms
    {
        public SMDCOMPONETSEntities smd = new SMDCOMPONETSEntities();
        //public FASEntities fas = new FASEntities();
        public string Name { get; set; }

        public string LineName { get; set; }

        public string QueryAOI { get; } = $@"SELECT DISTINCT(pi.PG_NAME) FROM INSP_RESULT_SUMMARY_INFO irsi JOIN PG_INFO pi ON IRSI.PG_ITEM_ID = pi.PG_ITEM_ID WHERE irsi.SYS_MACHINE_NAME = 'Line' AND pi.PG_NAME = 'model' ";

        public bool Omron(string query)
        {
           
            var data = OracleCommand.LoadDataOmron(query);
            if (data == null)
                return false;

            foreach (DataRow item in data.Tables[0].Rows)
                if (Name == item[0].ToString().Substring(0,Name.Length))
                    return true;

            return false;
        }

        public abstract bool CheckProgramm();
    }

    public class StencilPrinter : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            
            return true;
        }
    }

    public class LazerMarker : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            int LineID = 0;

            switch (LineName)
            {
                case "NXT1":
                    LineID = 10;
                   break;
                case "NXT2":
                    LineID = 8;
                    break;
                case "NXT3":
                    LineID = 9;
                    break;
            }

            var result = smd.LazerBase.Where(c=>c.PCID == LineID & c.ProductName != "NR").OrderByDescending(c => c.LogDate).Take(1).Select(c => c.ProductName).FirstOrDefault();
            if (result.Substring(0,Name.Length) == Name)          
                return true;

            return false;
            
        }
    }

    public class PPMachine : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            var result = smd.NewFujiScrapSQliteLine1.Where(c=> c.Line == LineName).OrderByDescending(c => c.DateTime).Take(1).Select(c => c.Recipe).FirstOrDefault();
            if (result.Substring(0, Name.Length) == Name)
                return true;
           return  false;

        }
    }

    public class ReflowOven : SMTProgramms
    {

        public override bool CheckProgramm()
        {
            return true;
        }
    }

    public class SPI : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            //var query = QueryAOI.Replace("Line", "VP9000-MC1");
            //return Omron(query);
            return true;
        }
    }

    public class PreReflowAOI : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            if (LineName == "NXT3")
            {

                var query = QueryAOI.Replace("Line", "VT-S530-1087").Replace("model",Name);
                return Omron(query);
            }

            return true;
           
        }
    }

    public class PostReflowAOI : SMTProgramms
    {
        public override bool CheckProgramm()
        {
            string LineName = "";

            switch (this.LineName)
            {
                case "NXT1":
                    LineName = "VT-S730H-0026";
                    break;
                case "NXT2":
                    LineName = "VT-S730H-0628";
                    break;
                case "NXT3":
                    LineName = "VT-S730H-0022";
                    break;
            }

            var query = QueryAOI.Replace($"WHERE irsi.SYS_MACHINE_NAME = 'Line'", $"WHERE irsi.SYS_MACHINE_NAME = '{LineName}'").Replace("model",Name);
            return Omron(query);
        }
    }




}