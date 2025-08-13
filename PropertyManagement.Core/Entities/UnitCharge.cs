using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class UnitCharge : Entity
    {
        public virtual Unit Unit { get; set; }

        public virtual Expense Expense { get; set; }

        public virtual decimal SumToPay { get; set; }

        public virtual decimal PayedSum { get; set; }

        private IList<IncomePayment> _incomesPayments;
        public virtual IList<IncomePayment> IncomePayments
        {
            get
            {
                if (_incomesPayments == null)
                {
                    _incomesPayments = new List<IncomePayment>();
                }

                return _incomesPayments;
            }
            set
            {
                _incomesPayments = value;
            }
            

        }

        public UnitCharge()
        {
        }
    }
}
