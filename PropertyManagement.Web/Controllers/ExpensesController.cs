using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Web.Models;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Core.Entities;
using PropertyManagement.Core;

namespace PropertyManagement.Web.Controllers
{
    [Authorize]
    public class ExpensesController : SessionController
    {
        //
        // GET: /Expenses/

        public ActionResult Index(Int64? detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            CurrencyDao currencyDao = new CurrencyDao(CurrentSession);

            var detections = detectionDao.GetAll();
            if (detectionId != null)
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name", detectionId);
            }
            else
            {
                ViewBag.Detections = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name");
            }

            ViewBag.ServicesList = GetServicesList(0);
            ViewBag.UnitsList = GetUnitsList();

            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            IList<ExpenseModel> expensesToShow = new List<ExpenseModel>();
            IList<Expense> detectionExpenses = new List<Expense>();

            var dId = detectionId != null ? (Int64)detectionId : detections.FirstOrDefault().Id;
            detectionExpenses = expenseDao.LoadByDetection(dId);
            var currency = GetCurrency(dId);
            ViewBag.Currency = currency.Symbol;

            foreach(var expense in detectionExpenses)
            {
                expensesToShow.Add(PMHelper.ConvertTo<ExpenseModel>(expense));
            }

            ExpensesViewModel model = new ExpensesViewModel();
            model.Expenses = expensesToShow;
            //model.CanResetExpenseDistribution = CanResetExpenseDistribution(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddExpense(FormCollection collection)
        {
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);
            CurrencyDao currencyDao = new CurrencyDao(CurrentSession);
            decimal overheadSum = -1;
            decimal individualSum = -1;
            Decimal.TryParse(collection["sum"].ToString(), out overheadSum);
            Decimal.TryParse(collection["individualSum"].ToString(), out individualSum);

            var detection = CurrentSession.Get<Detection>(Int64.Parse(collection["detection"].ToString()));
            var currency = GetCurrency(detection.Id);

            if ((overheadSum != -1 || individualSum != -1) && (overheadSum != 0 || individualSum != 0))
            {
                Expense newExpense = new Expense();
                newExpense.IsDistributed = false;
                newExpense.Detection = detection;
                newExpense.Currency = currency;

                if (collection["overHeadButton"] != null)
                {
                    if (collection["doNoDestribute"].ToString() != "false")
                    {
                        newExpense.ExpenseType = 3;
                        newExpense.IsDistributed = true;
                    }
                    else
                    {
                        newExpense.ExpenseType = 1;
                    }
                    
                    newExpense.Sum = overheadSum;
                    newExpense.Service = CurrentSession.Get<Service>(Int64.Parse(collection["serviceId"].ToString()));
                }
                else if (collection["individualButton"] != null)
                {
                    UnitDao unitDao = new UnitDao(CurrentSession);
                    newExpense.ExpenseType = 2;
                    newExpense.Unit = unitDao.LoadById(Int64.Parse(collection["unitId"].ToString()));
                    newExpense.Sum = individualSum;
                    newExpense.Service = CurrentSession.Get<Service>(Int64.Parse(collection["individualService"].ToString()));
                }
                
                expenseDao.Create(newExpense);

                ViewBag.Currency = currency.Symbol;

                var model = PMHelper.ConvertTo<ExpenseModel>(newExpense);
                return PartialView("_AddedExpense", model);
            }
            else
            {
                ErrorModel error = new ErrorModel(ErrorType.error, "Непаравилно въведена стойност за сума!");
                ViewBag.Error = error;
                return View("Index", detection.Id);
            }
        }

        // GET
        public ActionResult EditExpense(Int64 id)
        {
            ExpenseDao expDao = new ExpenseDao(CurrentSession);
            var selectedExpense = expDao.LoadById(id);
            var model = PMHelper.ConvertTo<ExpenseModel>(selectedExpense);
            if (model.ExpenseType == 1)
            {
                ViewBag.Services = GetServicesList(selectedExpense.Service.Id);
            }
            return PartialView("_EditExpense", model);
        }

        [HttpPost]
        public ActionResult EditExpense(ExpenseModel model)
        {
            ExpenseDao expensesDao = new ExpenseDao(CurrentSession);
            var expenseToEdit = expensesDao.LoadById(model.Id);
            if (model.ExpenseType == 1)
            {
                expenseToEdit.Service = CurrentSession.Get<Service>(model.Service.Id);
            }
            expenseToEdit.Sum = model.Sum;
            expensesDao.SaveOrUpdate(expenseToEdit);

            var modelToLoad = PMHelper.ConvertTo<ExpenseModel>(expenseToEdit);
            return PartialView("_AddedExpense", modelToLoad);
        }

        public ActionResult DeleteExpense(Int64 id)
        {
            ExpenseDao expDao = new ExpenseDao(CurrentSession);
            expDao.DeleteById(id);

            return PartialView("_DeleteExpense");
        }

        //
        // GET: /Expenses/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Expenses/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Expenses/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Expenses/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Expenses/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Expenses/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Expenses/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        protected SelectList GetServicesList(Int64 selectedValue)
        {
            ServiceDao serviceDao = new ServiceDao(CurrentSession);
            return PMHelper.ConvertEntityListToSelectList<Service>(serviceDao.LoadAll(), selectedValue);
        }

        protected SelectList GetUnitsList()
        {
            UnitDao unitDao = new UnitDao(CurrentSession);
            return new SelectList(unitDao.LoadAll(), "Id", "FamilyAndApNumber");
        }

        // got total for detection
        [HttpPost]
        public JsonResult GetTotal(Int64 id)
        {
            ExpenseDao expDao = new ExpenseDao(CurrentSession);
            var total = expDao.GetTotalForDetection(id);
            return Json(new { sum = total });
        }

        // флаг, който показва дали за дадения месец има разпределени разходи
        private bool CanResetExpenseDistribution(ExpensesViewModel model)
        {
            bool hasDistributedExpense = false;
            bool lastMonth = false;
            bool result = false;

                foreach (var expense in model.Expenses)
                {
                    if (expense.IsDistributed == true)
                    {
                        hasDistributedExpense = true;
                        break;
                    }
                }

                if (model.Expenses.Count > 0)
                {
                    DetectionDao detectionDao = new DetectionDao(CurrentSession);
                    if (model.Expenses[0].Detection.Id == detectionDao.GetLastDetectionId())
                    {
                        lastMonth = true;
                    }
                }

                if (hasDistributedExpense && lastMonth)
                {
                    result = true;
                }

                return result;
        }

        // връща валутата за дадения месец
        private Currency GetCurrency(Int64 detectionId)
        {
            var currencyDao = new CurrencyDao(CurrentSession);
            var expensesDao = new ExpenseDao(CurrentSession);

            var currency = currencyDao.GetDefaultCurrency();

            var detectionExpenses = expensesDao.LoadByDetection(detectionId);
            if (detectionExpenses != null && detectionExpenses.Any())
            {
                currency = detectionExpenses.FirstOrDefault().Currency;
            }

            return currency;
        }
    }
}
