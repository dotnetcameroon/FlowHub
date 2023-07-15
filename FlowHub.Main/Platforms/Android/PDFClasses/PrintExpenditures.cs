﻿using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using Color = iText.Kernel.Colors.Color;
using static FlowHub.Main.AdditionalResourcefulApiClasses.ExchangeRateAPI;
using FlowHub.Main.AdditionalResourcefulApiClasses;

namespace FlowHub.Main.PDF_Classes;

//Printing on Android
public static class PrintExpenditures
{
    public static async Task SaveExpenditureToPDF(ObservableCollection<ExpendituresModel> expList, string userCurrency, string printDisplayCurrency, string userName)
    {
        ConvertedRate ObjectWithRate = new() { result = 1, date = DateTime.UtcNow };

        if (!userCurrency.Equals(printDisplayCurrency))
        {
            ExchangeRateAPI JsonWithRates = new();
            ObjectWithRate = JsonWithRates.GetConvertedRate(userCurrency, printDisplayCurrency);
        }

        string path;
        path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string fileName = $"FlowOutsReport_{userCurrency}_{DateTime.Now:ddd, dd MMMM yyyy}.pdf";
        string PathFile = $"{path}/{fileName}";

        const string PdfTitle = "Flow Outs Report";

        await Task.Run(() => CreatePdfDoc(expList, PathFile, userCurrency, printDisplayCurrency, ObjectWithRate.result, ObjectWithRate.date, PdfTitle, userName));
    }

    static async Task CreatePdfDoc(ObservableCollection<ExpendituresModel> expList, string PathFile, string userCurrency, string printDisplayCurrency, double rate, DateTime dateOfRateUpdate, string pdfTitle, string username)
    {
        Color HeaderTextColor = WebColors.GetRGBColor("darkslateblue");

        using PdfWriter writer = new(PathFile);
        using PdfDocument pdf = new(writer);
        Document document = new(pdf, pageSize: iText.Kernel.Geom.PageSize.A4, immediateFlush: false);
        Paragraph header = new Paragraph(pdfTitle)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(HeaderTextColor)
            .SetBold()
            .SetFontSize(20);
        document.Add(header);
        document.Flush();

        Table table = new Table(4, false).UseAllAvailableWidth();

        table.AddHeaderCell("#")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Description")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Amount Spent")
                .SetTextAlignment(TextAlignment.CENTER);
        table.AddHeaderCell("Date Spent")
                .SetTextAlignment(TextAlignment.CENTER);

        double totall = 0;
        foreach (var item in expList)
        {
            double amount = item.AmountSpent * rate;

            table.AddCell(new Paragraph($"{expList.IndexOf(item) + 1}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.Reason}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{amount:n2} {printDisplayCurrency}")
                .SetTextAlignment(TextAlignment.CENTER));

            table.AddCell(new Paragraph($"{item.DateSpent.ToShortDateString()}")
                .SetTextAlignment(TextAlignment.CENTER));

            totall += amount;
        }

        document.Add(table);
        document.Flush();

        Paragraph footerText = new Paragraph($"Total Spent: {totall:n2} {printDisplayCurrency}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(24)
            .SetBold();

        Paragraph waterMarkText = new Paragraph($"Report Generated by FlowHub App for {username}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(15);

        Paragraph bottomNotesText = new Paragraph($"Converted using the rate of 1 {userCurrency} = {rate:n2} {printDisplayCurrency} \nRate updated on {dateOfRateUpdate:D}")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(10);

        document.Add(new Paragraph());
        document.Flush();

        document.Add(footerText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(waterMarkText);
        document.Flush();

        document.Add(new Paragraph());
        document.Flush();

        document.Add(bottomNotesText);
        document.Flush();

        int numberPages = pdf.GetNumberOfPages();
        for (int i = 1; i <= numberPages; i++)
        {
            document.ShowTextAligned(new Paragraph(string
               .Format("Page" + i + " of " + numberPages)),
               559, 806, i, TextAlignment.LEFT,
               iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
        }

        document.Close();
        await SharePdfFile(pdfTitle, PathFile);
    }

    static async Task SharePdfFile(string PdfTitle, string FilePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = PdfTitle,
            File = new ShareFile(FilePath)
        });
    }
}
