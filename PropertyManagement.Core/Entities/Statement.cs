using System;

namespace PropertyManagement.Core.Entities
{
    public class Statement : Entity
    {
        public virtual Detection Detection { get; set; }

        public virtual string Data { get; set; }

        public virtual DateTime InsertDate { get; set; }

        public Statement()
        {

        }
    }
}
