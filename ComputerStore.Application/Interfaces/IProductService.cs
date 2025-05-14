using ComputerStore.Application.DTOs;

namespace ComputerStore.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(ProductDto dto);
    Task UpdateAsync(int id, ProductDto dto);
    Task DeleteAsync(int id);
    Task ImportStockAsync(IEnumerable<ProductDto> importedProducts);
    Task<decimal> CalculateDiscountAsync(List<int> productIds);
}
