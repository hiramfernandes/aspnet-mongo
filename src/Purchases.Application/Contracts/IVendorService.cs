using Purchases.Application.Models.DTO.Vendor;

namespace Purchases.Application.Contracts
{
    public interface IVendorService
    {
        Task<IEnumerable<GetVendorDto>> GetAllAsync();
        Task<GetVendorDto> GetById(string id);
        Task<GetVendorDto?> GetByName(string name);
        Task CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken);
        Task UpdateVendor(string id, UpdateVendorDto updateVendorDto, CancellationToken cancellationToken);
        Task RemoveAsync(string id);
    }
}
