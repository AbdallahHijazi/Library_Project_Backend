using LibraryProject;
using LibraryProjectDomain.DTOS.UserDTO;
using LibraryProjectDomain.Models.UserModel;
using LibraryProjectRepository.Repositories.Users;
using LibraryProjectRepository.SheardRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly LibraryDbContext context;
        private readonly IRepository<User> irepository;
        private readonly UserRepository repository;
        private readonly IConnectionMultiplexer connection;
        private readonly IConfiguration config;
        private readonly AuthRepository auth;

        public UserController(LibraryDbContext context,
                              IRepository<User> irepository,
                              UserRepository repository, IConnectionMultiplexer connection,
                              IConfiguration config,AuthRepository auth)
        {
            this.context = context;
            this.irepository = irepository;
            this.repository = repository;
            this.connection = connection;
            this.config = config;
            this.auth = auth;
        }
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(CreateUser create)
        {
            var createdUser = await repository.Add(create);
            if (createdUser==null)
            {
                return BadRequest("Invalid registration data or email already exists.");
            }

            return CreatedAtRoute("GetUser", new { id = createdUser.Id }, createdUser);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await repository.GetAllUsers();
            if (users == null || !users.Any())
            {
                NotFound("No Users in DataBase");
            }
            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var users = await repository.GetById(id);
            if (users == null)
                return NotFound($"No Users in DataBase have same {id}");

            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id,UpdateUser updateUser)
        {
            try
            {
                var updated = await repository.UpdateUser(id, updateUser);
                if (updated == null)
                    return NotFound("This user does not exist");

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var isDeleted = await repository.DeleteUser(id);
            if (!isDeleted)
                return NotFound("This user is not Exist Or has been deleted");

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUser(Login login)
        {
            var loginUser = await auth.ValidateUserAsync(login.Email, login.Password);
            if (loginUser == null)
                return Unauthorized("بريد إلكتروني أو كلمة مرور خاطئة");
            var permissions = await GetUserPermissions(loginUser.Id);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, loginUser.Id.ToString()),
            new Claim(ClaimTypes.Email, loginUser.Email),
            new Claim(ClaimTypes.Role, loginUser.Role.Name),
            new Claim("permissions", string.Join(",", permissions)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new Claim(JwtRegisteredClaimNames.Iat,
                      DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                      ClaimValueTypes.Integer64),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Authentication:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(config["Authentication:TokenExpiryInHours"])),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token missing");

            var jti = JwtHelper.GetJtiFromToken(token);
            var exp = JwtHelper.GetExpiryFromToken(token);

            var db = connection.GetDatabase();
            var key = $"blacklist:{jti}";

            var isAlreadyRevoked = await db.KeyExistsAsync(key);
            if (isAlreadyRevoked)
                return BadRequest("This token has already been logged out.");

            var ttl = exp - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
                return BadRequest("Token already expired.");

            await db.StringSetAsync(key, "revoked", ttl);

            return Ok("Logged out successfully");
        }

        private async Task<List<string>> GetUserPermissions(Guid userId)
        {
            var user = await context.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission) 
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission) 
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<string>();
            }

            var directPermissions = user.UserPermissions
                .Select(up => up.Permission.Key)
                .Where(key => key != null);

            var rolePermissions = user.Role?.RolePermissions
                .Select(rp => rp.Permission.Key)
                .Where(key => key != null)
                ?? Enumerable.Empty<string>(); 

            var combinedKeys = directPermissions
                .Union(rolePermissions)
                .Distinct()
                .ToList();

            return combinedKeys;
        }
    }
}
