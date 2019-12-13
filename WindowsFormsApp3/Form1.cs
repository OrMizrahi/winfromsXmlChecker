using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public static DataSet DataSet { get; set; } = new DataSet();

        public Form1()
        {
            InitializeComponent();
        }

        /*
        public DataTable CreateDataTable(string fileName, string[] columns, string tableName)
        {
            var doc = XDocument.Load(fileName);

            var dt = new DataTable();

            foreach (var column in columns) dt.Columns.Add(column);

            var list = doc.Descendants(tableName).Select(desc => desc.Attributes().Select(attr => attr.Value)
                .ToArray<object>()).ToList();

            foreach (var item in list)
            {
                var row = dt.NewRow();
                row.ItemArray = item;

                dt.Rows.Add(row);
            }

            return dt;
        }
        */

        private void button1_Click_1(object sender, EventArgs e)
        {
            label1.Visible = false;
            tabControl1.Visible = false;
            tabControl2.Visible = false;
            tabControl3.Visible = false;

            var fd = new OpenFileDialog {Filter = "XML|*.xml"};

            if (fd.ShowDialog() != DialogResult.OK) return;

            var fileName = fd.FileName;

            DataSet.ReadXml(fileName);

            if (fileName.Contains("CIF"))
            {
                RemoveEmptyTables(new[] {"SubjectData", "Contract", "ContractData", "Link"});
                ShowAndPopulateTab(tabControl1);
            }

            else if (fileName.Contains("CMF"))
            {
                RemoveEmptyTables(new [] {"Delete","Link","SubjectData"});
                ShowAndPopulateTab(tabControl2);
            }
            else if (fileName.Contains("IMF"))
            {
                RemoveEmptyTables(new [] {"SubjectData","Immediate","ImmediateData","Link"});
                ShowAndPopulateTab(tabControl3);
            }
            else
            {
                MessageBox.Show("Error! wrong xml file selected");
            }

            AddTextToLabel(fileName);
        }

        private static void ShowAndPopulateTab(TabControl tabId)
        {
            var i = 0;

            foreach (TabPage tp in tabId.TabPages)
            {
                var dgv = new DataGridView
                {
                    DataSource = DataSet.Tables[i],
                    AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                    Height = 450,
                    Width = 1200,
                    ForeColor = Color.DarkBlue,
                    GridColor = Color.DarkGray,
                    BorderStyle = BorderStyle.Fixed3D,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                    ScrollBars = ScrollBars.Both
                };

                tp.Controls.Add(dgv);
                i++;

                if (i >= DataSet.Tables.Count
                ) //stops if we try to add a non existent table to a tabPage, for example we have 10 tables and 11 tabPage
                    break;
            }

            //only after the whole tab is ready, we display it
            tabId.Visible = true;
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

        private void AddTextToLabel(string fileName)
        {
            label1.Text = "Presented File Type Is : ";
            label1.Visible = true;

            if (fileName.Contains("CIF"))
                label1.Text += "CIF";
            else if (fileName.Contains("CMF"))
                label1.Text += "CMF";
            else if (fileName.Contains("IMF"))
                label1.Text += "IMF";
            else
                label1.Text += "Unsupported File";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Hide();
            tabControl2.Hide();
            tabControl3.Hide();
        }
    }
}