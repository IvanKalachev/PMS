using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace PropertyManagement.Web.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToBgDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        public static string ToBgDate(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}