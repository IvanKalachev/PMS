using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class IncomesModel
    {
        public Unit Unit { get; set; }

        public IList<UnitCharge> UnitCharges { get; set; }

        public decimal SumToPay { get; set; }

        public decimal PayedSum { get; set; }

        public decimal RealPayedSum { get; set; }

        public decimal Saldo
        {
            get
            {
                if ((SumToPay - PayedSum) < 0)
                    return 000;
                else
                    return SumToPay - PayedSum;
            }
        }

        public decimal PrevSaldo { get; set; }

        public Int64 DetectionId { get; set; }

        public decimal VneseniPari { get; set; }

        public decimal GrandTotal
        {
            get
            {
                return (SumToPay - PayedSum) + PrevSaldo;
            }
        }

        public decimal ResMoney { get; set; }
    }
}
