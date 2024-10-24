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


        [HttpGet("/balance/{customerId}")]
        public ActionResult GetBalance(int customerId)
        {
            Console.WriteLine(customerId);
            var customer = _context.Customer.Find(customerId);
            if (customer == null) { }
            return NotFound("Cusomer not found");

            return Ok(new { balance = customer.Balance });
        }

        [HttpGet("withdraw")]
        public IActionResult Withdraw([FromBody] WithdrawRequest request)
        {
            var customer = _context.Customer.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");
            if (customer.Balance < request.Amount)
                return BadRequest("Insufficient balance");
            customer.Balance -= request.Amount;

            var transacion = new Transaction{
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(transacion);
            _context.SaveChanges();
            return Ok(new { message="with draw successful",newBalance = customer.Balance});
        }
        [HttpGet("plush")]
        public IActionResult Plus([FromBody] WithdrawRequest request){
            var customer = _context.Customer.Find(request.CustomerId);
            if (customer == null)
                return NotFound("Customer not found");
            customer.Balance += request.Amount;
            var transacion = new Transaction{
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Timestamp = DateTime.Now,
                IsSuccessful = true
            };
            _context.Transaction.Add(transacion);
            _context.SaveChanges();
            return Ok(new { message="with draw successful",newBalance = customer.Balance});   
        }
    }
    public class WithdrawRequest{
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }
}