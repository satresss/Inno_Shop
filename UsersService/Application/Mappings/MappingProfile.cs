using AutoMapper;
using UserService.Application.DTO;
using UserService.Domain.Entities;

namespace UserService.API.Mappings 
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}