using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using ATMManagementApplication.Data;

public class InterestService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly ATMContext _context;

    public InterestService(ATMContext context)
    {
        _context = context;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
{
    // Thiết lập timer gọi hàm ApplyInterest mỗi tháng
    while (!cancellationToken.IsCancellationRequested)
    {
        ApplyInterest(); // Gọi hàm ApplyInterest
        await Task.Delay(TimeSpan.FromDays(30), cancellationToken); // Delay 30 ngày
    }
}


    public int ApplyInterest()
{
    var customers = _context.Customer.ToList();
    int updatedCount = 0; // Đếm số tài khoản được cập nhật

    foreach (var customer in customers)
    {
        // Kiểm tra xem đã cộng lãi suất chưa
        if (customer.LastInterestApplied.AddMonths(1) <= DateTime.Now)
        {
            decimal interest = customer.Balance * customer.InterestRate; // Tính lãi suất
            customer.Balance += interest; // Cộng lãi suất vào tài khoản
            customer.LastInterestApplied = DateTime.Now; // Cập nhật thời gian cộng lãi suất
            updatedCount++;
        }
    }

    _context.SaveChanges();
    return updatedCount; // Trả số tài khoản đã được cập nhật
}


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
