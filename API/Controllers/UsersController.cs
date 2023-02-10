using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public IPhotoService _photoService { get; }
    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _photoService = photoService;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromHeader] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        if(string.IsNullOrEmpty(userParams.Gender)) {
            var currentUser = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }
        
        var users = await _userRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(
            new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        return Ok(await _userRepository.GetMemberByUsernameAsync(username));

    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        _mapper.Map(memberUpdateDto, user);

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var username = User.GetUsername();
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null) return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count() == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _userRepository.SaveAllAsync())
            return CreatedAtAction(nameof(GetUser), new { username = username }, _mapper.Map<PhotoDto>(photo));

        return BadRequest("Something went wrong with adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound("user not found");

        var photo = user.Photos.FirstOrDefault(f => f.Id == photoId);
        if (photo == null) return NotFound("photo not found");

        if (photo.IsMain) return BadRequest("photo already set as main photo");
        var currentMainPhoto = user.Photos.FirstOrDefault(f => f.IsMain);
        if (currentMainPhoto != null) currentMainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Issue with setting main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound("user not found");

        var photo = user.Photos.FirstOrDefault(f => f.Id == photoId);
        if (photo == null) return NotFound("photo not found");

        if (photo.IsMain) return BadRequest("cannot delete main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Issue with deleting photo");
    }


    // [HttpGet("{id}")]
    // public async Task<ActionResult<AppUser>> GetUser(int id) 
    // {
    //     return Ok(await _userRepository.GetUserByIdAsync(id));
    // }
}
