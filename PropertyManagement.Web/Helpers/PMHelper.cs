using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Entities;
using System.ComponentModel;
using System.Reflection;
using System.Web.Mvc;

namespace PropertyManagement.Web.Helpers
{
    public static class PMHelper
    {
        public static TConvert ConvertTo<TConvert>(this object entity) where TConvert : new()
        {
            var convertProperties = TypeDescriptor.GetProperties(typeof(TConvert)).Cast<PropertyDescriptor>();
            var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();

            var convert = new TConvert();

            foreach (var entityProperty in entityProperties)
            {
                var property = entityProperty;
                var convertProperty = convertProperties.FirstOrDefault(prop => prop.Name == property.Name);
                if (convertProperty != null)
                {
                    convertProperty.SetValue(convert, entityProperty.GetValue(entity));
                }
            }

            return convert;
        }

        public static SelectList ConvertEntityListToSelectList<T>(IList<T> entities, Int64 selectedValue)
        {
            List<ComboItem> items = new List<ComboItem>();
            foreach (var entity in entities)
            {
                var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();
                var name = (from c in entityProperties
                            where c.Name == "Name"
                            select c).FirstOrDefault();

                var Id = (entity as Entity).Id;

                items.Add(new ComboItem(name.GetValue(entity).ToString(), Int64.Parse(Id.ToString())));
            }

            SelectList result = null;

            if(selectedValue != 0)
            {
                result = new SelectList(items, "Id", "Name", selectedValue);
            }
            else
            {
                result = new SelectList(items, "Id", "Name");
            }

            return result;
        }

    }
}