using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recruitment.Data;
using Recruitment.Services;
using Recruitment.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<RecruitmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key missing"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var offerServiceUrl = builder.Configuration["Services:OfferUrl"]
                      ?? throw new InvalidOperationException("Missing config: Services:OfferUrl");
var userServiceUrl = builder.Configuration["Services:UserUrl"]
                     ?? throw new InvalidOperationException("Missing config: Services:UserUrl");
var aiServiceUrl = builder.Configuration["Services:AiUrl"]
                   ?? throw new InvalidOperationException("Missing config: Services:AiUrl");
var internalApiKey = builder.Configuration["InternalApiKey"]
                     ?? throw new InvalidOperationException("Missing config: InternalApiKey");

builder.Services.AddHttpClient<IOfferServiceClient, OfferServiceClient>(c =>
    c.BaseAddress = new Uri(offerServiceUrl));

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(c =>
{
    c.BaseAddress = new Uri(userServiceUrl);
    c.DefaultRequestHeaders.Add("X-Api-Key", internalApiKey);
});

builder.Services.AddHttpClient<IAiServiceClient, AiServiceClient>(c =>
{
    c.BaseAddress = new Uri(aiServiceUrl);
    c.Timeout = TimeSpan.FromMinutes(15);
});

builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IAiEvaluationService, AiEvaluationService>();
builder.Services.AddHostedService<AiEvaluationWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
