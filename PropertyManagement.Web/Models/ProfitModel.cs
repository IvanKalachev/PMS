using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class ProfitModel
    {
        public Service Service { get; set; }

        public decimal Sum { get; set; }
    }
}