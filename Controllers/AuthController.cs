using Microsoft.AspNetCore.Mvc;
using ATMManagementApplication.Models;
using ATMManagementApplication.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace ATMManagementApplication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ATMContext _context;
        private readonly IConfiguration _configuration; // Thêm IConfiguration

        public AuthController(ATMContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Khởi tạo IConfiguration
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Customer login)
        {
            var customer = _context.Customer.FirstOrDefault(c => c.Name == login.Name && c.Password == login.Password);
            if (customer == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Tạo JWT
            var token = GenerateJwtToken(customer);
            return Ok(new { Token = token }); // Trả về token
        }

        // Hàm tạo JWT
        private string GenerateJwtToken(Customer customer)
        {
            // Lấy các giá trị từ file cấu hình
            var key = _configuration.GetValue<string>("Jwt:Key"); // Đảm bảo rằng key này có ít nhất 32 bytes
            var issuer = _configuration.GetValue<string>("Jwt:Issuer");
            var audience = _configuration.GetValue<string>("Jwt:Audience");
            var expiryDuration = _configuration.GetValue<int>("Jwt:ExpiryDuration");

            // Tạo các claims
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, customer.Name),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Sử dụng khóa bí mật mới với độ dài đủ
            var keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length < 32) // Đảm bảo khóa có độ dài tối thiểu là 32 bytes
            {
                throw new ArgumentException("The JWT key must be at least 256 bits (32 bytes) long.");
            }

            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryDuration),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Hàm đăng ký
        [HttpPost("register")]
        public IActionResult Register([FromBody] Customer newCustomer)
        {
            var existingCustomer = _context.Customer.FirstOrDefault(c => c.Name == newCustomer.Name);
            if (existingCustomer != null)
            {
                return Conflict("User already exists");
            }

            _context.Customer.Add(newCustomer);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Login), new { name = newCustomer.Name }, newCustomer);
        }
        [HttpPost("verify-token")]
        public IActionResult VerifyToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key"));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration.GetValue<string>("Jwt:Issuer"),
                    ValidAudience = _configuration.GetValue<string>("Jwt:Audience"),
                    ClockSkew = TimeSpan.Zero // Không cho phép khoảng trễ thời gian
                }, out SecurityToken validatedToken);

                return Ok("Token is valid");
            }
            catch
            {
                return Unauthorized("Invalid or expired token");
            }
        }

        // Hàm đổi mật khẩu
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Kiểm tra điều kiện đầu vào
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.OldPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest("All fields are required.");
            }

            // Tìm khách hàng theo tên và mật khẩu cũ
            var customer = _context.Customer.FirstOrDefault(c => c.Name == request.Name && c.Password == request.OldPassword);
            if (customer == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Kiểm tra mật khẩu mới
            if (request.NewPassword == request.OldPassword)
            {
                return BadRequest("New password cannot be the same as the old password.");
            }

            // Cập nhật mật khẩu
            customer.Password = request.NewPassword;

            try
            {
                _context.SaveChanges(); // Ghi thay đổi vào cơ sở dữ liệu
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while changing the password: " + ex.Message);
            }

            return Ok("Password changed successfully");
        }

    }

    public class ChangePasswordRequest
    {
        public string Name { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
