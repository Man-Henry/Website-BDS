using Website_QLPT.Models;

namespace Website_QLPT.ViewModels
{
    public class TenantDashboardViewModel
    {
        public string? Message { get; set; }
        public Tenant? Tenant { get; set; }
        public IReadOnlyList<Contract> ActiveContracts { get; set; } = [];
        public IReadOnlyList<Invoice> RecentInvoices { get; set; } = [];
        public IReadOnlyList<Contract> PastContracts { get; set; } = [];
        public List<int> ReviewedRoomIds { get; set; } = new List<int>();
    }
}
