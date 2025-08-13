using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PropertyManagement.Web.Models
{
    public class ExpensesViewModel
    {
        public IList<ExpenseModel> Expenses { get; set; }

        public ExpenseModel Expense { get; set; }

        // флаг, който показва дали за дадения месец има разпределени разходи
        public bool CanResetExpenseDistribution { get; set; }
    }
}