using asprule1020.Models;
using asprule1020.Utility;
using ClosedXML.Excel;

namespace asprule1020.Infrastructure.Documents.Report
{
    public class Rule1020Monitoring
    {
        private readonly IWebHostEnvironment _environment;

        public Rule1020Monitoring(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] BuildMonitoringWorkbook(IReadOnlyList<Register> items)
        {
            using var workbook = CreateDefaultWorkbook();
            var ws = workbook.Worksheet(1);
            var startRow = 3; // row 1 = headers in template/default


            var lastUsedRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
            if (lastUsedRow >= startRow)
            {
                ws.Range(startRow, 1, lastUsedRow, 23).Clear(XLClearOptions.Contents);
            }

            var row = startRow;
            foreach (var item in items)
            {
                var pctDay = item.EstStatus == SD.StatusApproved ? item.EstPoHeadEvalDate : null;
                ws.Cell(row, 1).Value = item.TransId;
                ws.Cell(row, 2).Value = item.EstRegistrationDate;
                ws.Cell(row, 2).Style.DateFormat.Format = "d-mmm-yy";
                ws.Cell(row, 3).Value = item.Rule1020Id ?? string.Empty;
                ws.Cell(row, 4).Value = item.EstName;
                ws.Cell(row, 5).Value = item.EstStreet;
                ws.Cell(row, 6).Value = item.EstBrgy;
                ws.Cell(row, 7).Value = item.EstCityMun;
                ws.Cell(row, 8).Value = item.EstProvince;
                ws.Cell(row, 9).Value = item.EstPhone;
                ws.Cell(row, 10).Value = "NONE";
                ws.Cell(row, 11).Value = item.Email ?? string.Empty;
                ws.Cell(row, 12).Value = $"{item.EstOwnerFirst} {item.EstOwnerMid} {item.EstOwnerLast}".Replace("  ", " ").Trim();
                ws.Cell(row, 13).Value = item.EstBusinessNature;
                ws.Cell(row, 14).Value = item.EstLegalOrg;
                ws.Cell(row, 15).Value = item.EstType;
                ws.Cell(row, 16).Value = item.EstIsPeza ? "YES" : "NO";
                ws.Cell(row, 17).Value = "NOT STATED";
                ws.Cell(row, 18).Value = "NOT STATED";
                ws.Cell(row, 19).Value = item.EstMaleCount;
                ws.Cell(row, 20).Value = item.EstFemaleCount;
                ws.Cell(row, 21).Value = item.EstTotalEmployees;
                ws.Cell(row, 22).Value = item.EstStatus != SD.StatusApproved ?"": item.EstPoHeadEvalDate ;
                ws.Cell(row, 22).Style.DateFormat.Format = "d-mmm-yy";
                ws.Cell(row, 23).Value = getProccessCycleTimeDifference(item.EstRegistrationDate, item.EstPoHeadEvalDate, item.EstStatus);
                ;

                row++;
            }

            var lastDataRow = Math.Max(1, row - 1);

            ws.Columns(1, 23).Width = 21.57;

            ws.Rows(1, lastDataRow).Height = 29.25;
            var reportRange = ws.Range(1, 1, lastDataRow, 23);
            reportRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            reportRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            reportRange.Style.Alignment.WrapText = true;
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static XLWorkbook CreateDefaultWorkbook()
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Monitoring");

            var headers = new[]
            {
                "NO","DATE FILED", "CONTROL NO.", "BUSINESS NAME", "ADDRESS", "BARANGAY", "CITY/ MUNICIPALITY",
                "PROVINCE", "CONTACTS","","", "NAME OF MANAGER/OWNER", "Main Economic Activity",
                "LEGAL ORGANIZATION", "ECONOMIC ORGANIZATION", "PEZA REGISTERED", "TOTAL EMPLOYMENT", "","","","","APPROVAL DATE","PCT"
            };
            var subHeaders = new[]
            {
                "","", "", "", "", "", "",
                "", "Tel/Cell No.","Fax No.","E-MAIL Address", "", "",
                "", "","", "Regular", "Non-Regular","Male","Female", "Total","",""
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
            }

            for (var i = 0; i < subHeaders.Length; i++)
            {
                ws.Cell(2, i + 1).Value = subHeaders[i];
            }

            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(2);

            return wb;
        }

        #region CUSTOM CLASS
        private static int getProccessCycleTimeDifference(DateTime startDate, DateTime? endDate, string status)
        {
            if (status != SD.StatusApproved || !endDate.HasValue)
            {
                return 0;
            }

            var start = startDate.Date;
            var end = endDate.Value.Date;

            if (end <= start)
            {
                return 0;
            }

            var count = 0;
            for (var day = start; day < end; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                {
                    count++;
                }
            }

            return count;
        }
        #endregion
    }
}
