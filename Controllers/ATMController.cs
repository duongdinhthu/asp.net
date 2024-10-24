using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System.Linq;
using System;
using System.Net.Mail;
using System.Net;

namespace ATMManagementApplication.Controllers
{
    [ApiController]
    [Route("/api/atm")]
    public class ATMController : ControllerBase
    {
        private readonly ATMContext _context;
        public ATMController(ATMContext context)
        {
            _context = context;
        }
        [HttpGet("balance/{customerId}")]
        public ActionResult GetBalance(int customerId)
        {
            Console.WriteLine(customerId);
            var customer = _context.Customer.Find(customerId);
            if (customer == null)
                return NotFound("Cusomer not found");

            return Ok(new { balance = customer.Balance });
        }

        [HttpPost("withdraw")]
        public IActionResult Withdraw([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customer.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            // Kiểm tra hạn mức giao dịch rút tiền
            if (!IsTransactionWithinLimit(request.CustomerId, request.Amount, true))  // Rút tiền là true
                return BadRequest("Transaction exceeds daily limit or transaction count limit");

            if (customer.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            customer.Balance -= request.Amount;

            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true,
                IsSender = true // Người rút tiền cũng được coi là người gửi
            };
            _context.Transaction.Add(transaction);
            _context.SaveChanges();
            return Ok(new { message = "Withdraw successful", newBalance = customer.Balance });
        }

        [HttpPost("plus")]
        public IActionResult Plus([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customer.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");

            // Nạp tiền không cần kiểm tra hạn mức
            customer.Balance += request.Amount;

            var transaction = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true,
                IsSender = false // Nạp tiền, nên IsSender là false
            };
            _context.Transaction.Add(transaction);
            _context.SaveChanges();
            return Ok(new { message = "Deposit successful", newBalance = customer.Balance });
        }


        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest request)
        {
            var sender = _context.Customer.Find(request.SenderId);
            if (sender == null)
                return NotFound("Sender not found");

            var receiver = _context.Customer.Find(request.ReceiverId);
            if (receiver == null)
                return NotFound("Receiver not found");

            // Kiểm tra số dư của người gửi
            if (sender.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            // Kiểm tra hạn mức giao dịch
            if (!IsTransactionWithinLimit(request.SenderId, request.Amount, true)) // Chuyển tiền là true
                return BadRequest("Transaction exceeds daily limit or transaction count limit");

            // Cập nhật số dư
            sender.Balance -= request.Amount;
            receiver.Balance += request.Amount;

            // Ghi lại giao dịch
            var sendTransaction = new Transaction
            {
                CustomerId = request.SenderId,
                Amount = -request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true,
                IsSender = true
            };
            _context.Transaction.Add(sendTransaction);

            var receiveTransaction = new Transaction
            {
                CustomerId = request.ReceiverId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true,
                IsSender = false
            };
            _context.Transaction.Add(receiveTransaction);

            _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

            // Gửi thông báo qua email
            SendEmail(sender.Email, "Transfer Notification", $"You have transferred {request.Amount} to {receiver.Email}. Your new balance is {sender.Balance}.");
            SendEmail(receiver.Email, "Transfer Notification", $"You have received {request.Amount} from {sender.Email}. Your new balance is {receiver.Balance}.");

            return Ok(new
            {
                message = "Transfer successful",
                senderNewBalance = sender.Balance,
                receiverNewBalance = receiver.Balance
            });
        }


        private bool IsTransactionWithinLimit(int customerId, decimal amount, bool isSender)
        {
            var today = DateTime.Today;
            var transactionsToday = _context.Transaction
                .Where(t => t.CustomerId == customerId && t.Timestamp.Date == today && t.IsSender == isSender)
                .ToList();

            // Tính tổng số tiền đã rút trong ngày cho khách hàng
            var totalAmountToday = transactionsToday
                .Where(t => t.Amount > 0)  // Chỉ tính các giao dịch rút
                .Sum(t => t.Amount);

            var transactionCountToday = transactionsToday.Count();

            var customer = _context.Customer.Find(customerId);

            // Kiểm tra nếu số tiền cộng thêm vượt hạn mức
            if (totalAmountToday + amount > customer.DailyLimit)
                return false; // Vượt quá hạn mức tiền
            if (transactionCountToday >= customer.TransactionCountLimit)
                return false; // Vượt quá hạn mức số giao dịch

            return true;
        }


        private void SendEmail(string to, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("thuddth2307004@fpt.edu.vn", "Duong Thu");
                var toAddress = new MailAddress(to);
                const string fromPassword = "kyxm zvbz nvsn uxxx"; // Địa chỉ email và mật khẩu
                const string smtpServer = "smtp.gmail.com"; // Địa chỉ server SMTP
                const int smtpPort = 587; // Port của server SMTP

                var smtp = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                // Có thể ghi log hoặc xử lý thêm nếu cần
            }
        }

        // Request model cho hàm chuyển khoản


    }
    public class WithdrawRequest
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
    public class TransferRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public decimal Amount { get; set; }

    }
}