using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(m => m.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.Where(w => w.IsMain).FirstOrDefault().Url))
                .ForMember(m => m.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            //CreateMap<IEnumerable<AppUser>, IEnumerable<MemberDto>>();
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<AppUser, MemberUpdateDto>();
            CreateMap<RegisterDto, AppUser>()
                .ForMember(m => m.UserName, opt => opt.MapFrom(src => src.Username));
            CreateMap<AppUser, UserDto>()
                .ForMember(m => m.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(m => m.Token, opt => opt.Ignore())
                .ForMember(m => m.PhotoUrl, opt => opt.Ignore());
            CreateMap<AppUser, LikesDto>()
                .ForMember(m => m.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(m => m.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()))
                .ForMember(m => m.PhotoUrl, opt => opt.MapFrom(src => src.Photos.Where(w => w.IsMain).FirstOrDefault().Url));
                //.ForMember(m => m.PhotoUrl, opt => opt.Ignore());
        }
    }
}