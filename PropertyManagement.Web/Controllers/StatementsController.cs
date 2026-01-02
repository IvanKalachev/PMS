using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Web.Models;
using Newtonsoft.Json;

namespace PropertyManagement.Web.Controllers
{
    public class StatementsController : SessionController
    {
        //
        // GET: /Report/

        public ActionResult Index()
        {
            var statementDao = new StatamentDao(CurrentSession);
            var statements = statementDao.GetAll();

            var model = new StatementsListModel();
            model.Statements = new List<StatementSimpleModel>();

            if (statements != null && statements.Any())
            {
                model.Statements = statements.Select(x => new StatementSimpleModel
                {
                    Id = x.Id,
                    CreatedOn = x.InsertDate,
                    Detection = MonthsYears.GetMonth(x.Detection.Month)  + " " + x.Detection.Year
                }).ToList();
            } 

            return View(model);
        }


        public ActionResult GenerateStatement(Int64? detectionId)
        {
            var detectionsDao = new DetectionDao(CurrentSession);
            var detectionsWithoutStatements = detectionsDao.GetDetectionsWithoutStatements();

            if (detectionsWithoutStatements == null || !detectionsWithoutStatements.Any())
            {
                TempData["Error"] = "Генерирали сте отчети за всички въведени отчетни периоди.";
                return RedirectToAction("Index");
            }

            var model = GetStatementModel(detectionId);
            return View("ViewStatement", model);
        }

