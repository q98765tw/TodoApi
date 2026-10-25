using Microsoft.EntityFrameworkCore;
using TodoApiGrokBot.Data;
using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

public class TodoService(AppDbContext db, TimeProvider clock) : ITodoService
{
    private IQueryable<TodoItem> Owned(int userId) => db.Todos.Where(t => t.UserId == userId);

    public Task<List<TodoItem>> GetAllAsync(int userId, TodoStatusFilter status = TodoStatusFilter.All)
    {
        var q = Owned(userId);
        var today = TaipeiTime.Today(clock);
        q = status switch
        {
            TodoStatusFilter.Active => q.Where(t => !t.IsDone),
            TodoStatusFilter.Completed => q.Where(t => t.IsDone),
            TodoStatusFilter.Overdue => q.Where(t => !t.IsDone && t.DueDate != null && t.DueDate < today),
            _ => q
        };
        return q.OrderBy(t => t.Id).ToListAsync();
    }

    public Task<TodoItem?> GetAsync(int userId, int id) =>
        Owned(userId).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TodoItem> CreateAsync(int userId, TodoInput input)
    {
        var item = new TodoItem { Title = input.Title, IsDone = input.IsDone, DueDate = input.DueDate, CreatedAt = DateTime.UtcNow, UserId = userId,
            CompletedAt = input.IsDone ? DateTime.UtcNow : null };
        db.Todos.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> UpdateAsync(int userId, int id, TodoInput input)
    {
        var t = await GetAsync(userId, id);
        if (t is null) return false;
        t.Title = input.Title;
        t.DueDate = input.DueDate;
        if (!t.IsDone && input.IsDone) t.CompletedAt = DateTime.UtcNow;
        else if (t.IsDone && !input.IsDone) t.CompletedAt = null;
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
