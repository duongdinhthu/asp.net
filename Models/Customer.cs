using System.ComponentModel.DataAnnotations;

namespace ATMManagementApplication.Models{
    public class Customer {
        [Key] //Annotation
        public int CustomerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password{ get; set; }
        public decimal Balance{ get; set; }
    }
}