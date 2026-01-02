using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Dao;
using System.Configuration;

namespace PropertyManagement.Web.Controllers
{
    [Authorize]
    public class HomeController : SessionController
    {
        public ActionResult Index()
        {
            var dateMonth = Helpers.MonthsYears.GetMonth(DateTime.Now.Month) + " " + DateTime.Now.Year;
            ViewBag.Month = dateMonth;

            StatisticsModel model = new StatisticsModel();
            UnitDao unitDao = new UnitDao(CurrentSession);

            model.UnitsWithCharges = unitDao.GetUnitsWithPlusBalance();
            model.UnitsWithPositiveSaldo = unitDao.GetUnitsWithMinusBalance();

            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
            ExpenseDao expenseDao = new ExpenseDao(CurrentSession);

            //var expensesSum = expenseDao.GetAllExpensesSum();
            //var incomesSum = incomePaymentDao.GetTotalPaied();

            //model.Saldo = incomesSum - expensesSum;

            decimal saldoTo2025 = 0;
            Decimal.TryParse(ConfigurationManager.AppSettings["startSaldo"], out saldoTo2025);

            // !!!!! ЩЕ БАВИ !!!!!!!
            model.Saldo = getTotalSaldo();
            model.SaldoTo2025 = saldoTo2025;
            model.TotalSaldo = model.Saldo + model.SaldoTo2025;

            return View(model);
        }

        public ActionResult ViewUnitUnpaidCharges(Int64 unitId)
        {
            var charges = GetModel(unitId);

            return View(charges);
        }

        protected decimal getTotalSaldo()
        {
            StatementsController statetment = new StatementsController();
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            var detections = detectionDao.GetAll();
            decimal saldo = 0;

            foreach (var det in detections)
            {
                var model = statetment.GetStatementModel(det.Id);
                saldo += model.RemainingSum;
            }

            return saldo;
        }

        private IList<UnitUnpaidChargesModel> GetModel(Int64 unitId)
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

            return charges;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult PrintUnitCharges(Int64 unitId)
        {
            var model = GetModel(unitId);
            ViewBag.ReportTitle = "Справка неплатени задължения - " + model[0].Charges[0].Unit.FamilyAndApNumber;
            return View(model);
        }

    }
}
