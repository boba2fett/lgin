using AutoMapper;
using WebApi.Entities;
using WebApi.Models.Users;
using WebApi.Models.Groups;
using WebApi.Models.UserGroups;

namespace WebApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<UpdateModel, User>();

            CreateMap<Group, GroupModel>();
            CreateMap<AddGroupModel, Group>();
            CreateMap<UpdateGroupModel, Group>();

            CreateMap<UserGroup, UserGroupModel>();
        }
    }
}