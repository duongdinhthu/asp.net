using Microsoft.EntityFrameworkCore;
using ATMManagementApplication.Data;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add service to container => thiết lập cấu hình data model
builder.Services.AddControllers();
builder.Services.AddDbContext<ATMContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins"); // Enable CORS
app.UseAuthorization();
app.MapControllers();

// Set the application to listen on port 5175
app.Urls.Add("http://localhost:5175");

app.Run();
