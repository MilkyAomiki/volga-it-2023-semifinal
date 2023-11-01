using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Simbir.GO.WebApi.Models;

namespace Simbir.GO.WebApi.Controllers;

[Route("api/Account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet("Me")]
    [Authorize]
    public async Task<IActionResult> GetAccountInfo()
    {
        // Get the current user's account information.
        return Ok(new { Username = HttpContext.User.Identity.Name });
    }

    [HttpPost("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] AccountRequest model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);

        if (user is null)
            return Unauthorized("Invalid username");

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid password");

        var token = GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
        return Ok(new { Token = token });
    }

    [HttpPost("SignUp")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] AccountRequest model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username);

        if (userExists != null)
            return Conflict("User already exists");

        IdentityUser user = new()
        {
            UserName = model.Username
        };
        user.PasswordHash = HashPassword(user, model.Password);

        var createUserResult = await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, "user");

        if (!createUserResult.Succeeded)
            return BadRequest("User creation failed! Please check user details and try again.");

        return Ok(new { Message = "Registration successful" });
    }

    [HttpPost("SignOut")]
    [Authorize]
    public IActionResult SignOut()
    {
        // make it smhw expire?
        return Ok(new { Message = "Sign out successful" });
    }

    [HttpPut("Update")]
    [Authorize]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountRequest model)
    {
        // Get the current user's account.
        var user = await _userManager.FindByNameAsync(model.Username);

        if (user is not null && HttpContext.User.Identity.Name != user.UserName)
            return NotFound("Username is already in use");

        user = new()
        {
            UserName = model.Username
        };
        user.PasswordHash = HashPassword(user, model.Password);

        var newUser = await _userManager.UpdateAsync(user);

        return Ok(new { Message = "Account updated successfully", updatedUser = newUser});
    }

    private string GenerateJwtToken(IdentityUser account, IList<string> roles)
    {
        roles ??= new List<string>();

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey:SecretKey"]));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWTKey:ValidIssuer"],
            Audience = _configuration["JWTKey:ValidAudience"],
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt64(_configuration["JWTKey:TokenExpiryTimeInMinutes"])),
            SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new List<Claim>
            {
               new(ClaimTypes.Name, account.UserName),
               new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            })
        };

        if (roles is not [])
        {
            tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, roles.FirstOrDefault()));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(IdentityUser user, string password)
    {
        return _userManager.PasswordHasher.HashPassword(user, password);
    }
}
