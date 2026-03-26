using Website_QLPT.Models;

namespace Website_QLPT.Services.Billing
{
    public class InvoiceCalculationResult
    {
        public decimal ElectricityFee { get; set; }
        public string? ElectricityDetails { get; set; }
        public decimal WaterFee { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public interface IInvoiceCalculatorService
    {
        Task<InvoiceCalculationResult> CalculateFeesAsync(Website_QLPT.Models.Invoice invoice, Property property);
    }

    public class InvoiceCalculatorService : IInvoiceCalculatorService
    {
        public Task<InvoiceCalculationResult> CalculateFeesAsync(Website_QLPT.Models.Invoice invoice, Property property)
        {
            var result = new InvoiceCalculationResult();

            // ─── TÍNH TIỀN NƯỚC (Thường là giá cố định / khối) ─────────────
            var waterUsed = Math.Max(0, invoice.WaterNew - invoice.WaterOld);
            result.WaterFee = waterUsed * invoice.WaterPrice;

            // ─── TÍNH TIỀN ĐIỆN ─────────────────────────────────────────────
            var electricityUsed = Math.Max(0M, invoice.ElectricityNew - invoice.ElectricityOld);

            if (!property.IsElectricityTiered || property.UtilityTiers == null || !property.UtilityTiers.Any(t => t.Type == UtilityType.Electricity))
            {
                // Giá cố định
                result.ElectricityFee = electricityUsed * invoice.ElectricityPrice;
                result.ElectricityDetails = $"{(int)electricityUsed} kWh x {invoice.ElectricityPrice:#,##0}đ";
            }
            else
            {
                // Tính theo bậc thang EVN
                var tiers = property.UtilityTiers.Where(t => t.Type == UtilityType.Electricity).OrderBy(t => t.FromUnit).ToList();
                decimal totalElectricityFee = 0;
                var details = new List<string>();
                int remainingUsage = (int)electricityUsed;

                foreach (var tier in tiers)
                {
                    if (remainingUsage <= 0) break;

                    int maxUnitForTier = tier.ToUnit.HasValue ? (tier.ToUnit.Value - tier.FromUnit + 1) : int.MaxValue;
                    int unitsInTier = Math.Min(remainingUsage, maxUnitForTier);

                    decimal costForTier = unitsInTier * tier.Price;
                    totalElectricityFee += costForTier;
                    details.Add($"{unitsInTier} kWh x {tier.Price:#,##0}đ = {costForTier:#,##0}đ");

                    remainingUsage -= unitsInTier;
                }

                result.ElectricityFee = totalElectricityFee;
                if (details.Any())
                {
                    result.ElectricityDetails = string.Join(" | ", details);
                }
            }

            // ─── TỔNG CỘNG ──────────────────────────────────────────────────
            result.TotalAmount = invoice.RoomFee + result.ElectricityFee + result.WaterFee + invoice.OtherFee;

            return Task.FromResult(result);
        }
    }
}
