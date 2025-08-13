using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class ReportUnitModel
    {
        public Detection Detection { get; set; }

        public Unit Unit { get; set; }

        public decimal Sum { get; set; }
    }
}