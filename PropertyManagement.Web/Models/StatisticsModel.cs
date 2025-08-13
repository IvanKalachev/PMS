using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class StatisticsModel
    {
        public IList<Unit> UnitsWithCharges { get; set; }

        public IList<Unit> UnitsWithPositiveSaldo { get; set; }

        public decimal Saldo { get; set; }

        public decimal SaldoTo2014 { get; set; }

        public decimal TotalSaldo { get; set; }
      
    }
}