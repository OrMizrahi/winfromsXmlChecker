using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace WindowsFormsApp3
{
    public class Checker
    {

        // Contract checks ------------------------------------------------------------------------------------------------------------------
        public static void CheckNumberOfSubjects(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var rows = dataSet.Tables["ContractDetail"].Rows;
            foreach (DataRow row in rows)
            {
                var numOfSubjects = Convert.ToInt32(row.Field<string>("NumberOfSubjects"));
                if (numOfSubjects < 0)
                    richTextBox.AppendText(
                        $"error at ContractDetail ! numOfSubjects can't be negative - found at row number {rows.IndexOf(row)} \n");
            }
        }

        public static void CheckActualEndDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b3 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = false;
                    DateTime actualEndDate = default;

                    if (row.Table.Columns.Contains("ActualEndDate"))
                        b1 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out actualEndDate);

                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var status = row.Field<string>("Status");

                    if (b1 && b2 && actualEndDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! ActualEndDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && b3 && actualEndDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! ActualEndDate can't be less than ReferenceDate - found at row number {rows.IndexOf(row)} \n");

                    if ((status == "T" || status == "B") && b1 == false)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status in (T or B) must have ActualEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (status != "T" && status != "B" && b1)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status not in ( T or B) must not have ActualEndDate - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckArrears(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                if (contractType == "V4_UnutilizedMortgageBalance")
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var arrearsExists = row.Table.Columns.Contains("Arrears");
                    var arrears = "";
                    if (arrearsExists)
                    {
                         arrears = row.Field<string>("Arrears");
                    }
                    
                    var b3 = long.TryParse(row.Field<string>("PastDueAmount"), out var pastDueAmount);
                    var status = row.Field<string>("Status");

                    var firstDefaultDateExists = row.Table.Columns.Contains("FirstDefaultDate");
                    var b2 = true;
                    var firstDefaultDate = new DateTime();
                    if (firstDefaultDateExists)
                    {
                         b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out firstDefaultDate);
                    }

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b1 && b2 && firstDefaultDateExists && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days - found at row number {rows.IndexOf(row)} \n");
                        if (pastDueAmount <= 200 || b3 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 and PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");
                        if (!b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");
                        if (arrearsExists && string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without arrears  - found at row number {rows.IndexOf(row)} \n");
                    }

                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with firstDefaultValue  - found at row number {rows.IndexOf(row)} \n");
                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with arrears  - found at row number {rows.IndexOf(row)} \n");
                        if (b3 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with PastDueAmount greater than 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckActualPaidAmount(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("ActualPaidAmount"), out var actualPaidAmount);

                    if (b1 && actualPaidAmount < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! ActualPaidAmount can't be negative - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckCollateralCaseDetail(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("CollateralCase") || !dataSet.Tables.Contains("CollateralCaseDetail"))
                return;

            var rows = dataSet.Tables["CollateralCaseDetail"].Rows;
            foreach (DataRow row in rows)
            {
                var b1 = long.TryParse(row.Field<string>("Amount"), out var amount);
                var type = row.Field<string>("Type");

                if (b1 && amount < 0)
                    richTextBox.AppendText(
                        $"error at CollateralCaseDetail ! Amount can't be empty/negative  - found at row number {rows.IndexOf(row)} \n");

                if (b1 == false || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at CollateralCaseDetail ! Amount is empty/negative or Type is empty - found at row number {rows.IndexOf(row)} \n");
            }
        }

        public static void CheckCountryNumberName(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            //add more rules about designation!!!
            if (!dataSet.Tables.Contains("CompanyInformation"))
                return;

            var rows = dataSet.Tables["CompanyInformation"].Rows;
            foreach (DataRow row in rows)
            {
                var country = row.Field<string>("Country");
                var number = row.Field<string>("Number");
                var name = row.Field<string>("Name");

                if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(number) || string.IsNullOrEmpty(name))
                    richTextBox.AppendText(
                        $"error at CompanyInformation ! country/number/name is/are null - found at row number {rows.IndexOf(row)} \n");
            }
        }

        public static void CheckCreditLimit(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType != "V4_UnutilizedMortgageBalance")
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("CreditLimit"), out var creditLimit);
                    var b2 = int.TryParse(row.Field<string>("Designation"), out _);

                    if (b1 && creditLimit < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! creditLimit can't be negative - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && creditLimit > 5000 && b2 == false)
                        richTextBox.AppendText(
                            $"error at {contractType} ! if CreditLimit > 5000, there must be a designation - found at row number {rows.IndexOf(row)} \n");

                    else if (b1 && creditLimit <= 5000 && b2)
                    {
                        row.SetField("Designation", "");
                        richTextBox.AppendText(
                            $"error at {contractType} ! if CreditLimit <= 5000, there must not be a designation , designation cleared at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckCurrentBalance(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("CurrentBalance"), out var currentBalance);

                    if (b1 && currentBalance < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! currentBalance can't be negative - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckFirstDefaultDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var arrearsExists = row.Table.Columns.Contains("Arrears");
                    var arrears = "";
                    if (arrearsExists)
                    {
                        arrears = row.Field<string>("Arrears");
                    }

                    var status = row.Field<string>("Status");
                    var b4 = long.TryParse(row.Field<string>("PastDueAmount"), out var pastDueAmount);
                    var firstDefaultDateExists = row.Table.Columns.Contains("FirstDefaultDate");
                    var b2 = true;
                    var firstDefaultDate = new DateTime();
                    if (firstDefaultDateExists)
                    {
                        b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out firstDefaultDate);
                    }
                    var b3 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    if (b1 && b2 && b3 && firstDefaultDateExists && (firstDefaultDate > referenceDate || firstDefaultDate < startingDate))
                        richTextBox.AppendText(
                            $"error at {contractType} ! firstDefaultDate can't be bigger than referenceDate/ firstDefaultDate can't be less than startingDate   - found at row number {rows.IndexOf(row)} \n");

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b1 && b2 && firstDefaultDateExists && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days - found at row number {rows.IndexOf(row)} \n");

                        if (pastDueAmount <= 200 || b4 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");

                        if (b2 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} !  status in (A or A1) must have firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");

                        if (arrearsExists && string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (A or A1) must have arrears  - found at row number {rows.IndexOf(row)} \n");
                    }

                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have firstDefaultValue  - found at row number {rows.IndexOf(row)} \n");

                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have arrears  - found at row number {rows.IndexOf(row)} \n");

                        if (b4 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have PastDueAmount greater than 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckFirstDeferredPaymentDate(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType == "V4_UnutilizedMortgageBalance")
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = false;
                    DateTime firstDeferredPaymentDate = default;

                    if (row.Table.Columns.Contains("FirstDeferredPaymentDate"))
                        b1 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"),
                            out firstDeferredPaymentDate);

                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    var plannedEndDateExists = row.Table.Columns.Contains("PlannedEndDate");
                    var b3 = true;
                    var plannedEndDate = new DateTime();
                    if (plannedEndDateExists)
                    {
                        b3 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out plannedEndDate);
                    }
                    var b4 = int.TryParse(row.Field<string>("PaymentFrequency"), out var paymentFrequency);

                    if ((b1 && b2 && firstDeferredPaymentDate < startingDate) ||
                        (b1 && b3 && plannedEndDateExists && firstDeferredPaymentDate > plannedEndDate))
                        richTextBox.AppendText(
                            $"error at {contractType} !firstDeferredPaymentDate can't be less than startingDate or more than plannedEndDate - found at row number {rows.IndexOf(row)} \n");

                    if ((b1 && b4 && paymentFrequency != 11) || (!b1 && b4 && paymentFrequency == 11))
                        richTextBox.AppendText(
                            $"error at {contractType} ! FirstDeferredPaymentDate can only exist with PaymentFrequency = 11 ,  and otherwise! - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckAmountAndType(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("Forgiveness"))
                return;

            var rows = dataSet.Tables["Forgiveness"].Rows;

            foreach (DataRow row in rows)
            {
                //need to check only if v2 or v3
                var amount = row.Field<string>("Amount");
                var doesAmountExists = !string.IsNullOrEmpty(row.Field<string>("Amount"));
                var type = row.Field<string>("Type");

                if (doesAmountExists && Convert.ToInt64(amount) < 0)
                    richTextBox.AppendText(
                        $"error at Forgiveness ! amount can't be negative - found at row number {rows.IndexOf(row)} \n");
                if (!doesAmountExists || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at Forgiveness ! amount and/or type can't be empty- found at row number {rows.IndexOf(row)} \n");
            }
        }

        /*
        public static void CheckAnnualizedInterest(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {

            if (!dataSet.Tables.Contains("InterestSchema"))
                return;

            var rows = dataSet.Tables["InterestSchema"].Rows;

            var b5 = dataSet.Tables["V2_Loan"].ChildRelations.Contains("V2_Loan_CompanyInformation");
            var b6 = dataSet.Tables["V2_Loan"].ChildRelations.Contains("V3_Mortgage_CompanyInformation");
            var b7 = dataSet.Tables["V2_Loan"].ChildRelations.Contains("V4_UnutilizedMortgageBalance_CompanyInformation");

            foreach (DataRow row in rows)
            {//need to check only if v2 or v3
                var b1 = double.TryParse(row.Field<string>("AnnualizedInterest"), out var annualizedInterestExists);
                var b2 = row.Table.ParentRelations.Contains("V2_Loan_InterestSchema");
                var b3 = row.Table.ParentRelations.Contains("V3_Mortgage_InterestSchema");
                var b4 = row.Table.ParentRelations.Contains("V4_UnutilizedMortgageBalance_InterestSchema");
               

                if ((b2 || b3) && b1 && annualizedInterestExists < 0)
                    richTextBox.AppendText($"error at Interest Schema ! AnnualizedInterest can't be negative - found at row number {rows.IndexOf(row)} \n");

                if (b2 && b5)
                {
                    richTextBox.AppendText($"error at Interest Schema ! Both InterestSchema and CompanyInformation can't exist on V2, InterestSchema cleared!- found at row number {rows.IndexOf(row)} \n");
                    row.Delete();
                }

                if (b3 && b6)
                {
                    richTextBox.AppendText($"error at Interest Schema ! Both InterestSchema and CompanyInformation can't exist on V3, InterestSchema cleared!- found at row number {rows.IndexOf(row)} \n");
                    row.Delete();
                }

                if (!b4 || !b7) continue;
                richTextBox.AppendText($"error at Interest Schema ! Both InterestSchema and CompanyInformation can't exist on V4, InterestSchema cleared!- found at row number {rows.IndexOf(row)} \n");
                row.Delete();

                if (b2 && row.Field<string>("Type") == "N")
                {

                }
            }
            
        }


        public static void CheckBase(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("InterestSchema"))
                return;

            var rows = dataSet.Tables["InterestSchema"].Rows;

            foreach (DataRow row in rows)
            {//need to check only if v2 or v3
                //var base1 = row.Field<string>("Base");
                
                //if (doesAnnualizedInterestExists && Convert.ToDecimal(annualizedInterestExists) < 0)
                  //  richTextBox.AppendText(
                    //    $"error at Interest Schema ! AnnualizedInterest can't be negative - found at row number {rows.IndexOf(row)} \n");
                  להמשיך את החוקים - לא מובן בכלל
                if (!doesAmountExists || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at Forgiveness ! amount and/or type can't be empty- found at row number {rows.IndexOf(row)} \n");
            }
        }*/


        public static void CheckDesignation(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;
                var b3 = dataSet.Tables[contractType].ChildRelations.Contains($"{contractType}_CompanyInformation");

                foreach (DataRow row in rows)
                {
                    var b1 = int.TryParse(row.Field<string>("Designation"), out var designation);

                    if (b1 && designation == 7)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Designation can't be 7 - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && designation == 11 && b3 == false)
                        richTextBox.AppendText(
                            $"error at {contractType} ! if Designation = 11 , there must be a CompanyInformation - found at row number {rows.IndexOf(row)} \n");

                    if (contractType == "V2_Loan" || contractType == "V3_Mortgage")
                    {
                        var b2 = long.TryParse(row.Field<string>("LoanUtilized"), out var loanUtilized);

                        if (b2 && loanUtilized > 5000 && b1 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! if LoanUtilized > 5000, there must be a designation - found at row number {rows.IndexOf(row)} \n");

                        if (!b2 || loanUtilized > 5000 || !b1)
                            continue;

                        richTextBox.AppendText(
                            $"error at {contractType} ! if LoanUtilized <= 5000, there must not be a designation, clearing Designation! - found at row number {rows.IndexOf(row)} \n");
                        row.SetField("Designation", "");
                    }

                    else
                    {
                        var b4 = long.TryParse(row.Field<string>("CreditLimit"), out var creditLimit);

                        if (b4 && creditLimit > 5000 && b1 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! if CreditLimit > 5000, there must be a designation - found at row number {rows.IndexOf(row)} \n");

                        if (b4 && creditLimit <= 5000 && b1)
                            richTextBox.AppendText(
                                $"error at {contractType} ! if CreditLimit <= 5000, there must not be a designation - found at row number {rows.IndexOf(row)} \n");
                    }

                }
            }
        }

        public static void CheckLastPaymentDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b3 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var lastPaymentDateExists = row.Table.Columns.Contains("LastPaymentDate");
                    var b1 = true;
                    var lastPaymentDate = new DateTime();
                    if (lastPaymentDateExists)
                    {
                        b1 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out lastPaymentDate);
                    }
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    if (b1 && b2 &&  lastPaymentDateExists && lastPaymentDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! LastPaymentDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && b3 && lastPaymentDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! LastPaymentDate can't be greater than ReferenceDate - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckLoanUtilized(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("LoanUtilized"), out var loanUtilized);
                    var b2 = int.TryParse(row.Field<string>("Designation"), out var designation);

                    if (b1 && loanUtilized < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! LoanUtilized can't be negative - found at row number {rows.IndexOf(row)} \n");

                    else if (b1 && loanUtilized > 5000 && b2 == false)
                        richTextBox.AppendText(
                            $"error at {contractType} ! if LoanUtilized > 5000, there must be a designation - found at row number {rows.IndexOf(row)} \n");

                    else if (b1 && loanUtilized <= 5000 && b2)
                    {
                        richTextBox.AppendText(
                            $"error at {contractType} ! if LoanUtilized <= 5000, there must not be a designation, clearing Designation! - found at row number {rows.IndexOf(row)} \n");
                        row.SetField("Designation", "");
                    }

                }
            }
        }


        public static void CheckPlannedMonthlyPayment(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("PlannedMonthlyPayment"), out var plannedMonthlyPayment);

                    var monthlyPaymentTypeExists = row.Table.Columns.Contains("MonthlyPaymentType");
                    var monthlyPaymentType = "";
                    if (monthlyPaymentTypeExists)
                    {
                        monthlyPaymentType = row.Field<string>("MonthlyPaymentType");
                    }
                    if (b1 && plannedMonthlyPayment < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedMonthlyPayment can't be negative - found at row number {rows.IndexOf(row)} \n");

                    if ((b1 == false || monthlyPaymentTypeExists && plannedMonthlyPayment != 0) && b1 || monthlyPaymentTypeExists && string.IsNullOrEmpty(monthlyPaymentType))
                        continue;

                    richTextBox.AppendText(
                        $"error at {contractType} ! MonthlyPaymentType must have PlannedMonthlyType > 0, MonthlyPaymentType cleared! - found at row number {rows.IndexOf(row)} \n");

                    if (monthlyPaymentTypeExists)
                        row.SetField("MonthlyPaymentType", "");

                }
            }
        }

        public static void CheckPastDueAmount(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b3 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("PastDueAmount"), out var pastDueAmount);
                    var status = row.Field<string>("Status");


                    var firstDefaultDateExists = row.Table.Columns.Contains("FirstDefaultDate");
                    var b2 = true;
                    var firstDefaultDate = new DateTime();
                    if (firstDefaultDateExists)
                    {
                        b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out firstDefaultDate);
                    }


                    var arrearsExists = row.Table.Columns.Contains("Arrears");
                    var arrears = "";
                    if (arrearsExists)
                    {
                        arrears = row.Field<string>("Arrears");
                    }

                    if (b1 && pastDueAmount < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PastDueAmount can't be negative  - found at row number {rows.IndexOf(row)} \n");

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b2 && b3 && firstDefaultDateExists && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days  - found at row number {rows.IndexOf(row)} \n");
                        if (b2 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (A or A1) must have firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");

                        if (arrearsExists && string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (A or A1) must have arrears  - found at row number {rows.IndexOf(row)} \n");

                        if (pastDueAmount <= 200 || !b1)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (A or A1) must have existing pastDueAmount > 2000   - found at row number {rows.IndexOf(row)} \n");
                    }

                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");

                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have arrears  - found at row number {rows.IndexOf(row)} \n");

                        if (b1 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status in (C or T) must not have/exist pastDueAmount > 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckPaymentFrequency(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = false;

                    if (row.Table.Columns.Contains("FirstDeferredPaymentDate"))
                        b1 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out _);

                    var b2 = int.TryParse(row.Field<string>("PaymentFrequency"), out var paymentFrequency);

                    var plannedBaloonDateExists = row.Table.Columns.Contains("PlannedBaloonDate");
                    var b3 = true;
                    if (plannedBaloonDateExists)
                    {
                        b3 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out _);
                    }

                    if ((b1 && b2 && paymentFrequency != 11) || (b1 == false && b2 && paymentFrequency == 11))
                        richTextBox.AppendText(
                            $"error at {contractType} ! FirstDeferredPaymentDate must only exist with PaymentFrequency = 11 ,  and otherwise!  - found at row number {rows.IndexOf(row)} \n");

                    if (b3 == false || b2 == false || paymentFrequency == 10)
                        continue;

                    richTextBox.AppendText(
                        $"error at {contractType} ! PlannedBaloonDate must only exist with PaymentFrequency = 10 , PlannedBaloonDate cleared!! - found at row number {rows.IndexOf(row)} \n");
                    if (plannedBaloonDateExists)
                        row.SetField("PlannedBaloonDate", "");

                }
            }
        }

        public static void CheckPaymentHistory(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var paymentHistoryExists = row.Table.Columns.Contains("PaymentHistory");
                    var paymentHistory = "";
                    if (paymentHistoryExists)
                    {
                        paymentHistory = row.Field<string>("Arrears");
                    }
                    //should check each character must belong to the PaymentHistoryDomain
                    if (paymentHistoryExists && paymentHistory.Length != 24)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PaymentHistory must be 24 characters long! - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckPlannedBaloonDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var plannedBaloonDateExists = row.Table.Columns.Contains("PlannedBaloonDate");
                    var b1 = true;
                    var plannedBaloonDate = new DateTime();
                    if (plannedBaloonDateExists)
                    {
                        b1 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out plannedBaloonDate);
                    }

                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    var plannedEndDateExists = row.Table.Columns.Contains("plannedEndDate");
                    var b3 = true;
                    var plannedEndDate = new DateTime();
                    if (plannedEndDateExists)
                    {
                        b3 = DateTime.TryParse(row.Field<string>("plannedEndDate"), out plannedEndDate);
                    }
                    var b4 = int.TryParse(row.Field<string>("PaymentFrequency"), out var paymentFrequency);

                    if (b1 && b2 && plannedBaloonDateExists && plannedBaloonDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && b3 && plannedBaloonDateExists && plannedEndDateExists && plannedBaloonDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 == false || b4 == false || paymentFrequency == 10)
                        continue;

                    richTextBox.AppendText(
                        $"error at {contractType} ! PlannedBaloonDate must only exist with paymentFrequency = 10 ,PlannedBaloonDate cleared! - found at row number {rows.IndexOf(row)} \n");
                    if(plannedBaloonDateExists)
                        row.SetField("PlannedBaloonDate", "");
                }
            }
        }

        public static void CheckPlannedEndDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var plannedEndDateExists = row.Table.Columns.Contains("PlannedEndDate");
                    var b1 = true;
                    DateTime plannedEndDate = default;
                    if (plannedEndDateExists)
                    {
                        b1 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out plannedEndDate);
                    }
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    if (b1 && b2 &&  plannedEndDateExists  &&  plannedEndDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedEndDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");

                    if (contractType == "V4_UnutilizedMortgageBalance")
                        goto REPEAT;


                    DateTime firstDeferredPaymentDate = default;
                    var b3 = false;
                    if (row.Table.Columns.Contains("FirstDeferredPaymentDate"))
                        b3 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"),
                            out firstDeferredPaymentDate);


                    DateTime plannedBaloonDate = default;
                    var b4 = false;
                    if (row.Table.Columns.Contains("PlannedBaloonDate"))
                        b4 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"),
                            out plannedBaloonDate);
                    

                    if (b3)
                    {
                        if (b2 && firstDeferredPaymentDate < startingDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDeferredPaymentDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");

                        if (b1 && firstDeferredPaymentDate > plannedEndDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDeferredPaymentDate can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    }

                    if (b4)
                    {
                        if (b2 && plannedBaloonDate < startingDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! PlannedBaloonDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
                        if (b1 && plannedBaloonDate > plannedEndDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! PlannedBaloonDate can't be more than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    }

                    REPEAT: ;
                }
            }
        }

        public static void CheckMonthlyPaymentType(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;

                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = long.TryParse(row.Field<string>("PlannedMonthlyPayment"), out var plannedMonthlyPayment);
                    var monthlyPaymentType = row.Field<string>("MonthlyPaymentType");

                    if ((!b1 || plannedMonthlyPayment != 0) && b1 || string.IsNullOrEmpty(monthlyPaymentType))
                        continue;

                    richTextBox.AppendText(
                        $"error at {contractType} ! MonthlyPaymentType must have plannedMonthlyPayment > 0, MonthlyPaymentType cleared! - found at row number {rows.IndexOf(row)} \n");
                    row.SetField("MonthlyPaymentType", "");

                }
            }
        }

        public static void CheckQs(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var statusesToBeChecked1 = new[] {"A", "A1", "B", "P"};
            var statusesToBeChecked2 = new[] {"C", "T", "A", "A1", "B", "P"};

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var status = row.Field<string>("Status");

                    if (contractType == "V2_Loan")
                    {
                        var q3 = row.Field<string>("Q3") == "1";
                        var q4 = row.Field<string>("Q4") == "1";
                        var q7 = row.Field<string>("Q7") == "1";

                        if (q3 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q3 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                        if (q4 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q4 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                        if (q7 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q7 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                    }
                    else if (contractType.Equals("V3_Mortgage"))
                    {
                        var q3 = row.Field<string>("Q3") == "1";
                        var q4 = row.Field<string>("Q4") == "1";
                        var q7 = row.Field<string>("Q7") == "1";
                        var q6 = row.Field<string>("Q6") == "1";

                        if (q3 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q3 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                        if (q4 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q4 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                        if (q7 && !statusesToBeChecked1.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q7 is true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                        if (q6 && !statusesToBeChecked2.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q6 is true and Status not in (C, T, A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                    }
                    else
                    {
                        var q6 = row.Field<string>("Q6") == "1";

                        if (q6 && !statusesToBeChecked2.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q6 is true and Status not in (C, T, A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckStartingDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var b3 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out var plannedEndDate);

                    var b4 = false;
                    DateTime actualEndDate = default;

                    if (row.Table.Columns.Contains("ActualEndDate"))
                        b4 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out actualEndDate);


                    if (b1 && b2 && startingDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than ReferenceDate - found at row number {rows.IndexOf(row)} \n");

                    if (b2 && b3 && startingDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (b2 && b4 && startingDate > actualEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than ActualEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && b4 && actualEndDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! ActualEndDate can't be greater than ReferenceDate - found at row number {rows.IndexOf(row)} \n");

                    if (contractType == "V4_UnutilizedMortgageBalance") goto REPEAT;

                    var b5 = DateTime.TryParse(row.Field<string>("LastPaymentDate"), out var lastPaymentDate);

                    var b6 = false;
                    DateTime firstDeferredPaymentDate = default;

                    if (row.Table.Columns.Contains("ActualEndDate"))
                        b6 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"),
                            out firstDeferredPaymentDate);

                    var b7 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out var plannedBaloonDate);
                    var b8 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);

                    if (b2 && b5 && startingDate > lastPaymentDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! StartingDate can't be greater than LastPaymentDate - found at row number {rows.IndexOf(row)} \n");

                    if (b1 && b5 && lastPaymentDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! LastPaymentDate can't be greater than ReferenceDate - found at row number {rows.IndexOf(row)} \n");

                    if (b2 && b6 && startingDate > firstDeferredPaymentDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! StartingDate can't be greater than firstDeferredPaymentDate - found at row number {rows.IndexOf(row)} \n");

                    if (b3 && b6 && firstDeferredPaymentDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! FirstDeferredPaymentDate can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (b2 && b7 && startingDate > plannedBaloonDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! StartingDate can't be greater than PlannedBaloonDate - found at row number {rows.IndexOf(row)} \n");

                    if (b3 && b7 && plannedBaloonDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be greater than PlannedBaloonDate - found at row number {rows.IndexOf(row)} \n");

                    if (b2 && b8 && startingDate > firstDefaultDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! StartingDate can't be greater than FirstDefaultDate - found at row number {rows.IndexOf(row)} \n");

                }

                REPEAT: ;
            }
        }

        public static void CheckStatus(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);
            var statusesToBeChecked = new[] {"C", "T", "A", "A1", "B", "P"};
            var statusesToBeChecked2 = new[] {"A", "A1", "B", "P"};

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var status = row.Field<string>("Status");
                    var b2 = false;

                    if (row.Table.Columns.Contains("ActualEndDate"))
                        b2 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out _);

                    if (status.Equals("T") || status.Equals("B") && b2 == false)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status in (T or B) must have ActualEndDate  - found at row number {rows.IndexOf(row)} \n");

                    if (!status.Equals("T") && !status.Equals("B") && b2)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status not in (T or B) must not have  ActualEndDate  - found at row number {rows.IndexOf(row)} \n");

                    if (contractType == "V4_UnutilizedMortgageBalance")
                    {
                        var q6 = row.Field<string>("Q6").Equals("1");

                        if (!status.Equals("C") && !status.Equals("T") && !status.Equals("P"))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Status not in (C or T or B)  - found at row number {rows.IndexOf(row)} \n");

                        if (q6 && !statusesToBeChecked.Contains(status))
                            richTextBox.AppendText(
                                $"error at {contractType} ! Q6 can't be true and Status not in (C, T, A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");

                        goto REPEAT;
                    }

                    var arrears = row.Field<string>("Arrears");
                    var b4 = long.TryParse(row.Field<string>("PastDueAmount"), out var pastDueAmount);
                    var q3 = row.Field<string>("Q3").Equals("1");
                    var q4 = row.Field<string>("Q4").Equals("1");
                    var q7 = row.Field<string>("Q7").Equals("1");
                    var b3 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);

                    if (q3 && !statusesToBeChecked2.Contains(status))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Q3 can't be true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");

                    if (q4 && !statusesToBeChecked2.Contains(status))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Q4 can't be true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");

                    if (q7 && !statusesToBeChecked2.Contains(status))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Q7 can't be true and Status not in (A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b1 && b3 && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days  - found at row number {rows.IndexOf(row)} \n");

                        if (b3 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status A/A1 must have firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");

                        if (string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status A/A1 must have arrears  - found at row number {rows.IndexOf(row)} \n");

                        if (pastDueAmount <= 200 || b4 == false)
                            richTextBox.AppendText(
                                $"error at {contractType} !status A/A1 must not have PastDueAmount  <= 200 or null  - found at row number {rows.IndexOf(row)} \n");
                    }

                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b3)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status C/T  must not have FirstDefaultDate - found at row number {rows.IndexOf(row)} \n");

                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! status C/T must not have arrears  - found at row number {rows.IndexOf(row)} \n");

                        if (b4 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! status C/T must not have  PastDueAmount greater > 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }

                REPEAT: ;
            }
        }

        public static void CheckTransferPartnership(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("TransferPartnership"))
                return;

            var rows = dataSet.Tables["TransferPartnership"].Rows;

            foreach (DataRow row in rows)
            {
                var indicator = row.Field<string>("Indicator");
                var companyName = row.Field<string>("CompanyName");
                var companyNo = row.Field<string>("CompanyNo");
                var b1 = double.TryParse(row.Field<string>("Percentage"), out var percentage);

                if (string.IsNullOrEmpty(indicator) || string.IsNullOrEmpty(companyName) ||
                    string.IsNullOrEmpty(companyNo))
                    richTextBox.AppendText(
                        $"error at TransferPartnership ! indicator/CompanyName/CompanyNo can't be empty - found at row number {rows.IndexOf(row)} \n");

                if (b1 && percentage < 0)
                    richTextBox.AppendText(
                        $"error at TransferPartnership ! Percentage can't be negative - found at row number {rows.IndexOf(row)} \n");
            }
        }


        // LinkData checks ----------------------------------------------------------------------------------------------------------------

        public static void CheckLinkData(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("LinkData"))
                return;

            var rows = dataSet.Tables["LinkData"].Rows;

            foreach (DataRow row in rows)
            {
                var b1 = int.TryParse(row.Field<string>("Role"), out var role);
                var guarantorPercentageRange = row.Field<string>("GuarantorPercentageRange");

                if (b1 && role == 2 && string.IsNullOrEmpty(guarantorPercentageRange))
                    richTextBox.AppendText(
                        $"error at LinkData ! if role = 2 there must be a GuarantorPercentageRange - found at row number {rows.IndexOf(row)} \n");

                if (b1 && role == 1 && !string.IsNullOrEmpty(guarantorPercentageRange))
                    richTextBox.AppendText(
                        $"error at LinkData ! if role = 1 there must not be a GuarantorPercentageRange - found at row number {rows.IndexOf(row)} \n");
            }
        }


        //SubjectData checks ---------------------------------------------------------------------------------------------------------

        public static void CheckAddress(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {

            //do again !!!!!!!!!!!!
            if (!dataSet.Tables.Contains("Address"))
                return;

            var rows = dataSet.Tables["Address"].Rows;
            var columns = dataSet.Tables["Address"].Columns;

            //loop inside rows, and inside each row loop inside all columns
            foreach (DataRow row in rows)
            {
                var flag = false;

                foreach (DataColumn column in columns)
                {
                    if (column.ToString() == "MilitaryPOB" || column.ToString() == "Confirmed" ||
                        column.ToString() == "SubjectData_Id")
                        continue;

                    if (string.IsNullOrEmpty(row[column].ToString()))
                        flag = true;
                }

                if (!flag)
                    continue;

                CleanRow(row, columns);
                richTextBox.AppendText(
                    $"error at Address ! one of the attributes was empty, cleared all row! - found at row number {rows.IndexOf(row)} \n");

            }

        }

        public static void CheckBirthDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            //need to complete
            var b2 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"), out var referenceDate);

            if (!dataSet.Tables.Contains("Individual"))
                return;

            var rows = dataSet.Tables["Individual"].Rows;

            foreach (DataRow row in rows)
            {
                var b1 = DateTime.TryParseExact(row.Field<string>("BirthDate"), "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var birthDate);
                var year = birthDate.Year;

                if (b1 == false)
                    richTextBox.AppendText($"error at Individual ! Birthday isn't a valid date/wrong format - found at row number {rows.IndexOf(row)} \n");

                else if (year > 9999 || year < 1800)
                    richTextBox.AppendText($"error at Individual ! Birthday year must be between 1800-9999 - found at row number {rows.IndexOf(row)} \n");

                if (b1 && b2 && (referenceDate.Year - birthDate.Year < 18))
                    richTextBox.AppendText($"error at Individual ! Birthday year - referenceDate Year must be > 18 - found at row number {rows.IndexOf(row)} \n");
                
                if (b1 && b2 && (birthDate.Year >= referenceDate.Year - 18))
                    richTextBox.AppendText($"error at Individual ! Birthday year must be >= referenceDate year - 18 - found at row number {rows.IndexOf(row)} \n");
            }

        }

        public static void CheckFirstName(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
           
            if (!dataSet.Tables.Contains("Individual"))
                return;

            var rows = dataSet.Tables["Individual"].Rows;

            foreach (DataRow row in rows)
            {
                var firstName = row.Field<string>("FirstName");
                var lastName = row.Field<string>("LastName");
                string latinFirstName = null;
                if(dataSet.Tables["Individual"].Columns.Contains("LatinFirstName"))
                    latinFirstName = row.Field<string>("LatinFirstName");

                string latinLastName = null;
                if (dataSet.Tables["Individual"].Columns.Contains("LatinLastName"))
                    latinLastName = row.Field<string>("LatinLastName");

                var b1 = long.TryParse(row.Field<string>("IdentityNumber"), out _);

                if (!string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                { 
                    richTextBox.AppendText($"error at Individual ! FirstName must be filled with LastName, cleared FirstName - found at row number {rows.IndexOf(row)} \n");
                    row.SetField("FirstName","");
                }

                if (!string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName))
                {
                    richTextBox.AppendText($"error at Individual ! LastName must be filled with FirstName, cleared LastName - found at row number {rows.IndexOf(row)} \n");
                    row.SetField("LastName", "");
                }

                if (b1 && (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)))
                    richTextBox.AppendText($"error at Individual ! if there's IdentityNumber both FirstName and LastName must exist - found at row number {rows.IndexOf(row)} \n");

                if ((string.IsNullOrEmpty(latinFirstName) || string.IsNullOrEmpty(latinLastName)) && (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)))
                    richTextBox.AppendText($"error at Individual ! Both Latin and hebrew names are empty, at least 1 should exist! - found at row number {rows.IndexOf(row)} \n");

            }

        }

        public static void CheckIdentityNumber(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            //add more checks
            if (!dataSet.Tables.Contains("Individual"))
                return;

            var rows = dataSet.Tables["Individual"].Rows;

            foreach (DataRow row in rows)
            {
                var b1 = long.TryParse(row.Field<string>("IdentityNumber"), out var identityNumber);
                var firstName = row.Field<string>("FirstName");
                var lastName = row.Field<string>("LastName");

                if (b1)
                {
                    if (identityNumber.ToString("000000000").Length != 9)
                        richTextBox.AppendText($"error at Individual ! IdentityNumber must be 9 digits long - found at row number {rows.IndexOf(row)} \n");

                    if (identityNumber < 0)
                        richTextBox.AppendText($"error at Individual ! IdentityNumber must be positive - found at row number {rows.IndexOf(row)} \n");

                    if (CheckLuhn(identityNumber.ToString("000000000")) == false)
                        richTextBox.AppendText($"error at Individual ! not a valid IdentityNumber! (Luhn's algorithm) - found at row number {rows.IndexOf(row)} \n");
                }
                
                if (b1 && (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)))
                    richTextBox.AppendText($"error at Individual !if there's IdentityNumber, both FirstName and LastName must exist- found at row number {rows.IndexOf(row)} \n");
            }

        }

        public static void CheckLatinName(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            //do later
            if (!dataSet.Tables.Contains("Individual"))
                return;

           
            /*
            foreach (DataRow row in rows)
            {

            }*/

        }

        public static void CheckPassport(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            //do later
            if (!dataSet.Tables.Contains("Passport"))
                return;
            var columns = dataSet.Tables["Passport"].Columns;
            var rows = dataSet.Tables["Passport"].Rows;

            foreach (DataRow row in rows)
            {
                var number = row.Field<string>("Number");
                var b1 = int.TryParse(row.Field<string>("Country"), out _);

                if (b1 == false && string.IsNullOrEmpty(number))
                { 
                    richTextBox.AppendText($"error at Individual ! at least country/number must exist, clearing Passport - found at row number {rows.IndexOf(row)} \n");
                   CleanRow(row,columns);
                }

                if ((string.IsNullOrEmpty(number) && b1) || (!string.IsNullOrEmpty(number) && b1 == false))
                    richTextBox.AppendText($"error at Individual ! country exists without number, or number exists without country - found at row number {rows.IndexOf(row)} \n");
            }

        }

        public static void CleanRow(DataRow row, DataColumnCollection columns)
        {
            foreach (DataColumn column in columns)
            {
                if(column.ToString() == "SubjectData_Id")
                    continue;

                row.SetField(column.ToString(),"");
            }
        }

        public static bool CheckLuhn(string cardNo)
        {
            var nDigits = cardNo.Length;

            var nSum = 0;
            var isSecond = false;
            for (var i = nDigits - 1; i >= 0; i--)
            {
                var d = cardNo[i] - '0';

                if (isSecond )
                    d *= 2;

              
                nSum += d / 10;
                nSum += d % 10;

                isSecond = !isSecond;
            }

            return nSum % 10 == 0;
        }

        public static IList<Action<DataSet, RichTextBox, IList<string>>> CifList =
            new List<Action<DataSet, RichTextBox, IList<string>>>
            {
                CheckNumberOfSubjects,
                CheckActualEndDate,
                CheckActualPaidAmount,
                CheckArrears,
                CheckCollateralCaseDetail,
                CheckCountryNumberName,
                CheckCreditLimit,
                CheckDesignation,
                CheckCurrentBalance,
                CheckFirstDefaultDate,
                CheckFirstDeferredPaymentDate,
                CheckAmountAndType,
                //CheckAnnualizedInterest,
               // CheckBase,
                CheckLastPaymentDate,
                CheckLoanUtilized,
                CheckPlannedMonthlyPayment,
                CheckPastDueAmount,
                CheckPaymentFrequency,
                CheckPaymentHistory,
                CheckPlannedBaloonDate,
                CheckPlannedEndDate,
                CheckMonthlyPaymentType,
                CheckQs,
                CheckStartingDate,
                CheckStatus,
                CheckTransferPartnership,
                CheckLinkData,
                CheckAddress,
                CheckBirthDate,
                CheckFirstName,
                CheckIdentityNumber,
                CheckLatinName,
                CheckPassport
            };

        public static void CheckImmediateData(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var idList = new List<int>();
            
            foreach (var contractType in contractTypes)
            {
                var prevId = 999;

                //check same contract appears in same node
                foreach (DataRow row in dataSet.Tables[contractType].Rows)
                {
                    var id = row.Field<int>("ImmediateData_Id");
                    if (prevId == id)
                    {
                        richTextBox.AppendText("error at ImmediateData ! only 1 contract can be in each ImmediateData node- found at row number \n");
                        return;
                    }
                    prevId = id;
                }
                //check different contract in same node
                var minId = dataSet.Tables[contractType].Rows[0].Field<int>("ImmediateData_Id");

                for (var i = minId; i < dataSet.Tables[contractType].Rows.Count + minId; i++)
                {
                    var rowsWithId = dataSet.Tables[contractType].Select($"ImmediateData_Id = {i}");
                    var currentId = rowsWithId.First().Field<int>($"ImmediateData_Id");
                    idList.Add(currentId);
                }

                switch (contractType)
                {
                    case "V2_Loan":
                    case "V3_Mortgage":
                    {
                        string legalProcedure = null, closedContractFlag = null;

                        foreach (DataRow row in dataSet.Tables[contractType].Rows)
                        {
                            if(row.Table.Columns.Contains("LegalProcedure"))
                                legalProcedure = row.Field<string>("LegalProcedure");

                            if (row.Table.Columns.Contains("ClosedContractFlag"))
                                closedContractFlag = row.Field<string>("ClosedContractFlag");

                            if(string.IsNullOrEmpty(legalProcedure) && string.IsNullOrEmpty(closedContractFlag))
                                richTextBox.AppendText($"error at V2_Loan/V3_Mortgage ! both legalProcedure and closedContractFlag are empty- found at row number {dataSet.Tables[contractType].Rows.IndexOf(row)} \n");

                            else if (!string.IsNullOrEmpty(legalProcedure) && !string.IsNullOrEmpty(closedContractFlag))
                                richTextBox.AppendText($"error at V2_Loan/V3_Mortgage ! both legalProcedure and closedContractFlag are filled- found at row number {dataSet.Tables[contractType].Rows.IndexOf(row)}\n");
                        }

                        break;
                    }
                    case "V4_UnutilizedMortgageBalance":
                    {
                        string closedContractFlag = null;

                        foreach (DataRow row in dataSet.Tables[contractType].Rows)
                        {
                            if (row.Table.Columns.Contains("ClosedContractFlag"))
                                closedContractFlag = row.Field<string>("ClosedContractFlag");

                            if (string.IsNullOrEmpty(closedContractFlag))
                                richTextBox.AppendText($"error at V4_UnutilizedMortgageBalance ! ClosedContractFlag is empty- found at row number {dataSet.Tables[contractType].Rows.IndexOf(row)}\n");
                        }

                        break;
                    }
                }
            }

            if (idList.Count != idList.Distinct().Count())
                richTextBox.AppendText("error at ImmediateData ! only 1 contract can be in each ImmediateData node- found at row number \n");

        }

        public static void CheckImfLink(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("LinkData"))
                return;
            
            var rows = dataSet.Tables["LinkData"].Rows;

            foreach (DataRow row in rows)
            {
                var b1 = false;
                var role = row.Field<string>("Role");

                if(row.Table.Columns.Contains("GuarantorPercentageRange"))
                    b1 = double.TryParse(row.Field<string>("GuarantorPercentageRange"), out _);

                if (role == "1" && b1)
                    richTextBox.AppendText($"error at LinkData ! Role = 1 and GuarantorPercentageRange is filled  - found at row number {rows.IndexOf(row)} \n");
            }
        }

        public static void CheckSubjectData(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            CheckAddress(dataSet, richTextBox, null);
            CheckBirthDate(dataSet, richTextBox, null);
            CheckFirstName(dataSet, richTextBox, null);
            CheckIdentityNumber(dataSet, richTextBox, null);
            CheckLatinName(dataSet, richTextBox, null);
            CheckPassport(dataSet, richTextBox, null);
        }

        public static IList<Action<DataSet, RichTextBox, IList<string>>> ImfList =
            new List<Action<DataSet, RichTextBox, IList<string>>>
        {
            CheckImmediateData,
            CheckImfLink,
            CheckSubjectData
        };


        public static void CheckReason(DataSet dataSet, RichTextBox richTextBox)
        {
            if (!dataSet.Tables.Contains("Contract"))
                return;

            var minId = dataSet.Tables["Contract"].Rows[0].Field<int>("Delete_Id");

            for (int i=minId,rowIndex = 0; i <dataSet.Tables["Contract"].Rows.Count + minId; i++,rowIndex++)
            {
                var rowsWithId = dataSet.Tables["Contract"].Select($"Delete_Id = {i}");
                var reason = rowsWithId.First().Field<string>("Reason");
                var contractCounter = rowsWithId.Count();
                var dataSourceContractCode = dataSet.Tables["Data"].Select($"Delete_Id = {i}").First().Field<string>("DataSourceContractCode");
              

                //make function getRowById(name,idName,idNumber);
                if (string.IsNullOrEmpty(reason) || string.IsNullOrEmpty(dataSourceContractCode))
                    richTextBox.AppendText($"error at Contract ! Reason and DataSourceContractCode must both exist - found at row number {rowIndex} \n");

                if (contractCounter <= 1) 
                    continue;
                richTextBox.AppendText($"error at Contract ! Only 1 ContractNode can be in DeleteNode - found at row number {rowIndex} \n");
                break;

            }

        }

        public static void CheckContractPartner(DataSet dataSet, RichTextBox richTextBox)
        {
            //do later
            if (!dataSet.Tables.Contains("ContractPartner"))
                return;

            var minId = dataSet.Tables["ContractPartner"].Rows[0].Field<int>("Delete_Id");

            for (int i = minId, rowIndex = 0; i < dataSet.Tables["ContractPartner"].Rows.Count + minId; i++,rowIndex++)
            {
                var rowsWithId = dataSet.Tables["ContractPartner"].Select($"Delete_Id = {i}");
                var indicator = rowsWithId.First().Field<string>("Indicator");
                var companyNo = rowsWithId.First().Field<string>("CompanyNo");
                var contractPartnerCounter = rowsWithId.Length;
                var dataSourceContractCode = dataSet.Tables["Data"].Select($"Delete_Id = {i}").First().Field<string>("DataSourceContractCode");


                //make function getRowById(name,idName,idNumber);
                if (string.IsNullOrEmpty(dataSourceContractCode) || string.IsNullOrEmpty(indicator) || string.IsNullOrEmpty(companyNo))
                    richTextBox.AppendText($"error at ContractPartner ! Indicator, CompanyNo and DataSourceContractCode must both exist - found at row number {rowIndex} \n");

                if (contractPartnerCounter <= 1)
                    continue;
                richTextBox.AppendText($"error at ContractPartner ! Only 1 ContractPartner can be in DeleteNode - found at row number {rowIndex} \n");
                break;

            }

        }

        public static void CheckContractRecord(DataSet dataSet, RichTextBox richTextBox)
        {
            //do later
            if (!dataSet.Tables.Contains("ContractRecord"))
                return;

            var minId = dataSet.Tables["ContractRecord"].Rows[0].Field<int>("Delete_Id");

            for (int i = minId,rowIndex=0; i < dataSet.Tables["ContractRecord"].Rows.Count + minId; i++,rowIndex++)
            {
                var rowsWithId = dataSet.Tables["ContractRecord"].Select($"Delete_Id = {i}");
                var b1 = DateTime.TryParse(rowsWithId.First().Field<string>("ReferenceDate"), out _);
                var contractRecordCounter = rowsWithId.Length;
                var dataSourceContractCode = dataSet.Tables["Data"].Select($"Delete_Id = {i}").First().Field<string>("DataSourceContractCode");

                //make function getRowById(name,idName,idNumber);
                if (string.IsNullOrEmpty(dataSourceContractCode) || b1 == false)
                    richTextBox.AppendText($"error at ContractRecord ! ReferenceDate and DataSourceContractCode must both exist - found at row number {rowIndex} \n");

                if (contractRecordCounter <= 1)
                    continue;
                richTextBox.AppendText($"error at ContractRecord ! Only 1 ContractRecord can be in DeleteNode - found at row number {rowIndex} \n");
                break;

            }

        }

        public static void CheckImmediate(DataSet dataSet, RichTextBox richTextBox)
        {
            //do later
            if (!dataSet.Tables.Contains("Immediate"))
                return;

            var minId = dataSet.Tables["Immediate"].Rows[0].Field<int>("Delete_Id");

            for (int i = minId, rowIndex = 0; i < dataSet.Tables["Immediate"].Rows.Count + minId; i++, rowIndex++)
            {
                var rowsWithId = dataSet.Tables["Immediate"].Select($"Delete_Id = {i}");
                var b1 = DateTime.TryParse(rowsWithId.First().Field<string>("ReferenceDate"), out _);
                var immediateCounter = rowsWithId.Length;
                var dataSourceContractCode = dataSet.Tables["Data"].Select($"Delete_Id = {i}").First().Field<string>("DataSourceContractCode");


                //make function getRowById(name,idName,idNumber);
                if (string.IsNullOrEmpty(dataSourceContractCode) || b1 == false)
                    richTextBox.AppendText($"error at Immediate ! ReferenceDate and DataSourceContractCode must both exist - found at row number {rowIndex} \n");

                if (immediateCounter <= 1)
                    continue;
                richTextBox.AppendText($"error at Immediate ! Only 1 Immediate can be in DeleteNode - found at row number {rowIndex} \n");
                break;

            }

        }


        public static void CheckLink(DataSet dataSet, RichTextBox richTextBox)
        {
            //do later
            if (!dataSet.Tables.Contains("Individual") && !dataSet.Tables.Contains("Passport") && !dataSet.Tables.Contains("Address"))
                return;

            var minId = dataSet.Tables["Link"].Rows[0].Field<int>("Delete_Id");

            for (int i = minId, rowIndex = 0; i < dataSet.Tables["Link"].Rows.Count + minId; i++, rowIndex++)
            {
                var rowsWithId = dataSet.Tables["Link"].Select($"Delete_Id = {i}");
                var individualCounter = rowsWithId.Length;

                if (individualCounter <= 1)
                    continue;
                richTextBox.AppendText($"error at Link ! Only 1 Link can be in DeleteNode - found at row number {rowIndex} \n");
                break;
            }
          

            CheckAddress(dataSet,richTextBox,null);
            //CheckBirthDate(dataSet, richTextBox, null);
            CheckFirstName(dataSet, richTextBox, null);
            CheckIdentityNumber(dataSet, richTextBox, null);
            CheckLatinName(dataSet, richTextBox, null);
            CheckPassport(dataSet,richTextBox,null);

        }
        public static IList<Action<DataSet, RichTextBox>> CmfList = new List<Action<DataSet, RichTextBox>>
        {
            CheckReason,
            CheckContractPartner,
            CheckContractRecord,
            CheckImmediate,
            CheckLink
        };
    }
}