        public ActionResult CreateStatement(Int64 detectionId)
        {
            try
            {
                var statementDao = new StatamentDao(CurrentSession);
                var detectionDao = new DetectionDao(CurrentSession);

                var detection = detectionDao.LoadById(detectionId);

                var statementModel = GetStatementModel(detectionId);

                var statementData = JsonConvert.SerializeObject(statementModel);

                statementDao.CreateStatement(detection, statementData);
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Възникна грешка при запис на отчета.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult ViewStatement(Int64 statementId)
        {
            var statementDao = new StatamentDao(CurrentSession);
            var statement = statementDao.LoadById(statementId);

            if (statement == null)
            {
                TempData["Error"] = "Отчетът не е намерен.";

                return RedirectToAction("Index");
            }

            var model = JsonConvert.DeserializeObject<StatementModel>(statement.Data);
            model.StatementId = statement.Id;

            return View(model);
        }

        public StatementModel GetStatementModel(Int64? detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);

            var detections = detectionDao.GetAll();
            var detectionsWithoutStatements = detectionDao.GetDetectionsWithoutStatements();
            Detection detection;
            if (detectionId != null)
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detectionsWithoutStatements), "Id", "Name", detectionId);
                detection = detections.Where(x => x.Id == detectionId).FirstOrDefault();
            }
            else
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detectionsWithoutStatements), "Id", "Name");
                detection = detections.FirstOrDefault();
            }


            StatementModel model = new StatementModel();

            var profits = expenseDao.GetProfitsForDetection(detection.Id);

            var profitsModel = (from c in profits
                               group c by c.Service into dataGroup
                               select new ProfitModel
                               {
                                   // new object due to serializarion errors when saving statement
                                   Service = new Service
                                   {
                                       Name = dataGroup.Key.Name,
                                       IsProfit = dataGroup.Key.IsProfit,
                                       IsCost = dataGroup.Key.IsCost,
                                       ChargeType = new ChargeType
                                       {
                                           Id = dataGroup.Key.ChargeType.Id,
                                           Name = dataGroup.Key.ChargeType.Name
                                       },
                                       Id = dataGroup.Key.Id
                                   },
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
                                  // new object due to serializarion errors when saving statement
                                  Service = new Service
                                  {
                                      Name = dataGroup.Key.Name,
                                      IsProfit = dataGroup.Key.IsProfit,
                                      IsCost = dataGroup.Key.IsCost,
                                      ChargeType = new ChargeType
                                      {
                                          Id = dataGroup.Key.ChargeType.Id,
                                          Name = dataGroup.Key.ChargeType.Name
                                      },
                                      Id = dataGroup.Key.Id
                                  },
                                  Sum = (from d in costs
                                         where d.Service.Id == dataGroup.Key.Id
                                         select d).Sum(x => x.Sum)
                              }).ToList();

            model.Costs = costsModel;

            model.Detection = detection;

            model.PayedChargesForCurrentDetection = unitChargeDao.GetTotalPayedForDetection(detection.Id);
            model.PayedIncomesForDetections = incomePaymentDao.GetTotalPayedForDetection(detection.Id);

            model.ChargedSumForDetection = unitChargeDao.GetAllByDetection(detection.Id).Sum(x => x.SumToPay);


            model.ExtraExpensesForDetection = new List<Expense>();

            var extraExpensesForDetection = expenseDao.GetExtraExpensesForMonth(detection.Id);
            if (extraExpensesForDetection != null && extraExpensesForDetection.Any())
            {
                // new object due to serializarion errors when saving statement
                model.ExtraExpensesForDetection = extraExpensesForDetection.Select(x => new Expense
                {
                    Detection = new Detection
                    {
                        Id = x.Detection.Id,
                        Month = x.Detection.Month,
                        Year = x.Detection.Year
                    },
                    ExpenseType = x.ExpenseType,
                    Id = x.Id,
                    IsDistributed = x.IsDistributed,
                    Service = new Service
                    {
                        Id = x.Service.Id,
                        ChargeType = new ChargeType
                        {
                            Id = x.Service.ChargeType.Id,
                            Name = x.Service.ChargeType.Name
                        },
                        IsCost = x.Service.IsCost,
                        IsProfit = x.Service.IsProfit,
                        Name = x.Service.Name
                    },
                    Sum = x.Sum,
                    Unit = x.Unit
                }).ToList();
            }

            //model.ExtraExpensesForDetection = expenseDao.GetExtraExpensesForMonth(detection.Id);

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
                        Unit = new Unit
                        {
                            Balance = item.Unit.Balance,
                            ChargedSum = item.Unit.ChargedSum,
                            Deactivated = item.Unit.Deactivated,
                            FamilyName = item.Unit.FamilyName,
                            Floor = item.Unit.Floor,
                            Id = item.Unit.Id,
                            IsDeleted = item.Unit.IsDeleted,
                            MembersCount = item.Unit.MembersCount,
                            NoChargeServices = item.Unit.NoChargeServices.Select(x => new Service
                            {
                                ChargeType = new ChargeType
                                {
                                    Id = x.ChargeType.Id,
                                    Name = x.ChargeType.Name
                                }
                            }).ToList(),
                            Number = item.Unit.Number,
                            PercentIdealParts = item.Unit.PercentIdealParts
                        }
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
                        Unit = new Unit
                        {
                            Balance = charge.Unit.Balance,
                            ChargedSum = charge.Unit.ChargedSum,
                            Deactivated = charge.Unit.Deactivated,
                            FamilyName = charge.Unit.FamilyName,
                            Floor = charge.Unit.Floor,
                            Id = charge.Unit.Id,
                            IsDeleted = charge.Unit.IsDeleted,
                            MembersCount = charge.Unit.MembersCount,
                            NoChargeServices = charge.Unit.NoChargeServices.Select(x => new Service
                            {
                                ChargeType = new ChargeType
                                {
                                    Id = x.ChargeType.Id,
                                    Name = x.ChargeType.Name
                                }
                            }).ToList(),
                            Number = charge.Unit.Number,
                            PercentIdealParts = charge.Unit.PercentIdealParts
                        }
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
                    reportUnitModel.Unit = new Unit
                    {
                        Balance = charge.Unit.Balance,
                        ChargedSum = charge.Unit.ChargedSum,
                        Deactivated = charge.Unit.Deactivated,
                        FamilyName = charge.Unit.FamilyName,
                        Floor = charge.Unit.Floor,
                        Id = charge.Unit.Id,
                        IsDeleted = charge.Unit.IsDeleted,
                        MembersCount = charge.Unit.MembersCount,
                        NoChargeServices = charge.Unit.NoChargeServices.Select(x => new Service
                        {
                            ChargeType = new ChargeType
                            {
                                Id = x.ChargeType.Id,
                                Name = x.ChargeType.Name
                            }
                        }).ToList(),
                        Number = charge.Unit.Number,
                        PercentIdealParts = charge.Unit.PercentIdealParts
                    };
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

        public ActionResult PrintMonthStatement(Int64 statementId)
        {
            var statementDao = new StatamentDao(CurrentSession);
            var statement = statementDao.LoadById(statementId);

            if (statement == null)
            {
                TempData["Error"] = "Отчетът не е намерен.";

                return RedirectToAction("Index");
            }

            var model = JsonConvert.DeserializeObject<StatementModel>(statement.Data);

            return View(model);
        }

    }
}
