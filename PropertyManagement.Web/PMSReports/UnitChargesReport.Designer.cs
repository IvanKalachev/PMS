//namespace PropertyManagement.Web.PMSReports
//{
//    partial class UnitChargesReport
//    {
//        #region Component Designer generated code
//        /// <summary>
//        /// Required method for telerik Reporting designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            Telerik.Reporting.TableGroup tableGroup1 = new Telerik.Reporting.TableGroup();
//            Telerik.Reporting.TableGroup tableGroup2 = new Telerik.Reporting.TableGroup();
//            Telerik.Reporting.TableGroup tableGroup3 = new Telerik.Reporting.TableGroup();
//            Telerik.Reporting.TableGroup tableGroup4 = new Telerik.Reporting.TableGroup();
//            Telerik.Reporting.TableGroup tableGroup5 = new Telerik.Reporting.TableGroup();
//            this.detail = new Telerik.Reporting.DetailSection();
//            this.table1 = new Telerik.Reporting.Table();
//            this.textBox2 = new Telerik.Reporting.TextBox();
//            this.textBox1 = new Telerik.Reporting.TextBox();
//            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
//            // 
//            // detail
//            // 
//            this.detail.Height = new Telerik.Reporting.Drawing.Unit(8.3000001907348633D, Telerik.Reporting.Drawing.UnitType.Cm);
//            this.detail.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
//            this.table1});
//            this.detail.Name = "detail";
//            // 
//            // table1
//            // 
//            this.table1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(new Telerik.Reporting.Drawing.Unit(5.1666669845581055D, Telerik.Reporting.Drawing.UnitType.Cm)));
//            this.table1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(new Telerik.Reporting.Drawing.Unit(5.1666669845581055D, Telerik.Reporting.Drawing.UnitType.Cm)));
//            this.table1.Body.Columns.Add(new Telerik.Reporting.TableBodyColumn(new Telerik.Reporting.Drawing.Unit(5.1666669845581055D, Telerik.Reporting.Drawing.UnitType.Cm)));
//            this.table1.Body.Rows.Add(new Telerik.Reporting.TableBodyRow(new Telerik.Reporting.Drawing.Unit(0.84999996423721313D, Telerik.Reporting.Drawing.UnitType.Cm)));
//            this.table1.Body.SetCellContent(0, 0, this.textBox1, 1, 3);
//            tableGroup2.Name = "Group1";
//            tableGroup1.ChildGroups.Add(tableGroup2);
//            tableGroup1.ChildGroups.Add(tableGroup3);
//            tableGroup1.ChildGroups.Add(tableGroup4);
//            tableGroup1.ReportItem = this.textBox2;
//            this.table1.ColumnGroups.Add(tableGroup1);
//            this.table1.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
//            this.textBox2,
//            this.textBox1});
//            this.table1.Location = new Telerik.Reporting.Drawing.PointU(new Telerik.Reporting.Drawing.Unit(0.70000004768371582D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(0.49999994039535522D, Telerik.Reporting.Drawing.UnitType.Cm));
//            this.table1.Name = "table1";
//            tableGroup5.Grouping.AddRange(new Telerik.Reporting.Data.Grouping[] {
//            new Telerik.Reporting.Data.Grouping("")});
//            tableGroup5.Name = "DetailGroup";
//            this.table1.RowGroups.Add(tableGroup5);
//            this.table1.Size = new Telerik.Reporting.Drawing.SizeU(new Telerik.Reporting.Drawing.Unit(15.5D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(1.6999999284744263D, Telerik.Reporting.Drawing.UnitType.Cm));
//            // 
//            // textBox2
//            // 
//            this.textBox2.Location = new Telerik.Reporting.Drawing.PointU(new Telerik.Reporting.Drawing.Unit(0.099999949336051941D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(0.10000015050172806D, Telerik.Reporting.Drawing.UnitType.Cm));
//            this.textBox2.Name = "textBox2";
//            this.textBox2.Size = new Telerik.Reporting.Drawing.SizeU(new Telerik.Reporting.Drawing.Unit(15.500000953674316D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(0.84999996423721313D, Telerik.Reporting.Drawing.UnitType.Cm));
//            this.textBox2.Style.Font.Bold = true;
//            this.textBox2.Style.Font.Size = new Telerik.Reporting.Drawing.Unit(12D, Telerik.Reporting.Drawing.UnitType.Point);
//            this.textBox2.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
//            this.textBox2.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
//            this.textBox2.StyleName = "";
//            this.textBox2.Value = "textBox2";
//            // 
//            // textBox1
//            // 
//            this.textBox1.Location = new Telerik.Reporting.Drawing.PointU(new Telerik.Reporting.Drawing.Unit(0.099999949336051941D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(0.90000015497207642D, Telerik.Reporting.Drawing.UnitType.Cm));
//            this.textBox1.Name = "textBox1";
//            this.textBox1.Size = new Telerik.Reporting.Drawing.SizeU(new Telerik.Reporting.Drawing.Unit(15.500000953674316D, Telerik.Reporting.Drawing.UnitType.Cm), new Telerik.Reporting.Drawing.Unit(0.84999996423721313D, Telerik.Reporting.Drawing.UnitType.Cm));
//            this.textBox1.Style.Font.Bold = false;
//            this.textBox1.Style.Font.Size = new Telerik.Reporting.Drawing.Unit(11D, Telerik.Reporting.Drawing.UnitType.Point);
//            this.textBox1.Style.TextAlign = Telerik.Reporting.Drawing.HorizontalAlign.Center;
//            this.textBox1.Style.VerticalAlign = Telerik.Reporting.Drawing.VerticalAlign.Middle;
//            this.textBox1.StyleName = "";
//            this.textBox1.Value = "textBox1";
//            // 
//            // UnitChargesReport
//            // 
//            this.Items.AddRange(new Telerik.Reporting.ReportItemBase[] {
//            this.detail});
//            this.PageSettings.Landscape = false;
//            this.PageSettings.Margins.Bottom = new Telerik.Reporting.Drawing.Unit(2.5399999618530273D, Telerik.Reporting.Drawing.UnitType.Cm);
//            this.PageSettings.Margins.Left = new Telerik.Reporting.Drawing.Unit(2.5399999618530273D, Telerik.Reporting.Drawing.UnitType.Cm);
//            this.PageSettings.Margins.Right = new Telerik.Reporting.Drawing.Unit(2.5399999618530273D, Telerik.Reporting.Drawing.UnitType.Cm);
//            this.PageSettings.Margins.Top = new Telerik.Reporting.Drawing.Unit(2.5399999618530273D, Telerik.Reporting.Drawing.UnitType.Cm);
//            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
//            this.Style.BackgroundColor = System.Drawing.Color.White;
//            this.Width = new Telerik.Reporting.Drawing.Unit(16.69999885559082D, Telerik.Reporting.Drawing.UnitType.Cm);
//            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

//        }
//        #endregion

//        private Telerik.Reporting.DetailSection detail;
//        private Telerik.Reporting.Table table1;
//        private Telerik.Reporting.TextBox textBox2;
//        private Telerik.Reporting.TextBox textBox1;
//    }
//}