using Diary_API.Domain;
using Diary_API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Diary_API.Services
{
    public interface IAdminService
    {
        User GetById(int id);
        IEnumerable<User> GetAll();
        void DeleteUser(int id);
        void BlockUser(int id);
    }

    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAll() => _context.Users.AsNoTracking().ToList();

        public User GetById(int id) => _context.Users.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public void BlockUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                throw new Exception("User not found");

            if (user.IsBlocked)
                return;

            user.IsBlocked = true;
            _context.SaveChanges();
        }
    }
}
