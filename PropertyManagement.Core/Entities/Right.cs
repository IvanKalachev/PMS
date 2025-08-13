using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Right : Entity
    {
        public Right()
        {
        }

        public virtual string Key { get; set; }

        public virtual string Description { get; set; }
    }
}
