using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyManagement.Core.Dao;
using PropertyManagement.Web.Models;
using PropertyManagement.Web.Helpers;
using PropertyManagement.Core.Entities;
using System.Text;

namespace PropertyManagement.Web.Controllers
{
    public class IncomesController : SessionController
    {
        //
        // GET: /Incomes/

        public ActionResult Index(Int64? detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);

            IList<IncomesModel> incomes = new List<IncomesModel>();

            if (detectionId == null)
            {
                detectionId = detectionDao.GetLastDetectionId();
            }
            var detections = detectionDao.GetAll();
            ViewBag.DetectionsList = new SelectList(MonthsYears.GetComboDetections(detections), "Id", "Name", detectionId);

            incomes = GetIncomes(detectionId);

            ViewBag.DetectionId = detectionId;

            return View(incomes);
        }

        private decimal GetRealPayedSum(Int64 detectionId, Int64 unitId)
        {
            IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);

            if (incomePaymentDao.GetIncomePaymentByUnitAndDetection(unitId, detectionId) != null)
            {
                var payedSum = incomePaymentDao.GetIncomePaymentByUnitAndDetection(unitId, detectionId).Sum(x => x.PayedSum);
                return payedSum;
            }
            else
            {
                return 0;
            }
        }

        [HttpPost]
        public ActionResult SaveIncomes(IList<IncomesModel> model, FormCollection collection)
        {
           UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
           DetectionDao detectionDao = new DetectionDao(CurrentSession);
           IncomePaymentDao incomePaymentDao = new IncomePaymentDao(CurrentSession);
           Detection detection = detectionDao.LoadById(Int64.Parse(collection["detectionid"].ToString()));
           ResMoneyDao resMoneyDao = new ResMoneyDao(CurrentSession);
           foreach(var m in model)
           {
               if (m.VneseniPari > 0)
               {
                    unitChargeDao.PayExpenses(m.VneseniPari, m.Unit.Id, detection);
                    resMoneyDao.CreateNew(detection, m.VneseniPari, m.Unit);
               }
               //else if (m.VneseniPari < 0)
               //{
               //    unitChargeDao.UnpayExpence(m.VneseniPari, m.Unit.Id, detection);
               //    resMoneyDao.CreateNew(detection, m.VneseniPari, m.Unit);
               //}

               //var payments = incomePaymentDao.GetIncomePaymentByUnitAndDetection((m.Unit.Id), (Int64)detection.Id);

               //if (payments.Count == 0)
               //{
               //    if (m.RealPayedSum > 0)
               //    {
               //        unitChargeDao.PayExpenses(m.RealPayedSum, m.Unit.Id, detection);
               //    }
               //}
               //else
               //{
               //    decimal difference = 0;
               //    decimal allPayedSum = GetRealPayedSum(detection.Id, m.Unit.Id); 
               //    decimal currentPayedSum = m.RealPayedSum;

               //    if(allPayedSum > currentPayedSum)
               //    {
               //        difference = currentPayedSum - allPayedSum;
               //    }
               //    else if(currentPayedSum > allPayedSum)
               //    {
               //        difference = currentPayedSum - allPayedSum;
               //    }

               //    if (difference > 0)
               //    {
               //        unitChargeDao.PayExpenses(difference, m.Unit.Id, detection);
               //    }
               //    else if(difference < 0)
               //    {
               //        unitChargeDao.UnpayExpence(difference, m.Unit.Id, detection);
               //    }
               //}
           }

           return RedirectToAction("Index", new { detectionId = detection.Id });
        }

        public ActionResult FullIncomes(Int64? detectionId)
        {
            var model = GetIncomesModel(detectionId, false);
            ViewBag.DetectionId = detectionId;
            return View((object)model);
        }

        private IList<IncomesModel> GetIncomes(Int64? detectionId)
        {
            DetectionDao detectionDao = new DetectionDao(CurrentSession);
            UnitChargeDao unitChargeDao = new UnitChargeDao(CurrentSession);
            ResMoneyDao resMoneyDao = new ResMoneyDao(CurrentSession);
            var detection = detectionDao.LoadById((Int64)detectionId);

            var unitChargesForDetection = unitChargeDao.GetAllByDetection((Int64)detectionId);
            var incomes = (from c in unitChargesForDetection
                       group c by c.Unit into dataGroup
                       select new IncomesModel
                       {
                           Unit = dataGroup.Key,
                           PayedSum = dataGroup.Select(x => x.PayedSum).Sum(),
                           RealPayedSum = GetRealPayedSum((Int64)detectionId, dataGroup.Key.Id),
                           SumToPay = dataGroup.Select(x => x.SumToPay).Sum(),
                           PrevSaldo = unitChargeDao.GetPreviousSaldo(dataGroup.Key.Id, detection),
                           DetectionId = dataGroup.Select(x => x.Expense.Detection.Id).FirstOrDefault(),
                           UnitCharges = unitChargeDao.GetAllByUnitAndDetection(dataGroup.Key.Id, detection.Id),
                           ResMoney = resMoneyDao.GetSumByUnitAndDetection(dataGroup.Key.Id, detection.Id)
                       }).ToList();

            return incomes;
        }

        private string GetIncomesModel(Int64? detectionId, bool forPrint)
        {
            IList<IncomesModel> incomes = new List<IncomesModel>();

            incomes = GetIncomes(detectionId);

            var model = GenerateTable(incomes, forPrint);
            ViewBag.Month = MonthsYears.GetMonth(incomes[0].UnitCharges[0].Expense.Detection.Month) + " " + incomes[0].UnitCharges[0].Expense.Detection.Year;
            ViewBag.Incomes = incomes;

            return model;
        }

        protected String GenerateTable(IList<IncomesModel> incomes, bool forPrint)
        {
            IList<Service> services = new List<Service>();
            foreach (var item in incomes)
            {
                foreach (var charge in item.UnitCharges)
                {
                    if (!services.Contains(charge.Expense.Service))
                    {
                        services.Add(charge.Expense.Service);
                    }
                }
            }

            services = services.OrderBy(x => x.Name).ToList();

            StringBuilder sb = new StringBuilder();

            string thTd = "";

            string printHeight = "";

            if (forPrint)
            {
                sb.Append("<table cellpadding=\"10px\" id=\"table_incomes_full_print\" class=\"full_payment_print\">");
                thTd = "td";

                printHeight = "height=\"55\"";
            }
            else
            {
                sb.Append("<table cellpadding=\"7px\" id=\"table_incomes_full\">");
                thTd = "th";
            }

            sb.Append("<tr>");



            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Ап.№");
            sb.Append("</" + thTd + ">");

            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Собственик");
            sb.Append("</" + thTd + ">");

            foreach (var service in services)
            {
                sb.Append("<" + thTd + " class=\"bold\">");
                sb.Append(service.Name);
                sb.Append("</" + thTd + ">");
            }

            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Дължима сума за месеца");
            sb.Append("</" + thTd + " class=\"bold\">");

            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Задължения от предишни месеци");
            sb.Append("</" + thTd + ">");

            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Внесени плащания за месеца (в брой)");
            sb.Append("</" + thTd + ">");

            sb.Append("<" + thTd + " class=\"bold\">");
            sb.Append("Общо за плащане");
            sb.Append("</" + thTd + ">");

 
            if (forPrint)
            {
                sb.Append("<" + thTd + " class=\"bold\" align=\"center\">");
                sb.Append("Получена сума (в брой)");
                sb.Append("</" + thTd + ">");

                sb.Append("<" + thTd + " class=\"bold\" align=\"center\">");
                sb.Append("Подпис");
                sb.Append("</" + thTd + ">");
            }

            sb.Append("<tr>");

           
            int i = 0;
            foreach (var charge in incomes)
            {
                string _class="";
                if (i % 2 == 0)
                {
                    _class = "normal";
                }
                else
                {
                    _class = "alterniting";
                }

                sb.Append("<tr class=\""+ _class+ "\">");

                sb.Append("<td " + printHeight + ">");
                sb.Append(charge.Unit.Number);
                sb.Append("</td>");

                sb.Append("<td class=\"bold\" " + printHeight + ">");
                sb.Append(charge.Unit.FamilyName);
                sb.Append("</td>");

                foreach (var item in services)
                {

                    var hasCharge = (from c in charge.UnitCharges
                                        where c.Expense.Service.Id == item.Id
                                        select c).FirstOrDefault();

                    if (hasCharge == null)
                    {
                        sb.Append("<td " + printHeight + ">");
                        sb.Append("0.00 €");
                        sb.Append("</td>");
                    }
                    else
                    {
                        sb.Append("<td " + printHeight + ">");
                        sb.Append(hasCharge.SumToPay.ToString("0.00") + " €");
                        sb.Append("</td>");
                    }
                   
                }

                sb.Append("<td class=\"bold\" " + printHeight + ">");
                sb.Append("<div>");
                sb.Append(charge.SumToPay.ToString("0.00") + " €");
                sb.Append("</div>");
                if (forPrint && ShowBGNEquivalent)
                {
                    sb.Append("<div>");
                    sb.Append(PMHelper.ConvertToBGN(charge.SumToPay));
                    sb.Append("</div>");
                }
                sb.Append("</td>");

                sb.Append("<td " + printHeight + ">");
                sb.Append(charge.PrevSaldo.ToString("0.00") + " €");
                sb.Append("</td>");


                sb.Append("<td " + printHeight + ">");
                sb.Append(charge.ResMoney.ToString("0.00") + " €");
                sb.Append("</td>");


                sb.Append("<td class=\"bold\" " + printHeight + ">");
                sb.Append("<div>");
                sb.Append(charge.GrandTotal.ToString("0.00") + " €");
                sb.Append("</div>");
                if (forPrint && ShowBGNEquivalent)
                {
                    sb.Append("<div>");
                    sb.Append(PMHelper.ConvertToBGN(charge.GrandTotal));
                    sb.Append("</div>");
                }
                sb.Append("</td>");


                //sb.Append("<td>");
                //if (!forPrint)
                //{
                //    sb.Append(charge.RealPayedSum.ToString("0.00") + " €");
                //}
                //else
                //{
                //    if (charge.RealPayedSum != 0)
                //    {
                //        sb.Append(charge.RealPayedSum.ToString("0.00") + " €");
                //    }
                //    else
                //    {
                //        sb.Append("&nbsp;");
                //    }
                //}
                //sb.Append("</td>");

                

                if (forPrint)
                {
                    sb.Append("<td class=\"info_col\" " + printHeight + ">");
                    sb.Append("&nbsp;");
                    sb.Append("</td>");

                    sb.Append("<td class=\"info_col\" " + printHeight + ">");
                    sb.Append("&nbsp;");
                    sb.Append("</td>");
                }

                sb.Append("</tr>");
                i++;
            }

            sb.Append("<tr>");

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append("</td>");

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append("</td>");


            foreach (var item in services)
            {

                 var chargesToSum = (from c in incomes
                                     from d in c.UnitCharges
                                     where d.Expense.Service.Id == item.Id
                                     select d).ToList();

                 sb.Append("<td class=\"full_incomes_footer\">");
                 sb.Append(chargesToSum.Sum(x => x.SumToPay).ToString("0.00") + " €");
                 sb.Append("</td>");

            }

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append("<div>");
            sb.Append(incomes.Sum(x => x.SumToPay).ToString("0.00") + " €");
            sb.Append("</div>");
            if (forPrint && ShowBGNEquivalent)
            {
                sb.Append("<div>");
                sb.Append(PMHelper.ConvertToBGN(incomes.Sum(x => x.SumToPay)));
                sb.Append("</div>");
            }
            sb.Append("</td>");

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append(incomes.Sum(x => x.PrevSaldo).ToString("0.00") + " €");
            sb.Append("</td>");

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append(incomes.Sum(x => x.ResMoney).ToString("0.00") + " €");
            sb.Append("</td>");

            sb.Append("<td class=\"full_incomes_footer\">");
            sb.Append("<div>");
            sb.Append(incomes.Sum(x => x.GrandTotal).ToString("0.00") + " €");
            sb.Append("</div>");
            if (forPrint && ShowBGNEquivalent)
            {
                sb.Append("<div>");
                sb.Append(PMHelper.ConvertToBGN(incomes.Sum(x => x.GrandTotal)));
                sb.Append("</div>");
            }
            sb.Append("</td>");


            if (forPrint)
            {
                sb.Append("<td class=\"full_incomes_footer\">");
                sb.Append("&nbsp;");
                sb.Append("</td>");

                sb.Append("<td class=\"full_incomes_footer\">");
                sb.Append("&nbsp;");
                sb.Append("</td>");
            }

            sb.Append("</tr>");

            sb.Append("</table>");

            return sb.ToString();

        }

        public ActionResult PrintFullIncomes(Int64 detectionId)
        {
            var model = GetIncomesModel(detectionId, true);
            var detection = CurrentSession.Get<Detection>(detectionId);
            ViewBag.Month = MonthsYears.GetMonth(detection.Month) + " " + detection.Year + " г.";
            return View((object)model);
        }

        public ActionResult PrintShortIncomes(Int64 detectionId)
        {
            var incomes = GetIncomes(detectionId);
            var detection = CurrentSession.Get<Detection>(detectionId);
            ViewBag.Month = MonthsYears.GetMonth(detection.Month) + " " + detection.Year + " г.";
            ViewBag.ShowBGNEquivalent = ShowBGNEquivalent;
            return View(incomes);
        }
    }
}
