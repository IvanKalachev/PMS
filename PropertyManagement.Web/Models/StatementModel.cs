using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class StatementModel
    {
        public Int64 StatementId { get; set; }

        // всички приходи за месеца
        public IList<ProfitModel> Profits { get; set; }

        // всички разходи за месеца
        public IList<CostModel> Costs { get; set; }

        public Detection Detection { get; set; }

        // платените начислени през текущия месец
        public decimal PayedChargesForCurrentDetection { get; set; }

        // взетите пари за този месец (може да включва и стари задължения)
        public decimal PayedIncomesForDetections { get; set; }

        // сумата на платените стари задължения (не са начислени в текущия месец)
        public decimal PayedOldCharges
        {
            get
            {
                return PayedIncomesForDetections - PayedChargesForCurrentDetection;
            }
        }

        //// всичко платено за месеца
        //public decimal TotalPayed
        //{
        //    get
        //    {
        //        return PayedChargesForCurrentDetection + PayedChargesForPreviousDetections.Sum(x => x.Sum);
        //    }
        //}

        // разплатените задължения начислени през предходните месеци
        private IList<ReportUnitModel> _payedChargesForPreviousDetections;
        public IList<ReportUnitModel> PayedChargesForPreviousDetections
        {
            get
            {
                if (_payedChargesForPreviousDetections == null)
                {
                    _payedChargesForPreviousDetections = new List<ReportUnitModel>();
                }

                return _payedChargesForPreviousDetections;
            }
            set
            {
                _payedChargesForPreviousDetections = value;
            }
            
        }

        // разплатените разходи начислени в текущия месец (разхoди за ток, асансьор и т.н.)
        public decimal PayedExpensesForDetection { get; set; }

        // неразплатените задължения за текушия месец (задълженията са начислени в текущия месец)
        private IList<ReportUnitModel> _notpayedForDetection;
        public IList<ReportUnitModel> NotpayedForDetection
        {
            get
            {
                if (_notpayedForDetection == null)
                {
                    _notpayedForDetection = new List<ReportUnitModel>();
                }

                return _notpayedForDetection;
            }
            set
            {
                _notpayedForDetection = value;
            }

        }

        // нераплатени задължения през текущия месец, но платени в друг месец
        private IList<ReportUnitModel> _notpayedForDetectionAndPaiedInOtherDet;
        public IList<ReportUnitModel> NotpayedForDetectionAndPaiedInOtherDet
        {
            get
            {
                if (_notpayedForDetectionAndPaiedInOtherDet == null)
                {
                    _notpayedForDetectionAndPaiedInOtherDet = new List<ReportUnitModel>();
                }

                return _notpayedForDetectionAndPaiedInOtherDet;
            }
            set
            {
                _notpayedForDetectionAndPaiedInOtherDet = value;
            }

        }

        // Сумата на всички начислени / разпределени услуги за месеца
        public decimal ChargedSumForDetection { get; set; }

        public decimal TotalPrihodi
        {
            get
            {
                return (Profits.Sum(x => x.Sum) + PayedChargesForPreviousDetections.Sum(x => x.Sum)) - (NotpayedForDetection.Sum(x => x.Sum) + NotpayedForDetectionAndPaiedInOtherDet.Sum(x => x.Sum));
            }
        }

        public decimal TotalRazhodi
        {
            get
            {
                return ExtraExpensesForDetection.Sum(x => x.Sum) + Costs.Sum(x => x.Sum);
            }
        }

        public decimal RemainingSum
        {
            get
            {
                return TotalPrihodi - TotalRazhodi;
            }
        }

        private IList<Expense> _extraExpenses;
        public IList<Expense> ExtraExpensesForDetection
        {
            get
            {
                if (_extraExpenses == null)
                {
                    _extraExpenses = new List<Expense>();
                }

                return _extraExpenses;
            }
            set
            {
                _extraExpenses = value;
            }
        }
 
    }
}