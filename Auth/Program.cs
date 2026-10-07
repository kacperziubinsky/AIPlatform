using System.Text.Json.Serialization;
using Auth.Data;
using Auth.Models;
using Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var userServiceUrl = builder.Configuration["Services:UserUrl"]
                     ?? throw new InvalidOperationException("Missing config: Services:UserUrl");
var internalApiKey = builder.Configuration["InternalApiKey"]
                     ?? throw new InvalidOperationException("Missing config: InternalApiKey");

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(c =>
    {
        c.BaseAddress = new Uri(userServiceUrl);
        c.DefaultRequestHeaders.Add("X-Api-Key", internalApiKey);
    })
    .AddStandardResilienceHandler();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();