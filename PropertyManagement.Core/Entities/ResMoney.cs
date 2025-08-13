using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class ResMoney : Entity
    {
        public ResMoney()
        {
        }

        public virtual Detection Detection { get; set; }

        public virtual Unit Unit { get; set; }

        public virtual decimal PayedSum { get; set; }

        public virtual DateTime InsertDate { get; set; }
    }
}
