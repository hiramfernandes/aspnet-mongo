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
        private readonly IPurchasesService _purchasesService;

        public PurchasesController(IPurchasesService purchasesService)
        {
            _purchasesService = purchasesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchases(CancellationToken cancellationToken)
        {
            var purchases = await _purchasesService.GetAllAsync();

            return Ok(purchases);
        }
    }
}
