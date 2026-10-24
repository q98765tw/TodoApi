using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TodosController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
    private IQueryable<TodoItem> MyTodos => db.Todos.Where(t => t.UserId == UserId);

    [HttpGet]
    public async Task<IEnumerable<TodoItem>> GetAll() => await MyTodos.OrderBy(t => t.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> Get(int id) =>
        await MyTodos.FirstOrDefaultAsync(t => t.Id == id) is { } t ? t : NotFound();

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(TodoInput input)
    {
        var item = new TodoItem { Title = input.Title, IsDone = input.IsDone, CreatedAt = DateTime.UtcNow, UserId = UserId };
        db.Todos.Add(item);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoInput input)
    {
        var t = await MyTodos.FirstOrDefaultAsync(t => t.Id == id);
        if (t is null) return NotFound();
        t.Title = input.Title;
        t.IsDone = input.IsDone;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await MyTodos.FirstOrDefaultAsync(t => t.Id == id);
        if (t is null) return NotFound();
        db.Todos.Remove(t);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
