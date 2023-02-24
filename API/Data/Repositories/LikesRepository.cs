using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public IMapper _mapper { get; }
        public LikesRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikesDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.Include(i => i.Photos).OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(w => w.SourceUserId == likesParams.UserId);
                users = likes.Select(s => s.TargetUser);
            }
            else if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(w => w.TargetUserId == likesParams.UserId);
                users = likes.Select(s => s.SourceUser);
            }
            return await PagedList<LikesDto>.CreateAsync(
                users.AsNoTracking().ProjectTo<LikesDto>(_mapper.ConfigurationProvider),
                likesParams.PageNumber,
                likesParams.PageSize
            );
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(i => i.UsersLiked).FirstOrDefaultAsync(f => f.Id == userId);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}