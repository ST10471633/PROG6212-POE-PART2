
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Controllers;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class CategoriesControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            _controller = new CategoriesController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // TEST 1: Get all categories
        [Fact]
        public async Task GetAllCategories_ReturnsOk()
        {
            _context.Categories.Add(new Category
            {
                CategoryName = "Junior",
                CategoryType = "10-17",
                EventID = 1
            });

            await _context.SaveChangesAsync();

            var result = await _controller.GetAllCategories();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 2: Get an existing category
        [Fact]
        public async Task GetCategoryById_ExistingCategory_ReturnsOk()
        {
            _context.Events.Add(new Event
            {
                EventID = 1,
            });

            var category = new Category
            {
                CategoryID = 1,
                CategoryName = "Open",
                CategoryType = "18-59",
                EventID = 1

            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = await _controller.GetCategoryById(1);

            Assert.IsType<OkObjectResult>(result);

        }

        // TEST 3: Get a category that does not exist
        [Fact]
        public async Task GetCategoryById_CategoryNotFound_ReturnsNotFound()
        {
            var result = await _controller.GetCategoryById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 4: Create a category
        [Fact]
        public async Task CreateCategory_ValidCategory_ReturnsCreated()
        {
            var category = new Category
            {
                CategoryName = "Senior",
                CategoryType = "60-99",
                EventID = 1
            };

            var result = await _controller.CreateCategory(category);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await _context.Categories.CountAsync());
        }

        // TEST 5: Create a null category
        [Fact]
        public async Task CreateCategory_NullCategory_ReturnsBadRequest()
        {
            var result = await _controller.CreateCategory(null!);

            Assert.IsType<BadRequestResult>(result);
        }

        // TEST 6: Update an existing category
        [Fact]
        public async Task UpdateCategory_ExistingCategory_ReturnsOk()
        {
            var category = new Category
            {
                CategoryID = 1,
                CategoryName = "Junior",
                CategoryType = "10-17",
                EventID = 1
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var updatedCategory = new Category
            {
                CategoryName = "Junior Updated",
                CategoryType = "10-16",
                EventID = 1
            };

            var result = await _controller.UpdateCategory(1, updatedCategory);

            Assert.IsType<OkObjectResult>(result);

            var savedCategory = await _context.Categories.FindAsync(1);

            Assert.NotNull(savedCategory);
            Assert.Equal("Junior Updated", savedCategory.CategoryName);
            Assert.Equal("10-16", savedCategory.CategoryType);
        }

        // TEST 7: Update a category that does not exist
        [Fact]
        public async Task UpdateCategory_CategoryNotFound_ReturnsNotFound()
        {
            var updatedCategory = new Category
            {
                CategoryName = "Updated",
                CategoryType = "18-59",
                EventID = 1
            };

            var result = await _controller.UpdateCategory(999, updatedCategory);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 8: Delete an existing category
        [Fact]
        public async Task DeleteCategory_ExistingCategory_ReturnsNoContent()
        {
            var category = new Category
            {
                CategoryID = 1,
                CategoryName = "Open",
                CategoryType = "18-59",
                EventID = 1
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await _context.Categories.CountAsync());
        }

        // TEST 9: Delete a category that does not exist
        [Fact]
        public async Task DeleteCategory_CategoryNotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteCategory(999);

            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}