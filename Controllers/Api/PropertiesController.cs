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
    [Route("api/v{version:apiVersion}/properties")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PropertiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/properties
        [HttpGet]
        public async Task<IActionResult> GetProperties(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Properties.AsQueryable();
            var totalCount = await query.CountAsync();

            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(1, page);

            var properties = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Address,
                    p.Description,
                    TotalRooms = p.Rooms.Count(),
                    AvailableRooms = p.Rooms.Count(r => r.Status == RoomStatus.Available),
                    RentedRooms = p.Rooms.Count(r => r.Status == RoomStatus.Rented),
                    MaintenanceRooms = p.Rooms.Count(r => r.Status == RoomStatus.Maintenance)
                })
                .ToListAsync();

            return Ok(new { 
                count = totalCount, 
                page = page, 
                pageSize = pageSize, 
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = properties 
            });
        }

        // GET: api/properties/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProperty(int id)
        {
            var property = await _context.Properties
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Property = new
                    {
                        p.Id,
                        p.Name,
                        p.Address,
                        p.Description,
                        p.Latitude,
                        p.Longitude,
                        TotalRooms = p.Rooms.Count(),
                        AvailableRooms = p.Rooms.Count(r => r.Status == RoomStatus.Available),
                        RentedRooms = p.Rooms.Count(r => r.Status == RoomStatus.Rented),
                        MaintenanceRooms = p.Rooms.Count(r => r.Status == RoomStatus.Maintenance)
                    },
                    Rooms = p.Rooms
                        .OrderBy(r => r.Name)
                        .Select(r => new
                        {
                            r.Id,
                            r.Name,
                            r.Price,
                            r.Area,
                            Status = r.Status.ToString(),
                            Note = r.Note,
                            Images = r.Images
                                .OrderByDescending(img => img.IsThumbnail)
                                .ThenBy(img => img.Id)
                                .Select(img => img.ImagePath)
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (property == null)
            {
                return NotFound(new { message = "Không tìm thấy khu nhà." });
            }

            return Ok(new { data = property });
        }

        [HttpGet("map-data")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMapData()
        {
            var propertiesRaw = await _context.Properties
                .Include(p => p.Rooms)
                .Where(p => p.Latitude != null && p.Longitude != null && p.Rooms.Any(r => r.Status == RoomStatus.Available))
                .ToListAsync();

            var mapData = propertiesRaw.Select(p => new
            {
                p.Id,
                p.Name,
                p.Address,
                p.Latitude,
                p.Longitude,
                AvailableRooms = p.Rooms.Count(r => r.Status == RoomStatus.Available),
                MinPrice = p.Rooms.Where(r => r.Status == RoomStatus.Available).Min(r => (decimal?)r.Price) ?? 0,
                ImageUrl = "https://via.placeholder.com/300x200?text=Khu+Tro"
            }).ToList();

            return Ok(mapData);
        }
    }
}
