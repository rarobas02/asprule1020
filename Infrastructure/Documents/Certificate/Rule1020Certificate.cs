using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace asprule1020.DataAccess.Documents.Certificate
{
    public class Rule1020Certificate
    {
        private readonly IWebHostEnvironment _env;

        public Rule1020Certificate(IWebHostEnvironment env)
        {
            _env = env;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var headerPath = Path.Combine(_env.WebRootPath, "img/Admin/header.png");
            var footerPath = Path.Combine(_env.WebRootPath, "img/Admin/footer.png");

            const float headerHeight = 120;
            const float footerHeight = 95;

            string transId = "RO4A-1234-1431-4646";
            var estName = "BSKP CONSTRUCTION ASDFSDFDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDASDAS";
            var estAddress = "Apple st. California, United States of America";

            DateTime regDate = DateTime.Now;
            var givenDateText = $"Given this day 5th of {regDate:MMMM yyyy}, Sta.Mesa, Manila.";
            string directorName = "done done";

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman"));

                page.Header()
                    .Height(headerHeight)
                    .AlignTop()
                    .ExtendHorizontal()
                    .Image(headerPath)
                    .FitArea();

                page.Footer()
                    .Height(footerHeight)
                    .AlignBottom()
                    .ExtendHorizontal()
                    .Image(footerPath)
                    .FitArea();

                page.Content()
                    .PaddingHorizontal(20)
                    .Column(col =>
                    {
                        col.Spacing(0);
                        col.Item().PaddingTop(0);
                        col.Item().AlignCenter().Text("CERTIFICATE OF REGISTRATION")
                            .FontSize(21.94f).Bold();

                        col.Item().AlignCenter().Text(transId)
                            .FontSize(12.63f);

                        col.Item().PaddingTop(50);

                        col.Item().AlignCenter().Text("This is to certify that")
                            .FontSize(11.63f);

                        col.Item().PaddingTop(40);

                        col.Item().AlignCenter().Text(estName)
                            .FontSize(23.94f).Bold();

                        col.Item().PaddingTop(8);

                        col.Item().AlignCenter().Text(estAddress)
                            .FontSize(23.94f);

                        col.Item().PaddingTop(23.94f);

                        col.Item().AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(11.97f));
                            text.Line("Is registered under the Rule 1020 of the Occupational Safety and Health Standards pursuant to");
                            text.Line("Article 162 of the Labor Code of the Philippines, as amended.");
                        });

                        col.Item().PaddingTop(35);

                        col.Item().AlignCenter().Text(givenDateText)
                            .FontSize(11.97f);

                        col.Item().PaddingTop(70);

                        col.Item().AlignCenter().Text(directorName.ToUpper())
                            .FontSize(11.97f).Bold();

                        col.Item().AlignCenter().Text("Director")
                            .FontSize(11.97f).Bold();

                        col.Item().PaddingTop(18);

                        col.Item().AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(11.97f));
                            text.Line("Registration is valid for the lifetime of the establishment except when there is a change of name,");
                            text.Line("location, ownership, and opening after previous closing, re-registration is needed.");
                            text.Line("ALL RULE 1020 TRANSACTIONS ARE FREE OF CHARGE");
                        });
                    });
            });
        }
    }
}
