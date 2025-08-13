using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Core.Dao;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Models;

namespace PropertyManagement.Web.Controllers
{
    [Authorize]
    public class DetectionsController : SessionController
    {
        //
        // GET: /Detections/

        public ActionResult Index()
        {
            var months = MonthsYears.GetMonthsList();
            var years = MonthsYears.GetYearsList();

            ViewBag.YearsList = new SelectList(years, "Id", "Name", DateTime.Now.Year);
            ViewBag.MonthsList = new SelectList(months, "Id", "Name", DateTime.Now.Month);

            DetectionDao detDao = new DetectionDao(CurrentSession);

            List<DetectionModel> models = new List<DetectionModel>();
            foreach (var det in detDao.GetAll())
            {
                models.Add(PMHelper.ConvertTo<DetectionModel>(det));
            }

            if (TempData["Error"] != null)
            {
                ViewBag.Error = (ErrorModel)TempData["Error"];
            }

            return View(models);
        }

        //
        // GET: /Detections/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Detections/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Detections/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                int month = Int32.Parse(collection["Month"]);
                int year = Int32.Parse(collection["Year"]);

                DetectionDao detеctionDao = new DetectionDao(CurrentSession);
                Detection detection = new Detection{Month = month, Year = year};
                if (!detеctionDao.Create(detection))
                {
                    ErrorModel error = new ErrorModel(ErrorType.error, "Съществува отчетен период с избраните параметри!");
                    TempData["Error"] = error;

                }
                    
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Detections/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Detections/Edit/5

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
        // GET: /Detections/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Detections/Delete/5

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
    }
}
