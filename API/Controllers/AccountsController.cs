using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountsController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountsController(DataContext context, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto register)
    {
        if (await UserExists(register.Username))
            return BadRequest("username already exists");

        using var hmac = new HMACSHA512();
        var user = new AppUser()
        {
            UserName = register.Username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
            PasswordSalt = hmac.Key
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        }; 
    }

    private async Task<Boolean> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto login)
    {
        var user = await _context.Users.SingleOrDefaultAsync(user => user.UserName.ToLower() == login.Username.ToLower());
        if (user == null)
            return Unauthorized("invalid username");
        
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
        for (int i =0; i < hashedPassword.Length; i++)
        {
            if(hashedPassword[i] != user.PasswordHash[i])
            return Unauthorized("invalid password");
        }

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        }; 
    }
}
