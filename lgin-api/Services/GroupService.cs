using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IGroupService
    {
        IEnumerable<Group> GetAll(int ownId);
        IEnumerable<User> GetMember(int groupId, int ownId);
        Group GetById(int id, int ownId);
        Group Create(Group group, int ownId);
        void Update(Group group, int ownId);
        void Delete(int id,int ownId);
    }

    public class GroupService : IGroupService
    {
        private DataContext _context;
        IUserService _userService;

        public GroupService(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IEnumerable<Group> GetAll(int ownId) //filter for the ones user is able to see
        {
            if(_userService.IsAdmin(ownId))
            {
                return _context.Groups;
            }
            return _userService.GetGroups(ownId,ownId);
        }

        public IEnumerable<User> GetMember(int groupId, int ownId)
        {
            if(_userService.IsAdmin(ownId) || _userService.HasGroup(ownId, groupId, ownId))
            {
                // var userIds = _context.UserGroup.Where(c => c.GroupId == groupId).Select(u => u.UserId);
                // IEnumerable<User> users = new List<User>();
                // foreach(int userId in userIds)
                // {
                //     users = users.Append(_context.Users.Find(userId));
                // }
                // return users;
                return _context.UserGroup.Where(UserGroup=>UserGroup.GroupId==groupId).Select(ug=>ug.User);
            }
            return null;
        }

        public Group GetById(int id,int ownId)
        {
            if(_userService.IsAdmin(ownId) || _userService.HasGroup(ownId, id, ownId))
            {
                return _context.Groups.Find(id);
            }
            return null;
        }

        public Group Create(Group group, int ownId)
        {
            if(_userService.IsAdmin(ownId))
            {
                if (_context.Groups.Any(x => x.Name == group.Name))
                    throw new AppException("GroupName \"" + group.Name + "\" is already taken");

                _context.Groups.Add(group);
                _context.SaveChanges();

                return group;
            }
            return null;
        }

        public void Update(Group groupParam, int ownId)
        {
            if(_userService.IsAdmin(ownId))
            {
                var group = _context.Groups.Find(groupParam.Id);

                if (group == null)
                    throw new AppException("Group not found");

                // update username if it has changed
                if (!string.IsNullOrWhiteSpace(groupParam.Name) && groupParam.Name != groupParam.Name)
                {
                    // throw error if the new username is already taken
                    if (_context.Groups.Any(x => x.Name == groupParam.Name))
                        throw new AppException("GroupName " + groupParam.Name + " is already taken");

                    group.Name = groupParam.Name;
                }

                _context.Groups.Update(group);
                _context.SaveChanges();
            }
        }

        public void Delete(int id, int ownId)
        {
            if(_userService.IsAdmin(ownId))
            {
                var group = _context.Groups.Find(id);
                if (group != null)
                {
                    _context.Groups.Remove(group);
                    _context.SaveChanges();
                }
            }
        }
    }
}