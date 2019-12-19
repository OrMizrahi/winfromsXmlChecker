using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

                    var b1 = long.TryParse(row.Field<string>("ActualPaidAmount"), out var actualPaidAmount);
                    
                    if (b1 && actualPaidAmount< 0 )
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
                        richTextBox.AppendText($"error at {contractType} ! creditLimit is lesser than 5000 and designation isn't empty!!  designation was cleared at row number {rows.IndexOf(row)} \n");
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
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
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
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out var firstDeferredPaymentDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var b3 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out var plannedEndDate);
                    var b4= int.TryParse(row.Field<string>("PaymentFrequency"), out var paymentFrequency);

                    if ( b1 && b2 && b3 && (firstDeferredPaymentDate < startingDate || firstDeferredPaymentDate > plannedEndDate))
                        richTextBox.AppendText(
                            $"error at {contractType} !firstDeferredPaymentDate can't be less than startingDate or more than plannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    if((b1 && b4 && paymentFrequency != 11 )|| (!b1 && b4 && paymentFrequency == 11))
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
                //var base1 = row.Field<string>("Base");
                
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

        public static void CheckLastPaymentDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;
                foreach (DataRow row in rows)
                {
                    var b1 = DateTime.TryParse(row.Field<string>("LastPaymentDate"), out var lastPaymentDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);

                    if (b1 && b2 && lastPaymentDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! LastPaymentDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
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
                    var loanUtilized = row.Field<string>("LoanUtilized");
                    var designation = row.Field<string>("Designation");
                    var doesLoanUtilizedExists = !string.IsNullOrEmpty(row.Field<string>("LoanUtilized"));

                    if (doesLoanUtilizedExists && Convert.ToInt64(loanUtilized) < 0)
                        richTextBox.AppendText(
                                $"error at {contractType} ! LoanUtilized can't be negative - found at row number {rows.IndexOf(row)} \n");
                    else if (Convert.ToInt64(loanUtilized) > 5000 && string.IsNullOrEmpty(designation))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Designation can't be empty when LoanUtilized is greater than 5000 - found at row number {rows.IndexOf(row)} \n");
                    else if (Convert.ToInt64(loanUtilized) <= 5000 && !string.IsNullOrEmpty(designation))
                    {
                        richTextBox.AppendText(
                            $"error at {contractType} ! Designation can't exist when LoanUtilized is lesser than 5000, clearing Designation! - found at row number {rows.IndexOf(row)} \n");
                        row.SetField("Designation","");
                    }
                        
                }
            }
        }


        public static void CheckPlannedMonthlyPayment(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                if (contractType.Equals("V4_UnutilizedMortgageBalance"))
                    continue;
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = int.TryParse(row.Field<string>("PlannedMonthlyPayment"), out var plannedMonthlyPayment);

                    if (b1 && plannedMonthlyPayment < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedMonthlyPayment can't be negative - found at row number {rows.IndexOf(row)} \n");
                    if (!b1 || plannedMonthlyPayment == 0 )
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedMonthlyPayment can't be 0/NonExistent  - found at row number {rows.IndexOf(row)} \n");
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
                    var b2 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);
                    var arrears = row.Field<string>("Arrears");

                    if (b1 && pastDueAmount < 0)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PastDueAmount can't be negative  - found at row number {rows.IndexOf(row)} \n");
                    if (status.Equals("A") || status.Equals("A1"))
                    {
                        if (b2 && b3 && firstDefaultDate > referenceDate.AddDays(-30))
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDefaultDate > ReferenceDate-30 days  - found at row number {rows.IndexOf(row)} \n");
                        if(!b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");
                        if(string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without arrears  - found at row number {rows.IndexOf(row)} \n");
                        if (pastDueAmount <= 200 || !b1)
                                richTextBox.AppendText(
                                    $"error at {contractType} ! PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");
                    }
                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b2)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with firstDefaultValue  - found at row number {rows.IndexOf(row)} \n");
                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with arrears  - found at row number {rows.IndexOf(row)} \n");
                        if (b1 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with PastDueAmount greater than 0  - found at row number {rows.IndexOf(row)} \n");
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
                    var b1 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out _);
                    var b2 = int.TryParse(row.Field<string>("PaymentFrequency"), out var paymentFrequency);
                    var b3 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out _);

                    if (b1 && b2 && paymentFrequency != 11 || !b1 && b2 && paymentFrequency == 11) 
                        richTextBox.AppendText(
                            $"error at {contractType} ! FirstDeferredPaymentDate can only exist with PaymentFrequency = 11 ,  and otherwise!  - found at row number {rows.IndexOf(row)} \n");
                    if (b3 && b2 && paymentFrequency != 10)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can only exist with  PaymentFrequency = 10 , PlannedBaloonDate cleared!! - found at row number {rows.IndexOf(row)} \n");
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
                    var paymentHistory = row.Field<string>("PaymentHistory");
                    //should check each character must belong to the PaymentHistoryDomain
                    if (paymentHistory.Length != 24 )
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
                    var b1 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out var plannedBaloonDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var b3 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out var plannedEndDate);

                    if (b1 && b2 && plannedBaloonDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
                    if (b1 && b3 && plannedBaloonDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckPlannedEndDate(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                var isV4 = contractType.Equals("V4_UnutilizedMortgageBalance");
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var b1 = DateTime.TryParse(row.Field<string>("PlannedEndDate"), out var plannedEndDate);
                    var b2 = DateTime.TryParse(row.Field<string>("StartingDate"), out var startingDate);
                    var b3 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out var firstDeferredPaymentDate);
                    var b4 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out var plannedBaloonDate);

                    if (b1 && b2 && plannedEndDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedEndDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
                    if (!isV4 && b3)
                    {
                        if (b2 && firstDeferredPaymentDate < startingDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDeferredPaymentDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
                        if (b1 && firstDeferredPaymentDate > plannedEndDate)
                            richTextBox.AppendText(
                                $"error at {contractType} ! FirstDeferredPaymentDate can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    }

                    if (isV4 || !b4) continue;
                    if (b2 && plannedBaloonDate < startingDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be less than StartingDate - found at row number {rows.IndexOf(row)} \n");
                    if (b1 && plannedBaloonDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! PlannedBaloonDate can't be more than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                }
            }
        }

        public static void CheckMonthlyPaymentType(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
           
        }

        public static void CheckQs(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var status = row.Field<string>("Status");
                    var statusesToBeChecked1 = new[] { "A", "A1", "B", "P" };
                    var statusesToBeChecked2 = new[] { "C", "T", "A", "A1", "B", "P" };
                    
                    if (contractType.Equals("V2_Loan"))
                    {
                        var q3 = row.Field<string>("Q3").Equals("1");
                        var q4 = row.Field<string>("Q4").Equals("1");
                        var q7 = row.Field<string>("Q7").Equals("1");

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
                        var q3 = row.Field<string>("Q3").Equals("1");
                        var q4 = row.Field<string>("Q4").Equals("1");
                        var q7 = row.Field<string>("Q7").Equals("1");
                        var q6 = row.Field<string>("Q6").Equals("1");

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
                        var q6 = row.Field<string>("Q6").Equals("1");
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
                    var b4 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out var actualEndDate);
                    var b5 = DateTime.TryParse(row.Field<string>("LastPaymentDate"), out var lastPaymentDate);
                    var b6 = DateTime.TryParse(row.Field<string>("FirstDeferredPaymentDate"), out var firstDeferredPaymentDate);
                    var b7 = DateTime.TryParse(row.Field<string>("PlannedBaloonDate"), out var plannedBaloonDate);
                    var b8 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);

                    if (b1 && b2 && startingDate > referenceDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than ReferenceDate - found at row number {rows.IndexOf(row)} \n");
                    if (b2 && b3 && startingDate > plannedEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than PlannedEndDate - found at row number {rows.IndexOf(row)} \n");
                    if (b2 && b4 && startingDate > actualEndDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than ActualEndDate - found at row number {rows.IndexOf(row)} \n");

                    if (contractType.Equals("V4_UnutilizedMortgageBalance")) goto REPEAT;

                    if (b2 && b5 && startingDate > lastPaymentDate )
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than LastPaymentDate - found at row number {rows.IndexOf(row)} \n");
                    if (b2 && b6 && startingDate > firstDeferredPaymentDate)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Starting Date can't be greater than firstDeferredPaymentDate - found at row number {rows.IndexOf(row)} \n");
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
                REPEAT:;
            }
        }

        public static void CheckStatus(DataSet dataSet, RichTextBox richTextBox, IList<string> contractTypes)
        {
            var b1 = DateTime.TryParse(dataSet.Tables["Header"].Rows[0].Field<string>("FileReferenceDate"),
                out var referenceDate);

            foreach (var contractType in contractTypes)
            {
                var rows = dataSet.Tables[contractType].Rows;

                foreach (DataRow row in rows)
                {
                    var status = row.Field<string>("Status");
                    var arrears = row.Field<string>("Arrears");
                    var b4 = long.TryParse(row.Field<string>("PastDueAmount"), out var pastDueAmount);
                    var q3 = row.Field<string>("Q3").Equals("1");
                    var q4 = row.Field<string>("Q4").Equals("1");
                    var q7 = row.Field<string>("Q7").Equals("1");
                    var q6 = row.Field<string>("Q6").Equals("1");
                    var statusesToBeChecked = new[] {"C", "T", "A", "A1", "B", "P"};
                    var statusesToBeChecked2 = new[] {"A", "A1", "B", "P" };
                    var b2 = DateTime.TryParse(row.Field<string>("ActualEndDate"), out _);
                    var b3 = DateTime.TryParse(row.Field<string>("FirstDefaultDate"), out var firstDefaultDate);
                    

                    if (status.Equals("T") || status.Equals("B") && !b2 )
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status in (T or B) can't have empty ActualEndDate  - found at row number {rows.IndexOf(row)} \n");
                    if (!status.Equals("T") && !status.Equals("B") && b2)
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status not in (T or B) can't have  ActualEndDate  - found at row number {rows.IndexOf(row)} \n");
                    if(contractType.Equals("V4_UnutilizedMortgageBalance") && !status.Equals("C") && !status.Equals("T") && !status.Equals("P"))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Status not in (C or T or B)  - found at row number {rows.IndexOf(row)} \n");
                    if (contractType.Equals("V4_UnutilizedMortgageBalance") &&  q6 && !statusesToBeChecked.Contains(status))
                        richTextBox.AppendText(
                            $"error at {contractType} ! Q6 can't be true and Status not in (C, T, A, A1, B, P) - found at row number {rows.IndexOf(row)} \n");
                    
                    if (contractType.Equals("V4_UnutilizedMortgageBalance")) goto REPEAT;

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
                        if (!b3)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without firstDefaultDate  - found at row number {rows.IndexOf(row)} \n");
                        if (string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status A/A1 without arrears  - found at row number {rows.IndexOf(row)} \n");
                        if (pastDueAmount <= 200 || !b4)
                            richTextBox.AppendText(
                                $"error at {contractType} ! PastDueAmount is less than 200 or null  - found at row number {rows.IndexOf(row)} \n");
                    }
                    else if (status.Equals("C") || status.Equals("T"))
                    {
                        if (b3)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with firstDefaultValue  - found at row number {rows.IndexOf(row)} \n");
                        if (!string.IsNullOrEmpty(arrears))
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with arrears  - found at row number {rows.IndexOf(row)} \n");
                        if (b4 && pastDueAmount != 0)
                            richTextBox.AppendText(
                                $"error at {contractType} ! can't have status C/T with PastDueAmount greater than 0  - found at row number {rows.IndexOf(row)} \n");
                    }
                }

                REPEAT:;
            }
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
                CheckAnnualizedInterest,
                CheckBase,
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
                CheckStatus
            };

        public static IList<Action<DataSet, RichTextBox>> ImfList = new List<Action<DataSet, RichTextBox>>
        {
            null
        };

        public static IList<Action<DataSet, RichTextBox>> CmfList = new List<Action<DataSet, RichTextBox>>
        {
            null
        };
    }
}