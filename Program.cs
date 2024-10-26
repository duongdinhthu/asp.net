using Microsoft.EntityFrameworkCore;
using ATMManagementApplication.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hangfire; // Thêm namespace này
using ATMManagementApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var key = jwtSection.GetValue<string>("Key");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Thêm dịch vụ vào container
builder.Services.AddControllers();
builder.Services.AddDbContext<ATMContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))));

// Cấu hình Hangfire
builder.Services.AddHangfire(configuration =>
    configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); // Thay đổi connection-string cho phù hợp
builder.Services.AddHangfireServer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins"); // Enable CORS
app.UseAuthorization();
app.MapControllers();

// Cấu hình bảng điều khiển Hangfire
app.UseHangfireDashboard();
app.UseHangfireServer();

// Lên lịch hàng tháng cho hàm ApplyInterest
RecurringJob.AddOrUpdate<ATMController>(x => x.ApplyInterest(), Cron.Monthly);

// Set the application to listen on port 5175
app.Urls.Add("http://localhost:5175");

app.Run();
