using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class UnitDao : BaseDao<Unit, Int64>
    {
        public UnitDao(ISession session) : base(session)
        {
        }

        public IList<Unit> LoadAll()
        {
            return CurrentSession.Query<Unit>()
                    .Where(x => x.IsDeleted == false)
                    .OrderBy(x => x.Floor)
                    .ThenBy(x => x.Number)
                    .FetchMany(x => x.NoChargeServices)
                    .ToList();
        }

        public IList<Unit> LoadAllActive()
        {
            return CurrentSession.Query<Unit>()
                    .Where(x => x.IsDeleted == false)
                    .Where(x => x.Deactivated  == false)
                    .OrderBy(x => x.Floor)
                    .ThenBy(x => x.Number)
                    .FetchMany(x => x.NoChargeServices)
                    .ToList();
        }

        public int GetUnitsCount()
        {
            return CurrentSession.Query<Unit>()
                    .Where(x => x.IsDeleted == false)
                    .Where(x => x.Deactivated == false)
                    .Count();
        }

        public int GetPeopleCount()
        {
            return (from c in CurrentSession.Query<Unit>()
                    where c.IsDeleted == false
                    && c.Deactivated == false
                    select c).Sum(c => c.MembersCount);
        }

        public int GetPeopleCountByService(Service service)
        {
            return (from c in CurrentSession.Query<Unit>()
                    where c.IsDeleted == false
                    && c.Deactivated == false
                    && !c.NoChargeServices.Contains(service)
                    select c).Sum(c => c.MembersCount);
        }

        public int GetUnitsCountByService(Service service)
        {
             return CurrentSession.Query<Unit>()
                    .Where(x => x.IsDeleted == false)
                    .Where(x => x.Deactivated == false)
                    .Where(x => x.MembersCount > 0)
                    .Where(x => !x.NoChargeServices.Contains(service))
                    .Count();
        }

        public decimal GetCommonIdelParts(Service service)
        {
            return CurrentSession.Query<Unit>()
                    .Where(x => x.IsDeleted == false)
                    .Where(x => x.Deactivated == false)
                    .Where(x => !x.NoChargeServices.Contains(service))
                    .Sum(x => x.PercentIdealParts);
        }

        public decimal GetCommonIdealPartsRemontObnv()
        {
            var units = LoadAllActive();
            decimal sum = 0;

            foreach (var unit in units)
            {
                if (unit.PercentIdealParts < 1)
                {
                    sum += 1;
                }
                else
                {
                    sum += unit.PercentIdealParts;
                }
            }

            return sum;
        }

        public IList<Unit> ChargeAndGetUnits(Int64 chargeTypeId, Expense expense)
        {
            int peopleCount = GetPeopleCountByService(expense.Service);
            int unitsCount = GetUnitsCountByService(expense.Service);

            var units = LoadAllActive();
            decimal totalChargedSum = 0;
            foreach (var unit in units)
            {
                if (!unit.NoChargeServices.Contains(expense.Service))
                {
                    var unitChargedSum = ChargeUnit(chargeTypeId, unit, expense.Sum, peopleCount, unitsCount, expense.Service);
                    totalChargedSum += unitChargedSum;
                }
            }

            if (totalChargedSum != expense.Sum)
            {
                decimal diff = expense.Sum - totalChargedSum;
                var randomUnit = (from c in units
                                  where c.ChargedSum > 0
                                  select c).LastOrDefault();

                randomUnit.ChargedSum += diff;
            }

            return units;
        }

        private decimal ChargeUnit(Int64 chargeTypeId, Unit unit, decimal sum, int peopleCount, int unitsCount, Service service)
        {
            decimal sumToCharge = 0;
            decimal singleSum = 0;
            switch (chargeTypeId)
            {
                case 1:
                    singleSum = sum / peopleCount;
                    //singleSum = Math.Round(singleSum, 2, MidpointRounding.ToEven);
                    sumToCharge = singleSum * unit.MembersCount;
                    break;
                case 2:
                    if (service.Id == 5)
                    {
                        if (unit.PercentIdealParts < 1)
                        {
                            sumToCharge = (sum / GetCommonIdealPartsRemontObnv()) * 1;
                        }
                        else
                        {
                            singleSum = sum / GetCommonIdealPartsRemontObnv();
                            //singleSum = Math.Round(singleSum, 2, MidpointRounding.ToEven);
                            sumToCharge = singleSum * unit.PercentIdealParts;
                        }
                    }
                    else
                    {
                        singleSum = sum / GetCommonIdelParts(service);
                        //singleSum = Math.Round(singleSum, 2, MidpointRounding.ToEven);
                        sumToCharge = singleSum * unit.PercentIdealParts;
                    }
                    
                    break;
                case 3:
                    if (unit.MembersCount > 0)
                    {
                        sumToCharge = sum / unitsCount;
                    }
                    break;
                default: sumToCharge = 0;
                    break;
            }

            unit.ChargedSum = Math.Round(sumToCharge, 2, MidpointRounding.AwayFromZero);

            return unit.ChargedSum;
        }

        public IList<Unit> GetUnitsWithPlusBalance()
        {
            return CurrentSession.Query<Unit>()
                   .Where(x => x.Balance > 0)
                   .OrderByDescending(x => x.Balance)
                   .ToList();
        }

        public IList<Unit> GetUnitsWithMinusBalance()
        {
            return CurrentSession.Query<Unit>()
                   .Where(x => x.Balance < 0)
                   .OrderByDescending(x => x.Balance)
                   .ToList();
        }
    }
}
