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
    [Authorize]
    public class NomenclaturesController : SessionController
    {
        public ActionResult Index()
        {
            ServiceDao serviceDao = new ServiceDao(CurrentSession);
            ChargeTypeDao chargeTypesDao = new ChargeTypeDao(CurrentSession);

            NomenclaturesModel model = new NomenclaturesModel();
            var chargeTypes = chargeTypesDao.LoadAll();
            ViewBag.ChargeTypes = PMHelper.ConvertEntityListToSelectList<ChargeType>(chargeTypes, 0);
            model.ChargeTypes = chargeTypes;
            model.Services = serviceDao.LoadAll();


            //Detections
            var months = MonthsYears.GetMonthsList();
            var years = MonthsYears.GetYearsList();

            ViewBag.YearsList = new SelectList(years, "Id", "Name", DateTime.Now.Year);
            ViewBag.MonthsList = new SelectList(months, "Id", "Name", DateTime.Now.Month);

            DetectionDao detDao = new DetectionDao(CurrentSession);

            List<DetectionModel> detections = new List<DetectionModel>();
            foreach (var det in detDao.GetAll())
            {
                detections.Add(PMHelper.ConvertTo<DetectionModel>(det));
            }

            if (TempData["Error"] != null)
            {
                ViewBag.Error = (ErrorModel)TempData["Error"];
            }

            model.Detections = detections;

            return View(model);
        }

        public ActionResult AddService(NomenclaturesModel model)
        {
            if (ModelState.IsValid)
            {
                ServiceDao serviceDao = new ServiceDao(CurrentSession);
                ChargeTypeDao chargeTypeDao = new ChargeTypeDao(CurrentSession);
                Service service = new Service
                {
                    Name = model.AddedService,
                    ChargeType = chargeTypeDao.LoadById(Int64.Parse(model.AddedServiceChargeType)),
                    IsProfit = model.AddedServiceIsProfit,
                    IsCost = model.AddedServiceIsCost
                };
                serviceDao.SaveOrUpdate(service);
            }

           return RedirectToAction("Index");
        }

        public ActionResult SaveServices(NomenclaturesModel model)
        {
            ServiceDao serviceDao = new ServiceDao(CurrentSession);
            foreach (var serviceModel in model.Services)
            {
                var serviceToUpdate = serviceDao.LoadById(serviceModel.Id);
                if (serviceToUpdate != null)
                {
                    serviceToUpdate.Name = serviceModel.Name;
                    if (serviceToUpdate.ChargeType.Id != serviceModel.ChargeType.Id)
                    {
                        serviceToUpdate.ChargeType = CurrentSession.Get<ChargeType>(serviceModel.ChargeType.Id);
                    }

                    serviceToUpdate.IsCost = serviceModel.IsCost;
                    serviceToUpdate.IsProfit = serviceModel.IsProfit;

                    serviceDao.SaveOrUpdate(serviceToUpdate);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CreateDetection(FormCollection collection)
        {
            try
            {
                int month = Int32.Parse(collection["Month"]);
                int year = Int32.Parse(collection["Year"]);

                DetectionDao detеctionDao = new DetectionDao(CurrentSession);
                Detection detection = new Detection { Month = month, Year = year };
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
    }
}