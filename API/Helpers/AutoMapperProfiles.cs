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
        }
    }
}