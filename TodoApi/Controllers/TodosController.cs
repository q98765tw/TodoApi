using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<TodoItem>> GetAll() => await db.Todos.OrderBy(t => t.Id).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> Get(int id) =>
        await db.Todos.FindAsync(id) is { } t ? t : NotFound();

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(TodoItem item)
    {
        item.Id = 0;
        item.CreatedAt = DateTime.UtcNow;
        db.Todos.Add(item);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoItem input)
    {
        var t = await db.Todos.FindAsync(id);
        if (t is null) return NotFound();
        t.Title = input.Title;
        t.IsDone = input.IsDone;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await db.Todos.FindAsync(id);
        if (t is null) return NotFound();
        db.Todos.Remove(t);
        await db.SaveChangesAsync();
        return NoContent();
    }
}