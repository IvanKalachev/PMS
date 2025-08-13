using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Web.Models;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Controllers
{
    public class ReportsController : SessionController
    {
        //
        // GET: /Reports/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CurrentStateReport()
        {
            return View(GetReportHeaderModel());
        }

        protected ReportModel GetReportHeaderModel()
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitDao unitDao = new UnitDao(CurrentSession);
            var detections = detectionDao.GetAll();
            var units = unitDao.LoadAll();
            var model = new ReportModel();
            model.Detections = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name");
            model.Units = new SelectList(units, "Id", "FamilyAndApNumber");

            return model;
        }

        [HttpPost]
        public JsonResult CheckReportPeriod(Int64 fromMonth, Int64 toMonth)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            var fromDet = detectionDao.LoadById(fromMonth);
            var toDet = detectionDao.LoadById(toMonth);

            object result;

            if (fromDet.GetDetectionDateTime() > toDet.GetDetectionDateTime())
            {
                result = new { Success = "False", Message = "Въведения период е невалиден!" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (toDet.GetDetectionDateTime() < fromDet.GetDetectionDateTime())
            {
                result = new { Success = "False", Message = "Въведения период е невалиден!" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = new { Success = "True", Message = "" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            //var result = new { Success = "True", Message = "" };
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GenerateCurrentStateReport(FormCollection collection)
        {
            var unitCharges = GetCurrentStateReportModel(Int64.Parse(collection["unitId"]), Int64.Parse(collection["fromDetection"]), Int64.Parse(collection["toDetection"]));
            return PartialView("_UnitCharges", unitCharges);
        }

        protected IList<UnitUnpaidChargesModel> GetCurrentStateReportModel(Int64 unitId, Int64 fromDetection, Int64 toDetction)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);

            var fromDet = detectionDao.LoadById(fromDetection);
            var toDet = detectionDao.LoadById(toDetction);

            var charges = unitChargeDao.GetChargesForPeriod(fromDet, toDet, unitId);

            var unitCharges = (from c in charges
                               group c by c.Expense.Detection into dataGroup
                               select new UnitUnpaidChargesModel
                               {
                                   Detection = dataGroup.Key,
                                   Charges = (from d in charges
                                              where d.Expense.Detection.Id == dataGroup.Key.Id
                                              select d).ToList()
                               }).ToList();

            return unitCharges;
        }

        public ActionResult PrintCurrentStateReport(Int64? unitId, Int64? fromDet, Int64? toDet)
        {
            var unitCharges = GetCurrentStateReportModel((Int64)unitId, (Int64)fromDet, (Int64)toDet);
            if (unitCharges.Count > 0)
            {
                var unit = unitCharges[0].Charges[0].Unit;
                ViewBag.ReportTitle = "Справка aктуално състояние - " + unit.FamilyAndApNumber;
            }
            else
            {
                ViewBag.ReportTitle = "";
            }
            return View("PrintUnitCharges", unitCharges);
        }

        public ActionResult UnitPaymnetsReport()
        {
            return View(GetReportHeaderModel());
        }

        public ActionResult GenerateUnitPaymnetsReport(FormCollection collection)
        {
            var model = GetUnitPaymentsReportModel(Int64.Parse(collection["unitId"]), Int64.Parse(collection["fromDetection"]), Int64.Parse(collection["toDetection"]));
            return PartialView("_UnitResMoney", model);
        }

        protected IList<ResMoney> GetUnitPaymentsReportModel(Int64 unitId, Int64 fromDet, Int64 toDet)
        {
            ResMoneyDao resMoneyDao = new ResMoneyDao(CurrentSession);
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            var fromDetection = detectionDao.LoadById(fromDet);
            var toDetection = detectionDao.LoadById(toDet);

            var unitResMoneyForPeriod = resMoneyDao.GetresMoneyForUnitAndPeriod(unitId, fromDetection, toDetection);

            return unitResMoneyForPeriod;
        }

        public ActionResult PrintUnitPayments(Int64 unitId, Int64 fromDet, Int64 toDet)
        {
            var model = GetUnitPaymentsReportModel(unitId, fromDet, toDet);

            if (model.Count > 0)
            {
                ViewBag.ReportTitle = "Внесени плащания - " + model[0].Unit.FamilyAndApNumber;
            }
            else
            {
                ViewBag.ReportTitle = "";
            }

            return View(GetUnitPaymentsReportModel(unitId, fromDet, toDet));
        }

    }
}
