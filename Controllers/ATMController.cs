using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using System.Linq;
using System;

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
            if (customer.Balance < request.Amount)
                return BadRequest("Insufficient balance");
            customer.Balance -= request.Amount;

            var transacion = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(transacion);
            _context.SaveChanges();
            return Ok(new { message = "with draw successful", newBalance = customer.Balance });
        }
        [HttpPost("plush")]
        public IActionResult Plus([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customer.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");
            customer.Balance += request.Amount;
            var transacion = new Transaction
            {
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(transacion);
            _context.SaveChanges();
            return Ok(new { message = "with draw successful", newBalance = customer.Balance });
        }
        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest request)
        {
            // Tìm thông tin người gửi
            var sender = _context.Customer.Find(request.SenderId);
            if (sender == null)
                return NotFound("Sender not found");

            // Tìm thông tin người nhận
            var receiver = _context.Customer.Find(request.ReceiverId);
            if (receiver == null)
                return NotFound("Receiver not found");

            // Kiểm tra số dư của người gửi
            if (sender.Balance < request.Amount)
                return BadRequest("Insufficient balance");

            // Thực hiện trừ số dư người gửi
            sender.Balance -= request.Amount;

            // Thực hiện cộng số dư người nhận
            receiver.Balance += request.Amount;

            // Tạo giao dịch cho người gửi
            var sendTransaction = new Transaction
            {
                CustomerId = request.SenderId,
                Amount = -request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(sendTransaction);

            // Tạo giao dịch cho người nhận
            var receiveTransaction = new Transaction
            {
                CustomerId = request.ReceiverId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(receiveTransaction);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Trả về kết quả thành công
            return Ok(new
            {
                message = "Transfer successful",
                senderNewBalance = sender.Balance,
                receiverNewBalance = receiver.Balance
            });
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