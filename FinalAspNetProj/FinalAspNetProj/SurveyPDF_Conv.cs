using FinalAspNetProj.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinalAspNetProj.Documents
{
    public class SurveyReportDocument : IDocument
    {
        public List<Survey> Surveys { get; }

        public SurveyReportDocument(List<Survey> surveys)
        {
            Surveys = surveys;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Survey Response Report")
                        .FontSize(20).Bold();

                    column.Item().Text($"Generated on: {System.DateTime.Now:yyyy-MM-dd}")
                        .FontSize(9);
                });
            });
        }

        [Obsolete]
        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn();
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("ID");
                        header.Cell().Text("Respondent");
                        header.Cell().Text("Date");
                        header.Cell().Text("Score (%)");
                    });

                    foreach (var survey in Surveys)
                    {
                        table.Cell().Text(survey.SurveyId);
                        table.Cell().Text(survey.RespondentName);
                        table.Cell().Text(survey.DateCompleted.ToString("yyyy-MM-dd"));
                        table.Cell().Text(survey.SurveyAnalysis?.PercentageScore.ToString("F2") ?? "N/A");
                    }
                });
            });
        }
    }
}