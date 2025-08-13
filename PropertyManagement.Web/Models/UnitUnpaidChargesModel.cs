using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class UnitUnpaidChargesModel
    {
        public Detection Detection { get; set; }

        public IList<UnitCharge> Charges { get; set; }

        public decimal ChargesTotal
        {
            get
            {
                return Charges.Sum(x => x.SumToPay);
            }
        }

        public decimal PaidTotal
        {
            get
            {
                return Charges.Sum(x => x.PayedSum);
            }
        }
    }
}