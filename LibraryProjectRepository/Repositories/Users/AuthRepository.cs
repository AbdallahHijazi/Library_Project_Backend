using LibraryProject;
using LibraryProjectDomain.Models.UserModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Users
{
    public class AuthRepository
    {
        private readonly LibraryDbContext context;
        private readonly IConfiguration config;

        public AuthRepository(LibraryDbContext context,
                              IConfiguration config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;

            return null;
        }
    }
}
