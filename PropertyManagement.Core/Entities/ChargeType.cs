using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class ChargeType : Entity
    {
        public virtual string Name { get; set; }

        public ChargeType()
        {
        }
    }
}
