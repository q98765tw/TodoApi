using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApiGrokBot.Extensions;
using TodoApiGrokBot.Models;
using TodoApiGrokBot.Models.Dtos;
using TodoApiGrokBot.Services;

namespace TodoApiGrokBot.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TodosController(ITodoService todos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll([FromQuery] string? status = null)
    {
        if (!TodoStatusFilterParser.TryParse(status, out var filter))
            return BadRequest(new { error = "Invalid status. Use all, active, completed or overdue." });
        return await todos.GetAllAsync(User.GetUserId(), filter);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> Get(int id) =>
        await todos.GetAsync(User.GetUserId(), id) is { } t ? t : NotFound();

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(TodoInput input)
    {
        var item = await todos.CreateAsync(User.GetUserId(), input);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoInput input) =>
        await todos.UpdateAsync(User.GetUserId(), id, input) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await todos.DeleteAsync(User.GetUserId(), id) ? NoContent() : NotFound();
}
