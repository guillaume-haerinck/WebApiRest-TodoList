using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

using TodoApi.Data;
using System.Security.Claims;

namespace TodoApi.Controllers
{
    // The name of the controller is the name of the file without the Controller suffix
    [Route("api/[controller]")]
    // Any data here is restricted to logged users with JWT header
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodosController : Controller
    {
        // Used to access the database
        private readonly TodoDbContext _context;

        // Used to link user to his todoes
        private readonly UserManager<User> _userManager;

        // Constuctor
        public TodosController(TodoDbContext context, UserManager<User> manager)
        {
            _context = context;
            _userManager = manager;
        }

        // Get /api/todos
        [HttpGet (Name = "GetTodo")]
        public IEnumerable<TodoItem> GetAll()
        {
            // Get the JWT sub claim
            var userId = _userManager.GetUserId(User);

            return _context.TodoItems.ToList().Where(todo => todo.UserForeignKey == userId);
        }

        // Post /api/todos
        [HttpPost]
        public IActionResult Create([FromBody] TodoItem request)
        {
            if (request.Content == null)
            {
                return BadRequest();
            }

            // Get the JWT sub claim
            var userId = _userManager.GetUserId(User);

            request.UserForeignKey = userId;
            _context.TodoItems.Add(request);
            _context.SaveChanges();

            return CreatedAtRoute("GetTodo", new { id = request.Id }, request);
        }

        // Patch /api/todos/{id}
        [HttpPatch("{id}")]
        public IActionResult Update(string id, [FromBody] TodoItem request)
        {
            if (request.Content == null)
            {
                return BadRequest();
            }

            var item = _context.TodoItems.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            // Get the JWT sub claim
            var userId = _userManager.GetUserId(User);

            if (item.UserForeignKey == userId)
            {
                item.Done = request.Done;
                item.Content = request.Content;

                _context.TodoItems.Update(item);
                _context.SaveChanges();
                return Ok(item);
            }

            return BadRequest();
        }

        // Delete /api/todos/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var item = _context.TodoItems.FirstOrDefault(t => t.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(item);
            _context.SaveChanges();
            return new NoContentResult();
        }
    }
}