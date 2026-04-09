using Purchases.Domain.Contracts.Repos;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models;
using Purchases.Domain.Models.DTO.Purchase;

namespace Purchases.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        //private readonly IVendorService _vendorService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IVendorService vendorService)
        {
            _purchaseRepository = purchaseRepository;
            //_vendorService = vendorService;
        }

        public async Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize, CancellationToken cancellationToken)
        {
            var purchases = await _purchaseRepository.GetAllAsync(pageSize, cancellationToken);

            //// TODO: Optimize vendors retrieval
            //var vendors = await _vendorService.GetAllAsync();
            var purchaseDtos = purchases.Select(purchase => MapFrom(purchase));

            return purchaseDtos;
        }

        public async Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken) =>
            await _purchaseRepository.GetAsync(id, cancellationToken);

        public async Task CreateAsync(PurchaseDto newPurchaseDto)
        {
            var newPurchase = new Purchase()
            {
                PurchaseDate = newPurchaseDto.PurchaseDate,
                PurchaseUrl = newPurchaseDto.Url,
                VendorName = newPurchaseDto.VendorName,
                VendorId = newPurchaseDto.VendorId,
                TotalAmount = newPurchaseDto.TotalAmount,
                Items = newPurchaseDto.Items?
                .Select(item =>
                    new PurchaseItem()
                    {
                        Description = item.Description,
                        UnitPrice = item.UnitPrice,
                        Tags = item.Tags
                    })
                .ToArray(),
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _purchaseRepository.CreateAsync(newPurchase);
        }

        public async Task UpdateAsync(
            string id, 
            PurchaseDto purchaseDto, 
            CancellationToken cancellationToken)
        {
            var purchaseFromDb = await _purchaseRepository.GetAsync(id, cancellationToken);
            if (purchaseFromDb == null)
                throw new InvalidOperationException("Unable to find purchase");

            purchaseFromDb.PurchaseDate = purchaseDto.PurchaseDate;
            purchaseFromDb.VendorName = purchaseDto.VendorName;
            purchaseFromDb.VendorId = purchaseDto.VendorId;
            purchaseFromDb.PurchaseUrl = purchaseDto.Url;
            purchaseFromDb.Items = purchaseDto.Items?
                .Select(item =>
                    new PurchaseItem()
                    {
                        Description = item.Description,
                        UnitPrice = item.UnitPrice,
                        Tags = item.Tags
                    })
                .ToArray();
            purchaseFromDb.TotalAmount = purchaseDto.TotalAmount;
            purchaseFromDb.UpdatedAtUtc = DateTime.UtcNow;

            await _purchaseRepository.UpdateAsync(id, purchaseFromDb);
        }
            

        public async Task RemoveAsync(string id) =>
            await _purchaseRepository.RemoveAsync(id);

        private GetPurchaseDto MapFrom(Purchase purchase)
        {
            return new GetPurchaseDto()
            {
                Id = purchase.Id,
                PurchaseDate = purchase.PurchaseDate,
                PurchaseUrl = purchase.PurchaseUrl,
                TotalAmount = purchase.TotalAmount,
                VendorId = purchase?.VendorId,
                VendorName = purchase?.VendorName,
                //VendorLogoUrl = vendor?.LogoUrl,
                Items = purchase?.Items?.Select(item =>
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

