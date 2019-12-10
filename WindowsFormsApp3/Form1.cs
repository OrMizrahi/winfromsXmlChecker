using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


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

        private void button1_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = "XML|*.xml";

            if (fd.ShowDialog() != DialogResult.OK) return;

            var fileName = fd.FileName;

            var dt1 = CreateDataTable(fileName,
                new[] {"NumberOfPartial", "DataSourceCode", "SubmissionMode", "FileReferenceDate", "SchemeVersion"},
                "Header");
            var dt2 = CreateDataTable(fileName, new[] {"DataSourceSubjectCode"}, "Subject");
            var dt3 = CreateDataTable(fileName, new[] {"StreetNo", "ZipCode", "Confirmed"}, "Address");


            headerDataGrid.DataSource = dt1;
            subjectDataGrid.DataSource = dt2;
            addressGridView.DataSource = dt3;
        }
    }
}