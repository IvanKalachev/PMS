using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Helpers
{
    public static class MonthsYears
    {
        public static List<ComboItem> GetMonthsList()
        {
            List<ComboItem> list = new List<ComboItem>();
            list.Add(new ComboItem("Януари", 1));
            list.Add(new ComboItem("Февруари", 2));
            list.Add(new ComboItem("Март", 3));
            list.Add(new ComboItem("Април", 4));
            list.Add(new ComboItem("Май", 5));
            list.Add(new ComboItem("Юни", 6));
            list.Add(new ComboItem("Юли", 7));
            list.Add(new ComboItem("Август", 8));
            list.Add(new ComboItem("Септември", 9));
            list.Add(new ComboItem("Октомври", 10));
            list.Add(new ComboItem("Ноември", 11));
            list.Add(new ComboItem("Декември", 12));

            return list;
        }

        public static List<ComboItem> GetYearsList()
        {
            var currentYear = DateTime.Now.Year;
            List<ComboItem> list = new List<ComboItem>();
            for (int i = (currentYear - 5); i <= currentYear + 1; i++)
            {
                list.Add(new ComboItem(i.ToString(), i));
            }

            return list;
        }

        public static string GetMonth(int i)
        {
            switch (i)
            {
                case 1: return "Януари";
                case 2: return "Февруари";
                case 3: return "Март";
                case 4: return "Април";
                case 5: return "Май";
                case 6: return "Юни";
                case 7: return "Юли";
                case 8: return "Август";
                case 9: return "Септември";
                case 10: return "Октомври";
                case 11: return "Ноември";
                case 12: return "Декември";
            }

            return "";
        }

        public static IList<ComboItem> GetComboDetections(IList<Detection> detections)
        {
            IList<ComboItem> result = new List<ComboItem>();
            foreach (var det in detections)
            {
                result.Add(new ComboItem(GetMonth(det.Month) + " " + det.Year, det.Id));
            }

            return result;
        }
    }
}