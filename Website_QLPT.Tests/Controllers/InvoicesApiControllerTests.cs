using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Website_QLPT.Controllers.Api;
using Website_QLPT.Models;
using Website_QLPT.Services.Export;
using Website_QLPT.Tests.Helpers;

namespace Website_QLPT.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho InvoicesApiController — TC-INV-001 đến TC-INV-004
    /// </summary>
    public class InvoicesApiControllerTests
    {
        private readonly Mock<IPdfExportService> _pdfServiceMock;

        public InvoicesApiControllerTests()
        {
            _pdfServiceMock = new Mock<IPdfExportService>();
        }

        /// <summary>
        /// TC-INV-004: Export PDF for valid invoice → returns PDF file
        /// </summary>
        [Fact]
        public async Task ExportPdf_ValidInvoice_ReturnsPdfFile()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var fakePdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF header
            _pdfServiceMock.Setup(x => x.GenerateInvoicePdf(It.IsAny<Invoice>())).Returns(fakePdf);

            var controller = new InvoicesApiController(context, _pdfServiceMock.Object);

            // Act
            var result = await controller.ExportPdf(1);

            // Assert
            var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileContents.Should().BeEquivalentTo(fakePdf);
            fileResult.FileDownloadName.Should().Contain("HoaDon");
        }

        /// <summary>
        /// TC-INV-004b: Export PDF for non-existent invoice → 404
        /// </summary>
        [Fact]
        public async Task ExportPdf_InvalidInvoiceId_ReturnsNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new InvoicesApiController(context, _pdfServiceMock.Object);

            // Act
            var result = await controller.ExportPdf(99999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = (NotFoundObjectResult)result;
            notFound.StatusCode.Should().Be(404);
        }

        /// <summary>
        /// TC-INV-004c: PDF generation throws exception → 500
        /// </summary>
        [Fact]
        public async Task ExportPdf_ServiceThrowsException_Returns500()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            _pdfServiceMock.Setup(x => x.GenerateInvoicePdf(It.IsAny<Invoice>()))
                .Throws(new InvalidOperationException("PDF generation failed"));

            var controller = new InvoicesApiController(context, _pdfServiceMock.Object);

            // Act
            var result = await controller.ExportPdf(1);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        /// <summary>
        /// Verify Invoice model has all required fields
        /// </summary>
        [Fact]
        public void InvoiceModel_HasRequiredProperties()
        {
            // Arrange & Act
            var invoice = new Invoice
            {
                ContractId = 1,
                Month = 3,
                Year = 2026,
                RoomFee = 3_500_000,
                ElectricityOld = 100,
                ElectricityNew = 145,
                ElectricityPrice = 3500,
                WaterOld = 20,
                WaterNew = 26,
                WaterPrice = 15000,
                Status = InvoiceStatus.Unpaid
            };

            // Assert
            invoice.ContractId.Should().Be(1);
            invoice.Month.Should().Be(3);
            invoice.Year.Should().Be(2026);
            invoice.Status.Should().Be(InvoiceStatus.Unpaid);
            invoice.RoomFee.Should().Be(3_500_000);

            // Verify electricity usage calculation
            var electricityUsed = invoice.ElectricityNew - invoice.ElectricityOld;
            electricityUsed.Should().Be(45);

            // Verify water usage calculation
            var waterUsed = invoice.WaterNew - invoice.WaterOld;
            waterUsed.Should().Be(6);
        }
    }
}
