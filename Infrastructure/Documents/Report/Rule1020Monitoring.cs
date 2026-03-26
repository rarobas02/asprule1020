using asprule1020.Models;
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
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "1020Monitoring.xlsx");

            using var workbook = File.Exists(templatePath)
                ? new XLWorkbook(templatePath)
                : CreateDefaultWorkbook();

            var ws = workbook.Worksheet(1);
            var startRow = 2; // row 1 = headers in template/default

            var lastUsedRow = ws.LastRowUsed()?.RowNumber() ?? startRow;
            if (lastUsedRow >= startRow)
            {
                ws.Range(startRow, 1, lastUsedRow, 20).Clear(XLClearOptions.Contents);
            }

            var row = startRow;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = item.TransId;
                ws.Cell(row, 2).Value = item.Rule1020Id ?? string.Empty;
                ws.Cell(row, 3).Value = item.EstName;
                ws.Cell(row, 4).Value = item.EstProvince;
                ws.Cell(row, 5).Value = item.EstCityMun;
                ws.Cell(row, 6).Value = item.EstBrgy;
                ws.Cell(row, 7).Value = item.EstStreet;
                ws.Cell(row, 8).Value = $"{item.EstOwnerFirst} {item.EstOwnerMid} {item.EstOwnerLast}".Replace("  ", " ").Trim();
                ws.Cell(row, 9).Value = item.EstPhone;
                ws.Cell(row, 10).Value = item.EstBusinessNature;
                ws.Cell(row, 11).Value = item.EstMaleCount;
                ws.Cell(row, 12).Value = item.EstFemaleCount;
                ws.Cell(row, 13).Value = item.EstTotalEmployees;
                ws.Cell(row, 14).Value = item.EstStatus;
                ws.Cell(row, 15).Value = item.EstRegistrationDate;
                ws.Cell(row, 15).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(row, 16).Value = item.EstEvalName ?? string.Empty;
                ws.Cell(row, 17).Value = item.EstEvalDate;
                ws.Cell(row, 17).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(row, 18).Value = item.EstPoHeadName ?? string.Empty;
                ws.Cell(row, 19).Value = item.EstPoHeadEvalDate;
                ws.Cell(row, 19).Style.DateFormat.Format = "yyyy-mm-dd";
                ws.Cell(row, 20).Value = item.EstPoHeadRemarks ?? string.Empty;

                row++;
            }

            var lastDataRow = Math.Max(1, row - 1);

            ws.Columns(1, 20).Width = 21.57;

            ws.Rows(1, lastDataRow).Height = 29.25;
            var reportRange = ws.Range(1, 1, lastDataRow, 20);
            reportRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            reportRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
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
                "Transaction No", "Rule 1020 No", "Establishment Name", "Province", "City/Municipality",
                "Barangay", "Street", "Owner", "Phone", "Business Nature",
                "Male", "Female", "Total Employees", "Status", "Registration Date",
                "Evaluator", "Evaluation Date", "PO Head", "PO Head Eval Date", "Remarks"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
            }

            ws.Row(1).Style.Font.Bold = true;
            ws.Range(1, 1, 1, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            return wb;
        }
    }
}
