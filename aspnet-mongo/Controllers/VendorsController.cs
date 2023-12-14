using aspnet_mongo.Models.DTO;
using aspnet_mongo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aspnet_mongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVendors(CancellationToken cancellationToken)
        {
            // TODO: Create output DTO
            var vendors = await _vendorService.GetAllAsync();

            return Ok(vendors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken)
        {
            await _vendorService.CreateVendor(vendorDto, cancellationToken);
            return Ok();
        }
    }
}
