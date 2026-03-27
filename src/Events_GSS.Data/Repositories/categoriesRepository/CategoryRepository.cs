using System;
using System.Collections.Generic;
using System.Text;

using Events_GSS.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace Events_GSS.Data.Repositories.categoriesRepository;

internal class CategoryRepository: ICategoryRepository
{
    public async Task<List<Category>> GetAllAsync()
    {
        using var db = Database.DatabaseConnection.CreateContext();
        return await db.Categories.ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        using var db = Database.DatabaseConnection.CreateContext();
        return await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }
}
