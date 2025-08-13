using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Web.Models;
using PropertyManagement.Core.Dao;
using PropertyManagement.Core.Entities;
using PropertyManagement.Web.Helpers;

namespace PropertyManagement.Web.Controllers
{   
    [Authorize]
    public class UnitsController : SessionController
    {
        //
        // GET: /Unit/

        public ActionResult Index()
        {
            UnitDao unitDao = new UnitDao(CurrentSession);
            var units = unitDao.LoadAll();
            List<UnitModel> models = new List<UnitModel>();
            foreach (var unit in units)
            {
                models.Add(PMHelper.ConvertTo<UnitModel>(unit));
            }

            return View(models);
        }

        //
        // GET: /Unit/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Unit/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Unit/Create

        [HttpPost]
        public ActionResult Create(UnitModel model)
        {
            try
            {
                UnitDao unitDao = new UnitDao(CurrentSession);
                Unit unit = new Unit
                {
                    FamilyName = model.FamilyName,
                    Floor = model.Floor,
                    IsDeleted = false,
                    MembersCount = model.MembersCount,
                    Number = model.Number,
                    PercentIdealParts = model.PercentIdealParts,
                };

                unitDao.Create(unit);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Unit/Edit/5

        public ActionResult Edit(int id)
        {
            Int64 _id = Convert.ToInt64(id);
            UnitDao unitDao = new UnitDao(CurrentSession);
            var selectedUnit = unitDao.LoadById(_id);

            ServiceDao serviceDao = new ServiceDao(CurrentSession);
            var services = PMHelper.ConvertEntityListToSelectList<Service>(serviceDao.LoadAll(), 0);
            ViewBag.Services = services;

            var model = PMHelper.ConvertTo<UnitModel>(selectedUnit);
            model.NoCharegeServices = selectedUnit.NoChargeServices;
            return View(model);
        }

        public ActionResult RemoveService(int serviceId, int unitId)
        {
            ServiceDao sDao = new ServiceDao(CurrentSession);
            UnitDao uDao = new UnitDao(CurrentSession);

            var service = sDao.LoadById(serviceId);
            var unit = uDao.LoadById(unitId);

            if (service != null && unit != null)
            {
                unit.NoChargeServices.Remove(service);
                uDao.SaveOrUpdate(unit);
            }

            return RedirectToAction("Edit", new { id = unitId });
        }

        //
        // POST: /Unit/Edit/5

        [HttpPost]
        public ActionResult Edit(UnitModel model, FormCollection collection)
        {
            try
            {
                UnitDao unitDao = new UnitDao(CurrentSession);
                ServiceDao serviceDao = new ServiceDao(CurrentSession);

                if (collection["button"] == "Запис")
                {
                    var orgUnit = unitDao.LoadById(model.Id);
                    orgUnit.MembersCount = model.MembersCount;
                    orgUnit.Number = model.Number;
                    orgUnit.PercentIdealParts = model.PercentIdealParts;
                    orgUnit.Deactivated = model.Deactivated;
                    orgUnit.FamilyName = model.FamilyName;
                    orgUnit.Floor = model.Floor;

                    unitDao.SaveOrUpdate(orgUnit);

                    return RedirectToAction("Index");
                }
                else if (collection["button"] == "Добави")
                {
                    int serviceId = Int32.Parse(collection["Service"]);
                    
                    var service = serviceDao.LoadById(serviceId);
                    if (service != null)
                    {
                        Unit unit = unitDao.LoadById(model.Id);
                        if (!unit.NoChargeServices.Contains(service))
                        {
                            unit.NoChargeServices.Add(service);
                            unitDao.SaveOrUpdate(unit);
                        }
                    }
                }

                return RedirectToAction("Edit", new { id = model.Id});
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Unit/Delete/5

        public ActionResult Delete(Int64 id)
        {
            UnitDao unitDao = new UnitDao(CurrentSession);
            var selectedUnit = unitDao.LoadById(id);
            if (selectedUnit != null)
            {
                selectedUnit.IsDeleted = true;
                unitDao.SaveOrUpdate(selectedUnit);
            }

            return RedirectToAction("Index");
        }

        //
        // POST: /Unit/Delete/5

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
