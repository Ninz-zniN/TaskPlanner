using OfficeOpenXml;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TaskPlannerClient.Service
{
    /// <summary>
    /// Предоставляет методы для экспорта данных в форматы Excel (.xlsx) и PDF (.pdf).
    /// </summary>
    public class MyColumnDefinition
    {
        public string Header { get; set; }      // отображаемое название колонки
        public double Weight { get; set; } = 1; // относительная ширина (по умолчанию 1)
    }
    /// <summary>
    /// Предоставляет методы для экспорта данных в форматы Excel (.xlsx) и PDF (.pdf).
    /// </summary>
    public class ExportService
    {
        /// <summary>
        /// Экспортирует коллекцию элементов в Excel-файл.
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции.</typeparam>
        /// <param name="items">Данные для экспорта.</param>
        /// <param name="columns">
        ///     Определения колонок: ключ — путь к свойству, значение — ColumnDefinition.
        ///     Свойство Weight игнорируется, так как Excel сам управляет шириной колонок.
        /// </param>
        /// <param name="title">Заголовок отчёта (имя листа).</param>
        /// <param name="headerSummary">
        ///     Дополнительный текст, выводимый перед таблицей. Может содержать символы переноса строки.
        /// </param>
        public static void ExportToExcel<T>(
            IEnumerable<T> items,
            Dictionary<string, MyColumnDefinition> columns,
            string title = "Отчёт",
            string? headerSummary = null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{title}_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            ExcelPackage.License.SetNonCommercialOrganization("TaskPlanner");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(title);

            int row = 1;

            if (!string.IsNullOrWhiteSpace(headerSummary))
            {
                // Убираем пустые строки в начале и конце
                var cleanSummary = headerSummary.Trim('\n', '\r');
                if (!string.IsNullOrWhiteSpace(cleanSummary))
                {
                    var headerCell = worksheet.Cells[row, 1];
                    headerCell.Value = cleanSummary;
                    headerCell.Style.Font.Size = 10;
                    headerCell.Style.WrapText = true;
                    worksheet.Cells[row, 1, row, columns.Count].Merge = true;

                    int lineCount = cleanSummary.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    double singleLineHeight = 15; // примерная высота строки в пунктах
                    worksheet.Row(row).CustomHeight = true; // разрешаем ручную высоту
                    worksheet.Row(row).Height = lineCount * singleLineHeight;

                    row += 2;
                }
            }

            // Заголовки таблицы
            int col = 1;
            foreach (var colDef in columns.Values)
            {
                worksheet.Cells[row, col].Value = colDef.Header;
                worksheet.Cells[row, col].Style.Font.Bold = true;
                col++;
            }
            row++;

            // Данные
            foreach (var item in items)
            {
                col = 1;
                foreach (var propName in columns.Keys)
                {
                    var value = GetNestedPropertyValue(item, propName);
                    worksheet.Cells[row, col].Value = value;
                    col++;
                }
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            package.SaveAs(new FileInfo(dialog.FileName));
            MessageBox.Show("Excel-файл сохранён.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static object GetNestedPropertyValue(object obj, string path)
        {
            if (obj == null) return null;

            // Обработка составных свойств (через '+') – без изменений
            if (path.Contains('+'))
            {
                var parts = path.Split('+', StringSplitOptions.RemoveEmptyEntries);
                var values = new List<string>();
                foreach (var part in parts)
                {
                    var trimmedPart = part.Trim();
                    if (trimmedPart.StartsWith("\"") && trimmedPart.EndsWith("\""))
                    {
                        values.Add(trimmedPart.Trim('"'));
                    }
                    else
                    {
                        var val = GetNestedPropertyValue(obj, trimmedPart);
                        if (val != null)
                            values.Add(val.ToString());
                    }
                }
                return string.Join("", values);
            }

            // Обычный доступ к свойству
            foreach (var part in path.Split('.'))
            {
                if (obj == null) return null;
                var prop = obj.GetType().GetProperty(part);
                if (prop == null) return part;
                obj = prop.GetValue(obj);
            }

            // ← Форматируем DateTime, если добрались до конечного значения
            if (obj is DateTime dt)
                return dt.ToString("dd.MM.yyyy");

            return obj;
        }

        /// <summary>
        /// Экспортирует коллекцию элементов в PDF-файл.
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции.</typeparam>
        /// <param name="items">Данные для экспорта.</param>
        /// <param name="columns">
        ///     Определения колонок: ключ — путь к свойству (может быть вложенный
        ///     или составной через '+'), значение — объект ColumnDefinition с заголовком и относительной шириной.
        /// </param>
        /// <param name="title">Заголовок отчёта.</param>
        /// <param name="landscape">
        ///     Альбомная ориентация. Если не задана, автоматически выбирается при количестве колонок больше 4.
        /// </param>
        /// <param name="headerSummary">
        ///     Дополнительный текст, выводимый перед таблицей. Может содержать символы переноса строки.
        /// </param>
        public static void ExportToPdf<T>(
            IEnumerable<T> items,
            Dictionary<string, MyColumnDefinition> columns,
            string title = "Отчёт",
            bool? landscape = null,
            string? headerSummary = null)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"{title}_{DateTime.Now:yyyy-MM-dd}.pdf"
            };
            if (dialog.ShowDialog() != true) return;

            bool useLandscape = landscape ?? columns.Count > 4;
            double pageWidth = useLandscape ? 842 : 595;
            double pageHeight = useLandscape ? 595 : 842;

            using var document = new PdfDocument();
            var page = document.AddPage();
            page.Width = pageWidth;
            page.Height = pageHeight;
            var gfx = XGraphics.FromPdfPage(page);
            var fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
            var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
            var fontCell = new XFont("Arial", 9, XFontStyle.Regular);
            var fontSummary = new XFont("Arial", 10, XFontStyle.Regular);

            double margin = 40;
            double y = margin;

            // Заголовок отчёта
            gfx.DrawString(title, fontTitle, XBrushes.Black,
                new XRect(margin, y, pageWidth - 2 * margin, 20), XStringFormats.TopLeft);
            y += 30;

            // Дополнительная информация
            if (!string.IsNullOrWhiteSpace(headerSummary))
            {
                double lineHeight = fontSummary.Height + 2;
                string[] lines = headerSummary.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    gfx.DrawString(line, fontSummary, XBrushes.Black,
                        new XRect(margin, y, pageWidth - 2 * margin, lineHeight), XStringFormats.TopLeft);
                    y += lineHeight;
                }
                y += 10*3; // дополнительный отступ после всей сводки
            }

            // Ширина колонок по весам
            double totalWeight = columns.Values.Sum(c => c.Weight);
            double availableWidth = pageWidth - 2 * margin;
            double[] widths = new double[columns.Count];
            int idx = 0;
            foreach (var col in columns.Values)
            {
                widths[idx] = availableWidth * col.Weight / totalWeight;
                idx++;
            }

            // Заголовки таблицы
            double x = margin;
            idx = 0;
            foreach (var col in columns.Values)
            {
                gfx.DrawString(col.Header, fontHeader, XBrushes.Black,
                    new XRect(x, y, widths[idx], 20), XStringFormats.TopLeft);
                x += widths[idx];
                idx++;
            }
            y += 20;

            // Строки данных с переносом
            foreach (var item in items)
            {
                double maxLineHeight = 0;
                var linesPerColumn = new List<string[]>(new string[columns.Count][]);

                // Считаем, сколько строк нужно для каждой ячейки
                idx = 0;
                foreach (var propName in columns.Keys)
                {
                    var value = GetNestedPropertyValue(item, propName)?.ToString() ?? "";
                    linesPerColumn[idx] = SplitTextToLines(value, fontCell, widths[idx], gfx);
                    double cellHeight = linesPerColumn[idx].Length * fontCell.Height;
                    if (cellHeight > maxLineHeight) maxLineHeight = cellHeight;
                    idx++;
                }

                // Если не помещается на текущей странице – создаём новую
                if (y + maxLineHeight > pageHeight - margin)
                {
                    page = document.AddPage();
                    page.Width = pageWidth;
                    page.Height = pageHeight;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                // Рисуем все строки
                idx = 0;
                double rowStartX = margin;
                foreach (var propName in columns.Keys)
                {
                    var lines = linesPerColumn[idx];
                    double cellY = y;
                    foreach (var line in lines)
                    {
                        gfx.DrawString(line, fontCell, XBrushes.Black,
                            new XRect(rowStartX, cellY, widths[idx], fontCell.Height), XStringFormats.TopLeft);
                        cellY += fontCell.Height;
                    }
                    rowStartX += widths[idx];
                    idx++;
                }
                y += maxLineHeight + 2;
            }

            document.Save(dialog.FileName);
            MessageBox.Show("PDF-файл сохранён.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Разбивает текст на строки, чтобы каждая строка по ширине не превышала maxWidth.
        /// </summary>
        private static string[] SplitTextToLines(string text, XFont font, double maxWidth, XGraphics gfx)
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            string currentLine = "";
            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                if (gfx.MeasureString(testLine, font).Width > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);
            return lines.ToArray();
        }
    }
}
