using LibraryProject;
using LibraryProjectDomain.DTOS.UserDTO;
using LibraryProjectDomain.Models.BookModel;
using LibraryProjectDomain.Models.BorrowingModel;
using LibraryProjectDomain.Models.MembersModel;
using LibraryProjectDomain.Models.PermissionModel;
using LibraryProjectDomain.Models.RoleModel;
using LibraryProjectDomain.Models.UserModel;
using LibraryProjectRepository.Repositories.Books;
using LibraryProjectRepository.Repositories.Borrowings;
using LibraryProjectRepository.Repositories.Members;
using LibraryProjectRepository.Repositories.Permissions;
using LibraryProjectRepository.Repositories.Reports;
using LibraryProjectRepository.Repositories.Roles;
using LibraryProjectRepository.Repositories.Users;
using LibraryProjectRepository.SheardRepository;
using LibraryProjectSecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


ExcelPackage.License.SetNonCommercialPersonal("Test"); 
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new()
                     {
                         ValidIssuer = builder.Configuration["Authentication:Issuer"],
                         ValidAudience = builder.Configuration["Authentication:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                                                            builder.Configuration["Authentication:SecretKey"]!)),
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateIssuerSigningKey = true,
                         ValidateLifetime = true,         
                         ClockSkew = TimeSpan.Zero     
                     };
                     options.Events = new JwtBearerEvents
                     {
                         OnMessageReceived = async context =>
                         {
                             var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                             if (!string.IsNullOrEmpty(token))
                             {
                                 var redis = context.HttpContext.RequestServices.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
                                 var db = redis.GetDatabase();

                                 var jti = JwtHelper.GetJtiFromToken(token);
                                 if (await db.KeyExistsAsync($"blacklist:{jti}"))
                                 {
                                     context.Fail("Token is revoked");
                                 }
                             }
                         }
                     };
                 });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
}); ;
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(
    StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));
builder.Services.AddScoped<LibraryDbContext>();
builder.Services.AddScoped<IRepository<Book>,BookRepository>();
builder.Services.AddScoped<IRepository<Borrowing>,BorrowingRepository>();
builder.Services.AddScoped<IRepository<Member>,MemberRepository>();
builder.Services.AddScoped<IRepository<Permission>, PermissionRepository>();
builder.Services.AddScoped<IRepository<Role>, RoleRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IHtmlToPdf, PlaywrightHtmlToPdf>();


builder.Services.AddScoped<ReportsRepository>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BorrowingRepository>();
builder.Services.AddScoped<MemberRepository>();
builder.Services.AddScoped<PermissionRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<PermissionAtrRepository>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
