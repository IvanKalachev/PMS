using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Unit : Entity
    {
        public virtual string Number { get; set; }

        public virtual string FamilyName { get; set; }

        public virtual int MembersCount { get; set; }

        public virtual decimal PercentIdealParts { get; set; }

        public virtual int? Floor { get; set; }

        public virtual bool IsDeleted { get; set; }

        private IList<Service> _noCharge;
        public virtual IList<Service> NoChargeServices
        {
            get
            {
                if (_noCharge == null)
                {
                    _noCharge = new List<Service>();
                }

                return _noCharge;
            }
            set
            {
                _noCharge = value;
            }
        }

        public Unit()
        {
        }

        public virtual decimal ChargedSum { get; set; }

        public virtual decimal Balance { get; set; }

        public virtual void AddToBalance(decimal sum)
        {
            this.Balance += sum;
        }

        public virtual void RemoveFromBalance(decimal sum)
        {
            if (this.Balance < 0)
            {
                this.Balance += sum;
            }
            else
            {
                this.Balance -= sum;
            }
        }

        public virtual string FamilyAndApNumber
        {
            get
            {
                return this.FamilyName + " ет. " + this.Floor + " ап. " + this.Number;
            }
        }

        public virtual bool Deactivated { get; set; }
    }
}
