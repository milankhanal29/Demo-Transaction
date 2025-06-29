using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Text;
using TransactionService.Data;
using TransactionService.Mapping;
using TransactionService.Repository;
using TransactionService.Services;
using TransactionService.Utils;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddHttpClient("UserService", c =>
    c.BaseAddress = new Uri("https://localhost:7200"));
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<ITransactionService, TransactionServiceImpl>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPaymentScheduler, PaymentScheduler>();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var jwtConfig = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

//builder.Services.AddHangfire(config =>
//    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); 

//builder.Services.AddHangfireServer();




builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
ExcelPackage.License.SetNonCommercialPersonal("milan khanal");
app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();
//app.UseHangfireDashboard();

app.MapControllers();

app.Run();
