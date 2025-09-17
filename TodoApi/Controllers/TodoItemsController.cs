using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Models;


namespace TodoApi.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(JwtTokenService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = InMemoryUserStore.Users
            .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            _logger.LogWarning("Invalid login attempt for user: {Username}", request.Username);
            return Unauthorized();
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }
}


[Route("api/[controller]")]
[ApiController]
//[Authorize]

public class TodoItemsController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(TodoContext context, ILogger<TodoItemsController> logger)
    {
        _context = context;
        _logger = logger;
    }


    // GET: api/TodoItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 20,
       [FromQuery] bool? isCompleted = null)
    {

        if (page <= 0 || pageSize <= 0)
        {
            _logger.LogWarning("Page and PageSize must be greater than 0");
            return BadRequest("Page and PageSize must be greater than 0");
        }
        var query = _context.TodoItems.AsQueryable();

        if (isCompleted.HasValue)
        {
            query = query.Where(item => item.IsComplete == isCompleted.Value);
        }

        var totalTodos = await query.CountAsync();

        Response.Headers.Add("X-Total-Todos", totalTodos.ToString());

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => ItemToDTO(item))
            .ToListAsync();
        return Ok(items);
    }

    // GET: api/TodoItems/5?page=4&?pagesize=20
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
    {



        var todoItem = await _context.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return ItemToDTO(todoItem);
    }
    // </snippet_GetByID>

    // PUT: api/TodoItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Update>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItemDTO todoDTO)
    {
        if (id != todoDTO.Id)
        {
            _logger.LogError("Try to modify wrong id");
            return BadRequest();
        }

        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            _logger.LogError("Id doesnt exist");
            return NotFound();
        }

        todoItem.Name = todoDTO.Name;
        todoItem.IsComplete = todoDTO.IsCompleted ?? false;

        try
        {
            _logger.LogInformation("Saving todo");
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
        {
            _logger.LogError("Todo item doesnt exist");
            return NotFound();
        }
        _logger.LogInformation("Todo saved");
        return Ok();
    }
    // </snippet_Update>

    // POST: api/TodoItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Create>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TodoItemDTO>> PostTodoItem(TodoItemDTO todoDTO)
    {
        var todoItem = new TodoItem
        {
            Id = todoDTO.Id,
            IsComplete = todoDTO.IsCompleted ?? false,
            Name = todoDTO.Name
        };
        _logger.LogInformation("New TodoItem received");
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();
        _logger.LogInformation("TodoItem added");
        return CreatedAtAction(
            nameof(GetTodoItem),
            new { id = todoItem.Id },
            ItemToDTO(todoItem));
    }
    // </snippet_Create>

    // DELETE: api/TodoItems/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            _logger.LogError("Id doesnt exist");
            return NotFound();
        }

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
        _logger.LogWarning("Deleted TodoItem");
        return Ok();
    }

    private bool TodoItemExists(long id)
    {
        return _context.TodoItems.Any(e => e.Id == id);
    }

    private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
       new TodoItemDTO
       {
           Id = todoItem.Id,
           Name = todoItem.Name,
           IsCompleted = todoItem.IsComplete
       };
}