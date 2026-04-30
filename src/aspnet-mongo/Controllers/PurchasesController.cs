using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models;
using Purchases.Domain.Models.DTO.Purchase;

namespace aspnet_mongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
     public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchasesService;

        public PurchasesController(IPurchaseService purchasesService)
        {
            _purchasesService = purchasesService;
        }

        [HttpGet]
        [Produces(typeof(List<GetPurchaseDto>))]
        public async Task<IActionResult> GetPurchases(CancellationToken cancellationToken)
        {
            // TODO: Move to appsettings
            var pageSize = 50;

            try
            {
                var purchases = await _purchasesService.GetAllAsync(pageSize, cancellationToken);

                return Ok(purchases);
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet("{id}")]
        [Produces(typeof(Purchase))]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var purchase = await _purchasesService.GetAsync(id, cancellationToken);

            return Ok(purchase);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePurchase(string id, PurchaseDto purchaseDto, CancellationToken cancellationToken)
        {
            try
            {
                await _purchasesService.UpdateAsync(id, purchaseDto, cancellationToken);

                return Ok();
            }
            catch (InvalidOperationException iExc)
            {
                return BadRequest(iExc.Message);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exc.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase(
            CancellationToken cancellationToken,
            PurchaseDto newPurchaseDto)
        {
            try
            {
                await _purchasesService.CreateAsync(newPurchaseDto, cancellationToken);
                return Ok();
            }
            catch (InvalidOperationException oexc)
            {
                return BadRequest(oexc.Message);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exc.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemovePurchase(string id, CancellationToken cancellationToken)
        {
            await _purchasesService.RemoveAsync(id);

            return Ok();
        }
    }
}
