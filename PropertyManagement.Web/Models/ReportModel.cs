using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PropertyManagement.Web.Models
{
    public class ReportModel
    {
        public SelectList Detections { get; set; }

        public SelectList Units { get; set; }
    }
}