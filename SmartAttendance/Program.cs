using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartAttendance.Data;
using SmartAttendance.Interfaces;
using SmartAttendance.Mapping;
using SmartAttendance.Models;
using SmartAttendance.Repositories;
using SmartAttendance.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy for development to allow local frontend to call the API.
// Change or restrict origins for production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure Identity (Required for UserManager<AppUser>)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//JWT Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";

})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
    };
});


// Register Repository & AutoMapper
builder.Services.AddScoped<IAuthRepository, AuthRepository>();   
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();
// Log effective DB connection info at startup to help diagnose schema mismatches
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var configConn = builder.Configuration.GetConnectionString("DefaultConnection");
        logger.LogInformation("Configured DefaultConnection: {conn}", configConn);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var canConnect = db.Database.CanConnect();
        var dbName = db.Database.GetDbConnection()?.Database;
        logger.LogInformation("Database reachable: {ok}, Database name: {dbName}", canConnect, dbName);
    }
    catch (Exception ex)
    {
        var loggerEx = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        loggerEx.LogError(ex, "Failed to read DB connection info at startup");
    }
}



// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS policy (development). Place before authentication/authorization so
// that preflight requests are handled and CORS headers are present on responses.
app.UseCors("AllowDev");

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();

