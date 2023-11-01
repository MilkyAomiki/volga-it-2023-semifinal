using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Simbir.GO.WebApi.Models;

namespace Simbir.GO.WebApi.Controllers;

[Route("api/Admin/Account")]
[ApiController]
public class AdminAccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public AdminAccountController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAccounts(int start, int count)
    {
        var accounts = _userManager.Users.Skip(start).Take(count).ToList();
        List<object> resultAccounts = new(accounts.Count());
        foreach (var user in accounts)
        {
            resultAccounts.Add(new { user, roles = await _userManager.GetRolesAsync(user)});
        }
        return Ok(resultAccounts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new { user, roles = await _userManager.GetRolesAsync(user)});
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateAccount([FromBody] AccountRequest model)
    {
        var existingUser = await _userManager.FindByNameAsync(model.Username);
        if (existingUser is not null)
        {
            return Conflict("Username already exists");
        }

        IdentityUser user = new()
        {
            UserName = model.Username
        };
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
        var result = await _userManager.CreateAsync(user);

        if (result.Succeeded)
        {
            // if (model.IsAdmin)
            // {
            //     await _userManager.AddToRoleAsync(newUser, "Admin");
            // }

            return Created("", user);
        }

        return BadRequest(result.Errors);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateAccount(string id, [FromBody] AccountRequest model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.UserName != model.Username)
        {
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists");
            }
        }

        user.UserName = model.Username;
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            // if (model.IsAdmin)
            // {
            //     await _userManager.AddToRoleAsync(user, "Admin");
            // }
            // else
            // {
            //     await _userManager.RemoveFromRoleAsync(user, "Admin");
            // }
            return Ok(user);
        }

        return BadRequest(result.Errors);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(result.Errors);
    }
}
