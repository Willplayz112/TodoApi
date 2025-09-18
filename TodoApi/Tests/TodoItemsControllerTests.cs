using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Models;
using Xunit;

namespace TodoApi.Tests
{
    public class TodoItemsControllerTests
    {
        private TodoContext GetInMemoryContexts()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new TodoContext(options);
            context.Database.EnsureCreated();
            return context;
        }


        [Fact]
        public async Task GetTodoItem_ReturnsItem_WhenIdExists()
        {
            var context = GetInMemoryContexts();
            var logger = new LoggerFactory().CreateLogger<TodoItemsController>();
            var controller = new TodoItemsController(context, logger);

            var newItem = new TodoItemDTO
            {
                Name = "Test Task For GET",
                IsCompleted = false
            };

            // Post the item
            var postResult = await controller.PostTodoItem(newItem);

            // Extract the CreatedAtActionResult
            var createdResult = Assert.IsType<CreatedAtActionResult>(postResult.Result);
            var createdItem = Assert.IsType<TodoItemDTO>(createdResult.Value);

            // Use the ID from the created item
            var result = await controller.GetTodoItem(createdItem.Id);

            // Assert the result
            var okResult = Assert.IsType<ActionResult<TodoItemDTO>>(result);
            var value = Assert.IsType<TodoItemDTO>(okResult.Value);
            Assert.Equal("Test Task For GET", value.Name);
        }


        [Fact]
        public async Task GetTodoItem_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var context = GetInMemoryContexts();
            var logger = new LoggerFactory().CreateLogger<TodoItemsController>();
            var controller = new TodoItemsController(context, logger);

            var result = await controller.GetTodoItem(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostTodoItem_ReturnsCreated_WhenAddingNewTodo()
        {
            // Config the Task
            var context = GetInMemoryContexts();
            var logger = new LoggerFactory().CreateLogger<TodoItemsController>();
            var controller = new TodoItemsController(context, logger);

            // JSON for the test Todo
            var newItem = new TodoItemDTO
            {
                Name = "Test Task For POST",
                IsCompleted = false
            };

            // Get the result
            var result = await controller.PostTodoItem(newItem);

            // Modify the result to expect the return values are the same
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<TodoItemDTO>(createdResult.Value);
            Assert.Equal("Test Task For POST", returnValue.Name);
            Assert.False(returnValue.IsCompleted);

            // Check if the Todo was added, you never know if it will be added always got to check every thing
            Assert.Single(context.TodoItems);
        }

        [Fact]
        public async Task DeleteTodoItem_ReturnsOk_WhenDeletingATodo()
        {
            // Config the Task
            var context = GetInMemoryContexts();
            var logger = new LoggerFactory().CreateLogger<TodoItemsController>();
            var controller = new TodoItemsController(context, logger);


            // JSON for the test Todo
            var newItem = new TodoItemDTO
            {
                Name = "Test Task For Deleting",
                IsCompleted = false
            };

            // Get the result
            var result = await controller.PostTodoItem(newItem);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdItem = Assert.IsType<TodoItemDTO>(createdResult.Value);

            // Tell the controller to delete the the newly created task
            var deleteResult = await controller.DeleteTodoItem(createdItem.Id);

            // Check if the result is a 200 Ok response
            Assert.IsType<OkResult>(deleteResult);

            // Double check if it is deleted in the DB
            var deletedItem = await context.TodoItems.FindAsync(createdItem.Id);
            Assert.Null(deletedItem);

        }
    }
}