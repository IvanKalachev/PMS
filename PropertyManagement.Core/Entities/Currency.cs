using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Currency : Entity
    {
        public virtual string Name { get; set; }
        public virtual string Symbol { get; set; }
        public virtual bool IsDefault { get; set; }
        public virtual decimal ExchangeRate { get; set;}

        public Currency()
        {

        }
    }
}
