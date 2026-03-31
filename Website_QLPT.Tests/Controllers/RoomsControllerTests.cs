using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Website_QLPT.Tests.Helpers;
using Website_QLPT.Models;
using ApiRoomsController = Website_QLPT.Controllers.Api.RoomsController;

namespace Website_QLPT.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho API RoomsController — TC-ROOM-001 đến TC-ROOM-005
    /// </summary>
    public class RoomsControllerTests
    {
        /// <summary>
        /// TC-ROOM-001: Get all available rooms → 200 + paginated list
        /// </summary>
        [Fact]
        public async Task GetAvailableRooms_ReturnsOkWithPaginatedRooms()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Act
            var result = await controller.GetAvailableRooms(null, null, null, 1, 10);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            var value = okResult.Value!;
            var countProp = value.GetType().GetProperty("count");
            countProp.Should().NotBeNull();
            var count = (int)countProp!.GetValue(value)!;
            count.Should().BeGreaterThan(0, "database should have available rooms");
        }

        /// <summary>
        /// TC-ROOM-001b: Get rooms with price filter → filtered results
        /// </summary>
        [Fact]
        public async Task GetAvailableRooms_WithPriceFilter_ReturnsFilteredResults()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Act - filter rooms with price <= 3,500,000
            var result = await controller.GetAvailableRooms(priceMax: 3_500_000, null, null, 1, 10);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = okResult.Value!;
            var dataProp = value.GetType().GetProperty("data");
            dataProp.Should().NotBeNull();
        }

        /// <summary>
        /// TC-ROOM-001c: Get rooms with property filter → scoped results
        /// </summary>
        [Fact]
        public async Task GetAvailableRooms_WithPropertyFilter_ReturnsScopedResults()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Act - filter rooms in property 1
            var result = await controller.GetAvailableRooms(null, null, propertyId: 1, 1, 10);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
        }

        /// <summary>
        /// TC-ROOM-002: Get room by valid ID → 200 + room details
        /// </summary>
        [Fact]
        public async Task GetRoom_ValidId_ReturnsOkWithRoomDetails()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Room 2 is Available (in our test data)
            // Act
            var result = await controller.GetRoom(2);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().NotBeNull();
        }

        /// <summary>
        /// TC-ROOM-005: Get room by invalid ID → 404 Not Found
        /// </summary>
        [Fact]
        public async Task GetRoom_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Act
            var result = await controller.GetRoom(99999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFound = (NotFoundObjectResult)result;
            notFound.StatusCode.Should().Be(404);
        }

        /// <summary>
        /// TC-ROOM: Pagination respects page size limits (max 50)
        /// </summary>
        [Fact]
        public async Task GetAvailableRooms_ExcessivePageSize_ClampedTo50()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateWithSeedData();
            var controller = new ApiRoomsController(context);

            // Act - request page size of 1000
            var result = await controller.GetAvailableRooms(null, null, null, 1, 1000);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = okResult.Value!;
            var pageSizeProp = value.GetType().GetProperty("pageSize");
            pageSizeProp.Should().NotBeNull();
            var pageSize = (int)pageSizeProp!.GetValue(value)!;
            pageSize.Should().BeLessThanOrEqualTo(50, "page size should be clamped to max 50");
        }
    }
}
