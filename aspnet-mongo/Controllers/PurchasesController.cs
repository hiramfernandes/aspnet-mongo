using aspnet_mongo.Models;
using aspnet_mongo.Models.DTO;
using aspnet_mongo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_mongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchasesService;

        public PurchasesController(IPurchaseService purchasesService)
        {
            _purchasesService = purchasesService;
        }

        [HttpGet]
        [Produces(typeof(List<Purchase>))]
        public async Task<IActionResult> GetPurchases(CancellationToken cancellationToken)
        {
            var purchases = await _purchasesService.GetAllAsync();

            return Ok(purchases);
        }

        [HttpGet("{id}")]
        [Produces(typeof(Purchase))]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var purchase = await _purchasesService.GetAsync(id);

            return Ok(purchase);
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchase(string id, PurchaseDto purchaseDto)
        {
            var purchase = await _purchasesService.GetAsync(id);
            if (purchase == null)
                return BadRequest("Unable to find purchase");

            purchase.PurchaseDate = purchaseDto.PurchaseDate;
            purchase.VendorName = purchaseDto.VendorName;
            purchase.VendorId = purchaseDto.VendorId;
            purchase.PurchaseUrl = purchaseDto.Url;
            purchase.Items = purchaseDto.Items;
            purchase.TotalAmount = purchaseDto.TotalAmount;

            await _purchasesService.UpdateAsync(id, purchase);

            return Ok(purchase);
        }


        [HttpPost]
        public async Task CreatePurchase(CancellationToken cancellationToken, PurchaseDto newPurchaseDto)
        {
            var purchase = new Purchase()
            {
                PurchaseDate = newPurchaseDto.PurchaseDate,
                PurchaseUrl = newPurchaseDto.Url,
                VendorName = newPurchaseDto.VendorName,
                VendorId = newPurchaseDto.VendorId,
                TotalAmount = newPurchaseDto.TotalAmount,
                Items = newPurchaseDto.Items
            };

            await _purchasesService.CreateAsync(purchase);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemovePurchase(string id, CancellationToken cancellationToken)
        {
            await _purchasesService.RemoveAsync(id);

            return Ok();
        }
    }
}
