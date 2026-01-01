using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Controllers
{
    [Authorize]
    public class ExpenseDistributionController : SessionController
    {
        //
        // GET: /ServiceDistribution/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ShowExpenseDistribution(Int64 id, Int64? chargeType)
        {
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            DistributeExpenseModel model = new DistributeExpenseModel();
            var expense = expenseDao.LoadById(id);
            UnitDao unitdao = new UnitDao(CurrentSession);

            Int64 chargeTypeId;

            if (chargeType != null)
            {
                chargeTypeId = (Int64)chargeType;
            }
            else
            {
                chargeTypeId = expense.Service.ChargeType.Id;
            }

            model.Units = unitdao.ChargeAndGetUnits(chargeTypeId, expense);
            model.Expense = PMHelper.ConvertTo<ExpenseModel>(expense);

            ChargeTypeDao chTypedao = new ChargeTypeDao(CurrentSession);
            ViewBag.ChargeTypesList = new SelectList(chTypedao.LoadAll(), "Id", "Name", chargeTypeId);

            return View(model);
        }

        [HttpPost]
        public ActionResult DistributeAndSave(FormCollection formCollection, DistributeExpenseModel model)
        {
            Int64 chargeTypeId = Int64.Parse(formCollection["chargeType"]);
            Int64 expenseId = Int64.Parse(formCollection["expenseId"]);
            
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            UnitDao unitDao = new UnitDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            var expense = expenseDao.LoadById(expenseId);

            expense.IsDistributed = true;
            var chargedUnits = unitDao.ChargeAndGetUnits(chargeTypeId, expense);

            foreach (var unit in chargedUnits)
            {
                var unitBalanceBeforeCharge = unit.Balance;
                var unitCharge = new UnitCharge();
                unitCharge.Expense = expense;
                unitCharge.Unit = unit;
                unitCharge.SumToPay = unit.ChargedSum;
                unitCharge.Currency = expense.Currency;
                
                // има предплатена сума => разплащаме новото задължение
                if (unitBalanceBeforeCharge < 0)
                {
                    if (((-1) * unitBalanceBeforeCharge) >= unitCharge.SumToPay)
                    {
                        unitCharge.PayedSum = unitCharge.SumToPay;
                        //unit.AddToBalance(unitCharge.SumToPay);
                    }
                    else
                    {
                        unitCharge.PayedSum = (-1) * unitBalanceBeforeCharge;
                        //unit.AddToBalance(unitCharge.SumToPay - unitCharge.PayedSum);
                    }
                }

                unit.AddToBalance(unit.ChargedSum);
                unitDao.SaveOrUpdate(unit);

                unitChargeDao.SaveOrUpdate(unitCharge);

                // приспадната е предплатена сума => генерираме плащане
                if (unitCharge.PayedSum > 0)
                {
                    IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
                    IncomePayment payment = new IncomePayment();
                    payment.Detection = unitCharge.Expense.Detection;
                    payment.PayDate = DateTime.Now;
                    payment.PayedCharges.Add(unitCharge);
                    payment.PayedSum = unitCharge.PayedSum;
                    payment.Unit = unitCharge.Unit;
                    payment.Currency = expense.Currency;

                    incomePaymentDao.SaveOrUpdate(payment);
                }

            }

            return RedirectToAction("Index", "Expenses", new { detectionId = expense.Detection.Id});
        }

        public ActionResult ShowDistributedExpense(Int64 expenseId)
        {
            UnitChargeDao unitChargesDao = new UnitChargeDao(CurrentSession);
            var unitsCharges = unitChargesDao.GetAllByExpense(expenseId);
            return View(unitsCharges);
        }

        public ActionResult ShowIndividualExpense(Int64 expenseId)
        {
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            var expense = expenseDao.LoadById(expenseId);
            var model = PMHelper.ConvertTo<ExpenseModel>(expense);

            return View("IndividualExpenseCharge", model);
        }

        public ActionResult ChargeIndividualExpense(ExpenseModel model)
        {
            UnitDao unitDao = new UnitDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);

            var expense = expenseDao.LoadById(model.Id);
            expense.IsDistributed = true;
            expense.Unit.AddToBalance(expense.Sum);

            var unitCharge = new UnitCharge();
            unitCharge.Expense = expense;
            unitCharge.Unit = expense.Unit;
            unitCharge.SumToPay = expense.Sum;
            unitCharge.Currency = expense.Currency;
            unitDao.SaveOrUpdate(expense.Unit);
            unitChargeDao.SaveOrUpdate(unitCharge);
            expenseDao.SaveOrUpdate(expense);

            return RedirectToAction("Index", "Expenses", new { detectionId = expense.Detection.Id });
        }

        public ActionResult ResetExpenseDistribution(FormCollection fcollection)
        {
            Int64 detectionId = Int64.Parse(fcollection["detectionId"]);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            UnitDao unitDao = new UnitDao(CurrentSession);
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            //var payedCharges = incomePaymentDao.GetPayedChargesByDetection(detectionId);
            //var notpayedCharges = unitChargeDao.GetAllUnpaiedChargesForDetection(detectionId);

            var unitCharges = unitChargeDao.GetAllByDetection(detectionId);

            var payments = incomePaymentDao.GetIncomePaymentsByDetection(detectionId);
            var expensesIndividual = expenseDao.GetIndividualsForMonth(detectionId);
            var expenseOverheads = expenseDao.GetOverheadsForMonth(detectionId);

            try
            {
                //foreach (var payedCharge in payedCharges)
                //{
                //    // премахва от баланса начислената сума
                //    if (payedCharge.SumToPay > payedCharge.PayedSum)
                //    {
                //        decimal difference = payedCharge.SumToPay - payedCharge.PayedSum;
                //        payedCharge.Unit.RemoveFromBalance(difference);
                //        unitDao.SaveOrUpdate(payedCharge.Unit);
                //    }
                //}

                foreach (var charge in unitCharges)
                {
                    // премахва от баланса начислената сума
                    if (charge.SumToPay > charge.PayedSum)
                    {
                        decimal difference = charge.SumToPay - charge.PayedSum;
                        charge.Unit.RemoveFromBalance(difference);
                        unitDao.SaveOrUpdate(charge.Unit);
                    }
                }

                incomePaymentDao.DeletePayments(payments);
                unitChargeDao.DeleteCharges(unitCharges);

                expenseDao.UnDestributeExpenses(expensesIndividual);
                expenseDao.UnDestributeExpenses(expenseOverheads);

                ErrorModel succsess = new ErrorModel(ErrorType.success, "Направените разпределения/начисляния на разходи за месеца бяха занулени успешно !");
                TempData["Error"] = succsess;
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel(ErrorType.error, "Възникна проблем със зануляването на разпределените/начислените разходи за месеца!");
                TempData["Error"] = error;
            }

            return RedirectToAction("Index", "Expenses", new { detectionId = detectionId});
        }
    }
}
