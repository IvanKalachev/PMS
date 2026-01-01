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

            if (selectedValue != 0)
            {
                result = new SelectList(items, "Id", "Name", selectedValue);
            }
            else
            {
                result = new SelectList(items, "Id", "Name");
            }

            return result;
        }

        public static decimal ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency, int precision = 2)
        {
            if (fromCurrency.Id == toCurrency.Id)
                return amount;

            var fromRate = fromCurrency.ExchangeRate;
            var toRate = toCurrency.ExchangeRate;

            decimal amountInBase = amount / fromRate;
            decimal convertedAmount = amountInBase * toRate;

            return Math.Round(convertedAmount, precision, MidpointRounding.AwayFromZero);
        }

        public static string ConvertCurrency(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency.Id == toCurrency.Id)
                return amount.ToString("0.00");

            var fromRate = fromCurrency.ExchangeRate;
            var toRate = toCurrency.ExchangeRate;

            decimal amountInBase = amount / fromRate;
            decimal convertedAmount = amountInBase * toRate;

            var result = Math.Round(convertedAmount, 2, MidpointRounding.AwayFromZero);

            return result.ToString("0.00") + " " + toCurrency.Symbol;
        }
    }
}