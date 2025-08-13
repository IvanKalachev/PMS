using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class UnitChargeDao : BaseDao<UnitCharge, Int64>
    {
        public UnitChargeDao(ISession session)
            : base(session)
        {
        }

        // връща всикчи задължения за дадено начисление
        public IList<UnitCharge> GetAllByExpense(Int64 expenseId)
        {
            return CurrentSession.Query<UnitCharge>()
                    .Where(x => x.Expense.Id == expenseId)
                    .Fetch(x => x.Unit)
                    .Fetch(x => x.Expense)
                    .ThenFetch(x => x.Service)
                    .Fetch(x => x.Expense)
                    .ThenFetch(x => x.Detection)
                    .ToList();     
        }

        // взима всички задължения за даден месец
        public IList<UnitCharge> GetAllByDetection(Int64 detectionId)
        {
            return CurrentSession.Query<UnitCharge>()
                    .Where(x => x.Expense.Detection.Id == detectionId)
                    .Fetch(x => x.Unit)
                    .Fetch(x => x.Expense)
                    .ToList();
        }

        // връща всички неплатени задължения за опрделен апартамент и даден месец
        public IList<UnitCharge> GetUnpaiedForUnitByDetection(Int64 unitId, Int64 detectionId)
        {
            return CurrentSession.Query<UnitCharge>()
                   .Where(x => x.Unit.Id == unitId)
                   .Where(x => x.Expense.Detection.Id == detectionId)
                   .Where(x => x.PayedSum < x.SumToPay)
                   .ToList();
        }

        // връща всички неплатени задължения за конкретен апартамент
        public IList<UnitCharge> GetAllUnpaiedForUnit(Int64 unitId)
        {
            var result =  CurrentSession.Query<UnitCharge>()
                            .Where(x => x.Unit.Id == unitId)
                            .Where(x => x.PayedSum < x.SumToPay)
                            .Fetch(x => x.Expense)
                            .ThenFetch(x => x.Detection)
                            .Fetch(x => x.Expense)
                            .ThenFetch(x => x.Service)
                            .ToList();

            return result.OrderBy(x => x.Expense.Detection.Year)
                         .ThenBy(x => x.Expense.Detection.Month)
                         .ToList();
        }

        // връща всички задължения за даден апартамент и даден месец
        public IList<UnitCharge> GetAllByUnitAndDetection(Int64 unitId, Int64 detectionId)
        {
            return CurrentSession.Query<UnitCharge>()
                   .Where(x => x.Unit.Id == unitId)
                   .Where(x => x.Expense.Detection.Id == detectionId)
                   .OrderBy(x => x.Expense.Service.Name)
                   .Fetch(x => x.Expense)
                   .Fetch(x => x.Unit)
                   .ToList();
        }

        // връща неплатената сума за всички предишни месеци
        public decimal GetPreviousSaldo(Int64 unitId, Detection detection)
        {
            return (from c in GetAllUnpaiedForUnit(unitId)
                    where c.Expense.Detection.GetDetectionDateTime() < detection.GetDetectionDateTime()
                    select c).Sum(x => x.SumToPay - x.PayedSum);
        }

        // разплаща задължение
        public void PayExpenses(decimal paiedSum, Int64 unitId, Detection detection)
        {
            UnitDao unitDao = new UnitDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            var unit = unitDao.LoadById(unitId);

            if (unit.Balance < 0)
            {
                unit.Balance -= paiedSum;
            }
            else
            {
                unit.RemoveFromBalance(paiedSum);
            }

            var unpaidCharges = GetAllUnpaiedForUnit(unit.Id);
            decimal totalPaiedSum = paiedSum;

            if (paiedSum > 0)
            {
                IList<UnitCharge> payedCharges = new List<UnitCharge>();

                foreach (var unpaiedCharge in unpaidCharges)
                {
                    if (paiedSum > 0)
                    {
                        if (unpaiedCharge.PayedSum == 0)
                        {
                            if (paiedSum >= unpaiedCharge.SumToPay)
                            {
                                paiedSum = paiedSum - unpaiedCharge.SumToPay;
                                unpaiedCharge.PayedSum = unpaiedCharge.SumToPay;
                            }
                            else
                            {
                                unpaiedCharge.PayedSum += paiedSum;
                                paiedSum = 0;
                            }
                        }
                        else
                        {
                            var sumToPay = (unpaiedCharge.SumToPay - unpaiedCharge.PayedSum);
                            if (paiedSum >= sumToPay)
                            {
                                paiedSum = paiedSum - sumToPay;
                                unpaiedCharge.PayedSum = unpaiedCharge.SumToPay;
                            }
                            else
                            {
                                unpaiedCharge.PayedSum += paiedSum;
                                paiedSum = 0;
                            }
                        }

                        payedCharges.Add(unpaiedCharge);
                    }
                    else
                    {
                        break;
                    }

                    unitChargeDao.SaveOrUpdate(unpaiedCharge);
                }

                IncomePayment payment = new IncomePayment
                {
                    Unit = unit,
                    Detection = detection,
                    PayedSum = totalPaiedSum,
                    PayDate = DateTime.Now,
                    PayedCharges = payedCharges
                };

                incomePaymentDao.SaveOrUpdate(payment);
                unitDao.SaveOrUpdate(unit);
            }
        }

        public void UnpayExpence(decimal sumToRemove, Int64 unitId, Detection detection)
        {
            IncomePaymentDao incomePaymnetDao = new IncomePaymentDao(CurrentSession);
            UnitDao unitDao = new UnitDao(CurrentSession);
            var unit = unitDao.LoadById(unitId);
            var payments = incomePaymnetDao.GetIncomePaymentByUnitAndDetection(unit.Id, detection.Id);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            IList<UnitCharge> payedCharges = new List<UnitCharge>();

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

            var sumToDistribute = (-1) * sumToRemove;
            var payedChargesOrdered = payedCharges.OrderByDescending(x => x.Expense.Detection.Year)
                                                    .ThenByDescending(x => x.Expense.Detection.Month)
                                                    .ToList();

            foreach (var item in payedChargesOrdered)
            {
                if (sumToDistribute > 0)
                {
                    if (sumToDistribute > item.PayedSum)
                    {
                        sumToDistribute -= item.PayedSum;
                        item.PayedSum = 0;
                    }
                    else
                    {
                        item.PayedSum -= sumToDistribute;
                        sumToDistribute = 0;
                    }

                    unitChargeDao.SaveOrUpdate(item);
                }
                else
                {
                    break;
                }
            }

            unit.Balance += (sumToRemove * (-1));
            unitDao.SaveOrUpdate(unit);

            IncomePayment creditPayment= new IncomePayment
            {
                Unit = unit,
                Detection = detection,
                PayedSum = sumToRemove,
                PayDate = DateTime.Now,
                PayedCharges = payedCharges
            };

            incomePaymnetDao.SaveOrUpdate(creditPayment);
        }

        // връща сумата на всички платени задължения за месеца
        public decimal GetTotalPayedForDetection(Int64 detectionId)
        {
            try
            {
                return CurrentSession.Query<UnitCharge>()
                        .Where(x => x.Expense.Detection.Id == detectionId)
                        .Sum(x => x.PayedSum);
            }
            catch
            {
                return 0;
            }
        }

        // връща всички частично платени и неплатени задължения за месеца
        public IList<UnitCharge> GetUnpaiedChargesForDetection(Int64 detectionId)
        {
            IList<UnitCharge> unpaidInCurrentDetection = CurrentSession.Query<UnitCharge>()
                                                        .Fetch(x => x.Unit)
                                                        .Fetch(x => x.Expense)
                                                        .ThenFetch(x => x.Detection)
                                                        .Where(x => x.Expense.Detection.Id == detectionId)
                                                        .Where(x => x.PayedSum < x.SumToPay)
                                                        .ToList();

            return unpaidInCurrentDetection;

        }

        // връща всички неразплатени през текущия месец, но платени в друг месец
        public IList<UnitCharge> GetUnpaidInCurrentDetectionAndPaiedInAnotherDetection(Int64 detectionId)
        {
            IList<UnitCharge> payedInOtherDetection = (from c in CurrentSession.Query<UnitCharge>()
                                                       from d in c.IncomePayments
                                                       where d.Detection.Id != detectionId
                                                       && c.Expense.Detection.Id == detectionId
                                                       select c).Fetch(x => x.Unit)
                                                                .Fetch(x => x.Expense)
                                                                .ThenFetch(x => x.Detection).ToList();
            return payedInOtherDetection;
        }

        // връща всички задължения по които няма никаво плащане за месеца
        public IList<UnitCharge> GetAllUnpaiedChargesForDetection(Int64 detectionId)
        {
            return CurrentSession.Query<UnitCharge>()
                    .Fetch(x => x.Unit)
                    .Fetch(x => x.Expense)
                    .ThenFetch(x => x.Detection)
                    .Where(x => x.Expense.Detection.Id == detectionId)
                    .Where(x => x.PayedSum == 0)
                    .ToList();
        }

        // изтрива всички подадени задължения. 
        public void DeleteCharges(IList<UnitCharge> charges)
        {
            foreach (var charge in charges)
            {
                Delete(charge);
            }
        }

        public IList<UnitCharge> GetAllChargesByUnit(Int64 unitId)
        {
            return CurrentSession.Query<UnitCharge>()
                    .Where(x => x.Unit.Id == unitId)
                    .Fetch(x => x.Expense)
                    .ThenFetch(x => x.Detection)
                    .ToList();
        }

        // взима всички начисления за даден апартамент, за определен период
        public IList<UnitCharge> GetChargesForPeriod(Detection startDetection, Detection endDetection, Int64 unitId)
        {
            var unitCharges = GetAllChargesByUnit(unitId);

            var result = (from c in unitCharges
                          where (c.Expense.Detection.GetDetectionDateTime() >= startDetection.GetDetectionDateTime()
                          && c.Expense.Detection.GetDetectionDateTime() <= endDetection.GetDetectionDateTime())
                          select c).ToList();

            return result;
        }
    }
}
