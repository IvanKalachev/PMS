namespace PropertyManagement.Web.PMSReports
{
    //using System;
    //using System.ComponentModel;
    //using System.Drawing;
    //using System.Windows.Forms;
    //using Telerik.Reporting;
    //using Telerik.Reporting.Drawing;
    //using PropertyManagement.Core.Dao;
    //using PropertyManagement.Web.Models;
    //using PropertyManagement.Core.Entities;
    //using NHibernate.Linq;
    //using System.Linq;
    //using System.Collections.Generic;
    //using PropertyManagement.Web.Helpers;

    /// <summary>
    /// Summary description for UnitChargesReport.
    /// </summary>
    //public partial class UnitChargesReport : Telerik.Reporting.Report
    //{
    //    private IList<UnitUnpaidChargesModel> Charges { get; set; }

    //    private Int64 UnitId { get; set; }

    //    public UnitChargesReport(Int64 unitId)
    //    {
    //        //
    //        // Required for telerik Reporting designer support
    //        //
    //        InitializeComponent();
    //        UnitId = unitId;
    //        InitReporting();
    //    }

    //    private void InitReporting()
    //    {
    //        GetUnpaidCharges();
    //        UnitDao unitDao = new UnitDao(MvcApplication.CurrentSession);
    //        var unit = unitDao.LoadById(UnitId);

    //        textBox2.Value = "Неплатени задължения - " + unit.FamilyName;

    //        Telerik.Reporting.TextBox textboxGroup;

    //        foreach (var charge in Charges)
    //        {
    //            textboxGroup = new Telerik.Reporting.TextBox();
    //            textboxGroup.Value = MonthsYears.GetMonth(charge.Detection.Month) + " " + charge.Detection.Year;
    //            textboxGroup.Size = new SizeU(Telerik.Reporting.Drawing.Unit.Inch(1.1), Telerik.Reporting.Drawing.Unit.Inch(0.3));
    //            this.detail.Items.Add(textboxGroup);
    //        }
    //    }

    //    private void GetUnpaidCharges()
    //    {
    //        UnitChargeDao dao = new UnitChargeDao(MvcApplication.CurrentSession);
    //        var unPaiedCharges = dao.GetAllUnpaiedForUnit(UnitId);

    //        Charges = (from c in unPaiedCharges
    //                       group c by c.Expense.Detection into dataGroup
    //                       select new UnitUnpaidChargesModel
    //                       {
    //                           Detection = dataGroup.Key,
    //                           Charges = (from d in unPaiedCharges
    //                                      where d.Expense.Detection.Id == dataGroup.Key.Id
    //                                      select d).ToList()
    //                       }).ToList();
    //    }
    //}
}