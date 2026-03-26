using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_QLPT.Data;
using Website_QLPT.Models;

namespace Website_QLPT.Filters
{
    public class CheckSubscriptionLimitAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin"))
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    
                    var profile = await dbContext.LandlordProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                    var plan = profile?.Plan ?? SubscriptionPlan.Free;
                    
                    int maxRooms = plan switch
                    {
                        SubscriptionPlan.Free => 5,
                        SubscriptionPlan.Pro => 20,
                        SubscriptionPlan.Enterprise => int.MaxValue,
                        _ => 5
                    };

                    // Only check POST requests to Create action
                    if (context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                        context.RouteData.Values["action"]?.ToString() == "Create")
                    {
                        // Count current rooms
                        // Since rooms belong to properties owned by user
                        var currentRoomCount = await dbContext.Rooms
                            .CountAsync(r => r.Property!.OwnerId == userId);

                        if (currentRoomCount >= maxRooms)
                        {
                            var controller = context.Controller as Controller;
                            if (controller != null)
                            {
                                controller.TempData["Error"] = $"Bạn đã đạt giới hạn {maxRooms} phòng của gói {plan}. Vui lòng nâng cấp gói cước!";
                                context.Result = new RedirectToActionResult("Index", "Rooms", null);
                                return;
                            }
                        }
                    }
                }
            }

            await next();
        }
    }
}
