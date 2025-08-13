using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class IncomePayment : Entity
    {
        public virtual Detection Detection { get; set; }

        public virtual Unit Unit { get; set; }

        public virtual decimal PayedSum { get; set; }

        public virtual DateTime PayDate { get; set; }

        public IncomePayment()
        {
        }

        protected IList<UnitCharge> _payedCharges;
        public virtual IList<UnitCharge> PayedCharges
        {
            get
            {
                if (_payedCharges == null)
                {
                    _payedCharges = new List<UnitCharge>();
                }

                return _payedCharges;
            }
            set
            {
                _payedCharges = value;
            }
        }
    }
}
