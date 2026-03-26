using Website_QLPT.Models;

namespace Website_QLPT.ViewModels
{
    public class DashboardViewModel
    {
        // KPIs tổng quan
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int VacantRooms { get; set; }
        public decimal OccupancyRate => TotalRooms > 0 ? Math.Round((decimal)OccupiedRooms / TotalRooms * 100, 1) : 0;

        public decimal TotalRevenuePaid { get; set; }
        public decimal TotalRevenueUnpaid { get; set; }

        public int OpenTickets { get; set; }
        public int ActiveContracts { get; set; }

        // Dữ liệu Chart.js - Doanh thu 6 tháng
        public List<string> MonthLabels { get; set; } = new();
        public List<decimal> MonthlyRevenueData { get; set; } = new();

        // Dữ liệu Chart.js - Phân phối trạng thái phòng
        public List<string> RoomStatusLabels { get; set; } = new();
        public List<int> RoomStatusData { get; set; } = new();

        // Hóa đơn chưa thanh toán gần nhất (top 5)
        public List<Invoice> RecentUnpaidInvoices { get; set; } = new();

        // Sự cố chưa xử lý (top 5)
        public List<MaintenanceTicket> RecentOpenTickets { get; set; } = new();
    }
}
