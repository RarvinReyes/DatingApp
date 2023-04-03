using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountsController : BaseApiController
{
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    public UserManager<AppUser> _userManager { get; }
    public AccountsController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto register)
    {
        if (await UserExists(register.Username))
            return BadRequest("username already exists");

        var user = _mapper.Map<AppUser>(register);

        user.UserName = register.Username.ToLower();

        var result = await _userManager.CreateAsync(user, register.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, "Member");

        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);

        var userDto = _mapper.Map<UserDto>(user);

        userDto.Token = await _tokenService.CreateToken(user);
        userDto.PhotoUrl = user.Photos?.FirstOrDefault(f => f.IsMain)?.Url;

        return userDto;
    }

    private async Task<Boolean> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto login)
    {
        var user = await _userManager.Users
        .Include(i => i.Photos)
        .SingleOrDefaultAsync(user => user.UserName.ToLower() == login.Username.ToLower());

        if (user == null)
            return Unauthorized("invalid username");

        var result = _userManager.CheckPasswordAsync(user, login.Password);

        if (!result.Result) Unauthorized("invalid password");

        var userDto = _mapper.Map<UserDto>(user);

        userDto.Token = await _tokenService.CreateToken(user);
        userDto.PhotoUrl = user.Photos?.FirstOrDefault(f => f.IsMain)?.Url;

        return userDto;
    }
}
