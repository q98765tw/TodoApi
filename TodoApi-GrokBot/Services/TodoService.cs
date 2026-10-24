using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public class TodoService(AppDbContext db) : ITodoService
{
    private IQueryable<TodoItem> Owned(int userId) => db.Todos.Where(t => t.UserId == userId);

    public Task<List<TodoItem>> GetAllAsync(int userId) =>
        Owned(userId).OrderBy(t => t.Id).ToListAsync();

    public Task<TodoItem?> GetAsync(int userId, int id) =>
        Owned(userId).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TodoItem> CreateAsync(int userId, TodoInput input)
    {
        var item = new TodoItem { Title = input.Title, IsDone = input.IsDone, CreatedAt = DateTime.UtcNow, UserId = userId };
        db.Todos.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(int userId, int id, TodoInput input)
    {
        var t = await GetAsync(userId, id);
        if (t is null) return false;
        t.Title = input.Title;
        t.IsDone = input.IsDone;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int userId, int id)
    {
        var t = await GetAsync(userId, id);
        if (t is null) return false;
        db.Todos.Remove(t);
        await db.SaveChangesAsync();
        return true;
    }
}
