using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Helpers;
using System.Web.Mvc;
using PropertyManagement.Core;

namespace PropertyManagement.Web.Models
{
    public class ExpenseModel
    {
        public Int64 Id { get; set; }

        public Service Service { get; set; }

        public int ServiceId { get; set; }

        public Detection Detection { get; set; }

        public decimal Sum { get; set; }

        public bool IsDistributed { get; set; }

        public int  ExpenseType { get; set; }

        public Unit Unit { get; set; }

        public Currency Currency { get; set; }

        public Int64 getId()
        {
            return this.Id;
        }

        public string GetMonthYear()
        {
            return MonthsYears.GetMonth(Detection.Month) + " " + Detection.Year.ToString();
        }

        public string IndividualServiceName
        {
            get
            {
                return Service.Name + " - " + Unit.FamilyName + " ет " + Unit.Floor + " ап " + Unit.Number;
            }
        }
    }

    
}