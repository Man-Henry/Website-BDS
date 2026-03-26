using Website_QLPT.Models;

namespace Website_QLPT.ViewModels
{
    public class ReportsIndexViewModel
    {
        public IReadOnlyList<PropertyRoomStatusReportItem> PropertyStatuses { get; set; } = [];
        public IReadOnlyList<Contract> ExpiringContracts { get; set; } = [];
        
        // Annual Revenue Chart Data
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = [];
        public List<decimal> MonthlyPaidRevenue { get; set; } = []; // 12 elements
        public List<decimal> MonthlyUnpaidRevenue { get; set; } = []; // 12 elements
    }

    public class PropertyRoomStatusReportItem
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int RentedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
    }
}
