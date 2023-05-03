using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public IMapper _mapper { get; }
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public async Task<MemberDto> GetMemberByIdAsync(int id)
        {
            return await _context.Users
            .Where(w => w.Id == id)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }

        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            return await _context.Users
            .Where(w => w.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();

            query = query.Where(w => w.UserName != userParams.CurrentUsername);
            query = query.Where(w => w.Gender == userParams.Gender);

            var minDob = DateOnly.FromDateTime(DateTime.Now.AddYears(-(userParams.MaxAge + 1)));
            var maxDob = DateOnly.FromDateTime(DateTime.Now.AddYears(-userParams.MinAge));

            query = query.Where(w => (w.DateOfBirth >= minDob && w.DateOfBirth <= maxDob));

            query = userParams.OrderBy switch
            {
                "dateCreated" => query.OrderByDescending(o => o.DateCreated),
                _ => query.OrderByDescending(o => o.LastActive)
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
                userParams.PageNumber,
                userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(i => i.Photos).FirstOrDefaultAsync(f => f.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(i => i.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
            .Where(w => w.UserName == username)
            .Select(s => s.Gender)
            .FirstOrDefaultAsync();
        }
    }
}