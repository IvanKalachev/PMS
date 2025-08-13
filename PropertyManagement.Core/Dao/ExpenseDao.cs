using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class ExpenseDao : BaseDao<Expense, Int64>
    {
        public ExpenseDao(ISession session)
            : base(session)
        {
        }

        public IList<Expense> LoadAll()
        {
            return CurrentSession.Query<Expense>()
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public IList<Expense> LoadExtraExpenses()
        {
            return CurrentSession.Query<Expense>()
                      .Where(x => x.ExpenseType == 3)
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public IList<Expense> LoadByDetection(Int64 detectionId)
        {
            return CurrentSession.Query<Expense>()
                      .Where(x => x.Detection.Id == detectionId)
                      .OrderByDescending(x => x.Id)
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public IList<Expense> GetOverheadsForMonth(Int64 detectionId)
        {
            return CurrentSession.Query<Expense>()
                      .Where(x => x.Detection.Id == detectionId)
                      .Where(x => x.ExpenseType == 1)
                      .OrderByDescending(x => x.Id)
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public IList<Expense> GetIndividualsForMonth(Int64 detectionId)
        {
            return CurrentSession.Query<Expense>()
                      .Where(x => x.Detection.Id == detectionId)
                      .Where(x => x.ExpenseType == 2)
                      .OrderByDescending(x => x.Id)
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public IList<Expense> GetExtraExpensesForMonth(Int64 detectionId)
        {
            return CurrentSession.Query<Expense>()
                      .Where(x => x.Detection.Id == detectionId)
                      .Where(x => x.ExpenseType == 3)
                      .OrderByDescending(x => x.Id)
                      .Fetch(x => x.Detection)
                      .Fetch(x => x.Service)
                      .ToList();
        }

        public decimal GetTotalForDetection(Int64 detectionId)
        {
            var count = LoadAll().Count;
            if (count > 0)
            {
                try
                {
                    return CurrentSession.Query<Expense>()
                       .Where(x => x.Detection.Id == detectionId)
                       .Sum(x => x.Sum);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
           
        }

        public decimal GetAllExpensesSum()
        {
            return LoadAll().Sum(x => x.Sum);
        }

        public void UnDestributeExpenses(IList<Expense> expenses)
        {
            foreach (var expense in expenses)
            {
                expense.IsDistributed = false;
                SaveOrUpdate(expense);
            }
        }

        // връща всички приходи за месеца
        public IList<Expense> GetProfitsForDetection(Int64 detectionId)
        {
            var result = (from c in CurrentSession.Query<Expense>()
                          where c.Service.IsProfit == true
                          && c.Detection.Id == detectionId
                          select c).ToList();

            return result.ToList();
        }

        // връща всички разходи за месеца
        public IList<Expense> GetCostsForDetection(Int64 detectionId)
        {
            var result = (from c in CurrentSession.Query<Expense>()
                          where c.Service.IsCost == true
                          && c.ExpenseType != 3
                          && c.Detection.Id == detectionId
                          select c).ToList();

            return result.ToList();
        }
    }
}
