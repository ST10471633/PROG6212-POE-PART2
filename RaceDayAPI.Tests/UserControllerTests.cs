using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDay.Controllers;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class UserControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);
            _controller = new UserController(_context);
        }

        private User CreateTestUser(int id = 1)
        {
            return new User
            {
                UserID = id,
                Name = "Test",
                Surname = "User",
                Email = $"test{id}@example.com",
                PasswordHash = "TestPassword123",
                Role = "Participant"
            };
        }

        // TEST 1: Get all users
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            _context.Users.Add(CreateTestUser(1));
            _context.Users.Add(CreateTestUser(2));
            await _context.SaveChangesAsync();

            var result = await _controller.GetUsers();

            var users = Assert.IsType<List<User>>(result.Value);
            Assert.Equal(2, users.Count);
        }

        // TEST 2: Get an existing user
        [Fact]
        public async Task GetUser_ExistingUser_ReturnsUser()
        {
            _context.Users.Add(CreateTestUser(1));
            await _context.SaveChangesAsync();

            var result = await _controller.GetUser(1);

            var user = Assert.IsType<User>(result.Value);

            Assert.Equal(1, user.UserID);
            Assert.Equal("Test", user.Name);
            Assert.Equal("User", user.Surname);
        }

        // TEST 3: Get a user that does not exist
        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNotFound()
        {
            var result = await _controller.GetUser(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        // TEST 4: Create a user
        [Fact]
        public async Task CreateUser_ValidUser_ReturnsCreated()
        {
            var user = CreateTestUser(1);

            var result = await _controller.CreateUser(user);

            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result.Result);

            Assert.Equal(
                nameof(UserController.GetUser),
                createdResult.ActionName);

            Assert.Equal(1, await _context.Users.CountAsync());

            var savedUser = await _context.Users.FindAsync(1);

            Assert.NotNull(savedUser);
            Assert.Equal("Test", savedUser.Name);
            Assert.Equal("User", savedUser.Surname);
        }

        // TEST 5: Update an existing user
        [Fact]
        public async Task UpdateUser_ValidUser_ReturnsNoContent()
        {
            var user = CreateTestUser(1);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Detach the original entity to simulate a separate request.
            _context.Entry(user).State = EntityState.Detached;

            var updatedUser = CreateTestUser(1);
            updatedUser.Name = "Updated";
            updatedUser.Surname = "Person";

            var result = await _controller.UpdateUser(1, updatedUser);

            Assert.IsType<NoContentResult>(result);

            var savedUser = await _context.Users.FindAsync(1);

            Assert.NotNull(savedUser);
            Assert.Equal("Updated", savedUser.Name);
            Assert.Equal("Person", savedUser.Surname);
        }

        // TEST 6: Update with mismatched IDs
        [Fact]
        public async Task UpdateUser_MismatchedIds_ReturnsBadRequest()
        {
            var user = CreateTestUser(2);

            var result = await _controller.UpdateUser(1, user);

            Assert.IsType<BadRequestResult>(result);
        }

        // TEST 7: Update a user that does not exist
        [Fact]
        public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
        {
            var user = CreateTestUser(999);

            var result = await _controller.UpdateUser(999, user);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 8: Delete an existing user
        [Fact]
        public async Task DeleteUser_ExistingUser_ReturnsNoContent()
        {
            _context.Users.Add(CreateTestUser(1));
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteUser(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await _context.Users.CountAsync());
        }

        // TEST 9: Delete a user that does not exist
        [Fact]
        public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
        {
            var result = await _controller.DeleteUser(999);

            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}