using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountsController : BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    public AccountsController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto register)
    {
        if (await UserExists(register.Username))
            return BadRequest("username already exists");

        var user = _mapper.Map<AppUser>(register);

        using var hmac = new HMACSHA512();

        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password));
        user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = _mapper.Map<UserDto>(user);

        userDto.Token = _tokenService.CreateToken(user);
        userDto.PhotoUrl = user.Photos?.FirstOrDefault(f => f.IsMain)?.Url;

        return userDto;
    }

    private async Task<Boolean> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto login)
    {
        var user = await _context.Users
        .Include(i => i.Photos)
        .SingleOrDefaultAsync(user => user.UserName.ToLower() == login.Username.ToLower());

        if (user == null)
            return Unauthorized("invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));
        for (int i = 0; i < hashedPassword.Length; i++)
        {
            if (hashedPassword[i] != user.PasswordHash[i])
                return Unauthorized("invalid password");
        }

        var userDto = _mapper.Map<UserDto>(user);

        userDto.Token = _tokenService.CreateToken(user);
        userDto.PhotoUrl = user.Photos?.FirstOrDefault(f => f.IsMain)?.Url;

        return userDto;
    }
}
