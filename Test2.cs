using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;

namespace WebServiceTest2
{
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [WebService(Description = "Фин. отчеты", Namespace = XmlNS, Name = "Banana")]
    public class Test2 : WebService
    {
        public const string XmlNS = "http://asmx.habrahabr.ru/";

        [WebMethod(Description = "Получение списка ID отчетов по периоду")]
        public GetReportIdArrayResult GetReportIdArray(GetReportIdArrayArg arg)
        {
            return new GetReportIdArrayResult()
            {
                ReportIdArray = new int[] { 357, 358, 360, 361 }
            };
        }

        [WebMethod(Description = "Получение отчета по ID")]
        public GetReportResult GetReport(GetReportArg arg)
        {
            return new GetReportResult()
            {
                Report = new FinReport
                {
                    ReportID = arg.ReportID,
                    Date = new DateTime(2015, 03, 15),
                    Info = getReportInfo(arg.ReportID)
                }
            };
        }

        private string getReportInfo(int reportID)
        {
            return "ReportID = " + reportID;
        }
    }

    public class FinReport
    {
        public int ReportID { get; set; }
        public DateTime Date { get; set; }
        public string Info { get; set; }
    }

    public class GetReportIdArrayArg
    {
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
    }

    public class GetReportIdArrayResult
    {
        public int[] ReportIdArray { get; set; }
    }

    public class GetReportArg
    {
        public int ReportID { get; set; }
    }

    public class GetReportResult
    {
        public FinReport Report { get; set; }
    }
}