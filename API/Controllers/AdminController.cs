using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        public UserManager<AppUser> _userManager { get; }
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(s => new
            {
                s.Id,
                Username = s.UserName,
                Roles = s.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("edit-roles/{username}")]
        public async Task<ActionResult> EditUserRoles(string username, [FromQuery] string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("select at least one role");
            
            var selectedRoles = roles.Split(',').ToArray();

            var user = await _userManager.FindByNameAsync(username);

            if(user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded) return BadRequest(result.Errors);

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest(result.Errors);

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Only admins and moderators can see this");
        }
    }
}