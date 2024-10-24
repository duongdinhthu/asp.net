using System.ComponentModel.DataAnnotations;

namespace ATMManagementApplication.Models
{
    public class Customer
    {
        [Key] //Annotation
        public int CustomerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        public decimal Balance { get; set; }
        public string Email { get; set; } // Thêm trường Email

         public decimal DailyLimit { get; set; } = 5000;
          public int TransactionCountLimit { get; set; } = 10; 
    }
}