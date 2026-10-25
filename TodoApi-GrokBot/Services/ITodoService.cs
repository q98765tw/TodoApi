using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;

namespace TodoApiGrokBot.Services;

/// <summary>所有操作都限定在指定使用者的 Todo；找不到回傳 null / false。</summary>
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync(int userId, TodoStatusFilter status = TodoStatusFilter.All);
    Task<TodoItem?> GetAsync(int userId, int id);
    Task<TodoItem> CreateAsync(int userId, TodoInput input);
    Task<bool> UpdateAsync(int userId, int id, TodoInput input);
    Task<bool> DeleteAsync(int userId, int id);
}
