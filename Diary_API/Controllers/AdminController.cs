using Diary_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diary_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var users = _adminService.GetAll();
            return Ok(users);
        }

        [HttpDelete("users/{id}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            _adminService.DeleteUser(id);
            _logger.LogInformation("Accountant deleted user {UserId}", id);
            return Ok();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            var user = _adminService.GetById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPut("users/{id}/block")]
        public IActionResult BlockUser(int id)
        {
            _adminService.BlockUser(id);
            _logger.LogInformation("Accountant blocked user {UserId}", id);
            return Ok();
        }
    }
}
