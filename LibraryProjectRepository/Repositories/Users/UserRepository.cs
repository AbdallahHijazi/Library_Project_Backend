using BCrypt.Net;
using LibraryProject;
using LibraryProjectDomain.DTOS.UserDTO;
using LibraryProjectDomain.Models.RoleModel;
using LibraryProjectDomain.Models.UserModel;
using LibraryProjectRepository.SheardRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static LibraryProjectRepository.Repositories.Users.AuthRepository;

namespace LibraryProjectRepository.Repositories.Users
{
    public class UserRepository : GinericRepository<User>
    {
        private readonly LibraryDbContext context;
        private readonly IMemoryCache cache;
        private readonly IConfiguration configuration;
        private readonly IEmailSender emailSender;

        public UserRepository(LibraryDbContext context,
                              IMemoryCache cache,
                              IConfiguration configuration,
                              IEmailSender emailSender
                                ) : base(context)
        {
            this.context = context;
            this.cache = cache;
            this.configuration = configuration;
            this.emailSender = emailSender;
        }
        public async Task<List<ReadUser>> GetAllUsers()
        {
            var users = await context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions) 
                .Include(u => u.UserPermissions) 
                .ToListAsync();

            var usersDto = users.Select(user =>
            {
                var directPermissions = user.UserPermissions.Select(up => up.PermissionId);

                var rolePermissions = user.Role?.RolePermissions.Select(rp => rp.PermissionId) ?? Enumerable.Empty<Guid>();

                var combinedPermissionIds = directPermissions
                                            .Union(rolePermissions)
                                            .Distinct()
                                            .ToList();

                return new ReadUser
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    PermissionIds = combinedPermissionIds
                };
            }).ToList();

            return usersDto;
        }
        public async Task<ReadUser?> GetById(Guid id)
        {
            var user = await context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions) 
                .Include(u => u.UserPermissions) 
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            var directPermissions = user.UserPermissions.Select(up => up.PermissionId);

            var rolePermissions = user.Role?.RolePermissions.Select(rp => rp.PermissionId)
                                  ?? Enumerable.Empty<Guid>();

            var combinedPermissionIds = directPermissions
                                        .Union(rolePermissions)
                                        .Distinct()
                                        .ToList();

            var userDto = new ReadUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                // 4. تعيين القائمة المدموجة
                PermissionIds = combinedPermissionIds
            };

            return userDto;
        }
        //public async Task<RegisterResult?> Add(CreateUser user)
        //{
        //    if (user.Password != user.CaniformPassword)
        //        return null;

        //    var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");
        //    if (userRole == null)
        //        return null;

        //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

        //    var users = new User
        //    {
        //        Email = user.Email,
        //        FullName = user.FullName,
        //        Password = hashedPassword,
        //        RoleId = userRole.Id
        //    };

        //    context.Users.Add(users);
        //    await context.SaveChangesAsync();

        //    var issuer = configuration["Authentication:Issuer"];
        //    var audience = configuration["Authentication:Audience"];
        //    var secret = configuration["Authentication:SecretKey"];
        //    var expires = DateTime.UtcNow.AddMinutes(60);

        //    var claims = new List<Claim>
        //{
        //    new(JwtRegisteredClaimNames.Sub, users.Id.ToString()),
        //    new(JwtRegisteredClaimNames.Email, users.Email),
        //    new(ClaimTypes.Name, users.FullName),
        //    new(ClaimTypes.Role, "User"),
        //    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //};

        //    var creds = new SigningCredentials(
        //        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret!)),
        //        SecurityAlgorithms.HmacSha256
        //    );

        //    var jwt = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        notBefore: DateTime.UtcNow,
        //        expires: expires,
        //        signingCredentials: creds
        //    );

        //    var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        //    return new RegisterResult
        //    {
        //        Id = users.Id,
        //        FullName = users.FullName,
        //        Email = users.Email,
        //        Role = "User",
        //        Token = token,
        //        ExpiresAt = expires
        //    };
        //}
        public async Task<RegisterResult?> Add(CreateUser user)
        {
            if (user.Password != user.CaniformPassword)
                return null;

            var exists = await context.Users.AnyAsync(u => u.Email == user.Email);
            if (exists) return null;

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
                return null;

            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            await emailSender.SendVerificationEmailAsync(user.LinkedEmail, code);

            var pendingUser = new PendingUser
            {
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                VerificationCode = code,
                LinkedEmail = user.LinkedEmail
            };
            context.PendingUsers.Add(pendingUser);
            await context.SaveChangesAsync();

            return new RegisterResult
            {
                Id = pendingUser.Id,
                FullName = pendingUser.FullName,
                Email = pendingUser.Email,
                Role = "User",
                Token = string.Empty,
                ExpiresAt = DateTime.MinValue
            };
        }
        public async Task<ReadUser?> UpdateUser(Guid id, UpdateUser updateUser)
        {
            var user = await context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) 
                return null;

            if (!string.IsNullOrWhiteSpace(updateUser.FullName))
                user.FullName = updateUser.FullName;

            if (!string.IsNullOrWhiteSpace(updateUser.Email))
                user.Email = updateUser.Email;

            if (updateUser.RoleId.HasValue)
                user.RoleId = updateUser.RoleId.Value;

            if (updateUser.RoleId.HasValue)
            {
                var exists = await context.Roles.AnyAsync(s => s.Id == updateUser.RoleId.Value);
                if (!exists) throw new ArgumentException("StudentId غير موجود.");
                updateUser.RoleId = updateUser.RoleId.Value;
            }

            if (updateUser.PermissionIds != null)
            {
                if (user.UserPermissions?.Count > 0)
                    context.UserPermissions.RemoveRange(user.UserPermissions);

                if (updateUser.PermissionIds.Count > 0)
                {
                    var validIds = await context.Permissions
                        .Where(p => updateUser.PermissionIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();
                    user.UserPermissions = validIds.Select(pid => new UserPermission
                    {
                        UserId = user.Id,
                        PermissionId = pid
                    }).ToList();

                }
                else
                {
                    user.UserPermissions = new List<UserPermission>();
                }
            }
            var userDto = new ReadUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                PermissionIds = user.UserPermissions.Select(up => up.PermissionId).ToList()
            };
            cache.Remove($"perms:{user.Id}");
            await context.SaveChangesAsync();
            return userDto;
        }
        public async Task<bool> DeleteUser(Guid id)
        {
            var existingUser = await context.Users.FindAsync(id);
            if (existingUser == null) return false;

            context.Users.Remove(existingUser);
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> VerifyEmail(string linkedEmail, string code)
        {
            var pending = await context.PendingUsers
                .FirstOrDefaultAsync(u => u.LinkedEmail == linkedEmail && u.VerificationCode == code);

            if (pending == null)
                return false;

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var newUser = new User
            {
                FullName = pending.FullName,
                Email = pending.Email,
                Password = pending.PasswordHash,
                RoleId = userRole.Id,
                EmailConfirmed = true
            };
            context.Users.Add(newUser);

            context.PendingUsers.Remove(pending);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
