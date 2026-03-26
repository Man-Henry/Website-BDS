using Website_QLPT.Models;
using System.Collections.Generic;
using System.Linq;

namespace Website_QLPT.Services.Payment
{
    public interface IPaymentServiceResolver
    {
        IPaymentProvider GetProvider(PaymentProvider providerType);
    }

    public class PaymentServiceResolver : IPaymentServiceResolver
    {
        private readonly IEnumerable<IPaymentProvider> _providers;

        public PaymentServiceResolver(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers;
        }

        public IPaymentProvider GetProvider(PaymentProvider providerType)
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderType == providerType);
            if (provider == null)
            {
                throw new InvalidOperationException($"Không tìm thấy service xử lý cho cổng {providerType}");
            }
            return provider;
        }
    }
}