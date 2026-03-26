using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Controllers.Api
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/rooms
        [HttpGet]
        public async Task<IActionResult> GetAvailableRooms(
            [FromQuery] decimal? priceMax,
            [FromQuery] decimal? areaMin,
            [FromQuery] int? propertyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Rooms
                .Include(r => r.Property)
                .Include(r => r.Images)
                .Where(r => r.Status == RoomStatus.Available)
                .AsQueryable();

            if (priceMax.HasValue)
                query = query.Where(r => r.Price <= priceMax.Value);

            if (areaMin.HasValue)
                query = query.Where(r => r.Area >= areaMin.Value);

            if (propertyId.HasValue)
                query = query.Where(r => r.PropertyId == propertyId.Value);

            var totalCount = await query.CountAsync();

            pageSize = Math.Clamp(pageSize, 1, 50); // Max 50 items per page
            page = Math.Max(1, page);

            var rooms = await query
                .OrderBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Price,
                    r.Area,
                    Note = r.Note,
                    Status = r.Status.ToString(),
                    PropertyName = r.Property != null ? r.Property.Name : string.Empty,
                    PropertyAddress = r.Property != null ? r.Property.Address : null,
                    Images = r.Images
                        .OrderByDescending(img => img.IsThumbnail)
                        .ThenBy(img => img.Id)
                        .Select(img => img.ImagePath)
                        .ToList()
                })
                .ToListAsync();

            return Ok(new { 
                count = totalCount, 
                page = page, 
                pageSize = pageSize, 
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = rooms 
            });
        }

        // GET: api/rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Property)
                .Include(r => r.Images)
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Price,
                    r.Area,
                    Status = r.Status.ToString(),
                    Note = r.Note,
                    Property = new
                    {
                        Id = r.PropertyId,
                        Name = r.Property != null ? r.Property.Name : string.Empty,
                        Address = r.Property != null ? r.Property.Address : null,
                        Latitude = r.Property != null ? r.Property.Latitude : null,
                        Longitude = r.Property != null ? r.Property.Longitude : null
                    },
                    Images = r.Images
                        .OrderByDescending(img => img.IsThumbnail)
                        .ThenBy(img => img.Id)
                        .Select(img => img.ImagePath)
                        .ToList(),
                    DepositAmount = r.Price // Default deposit normally matches 1 month price
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(new { message = "Không tìm thấy phòng trọ." });
            }

            return Ok(new { data = room });
        }
    }
}
