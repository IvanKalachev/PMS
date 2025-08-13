using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Expense : Entity
    {
        public virtual Service Service { get; set; }

        public virtual Detection Detection { get; set; }

        public virtual decimal Sum { get; set; }

        public virtual bool IsDistributed { get; set; }

        public virtual int ExpenseType { get; set; }

        public virtual Unit Unit { get; set; }

        public Expense()
        {
        }
    }
}
