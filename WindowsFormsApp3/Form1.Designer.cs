namespace WindowsFormsApp3
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.headerDataGrid = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.subjectDataGrid = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addressGridView = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.headerDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.subjectDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.addressGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // headerDataGrid
            // 
            this.headerDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.headerDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.headerDataGrid.Location = new System.Drawing.Point(12, 112);
            this.headerDataGrid.Name = "headerDataGrid";
            this.headerDataGrid.RowHeadersWidth = 62;
            this.headerDataGrid.RowTemplate.Height = 28;
            this.headerDataGrid.Size = new System.Drawing.Size(863, 110);
            this.headerDataGrid.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.AllowDrop = true;
            this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button1.Location = new System.Drawing.Point(674, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 53);
            this.button1.TabIndex = 1;
            this.button1.Text = "load File";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // subjectDataGrid
            // 
            this.subjectDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.subjectDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.subjectDataGrid.Location = new System.Drawing.Point(981, 112);
            this.subjectDataGrid.Name = "subjectDataGrid";
            this.subjectDataGrid.RowHeadersWidth = 62;
            this.subjectDataGrid.RowTemplate.Height = 28;
            this.subjectDataGrid.Size = new System.Drawing.Size(262, 110);
            this.subjectDataGrid.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(390, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "header";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1096, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "subject";
            // 
            // addressGridView
            // 
            this.addressGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.addressGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.addressGridView.Location = new System.Drawing.Point(16, 257);
            this.addressGridView.Name = "addressGridView";
            this.addressGridView.RowHeadersWidth = 62;
            this.addressGridView.RowTemplate.Height = 28;
            this.addressGridView.Size = new System.Drawing.Size(433, 150);
            this.addressGridView.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(222, 229);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Address";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1499, 648);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.addressGridView);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.subjectDataGrid);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.headerDataGrid);
            this.Name = "Form1";
            this.Text = "BoiXmlFileChecker";
            ((System.ComponentModel.ISupportInitialize)(this.headerDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.subjectDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.addressGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView headerDataGrid;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView subjectDataGrid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView addressGridView;
        private System.Windows.Forms.Label label3;
    }
}

