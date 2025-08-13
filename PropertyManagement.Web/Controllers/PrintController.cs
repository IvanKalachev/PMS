using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Helpers;

namespace PropertyManagement.Web.Controllers
{
    public class PrintController : SessionController
    {
        //
        // GET: /Print/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PrintUnitCharges(Int64 unitId)
        {
            UnitChargeDao dao = new UnitChargeDao(CurrentSession);
            var unPaiedCharges = dao.GetAllUnpaiedForUnit(unitId);

            var charges = (from c in unPaiedCharges
                           group c by c.Expense.Detection into dataGroup
                           select new UnitUnpaidChargesModel
                           {
                               Detection = dataGroup.Key,
                               Charges = (from d in unPaiedCharges
                                          where d.Expense.Detection.Id == dataGroup.Key.Id
                                          select d).ToList()
                           }).ToList();

            return View(charges);
        }

        public ActionResult PrintMonthReport(Int64 detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);

            var detections = detectionDao.GetAll();
            Detection detection;
            if (detectionId != null)
                detection = detections.Where(x => x.Id == detectionId).FirstOrDefault();
            else
                detection = detections.FirstOrDefault();


            StatementModel model = new StatementModel();
            model.Detection = detection;

            model.PayedChargesForCurrentDetection = unitChargeDao.GetTotalPayedForDetection(detection.Id);
            model.PayedIncomesForDetections = incomePaymentDao.GetTotalPayedForDetection(detection.Id);

            model.ChargedSumForDetection = unitChargeDao.GetAllByDetection(detection.Id).Sum(x => x.SumToPay);

            model.ExtraExpensesForDetection = expenseDao.GetExtraExpensesForMonth(detection.Id);

            var payedChargesForPreviousDetections = incomePaymentDao.GetPayedChargesForPreviousDetections(detection);

            var payedChargesForPreviousDetectionsModel = new List<ReportUnitModel>();

            foreach (var payedCharge in payedChargesForPreviousDetections)
            {
                        payedChargesForPreviousDetectionsModel.Add(new ReportUnitModel
                        {
                            Detection = payedCharge.Expense.Detection,
                            Sum = payedCharge.PayedSum,
                            Unit = payedCharge.Unit
                        });
            }

            foreach (var item in payedChargesForPreviousDetectionsModel)
            {
                var isAdded = (from c in model.PayedChargesForPreviousDetections
                               where c.Unit.Id == item.Unit.Id
                               && c.Detection.Id == item.Detection.Id
                               select c).FirstOrDefault();

                if (isAdded == null)
                {
                    model.PayedChargesForPreviousDetections.Add(new ReportUnitModel
                                                     {
                                                         Detection = item.Detection,
                                                         Sum = item.Sum,
                                                         Unit = item.Unit
                                                     });
                }
                else
                {
                    isAdded.Sum += item.Sum;
                }
            }


            var unpaidForDetection = unitChargeDao.GetUnpaiedChargesForDetection(detection.Id);

            foreach (var charge in unpaidForDetection)
            {
                var isAdded = (from c in model.NotpayedForDetection
                               where c.Unit.Id == charge.Unit.Id
                               select c).FirstOrDefault();

                if (isAdded == null)
                {
                    model.NotpayedForDetection.Add(new ReportUnitModel
                    {
                        Sum = charge.SumToPay - charge.PayedSum,
                        Unit = charge.Unit
                    });
                }
                else
                {
                    isAdded.Sum += (charge.SumToPay - charge.PayedSum);
                }
            }

            model.PayedExpensesForDetection = expenseDao.GetTotalForDetection(detection.Id);

            return View(model);
        }
    }
}
