using Diary_API.Domain;
using Diary_API.DTOs;
using Diary_API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Diary_API.Services
{
    public interface IUserService
    {
        User Register(RegisterUserDto model);
        User Login(LoginDto model);
        //User GetById(int id);
        //IEnumerable<User> GetAll();
        //void DeleteUser(int id);
    }

    public class UserService : IUserService
    {
        //public void DeleteUser(int id)
        //{
        //    var user = _context.Users.FirstOrDefault(u => u.Id == id);

        //    if (user == null)
        //        throw new Exception("User not found");

        //    _context.Users.Remove(user);
        //    _context.SaveChanges();
        //}

        //public IEnumerable<User> GetAll() => _context.Users.AsNoTracking().ToList();

        //public User GetById(int id) => _context.Users.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public User Login(LoginDto model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);
            if (user == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.Password,
                model.Password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public User Register(RegisterUserDto model)
        {
            if (_context.Users.Any(u => u.Username == model.Username))
                return null;

            var user = new User
            {
                Username = model.Username,
                IsBlocked = false
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }
    }
}
