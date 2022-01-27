using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using PdfSharp.Xps;

namespace report
{
	
    public static class pdfwriter
    {
        const int DIUPerInch = 96;
        private static readonly Thickness defaultMargin = new Thickness(25);

        public static void ExportReportAsPdf(
            StackPanel reportContainer,
            object dataContext,
            Thickness margin,
            ReportOrientation orientation,
            ResourceDictionary resourceDictionary = null,
            Brush backgroundBrush = null,
            DataTemplate reportHeaderDataTemplate = null,
            bool headerOnlyOnTheFirstPage = false,
            DataTemplate reportFooterDataTemplate = null,
            bool footerStartsFromTheSecondPage = false)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".pdf",
                Filter = "PDF Documents (.pdf)|*.pdf"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result != true) return;

            Size reportSize = GetReportSize(reportContainer, margin, orientation);

            List<FrameworkElement> ReportElements = new List<FrameworkElement>(reportContainer.Children.Cast<FrameworkElement>());
            reportContainer.Children.Clear(); //to avoid exception "Specified element is already the logical child of another element."

            List<ReportPage> ReportPages =
                GetReportPages(
                    resourceDictionary,
                    backgroundBrush,
                    ReportElements,
                    dataContext,
                    margin,
                    reportSize,
                    reportHeaderDataTemplate,
                    headerOnlyOnTheFirstPage,
                    reportFooterDataTemplate,
                    footerStartsFromTheSecondPage);

            FixedDocument fixedDocument = new FixedDocument();

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(memoryStream, FileMode.Create);
                    XpsDocument xpsDocument = new XpsDocument(package);
                    XpsDocumentWriter xpsDocumentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);

                    foreach (Grid reportPage in ReportPages.Select(reportPage => reportPage.LayoutRoot))
                    {
                        reportPage.Width = reportPage.ActualWidth;
                        reportPage.Height = reportPage.ActualHeight;

                        FixedPage newFixedPage = new FixedPage();
                        newFixedPage.Children.Add(reportPage);
                        newFixedPage.Measure(reportSize);
                        newFixedPage.Arrange(new Rect(reportSize));
                        newFixedPage.Width = newFixedPage.ActualWidth;
                        newFixedPage.Height = newFixedPage.ActualHeight;
                        newFixedPage.Background = backgroundBrush;
                        newFixedPage.UpdateLayout();

                        PageContent pageContent = new PageContent();
                        ((IAddChild)pageContent).AddChild(newFixedPage);

                        fixedDocument.Pages.Add(pageContent);
                    }

                    xpsDocumentWriter.Write(fixedDocument);
                    xpsDocument.Close();
                    package.Close();

                    var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(memoryStream); 
                    try
                    {
                        XpsConverter.Convert(pdfXpsDoc, saveFileDialog.FileName, 0);
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("File write exception. \nif the same name file is opening, please close it before export pdf file.", "File write Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                        
                }
            }
            finally
            {
                ReportPages.ForEach(reportPage => reportPage.ClearChildren());
                ReportElements.ForEach(elm => reportContainer.Children.Add(elm));
                reportContainer.UpdateLayout();
            }
        }
        private static List<ReportPage> GetReportPages(
        ResourceDictionary resourceDictionary,
        Brush backgroundBrush,
        List<FrameworkElement> ReportElements,
        object dataContext,
        Thickness margin,
        Size reportSize,
        DataTemplate reportHeaderDataTemplate,
        bool headerOnlyOnTheFirstPage,
        DataTemplate reportFooterDataTemplate,
        bool footerStartsFromTheSecondPage)
        {
            int pageNumber = 1;

            List<ReportPage> ReportPages =
                new List<ReportPage>
                {
                new ReportPage(
                    reportSize,
                    backgroundBrush,
                    margin,
                    dataContext,
                    resourceDictionary,
                    (headerOnlyOnTheFirstPage) ? null : reportHeaderDataTemplate,
                    (footerStartsFromTheSecondPage) ? null : reportFooterDataTemplate,
                    pageNumber)
                };

            foreach (FrameworkElement reportVisualElement in ReportElements)
            {
                if (ReportPages.Last().GetChildrenActualHeight() + GetActualHeightPlusMargin(reportVisualElement) > reportSize.Height - margin.Top - margin.Bottom)
                {
                    pageNumber++;

                    ReportPages.Add(
                        new ReportPage(
                            reportSize,
                            backgroundBrush,
                            margin,
                            dataContext,
                            resourceDictionary,
                            (headerOnlyOnTheFirstPage) ? reportHeaderDataTemplate : null,
                            reportFooterDataTemplate,
                            pageNumber));
                }

                ReportPages.Last().AddElement(reportVisualElement);
            }

            foreach (ReportPage reportPage in ReportPages)
            {
                reportPage.LayoutRoot.Measure(reportSize);
                reportPage.LayoutRoot.Arrange(new Rect(reportSize));
                reportPage.LayoutRoot.UpdateLayout();
            }

            return ReportPages;
        }

        private static Size GetReportSize(StackPanel reportContainer, Thickness margin, ReportOrientation orientation, PrintDialog printDialog = null)
        {
            if (printDialog == null)
                printDialog = new PrintDialog();

            double reportWidth = reportContainer.ActualWidth + margin.Left + margin.Right;

            if (orientation == ReportOrientation.Horizontal)
                printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

            double reportHeight = (reportWidth / printDialog.PrintableAreaWidth) * printDialog.PrintableAreaHeight;

            return new Size(reportWidth, reportHeight);
        }

        private static double GetActualHeightPlusMargin(FrameworkElement elm)
        {
            return elm.ActualHeight + elm.Margin.Top + elm.Margin.Bottom;
        }
    }        
    
    public enum ReportOrientation
    {
        Horizontal,
        Vertical
    }
}
