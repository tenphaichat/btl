using ClosedXML.Excel;

namespace QLCuaHangSach.Utils;

/// <summary>
/// Tiện ích xuất bảng dữ liệu DataTable / DataGridView ra tệp Excel (.xlsx).
/// </summary>
public static class ExcelExportHelper
{
    /// <summary>Class Method 8: Xuất DataTable ra tệp Excel có định dạng tiêu đề và bảng biểu.</summary>
    public static void ExportDataTableToExcel(System.Data.DataTable table, string filePath, string reportTitle, string subTitle = "")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("BaoCao");

        int colCount = Math.Max(1, table.Columns.Count);
        ws.Cell(1, 1).Value = "NHÀ SÁCH ĐÔNG Á - HỆ THỐNG QUẢN LÝ CỬA HÀNG SÁCH";
        ws.Range(1, 1, 1, colCount).Merge().Style.Font.SetBold().Font.SetFontSize(12);

        ws.Cell(2, 1).Value = reportTitle.ToUpperInvariant();
        ws.Range(2, 1, 2, colCount).Merge().Style
            .Font.SetBold().Font.SetFontSize(14)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        if (!string.IsNullOrWhiteSpace(subTitle))
        {
            ws.Cell(3, 1).Value = subTitle;
            ws.Range(3, 1, 3, colCount).Merge().Style
                .Font.SetItalic().Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        int headerRow = 5;
        for (int c = 0; c < table.Columns.Count; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = table.Columns[c].ColumnName;
            cell.Style.Font.SetBold();
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#1E3A8A"));
            cell.Style.Font.SetFontColor(XLColor.White);
            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        for (int r = 0; r < table.Rows.Count; r++)
        {
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var cell = ws.Cell(headerRow + 1 + r, c + 1);
                object val = table.Rows[r][c];
                if (val == null || val == DBNull.Value)
                    cell.Value = "";
                else if (val is decimal dec)
                {
                    cell.Value = dec;
                    cell.Style.NumberFormat.Format = "#,##0";
                }
                else if (val is int or long or double)
                    cell.Value = Convert.ToDouble(val);
                else if (val is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                }
                else
                    cell.Value = val.ToString();

                cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            }
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }
}
