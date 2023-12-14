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
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<Vendor>))]
        public async Task<IActionResult> GetVendors(CancellationToken cancellationToken)
        {
            // TODO: Create output DTO
            var vendors = await _vendorService.GetAllAsync();
            return Ok(vendors);
        }

        [HttpGet("{id}")]
        [Produces(typeof(Vendor))]
        public async Task<IActionResult> GetVendorById(string id)
        {
            var vendor = await _vendorService.GetById(id);

            return Ok(vendor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken)
        {
            await _vendorService.CreateVendor(vendorDto, cancellationToken);
            return Ok();
        }
    }
}
