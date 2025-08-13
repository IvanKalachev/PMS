using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class DistributeExpenseModel
    {
        public ExpenseModel Expense;

        public IList<Unit> Units;
    }
}