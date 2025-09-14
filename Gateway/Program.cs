using Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text;

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// Add JSON config for Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Ocelot with caching
builder.Services.AddOcelot()
    .AddCacheManager(x => x.WithDictionaryHandle());

// Configure CORS (allow all - adjust for production!)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// JWT Authentication setup
var jwtConfig = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var cfg = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = cfg["Issuer"],
            ValidateAudience = true,
            ValidAudience = cfg["Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();
ExcelPackage.License.SetNonCommercialPersonal("milan khanal");

app.UseAuthentication(); 
app.UseAuthorization();  

app.UseMiddleware<InterceptionMiddleware>(); 

app.MapControllers();
app.UseCors("AllowNextJsApp");
await app.UseOcelot();

app.Run();
