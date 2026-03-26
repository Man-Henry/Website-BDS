using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Website_QLPT.Models;

namespace Website_QLPT.Services.Export
{
    public interface IPdfExportService
    {
        byte[] GenerateInvoicePdf(Invoice invoice);
    }

    public class PdfExportService : IPdfExportService
    {
        public PdfExportService()
        {
            // Set license type for QuestPDF 2022.12+ 
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoicePdf(Invoice invoice)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(header => ComposeHeader(header, invoice));
                    page.Content().Element(content => ComposeContent(content, invoice));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container, Invoice invoice)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"HÓA ĐƠN TIỀN NHÀ THÁNG {invoice.Month}/{invoice.Year}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);

                    var propertyName = invoice.Contract?.Room?.Property?.Name ?? "Nhà trọ của chúng tôi";
                    column.Item().Text(propertyName).FontSize(14);
                    column.Item().Text($"Phòng: {invoice.Contract?.Room?.Name ?? "N/A"}");
                    column.Item().Text($"Ngày lập: {invoice.CreatedAt:dd/MM/yyyy}");
                });
            });
        }

        private void ComposeContent(IContainer container, Invoice invoice)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                // Thông tin khách thuê
                var tenantName = invoice.Contract?.Tenant?.FullName ?? "Khách thuê";
                column.Item().Text($"Kính gửi: Ông/Bà {tenantName}").SemiBold();
                column.Item().PaddingBottom(10).Text("Chi tiết các khoản phí tháng này như sau:");

                // Bảng chi tiết
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("#").SemiBold();
                        header.Cell().Text("Khoản mục").SemiBold();
                        header.Cell().AlignRight().Text("Ghi chú / CS Mới-Cũ").SemiBold();
                        header.Cell().AlignRight().Text("Thành tiền (VND)").SemiBold();

                        header.Cell().ColumnSpan(4).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    // Tiền phòng
                    table.Cell().Text("1");
                    table.Cell().Text("Tiền thuê phòng");
                    table.Cell().AlignRight().Text("");
                    table.Cell().AlignRight().Text($"{invoice.RoomFee:N0}");

                    // Tiền điện
                    table.Cell().Text("2");
                    table.Cell().Text("Tiền điện");
                    table.Cell().AlignRight().Text($"{invoice.ElectricityNew} - {invoice.ElectricityOld}");
                    table.Cell().AlignRight().Text($"{invoice.ElectricityFee:N0}");

                    // Tiền nước
                    table.Cell().Text("3");
                    table.Cell().Text("Tiền nước");
                    table.Cell().AlignRight().Text($"{invoice.WaterNew} - {invoice.WaterOld}");
                    table.Cell().AlignRight().Text($"{invoice.WaterFee:N0}");

                    // Phí khác
                    if (invoice.OtherFee > 0)
                    {
                        table.Cell().Text("4");
                        table.Cell().Text("Các khoản phụ phí");
                        table.Cell().AlignRight().Text(invoice.OtherFeeNote ?? "");
                        table.Cell().AlignRight().Text($"{invoice.OtherFee:N0}");
                    }

                    // Tổng cộng
                    table.Cell().ColumnSpan(4).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    
                    table.Cell().ColumnSpan(3).AlignRight().Text("Tổng thanh toán:").SemiBold().FontSize(14);
                    table.Cell().AlignRight().Text($"{invoice.TotalAmount:N0}").SemiBold().FontSize(14).FontColor(Colors.Red.Medium);
                });

                // Nếu có diễn giải tính tiền điện
                if (!string.IsNullOrEmpty(invoice.ElectricityCalculationDetails))
                {
                    column.Item().PaddingTop(15).Text("Bảng kê chi tiết tiền điện:").Italic();
                    column.Item().Text(invoice.ElectricityCalculationDetails).FontSize(10).FontColor(Colors.Grey.Darken1);
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Trang ");
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        }
    }
}
