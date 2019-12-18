using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public class Checker
    {
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
            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out var actualEndDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    if (b1 && b2 && actualEndDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! StartingDate  can't be bigger than EndingDate - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckArrears(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;
                if (dataSet.Tables[contractType].Columns["Arrears"] == null)
                    continue;
                foreach (DataRow row in rows)
                {
                    var arrears = row.Field<string>("Arrears");
                    var pastDueAmount = Convert.ToInt64(row.Field<string>("PastDueAmount"));
                    var status = row.Field<string>("Status");
                    var b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b1 && b2 && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days - found at row number {rows.IndexOf(row)} \n");
                        if (pastDueAmount <= 200 || string.IsNullOrEmpty(pastDueAmount.ToString()))
                            richTextBox.AppendText(
                                $"error at {contractType} ! PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");
                        if (!b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");
                        if (string.IsNullOrEmpty(arrears))
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
                        if (!string.IsNullOrEmpty(pastDueAmount.ToString()) && pastDueAmount != 0)
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
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var actualPaidAmount = Convert.ToInt64(row.Field<string>("ActualPaidAmount"));
                    if (actualPaidAmount < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! ActualPaidAmount can't be negative - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckCollateralCaseDetailAmount(DataSet dataSet, RichTextBox richTextBox,
            IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("CollateralCase") || !dataSet.Tables.Contains("CollateralCaseDetail"))
                return;

            var rows = dataSet.Tables["CollateralCaseDetail"].Rows;
            foreach (DataRow row in rows)
            {
                var amount = Convert.ToInt64(row.Field<string>("Amount"));
                var type = row.Field<string>("Type");
                if (amount < 0 || string.IsNullOrEmpty(amount.ToString()) || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at CollateralCaseDetail ! amount is null/negative or type is null - found at row number {rows.IndexOf(row)} \n");
            }
        }

        public static void CheckCountryNumberName(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
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
                if (!dataSet.Tables[contractType].TableName.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var creditLimit = Convert.ToInt64(row.Field<string>("CreditLimit"));
                    if (string.IsNullOrEmpty(creditLimit.ToString()))
                        continue;
                    var designation = row.Field<string>("Designation");
                    if ((creditLimit < 0 || creditLimit > 5000) && string.IsNullOrEmpty(designation))
                    {
                        richTextBox.AppendText(
                            $"error at {contractType} ! creditLimit/Designation are wrong- found at row number {rows.IndexOf(row)} \n");
                    }
                    else if (!string.IsNullOrEmpty(creditLimit.ToString()) && creditLimit <= 5000 &&
                             !string.IsNullOrEmpty(designation))
                    {
                        row.SetField("Designation", "");
                        richTextBox.AppendText($"creditLimit is lesser than 5000 and designation isn't empty!!  designation was cleared at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckCurrentBalance(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (!dataSet.Tables[contractType].TableName.Equals("V2_Loan") && !dataSet.Tables[contractType].TableName.Equals("V3_Mortgage"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var currentBalance = Convert.ToInt64(row.Field<string>("CurrentBalance"));
                    if (!string.IsNullOrEmpty(currentBalance.ToString()) && currentBalance < 0)
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
                if (!dataSet.Tables[contractType].TableName.Equals("V2_Loan") && !dataSet.Tables[contractType].TableName.Equals("V3_Mortgage"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var status = row.Field<string>("Status");
                    var arrears = row.Field<string>("Arrears");
                    var pastDueAmount = Convert.ToInt64(row.Field<string>("PastDueAmount"));
                    var b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);
                    var b3 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    if (b1 && b2 && b3 && (firstDefaultDate > referenceDate || firstDefaultDate < startingDate))
                        richTextBox.AppendText(
                            $"error at {contractType} ! firstDefaultDate can't be bigger than referenceDate/ firstDefaultDate can't be less than startingDate   - found at row number {rows.IndexOf(row)} \n");

                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b1 && b2 && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days - found at row number {rows.IndexOf(row)} \n");
                        if (pastDueAmount <= 200 || string.IsNullOrEmpty(pastDueAmount.ToString()))
                            richTextBox.AppendText(
                                $"error at {contractType} ! PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");
                        if (!b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");
                        if (string.IsNullOrEmpty(arrears))
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
                        if (!string.IsNullOrEmpty(pastDueAmount.ToString()) && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with PastDueAmount greater than 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }
            }
        }

        public static void CheckFirstDeferredPaymentDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (!dataSet.Tables[contractType].TableName.Equals("V2_Loan") && !dataSet.Tables[contractType].TableName.Equals("V3_Mortgage"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out var firstDeferredPaymentDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var b3 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out var plannedEndDate);
                    var paymentFrequency= Convert.ToInt32(row.Field<string>("PaymentFrequency"));

                    if ( b1 && b2 && b3 && (firstDeferredPaymentDate < startingDate || firstDeferredPaymentDate > plannedEndDate))
                        richTextBox.AppendText(
                            $"error at {contractType} !firstDeferredPaymentDate can't be less than startingDate or more than plannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    if((b1 && paymentFrequency != 11 )|| (!b1 && paymentFrequency == 11))
                        richTextBox.AppendText(
                            $"error at {contractType} ! can't have firstDeferredPaymentDate and paymentFrequency != 11.. and the opposite - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckAmountAndType(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("Forgiveness"))
                return;

            var rows = dataSet.Tables["Forgiveness"].Rows;

            foreach (DataRow row in rows)
            {//need to check only if v2 or v3
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
        
        public static void CheckAnnualizedInterest(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("InterestSchema"))
                return;

            var rows = dataSet.Tables["InterestSchema"].Rows;

            foreach (DataRow row in rows)
            {//need to check only if v2 or v3
                var annualizedInterestExists =row.Field<string>("AnnualizedInterest");
                var doesAnnualizedInterestExists = !string.IsNullOrEmpty(row.Field<string>("AnnualizedInterest"));
               
                if (doesAnnualizedInterestExists && Convert.ToDecimal(annualizedInterestExists) < 0)
                    richTextBox.AppendText(
                        $"error at Interest Schema ! AnnualizedInterest can't be negative - found at row number {rows.IndexOf(row)} \n");
                /*   להמשיך את החוקים - לא מובן בכלל
                if (!doesAmountExists || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at Forgiveness ! amount and/or type can't be empty- found at row number {rows.IndexOf(row)} \n");*/
            }
        }


        public static void CheckBase(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            if (!dataSet.Tables.Contains("InterestSchema"))
                return;

            var rows = dataSet.Tables["InterestSchema"].Rows;

            foreach (DataRow row in rows)
            {//need to check only if v2 or v3
                var base1 = row.Field<string>("Base");
                
                //if (doesAnnualizedInterestExists && Convert.ToDecimal(annualizedInterestExists) < 0)
                  //  richTextBox.AppendText(
                    //    $"error at Interest Schema ! AnnualizedInterest can't be negative - found at row number {rows.IndexOf(row)} \n");
                /*   להמשיך את החוקים - לא מובן בכלל
                if (!doesAmountExists || string.IsNullOrEmpty(type))
                    richTextBox.AppendText(
                        $"error at Forgiveness ! amount and/or type can't be empty- found at row number {rows.IndexOf(row)} \n");*/
            }
        }

        // להמשיך את ה interest schema



        public static void CheckDesignation(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
           
            // do again
        }


        public static IList<Action<DataSet, RichTextBox, IList<string>>> CifList =
            new List<Action<DataSet, RichTextBox, IList<string>>>
            {
                CheckNumberOfSubjects,
                CheckActualEndDate,
                CheckActualPaidAmount,
                CheckArrears,
                CheckCollateralCaseDetailAmount,
                CheckCountryNumberName,
                CheckCreditLimit,
                CheckDesignation,
                CheckCurrentBalance,
                CheckFirstDefaultDate,
                CheckFirstDeferredPaymentDate,
                CheckAmountAndType,
                CheckAnnualizedInterest
            };

        public static IList<Action<DataSet, RichTextBox>> ImfList = new List<Action<DataSet, RichTextBox>>
        {
        };

        public static IList<Action<DataSet, RichTextBox>> CmfList = new List<Action<DataSet, RichTextBox>>
        {
        };
    }
}