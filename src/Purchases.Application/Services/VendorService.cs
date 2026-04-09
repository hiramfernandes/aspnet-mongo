using Purchases.Application.Contracts;
using Purchases.Application.Repository;
using Purchases.Domain.Models;
using Purchases.Domain.Models.DTO.Vendor;

namespace Purchases.Application.Services
{
    public class VendorService : IVendorService
    {
        private readonly IVendorRepository _vendorRepository;

        public VendorService(IVendorRepository vendorRepository)
        {
            _vendorRepository = vendorRepository;
        }

        public async Task<IEnumerable<GetVendorDto>> GetAllAsync()
        {
            var vendors = await _vendorRepository.GetAllAsync();

            return vendors.Select(x => new GetVendorDto { Id = x.Id, Name = x.Name, LogoUrl = x.LogoUrl });
        }

        public async Task<GetVendorDto> GetById(string id)
        {
            var vendor = await _vendorRepository.GetAsync(id);
            return new GetVendorDto { Id = vendor.Id, Name = vendor.Name, LogoUrl = vendor.LogoUrl };
        }

        public async Task<GetVendorDto?> GetByName(string name, CancellationToken cancellationToken)
        {
            var vendor = await _vendorRepository.GetByNameAsync(name, cancellationToken);

            return new GetVendorDto { Id = vendor?.Id, Name = vendor?.Name, LogoUrl = vendor?.LogoUrl };
        }

        public async Task CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken)
        {
            var vendor = new Vendor()
            {
                Name = vendorDto.Name,
                Location = vendorDto.Location,
                LogoUrl = vendorDto.LogoUrl,
                AddedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };

            await _vendorRepository.CreateAsync(vendor, cancellationToken);
        }

        public async Task UpdateVendor(string id, UpdateVendorDto updateVendorDto, CancellationToken cancellationToken)
        {
            var vendor = await _vendorRepository.GetAsync(id);
            if (vendor == null)
                throw new InvalidOperationException($"Vendor with Id {id} does not exist");

            vendor.LogoUrl = updateVendorDto.Url;
            vendor.Name = updateVendorDto.Name;
            vendor.UpdatedOn = DateTime.Now;

            await _vendorRepository.UpdateVendor(id, vendor, cancellationToken);
        }

        public async Task RemoveAsync(string id, CancellationToken cancellationToken) =>
            await _vendorRepository.RemoveAsync(id, cancellationToken);
    }
}
