using System;
using System.Data;
using System.Drawing;
using System.Linq;
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
            tabControl1.Hide();
            tabControl2.Hide();
            tabControl3.Hide();

            var fd = new OpenFileDialog {Filter = "XML|*.xml"};

            if (fd.ShowDialog() != DialogResult.OK) return;

            var fileName = fd.FileName;

            DataSet.ReadXml(fileName);

            if (fileName.Contains("CIF"))
                ShowAndPopulateTab(tabControl1);
            else if (fileName.Contains("CMF"))
                ShowAndPopulateTab(tabControl2);
            else if (fileName.Contains("IMF"))
                ShowAndPopulateTab(tabControl3);
            else
                MessageBox.Show("Error! wrong xml file selected");
        }

        private static void ShowAndPopulateTab(TabControl tabId)
        {
            var i = 0;
            MessageBox.Show(DataSet.Tables.Count.ToString());
            foreach (TabPage tp in tabId.TabPages)
            {
                var dgv = new DataGridView
                {
                    DataSource = DataSet.Tables[i],
                    AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllHeaders,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                    Height = 450,
                    Width = 1200,
                    ForeColor =  Color.DarkBlue,
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

            tabId.Show();
            DataSet = new DataSet();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Hide();
            tabControl2.Hide();
            tabControl3.Hide();
        }
    }
}