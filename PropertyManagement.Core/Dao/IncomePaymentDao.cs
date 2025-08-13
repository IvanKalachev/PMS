using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class IncomePaymentDao : BaseDao<IncomePayment, Int64>
    {
        public IncomePaymentDao(ISession session)
            : base(session)
        {
        }

        public IList<IncomePayment> GetIncomePaymentByUnitAndDetection(Int64 unitId, Int64 detectionId)
        {
            return CurrentSession.Query<IncomePayment>()
                    .Where(x => x.Detection.Id == detectionId)
                    .Where(x => x.Unit.Id == unitId)
                    .ToList();
        }

        public IList<IncomePayment> GetIncomePaymentsByDetection(Int64 detectionId)
        {
            return CurrentSession.Query<IncomePayment>()
                    .Where(x => x.Detection.Id == detectionId)
                    .FetchMany(x => x.PayedCharges)
                    .ToList();
        }

        public decimal GetTotalPaied()
        {
            return LoadAll().Sum(x => x.PayedSum);
        }

        public decimal GetTotalPayedForDetection(Int64 detectionId)
        {
            var totalPayedForDetection = CurrentSession.Query<IncomePayment>()
                    .Where(x => x.Detection.Id == detectionId)
                    .ToList();

            return totalPayedForDetection.Sum(x => x.PayedSum);
        }

        public IList<UnitCharge> GetPayedChargesForPreviousDetections(Detection detection)
        {
            var incomes = CurrentSession.Query<IncomePayment>()
                            .Where(x => x.Detection.Id == detection.Id)
                            .FetchMany(x => x.PayedCharges)
                            .ToList();

            IList<UnitCharge> result = new List<UnitCharge>();

            foreach (var income in incomes)
            {
                foreach (var item in income.PayedCharges)
                {
                    if (!result.Contains(item) && item.Expense.Detection.GetDetectionDateTime() < detection.GetDetectionDateTime())
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        // Връща всички разплатени задължения за опрделен месец
        public IList<UnitCharge> GetPayedChargesByDetection(Int64 detectionId)
        {
            IList<UnitCharge> payedCharges = new List<UnitCharge>();
            var payments = GetIncomePaymentsByDetection(detectionId);

            foreach (var payment in payments)
            {
                foreach (var payedCharge in payment.PayedCharges)
                {
                    if (!payedCharges.Contains(payedCharge))
                    {
                        payedCharges.Add(payedCharge);
                    }
                }
            }

            return payedCharges;
        }

        public void DeletePayments(IList<IncomePayment> payments)
        {
            foreach (var item in payments)
            {
                Delete(item);
            }
        }
    }
}
