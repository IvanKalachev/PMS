using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyManagement.Core.Entities;
using NHibernate;
using NHibernate.Linq;

namespace PropertyManagement.Core.Dao
{
    public class ResMoneyDao : BaseDao<ResMoney, Int64>
    {
        public ResMoneyDao(ISession session)
            : base(session)
        {
        }

        // Създава ново разплащане
        public void CreateNew(Detection detection, decimal payedSum, Unit unit)
        {
            ResMoney resMo = new ResMoney
            {
                Detection = detection,
                PayedSum = payedSum,
                Unit = unit,
                InsertDate = DateTime.Now
            };

            SaveOrUpdate(resMo);
        }

        // Връща всички внесени пари за даден апартамент и подаден месец
        public decimal GetSumByUnitAndDetection(Int64 unitId, Int64 detectionId)
        {
            var resmoney =  CurrentSession.Query<ResMoney>()
                            .Where(x => x.Unit.Id == unitId)
                            .Where(x => x.Detection.Id == detectionId)
                            .ToList();

            return resmoney.Sum(x => x.PayedSum);
        }

        // Връща всички внесени пари за даден месец
        public IList<ResMoney> GetAllByDetection(Int64 detectionId)
        {
            return CurrentSession.Query<ResMoney>()
                    .Where(x => x.Detection.Id == detectionId)
                    .ToList();
        }

        // връща платените (внесени) пари от определен апартамент и избран период
        public IList<ResMoney> GetresMoneyForUnitAndPeriod(Int64 unitId, Detection fromDetection, Detection toDetection)
        {
            var unitResMoney = (from c in CurrentSession.Query<ResMoney>()
                          where c.Unit.Id == unitId
                          select c).ToList();

            var result = (from p in unitResMoney
                          where p.Detection.GetDetectionDateTime() >= fromDetection.GetDetectionDateTime()
                          && p.Detection.GetDetectionDateTime() <= toDetection.GetDetectionDateTime()
                          select p).ToList();

            return result;
        }
    }
}
