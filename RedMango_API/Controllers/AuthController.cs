using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.Dto;
using RedMango_API.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace RedMango_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;                   // Using Database dependency injection
        private readonly UserManager<ApplicationUser> _userManager;  // Identity Helper Methods for Users
        private readonly RoleManager<IdentityRole> _roleManager;     // Identity Helper Methods for Roles
        private ApiResponse _response;                               // Using Response Model (Specially for our LoginResponseDTO)
        private string secretKey;                                    // The SecretKey for our JWT Token

        public AuthController(ApplicationDbContext db, IConfiguration configuration
            , UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");  // This is the way to access the value in our "appsettings.json"
            _response = new ApiResponse();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            ApplicationUser userFromDb=
                _db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower()==model.UserName.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);  //Check The password by helper method
            if (isValid ==  false)
            {
                _response.Result = new LoginResponseDTO();
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username or Password is incorrect!");
                return BadRequest(_response);
            }

            //We have to generate JWT Token

            var roles = await _userManager.GetRolesAsync(userFromDb);  // To get user role ( we get roles in type of List<>)

            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key=Encoding.ASCII.GetBytes(secretKey);  // Store our secret key (inside appsettings.json) inside our key type of byte array
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                // In "Subject" we define all the properties that we want to define inside token 
                // In "Expires" we define How long is token valid for
                // In "SigningCredentials" we define that to make our token secure!

                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullName", userFromDb.Name),
                    new Claim("id", userFromDb.Id.ToString()),
                    new Claim(ClaimTypes.Email, userFromDb.UserName.ToString()),  
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),  // We retrieve List<> of roles! but we want omly one role of the user thus we use FirstOrDefault<> method 
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token=tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponse = new()
            {
                Email = userFromDb.Email,
                Token = tokenHandler.WriteToken(token)  // It converts the token to string and pass that to "Token"

            };

            if (loginResponse.Email==null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username or Password is incorrect!");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            ApplicationUser userFromDb =
                _db.ApplicationUsers.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());

            if (userFromDb != null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            ApplicationUser newUSer = new()
            {
                UserName = model.UserName,
                Email = model.UserName,
                NormalizedEmail = model.UserName.ToUpper(),
                Name = model.Name
            };

            var result= await _userManager.CreateAsync(newUSer,model.Password);  // We Create the User in DB by using Helper Method
            try
            {
                if (result.Succeeded)
                {

                    if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult()) // GetAwaiter().GetResult() is because it's async condition and we need await right here!
                    {
                        // If we didn't have any roles, create Roles in DB
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    if (model.Role.ToLower() == SD.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUSer, SD.Role_Admin); // Add that user to Admins
                    }
                    if (model.Role.ToLower() == SD.Role_Customer)
                    {
                        await _userManager.AddToRoleAsync(newUSer, SD.Role_Customer); // Add that user to Customers
                    }
                    _response.IsSuccess = true;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch (Exception)
            {

            }
            _response.IsSuccess= false;
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.ErrorMessages.Add("Error While Registering");
            return BadRequest(_response);
        }
    }
}
