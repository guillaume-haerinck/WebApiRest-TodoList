using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using TodoApi.Data;
using TodoApi.Models;


namespace TodoApi.Controllers
{
    // By default any data here is restricted to logged users with JWT header
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly TodoDbContext _context;

        public UsersController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            TodoDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        // ****************************************
        //          Start GET operations
        // ****************************************

        // Get /api/users
        [HttpGet]
        [Route("api/[controller]")]
        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToList();
        }

        // Get /api/users/{id}
        [HttpGet("{id}", Name = "GetUser")]
        [Route("api/[controller]/{id}")]
        public IActionResult GetById(string id)
        {
            var user = _context.Users.FirstOrDefault(t => t.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return new ObjectResult(user);
        }

        // ***********************************************
        //          Start Login and JWT operations
        // ***********************************************

        // Post /api/login
        [HttpPost]
        [Route("api/[action]")]
        [AllowAnonymous]
        public async Task<object> Login([FromBody] UserRequest request)
        {
            if (request.UserName == null || request.Password == null)
            {
                return BadRequest();
            }

            var result = await _signInManager.PasswordSignInAsync(request.UserName,
                                                                request.Password,
                                                                isPersistent: false,
                                                                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = _userManager.Users.SingleOrDefault(r => r.UserName == request.UserName);
                return await GenerateJwtToken(user);
            }

            return BadRequest();
            //throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        // Post /api/users/
        [HttpPost]
        [Route("api/[controller]")]
        [AllowAnonymous]
        public async Task<object> Register([FromBody] UserRequest request)
        {
            if ((request.UserName == null || request.Password == null) || request.Mail == null)
            {
                return BadRequest();
            }

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Mail
            };

            // Automatically Hash the password with salt with the Rfc2898DeriveBytes function
            // https://aspnetidentity.codeplex.com/SourceControl/latest#src/Microsoft.AspNet.Identity.Core/Crypto.cs
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return await GenerateJwtToken(user);
            }

            return BadRequest();
            //throw new ApplicationException("USER_NOT_CREATED");
        }

        // Cannot be called by browser
        private async Task<object> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Token:JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["Token:JwtIssuer"],
                _configuration["Token:JwtAudience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
