using System.Text;
using HomeFlow.API.Infrastructure;
using HomeFlow.API.Middleware;
using HomeFlow.Application.Interfaces;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Auth;
using HomeFlow.Infrastructure.Database;
using HomeFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"]!;
var jwtIssuer = jwtConfig["Issuer"]!;
var jwtAudience = jwtConfig["Audience"]!;
var jwtExpiration = int.Parse(jwtConfig["ExpirationMinutes"]!);

var dbFactory = new NpgsqlConnectionFactory(connectionString);

builder.Services.AddSingleton<IDbConnectionFactory>(dbFactory);
builder.Services.AddSingleton(new MigrationRunner(dbFactory));

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IRecurringTaskTemplateRepository, RecurringTaskTemplateRepository>();
builder.Services.AddScoped<IRotationEntryRepository, RotationEntryRepository>();

builder.Services.AddScoped<IJwtTokenProvider>(_ => new JwtTokenProvider(jwtKey, jwtIssuer, jwtAudience, jwtExpiration));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<RecurringTaskService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers(options =>
    options.Conventions.Add(new GlobalRoutePrefixConvention("api")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await migrationRunner.RunAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
