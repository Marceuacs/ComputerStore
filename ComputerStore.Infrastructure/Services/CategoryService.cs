using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ComputerStoreDbContext _context;

    public CategoryService(ComputerStoreDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) throw new Exception("Category not found.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) throw new Exception("Category not found.");

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task UpdateAsync(int id, CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null) throw new Exception("Category not found.");

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _context.SaveChangesAsync();
    }
}
