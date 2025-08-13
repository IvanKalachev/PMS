using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class Detection : Entity
    {
        public virtual int Year { get; set; }

        public virtual int Month { get; set; }

        public Detection()
        {
        }

        public virtual DateTime GetDetectionDateTime()
        {
            var datet = new DateTime(Year, Month, 1);
            return datet;
        }
    }
}
