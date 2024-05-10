using AutoMapper;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;

namespace BaretStoreWebAPI.Profiles
{
    public class UserProfile: Profile
    {
        public UserProfile()
        {        
            CreateMap<UserDTO, User>();
            CreateMap<User, UserDTO>();

            CreateMap<CreateUserDTO, UserDTO>();

            CreateMap<CreateUserDTO, User>();
            CreateMap<User, CreateUserDTO>();
        }
    }
}
