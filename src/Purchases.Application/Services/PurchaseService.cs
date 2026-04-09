using MongoDB.Driver;
using Purchases.Application.Contracts;
using Purchases.Application.Repository;
using Purchases.Domain.Contracts;
using Purchases.Domain.Models;
using Purchases.Domain.Models.DTO.Purchase;
using Purchases.Domain.Models.DTO.Vendor;

namespace Purchases.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IVendorService _vendorService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IVendorService vendorService)
        {
            _purchaseRepository = purchaseRepository;
            _vendorService = vendorService;
        }

        public async Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize, CancellationToken cancellationToken)
        {
            var purchases = await _purchaseRepository.GetAllAsync(pageSize, cancellationToken);

            // TODO: Optimize vendors retrieval
            var vendors = await _vendorService.GetAllAsync();
            var purchaseDtos = purchases.Select(purchase => MapFrom(purchase, vendors));

            return purchaseDtos;
        }

        public async Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken) =>
            await _purchaseRepository.GetAsync(id, cancellationToken);

        public async Task CreateAsync(Purchase newPurchase) =>
            await _purchaseRepository.CreateAsync(newPurchase);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchaseRepository.UpdateAsync(id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchaseRepository.RemoveAsync(id);

        private GetPurchaseDto MapFrom(Purchase purchase, IEnumerable<GetVendorDto> vendors)
        {
            var vendor = vendors.FirstOrDefault(x => x.Id == purchase.VendorId);
            return new GetPurchaseDto()
            {
                Id = purchase.Id,
                PurchaseDate = purchase.PurchaseDate,
                PurchaseUrl = purchase.PurchaseUrl,
                TotalAmount = purchase.TotalAmount,
                VendorId = vendor?.Id,
                VendorName = vendor?.Name,
                VendorLogoUrl = vendor?.LogoUrl,
                Items = purchase.Items?.Select(item =>
                    new PurchaseItemDto()
                    {
                        Description = item.Description,
                        UnitPrice = item.UnitPrice,
                        Tags = item.Tags
                    }
                ).ToArray()
            };
        }
    }
}

