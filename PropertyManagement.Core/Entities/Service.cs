using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Service : Entity
    {
        public virtual string Name { get; set; }

        public virtual ChargeType ChargeType { get; set; }

        // приход
        public virtual bool IsProfit { get; set; }

        // разход
        public virtual bool IsCost { get; set; }

        public Service()
        {
        }
    }
}
