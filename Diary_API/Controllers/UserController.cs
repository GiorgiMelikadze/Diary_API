using Diary_API.Domain;
using Diary_API.DTOs;
using Diary_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.Results;

namespace Diary_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IOptions<AppSettings> appSettings,
            IValidator<RegisterUserDto> registerValidator,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto model)
        {
            var user = _userService.Login(model);

            _logger.LogInformation("Login attempt for Username: {Username}", model.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed for Username: {Username}", model.Username);
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            var token = GenerateToken(user);

            _logger.LogInformation("User logged in successfully: {Username}", model.Username);

            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Token = token
            });
        }

        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public IActionResult GetAll()
        //{
        //    var users = _userService.GetAll();
        //    return Ok(users);
        //}


        //[HttpDelete("users/{id}/delete")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult DeleteUser(int id)
        //{
        //    _userService.DeleteUser(id);
        //    _logger.LogInformation("Accountant deleted user {UserId}", id);
        //    return Ok();
        //}

        //[HttpGet("{id}")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult GetById(int id)
        //{
        //    var user = _userService.GetById(id);
        //    if (user == null)
        //        return NotFound();
        //    return Ok(user);
        //}

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDto model)
        {
            var validationResult = _registerValidator.Validate(model);

            _logger.LogInformation("Registration attempt for Username: {Username}", model.Username);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registration validation failed for Username: {Username}", model.Username);
                return ValidationErrorResponse(validationResult);
            }

            var user = _userService.Register(model);

            if (user == null)
            {
                _logger.LogWarning("Registration failed: Username already exists ({Username})", model.Username);
                return BadRequest("Username already exists");
            }

            _logger.LogInformation("User registered successfully: {Username}", model.Username);

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role
            });
        }

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        private IActionResult ValidationErrorResponse(FluentValidation.Results.ValidationResult validationResult) // had to add FluentValidation.Results. to ValitadionResult this time
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return BadRequest(new { errors });
        }
    }
}
