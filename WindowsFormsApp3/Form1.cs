using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public static DataSet DataSet { get; set; } = new DataSet();
        public static string FileName { get; set; } // cif/cmf/imf
        public static IList<string> ContractType { get; set; } = new List<string>(); //v2_loan v3_mortgage ...

        public Form1()
        {
            InitializeComponent();
        }


        private void Button1_Click_1(object sender, EventArgs e)
        {
            label1.Visible = false;
            tabControl1.Visible = false;
            richTextBox1.Visible = false;

            var fd = new OpenFileDialog {Filter = "XML|*.xml"};

            if (fd.ShowDialog() != DialogResult.OK) return;

            FileName = fd.FileName;

           DataSet.ReadXml(FileName);

            if (FileName.Contains("CIF"))
            {
                RemoveEmptyTables(new[] {"SubjectData", "Contract", "ContractData", "Link"});
                ShowAndPopulateTab();
            }

            else if (FileName.Contains("CMF"))
            {
                RemoveEmptyTables(new[] {"Delete", "Subject"});
                ShowAndPopulateTab();
            }
            else if (FileName.Contains("IMF"))
            {
                RemoveEmptyTables(new[] {"SubjectData", "Immediate", "ImmediateData", "Link"});
                ShowAndPopulateTab();
            }
            else
            {
                MessageBox.Show("Error! wrong xml file selected");
            }

            AddTextToLabel();
        }

        private void ShowAndPopulateTab()
        {
            //need to reset the TabControl so in next time we populate it it would be without tabs
            tabControl1.Controls.Clear();

            foreach (DataTable table in DataSet.Tables)
            {
                var dataGridView = CreateDataGridView(table);
                var tabPage = CreateTabPage(table.TableName, dataGridView);
                tabControl1.Controls.Add(tabPage);
                if (!table.TableName.Equals("V2_Loan") && !table.TableName.Equals("V3_Mortgage") &&
                    !table.TableName.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                ContractType.Add(table.TableName); //for the cif error only
            }

            //only after the whole tab is ready, we display it
            tabControl1.Visible = true;
            Check();
            //need to reset the dataSet from all data that it stores, so that in the next file the dataSet will be empty
            DataSet = new DataSet();
        }

        private static void RemoveEmptyTables(IEnumerable<string> tablesToRemove)
        {
            foreach (var tableToRemove in tablesToRemove)
            {
                var table = DataSet.Tables[tableToRemove];
                while (table.ChildRelations.Count > 0)
                {
                    var relation = table.ChildRelations[0];
                    DataSet.Tables[relation.ChildTable.TableName].Constraints.Remove(relation.RelationName);
                    DataSet.Relations.Remove(relation);
                }

                while (table.ParentRelations.Count > 0) DataSet.Relations.Remove(table.ParentRelations[0]);

                table.Constraints.Clear();

                DataSet.Tables.Remove(table);
                table.Dispose();
            }
        }

        private void AddTextToLabel()
        {
            label1.Text = "Presented File Type Is : ";
            label1.Visible = true;

            if (FileName.Contains("CIF"))
                label1.Text += "CIF";
            else if (FileName.Contains("CMF"))
                label1.Text += "CMF";
            else if (FileName.Contains("IMF"))
                label1.Text += "IMF";
            else
                label1.Text += "Unsupported File";
        }

        private static DataGridView CreateDataGridView(object dataToPresent)
        {
            return new DataGridView
            {
                //displays the table into the DataSource
                DataSource = dataToPresent,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                Height = 300,
                Width = 1200,
                ForeColor = Color.DarkBlue,
                GridColor = Color.DarkGray,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ScrollBars = ScrollBars.Both
            };
        }

        private static TabPage CreateTabPage(string tabPageName, Control dataGridViewToPresent)
        {
            var tabPage = new TabPage
            {
                Location = new Point(4, 29),
                Name = "tabPage1",
                Padding = new Padding(3),
                Size = new Size(1200, 450),
                Text = tabPageName,
                UseVisualStyleBackColor = true
            };
            //adding DataGrid to TabPage
            tabPage.Controls.Add(dataGridViewToPresent);
            return tabPage;
        }

        public void Check()
        {
            richTextBox1.ResetText();

            if (FileName.Contains("CIF"))
                Checker.CifList.ToList().ForEach(action => action(DataSet,richTextBox1,ContractType));
            else if (FileName.Contains("IMF"))
                Checker.ImfList.ToList().ForEach(action => action(DataSet,richTextBox1,ContractType));
            else
                Checker.CmfList.ToList().ForEach(action => action(DataSet,richTextBox1));

            richTextBox1.Visible = true;
        }

       

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Visible = false;
            tabControl1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Check("");
        }
    }
}