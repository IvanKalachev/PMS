using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Web.Models;

namespace PropertyManagement.Web.Controllers
{
    public class StatementsController : SessionController
    {
        //
        // GET: /Report/

        public ActionResult Index(Int64? detectionId)
        {
            var model = GetStatementModel(detectionId);

            return View(model);
        }

        public StatementModel GetStatementModel(Int64? detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);

            var detections = detectionDao.GetAll();
            Detection detection;
            if (detectionId != null)
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name", detectionId);
                detection = detections.Where(x => x.Id == detectionId).FirstOrDefault();
            }
            else
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name");
                detection = detections.FirstOrDefault();
            }


            StatementModel model = new StatementModel();

            var profits = expenseDao.GetProfitsForDetection(detection.Id);

            var profitsModel = (from c in profits
                               group c by c.Service into dataGroup
                               select new ProfitModel
                               {
                                   Service = dataGroup.Key,
                                   Sum = (from d in profits
                                            where d.Service.Id == dataGroup.Key.Id
                                            select d).Sum(x => x.Sum)
                               }).ToList();

            model.Profits = profitsModel;


            var costs = expenseDao.GetCostsForDetection(detection.Id);

            var costsModel = (from c in costs
                              group c by c.Service into dataGroup
                              select new CostModel
                              {
                                  Service = dataGroup.Key,
                                  Sum = (from d in costs
                                         where d.Service.Id == dataGroup.Key.Id
                                         select d).Sum(x => x.Sum)
                              }).ToList();

            model.Costs = costsModel;

            model.Detection = detection;

            model.PayedChargesForCurrentDetection = unitChargeDao.GetTotalPayedForDetection(detection.Id);
            model.PayedIncomesForDetections = incomePaymentDao.GetTotalPayedForDetection(detection.Id);

            model.ChargedSumForDetection = unitChargeDao.GetAllByDetection(detection.Id).Sum(x => x.SumToPay);

            model.ExtraExpensesForDetection = expenseDao.GetExtraExpensesForMonth(detection.Id);

            var payedChargesForPreviousDetections = incomePaymentDao.GetPayedChargesForPreviousDetections(detection);

            var payedChargesForPreviousDetectionsModel = new List<ReportUnitModel>();

            foreach (var payedCharge in payedChargesForPreviousDetections)
            {
                //decimal oldCharges = model.PayedOldCharges;

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

            var unpaidForDetectionAndPaiedInOtherD = unitChargeDao.GetUnpaidInCurrentDetectionAndPaiedInAnotherDetection(detection.Id).Where(x => x.Expense.Detection.Id != detection.Id);

            foreach (var charge in unpaidForDetectionAndPaiedInOtherD)
            {
                var isAdded = (from c in model.NotpayedForDetectionAndPaiedInOtherDet
                               where c.Unit.Id == charge.Unit.Id
                               select c).FirstOrDefault();

                if (isAdded == null)
                {
                    decimal sum = 0;
                    if(charge.SumToPay == charge.PayedSum)
                    {
                        sum = charge.SumToPay;
                    }
                    else
                    {
                        sum = charge.SumToPay - charge.PayedSum;
                    }

                    var reportUnitModel = new ReportUnitModel();
                    reportUnitModel.Unit = charge.Unit;
                    reportUnitModel.Sum = sum;
                    model.NotpayedForDetectionAndPaiedInOtherDet.Add(reportUnitModel);
                }
                else
                {
                    if (charge.SumToPay == charge.PayedSum)
                    {
                        isAdded.Sum += (charge.SumToPay);
                    }
                    else
                    {
                        isAdded.Sum += (charge.SumToPay - charge.PayedSum);
                    }
                }
            }

            model.PayedExpensesForDetection = expenseDao.GetTotalForDetection(detection.Id);

            return model;
        }

        public ActionResult ShowStatemant(Int64? unitId)
        {
            PrintReportModel model = new PrintReportModel();
            if (unitId != null)
            {
                model.UnitId = (Int64)unitId;
            }

            return View(model);
        }

        public ActionResult PrintMonthStatement(Int64 detectionId)
        {
            var model = GetStatementModel(detectionId);

            return View(model);
        }

    }
}
