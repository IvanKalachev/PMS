using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyManagement.Web.Helpers
{
    public class ComboItem
    {
        public string Name { get; set; }

        public Int64 Id { get; set; }

        public ComboItem(string name, Int64 id)
        {
            Name = name;
            Id = id;
        }
    }
}