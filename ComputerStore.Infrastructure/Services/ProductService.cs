using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ComputerStoreDbContext _context;

    public ProductService(ComputerStoreDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        var categories = await _context.Categories
            .Where(c => dto.Categories.Contains(c.Name))
            .ToListAsync();

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Categories = categories
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            Categories = categories.Select(c => c.Name).ToList()
        };
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) throw new Exception("Product not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Categories)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                Categories = p.Categories.Select(c => c.Name).ToList()
            }).ToListAsync();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) throw new Exception("Product not found.");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            Categories = product.Categories.Select(c => c.Name).ToList()
        };
    }

    public async Task ImportStockAsync(IEnumerable<ProductDto> importedProducts)
    {
        foreach (var dto in importedProducts)
        {
            var categories = new List<Category>();

            foreach (var catName in dto.Categories)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == catName.Trim());

                if (category == null)
                {
                    category = new Category { Name = catName.Trim() };
                    _context.Categories.Add(category);
                }

                categories.Add(category);
            }

            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Name == dto.Name);

            if (product == null)
            {
                product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Quantity = dto.Quantity,
                    Categories = categories
                };
                _context.Products.Add(product);
            }
            else
            {
                product.Quantity += dto.Quantity;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, ProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) throw new Exception("Product not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Quantity = dto.Quantity;

        var categories = await _context.Categories
            .Where(c => dto.Categories.Contains(c.Name))
            .ToListAsync();

        product.Categories = categories;

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> CalculateDiscountAsync(List<int> productIds)
    {
        var productsFromDb = await _context.Products
            .Include(p => p.Categories)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var productCounts = productIds.GroupBy(id => id)
                                      .ToDictionary(g => g.Key, g => g.Count());

        var productList = new List<Product>();

        foreach (var kvp in productCounts)
        {
            var product = productsFromDb.FirstOrDefault(p => p.Id == kvp.Key);
            if (product == null) continue;

            if (kvp.Value > product.Quantity)
                throw new Exception($"Not enough stock for {product.Name}");

            for (int i = 0; i < kvp.Value; i++)
            {
                productList.Add(product);
            }
        }

        var byCategory = productList
            .GroupBy(p => p.Categories.FirstOrDefault()?.Name ?? "Unknown");

        decimal total = 0;

        foreach (var group in byCategory)
        {
            var list = group.ToList();
            if (list.Count > 1)
            {
                total += list[0].Price * 0.95m; // 5% off first
                total += list.Skip(1).Sum(p => p.Price);
            }
            else
            {
                total += list.Sum(p => p.Price);
            }
        }

        return total;
    }
}
