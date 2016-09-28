using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebServiceTest2
{
    public class Test1
    {
        [WebMethod]
        public int[] GetReportIdArray(DateTime dateBegin, DateTime dateEnd)
        {
            int[] array = new int[] { 357, 358, 360, 361 };
            return array;
        }

        [WebMethod]
        public FinReport GetReport(int reportID)
        {
            FinReport finReport = new FinReport()
            {
                ReportID = reportID,
                Date = new DateTime(2015, 03, 15),
                Info = "Some info"
            };

            return finReport;
        }

        public class FinReport
        {
            public int ReportID { get; set; }
            public DateTime Date { get; set; }
            public string Info { get; set; }
        }
    }
}