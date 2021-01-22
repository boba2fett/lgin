using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Entities;
using WebApi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public interface IUserService
    {
        bool IsAdmin(int userId);
        bool HasGroup(int userId, int groupId, int ownId);
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll(int ownId);
        IEnumerable<Group> GetGroups(int userId, int ownId);
        IEnumerable<UserGroup> GetUserGroups(int userId, int ownId);
        void AddGroups(int userId, IEnumerable<int> groupIds, int ownId);
        void AddGroup(int userId, int groupId, int ownId);
        void RemoveGroup(UserGroup userGroup, int ownId);
        void RemoveGroups(int userId, IEnumerable<int> groupIds, int ownId);
        User GetById(int id, int ownId);
        User Create(User user, string password);
        void Update(User user, int ownId, string password = null);
        void Delete(int id,int ownId);
        void Test();
    }

    public class UserService : IUserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public bool IsAdmin(int userId)
        {
            var group = _context.Groups.SingleOrDefault(x => x.Name == "Admin");
            return group != null && HasGroup(userId, group.Id, userId);
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll(int ownId)
        {
            return _context.Users;
        }

        public IEnumerable<Group> GetGroups(int userId, int ownId)
        {
            //var groups = _context.Groups.Where(x => x.UserGroup.Any(c => c.UserId == userId));
            //return groups;
            if(userId == ownId || IsAdmin(ownId))
            {
                // var userGroups = _context.UserGroup.Where(c => c.UserId == userId);
                // IEnumerable<Group> groups = new List<Group>();
                // foreach(UserGroup userGroup in userGroups)
                // {
                //     groups = groups.Append(_context.Groups.Find(userGroup.GroupId));
                // }
                // return groups;
                return _context.UserGroup.Where(UserGroup=>UserGroup.UserId==userId).Select(ug=>ug.Group);
            }
            return new List<Group>();
        }

        public IEnumerable<UserGroup> GetUserGroups(int userId, int ownId)
        {
            if(userId == ownId || IsAdmin(ownId))
            {
                //var groups = _context.Groups.Where(x => x.UserGroup.Any(c => c.UserId == userId));
                //return groups;
                var groups = _context.UserGroup.Where(c => c.UserId == userId);
                return groups;
            }
            return new List<UserGroup>();
        }

        public bool HasGroup(int userId, int groupId, int ownId)
        {
            if(ownId == userId || IsAdmin(ownId))
            {
                var res =_context.UserGroup.Where(c => c.UserId == userId).Where(c=>c.GroupId == groupId).Any();
                //Console.WriteLine($"Has User {userId} got group {groupId} asks {ownId}? {res}");
                return res;
            }
            return false;
        }

        public void AddGroups(int userId, IEnumerable<int> groupIds, int ownId)
        {
            foreach(int groupId in groupIds)
            {
                AddGroup(userId, groupId, ownId);
            }
        }

        public void AddGroup(int userId, int groupId, int ownId)
        {
            if(IsAdmin(ownId) || HasGroup(ownId, groupId, ownId))
            {
                if(!HasGroup(userId, groupId, ownId))
                {
                    var user =_context.Users.Find(userId);
                    var group =_context.Groups.Find(groupId);
                    
                    var userGroup = new UserGroup{
                        UserId=userId,
                        User = user,
                        GroupId=groupId,
                        Group = group
                    };

                    Console.WriteLine($"Add group {userGroup.GroupId} to User {userGroup.UserId}");
                    
                    //_context.Groups.Attach(group);
                    //_context.Users.Attach(user);
                    
                    //_context.ChangeTracker.TrackGraph(userGroup, node =>
                    //    node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged);
                    _context.UserGroup.Add(userGroup);
                    _context.SaveChanges();
                }
            }
            
        }

        public void RemoveGroup(UserGroup userGroup, int ownId)
        {
            if(IsAdmin(ownId) || HasGroup(ownId, userGroup.GroupId, ownId) && ownId == userGroup.UserId)
            {
                if(HasGroup(userGroup.UserId, userGroup.GroupId, ownId))
                {
                    _context.UserGroup.Remove(userGroup);
                    _context.SaveChanges();
                }
            }
        }

        public void RemoveGroups(int userId, IEnumerable<int> groupIds, int ownId)
        {
            foreach(int groupId in groupIds)
            {
                RemoveGroup(new UserGroup
                {
                    UserId=userId,
                    GroupId=groupId
                },
                 ownId);
            }
        }

        public void Test()
        {

        }

        public User GetById(int id,int ownId)
        {
            return _context.Users.Find(id);
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_context.Users.Any(x => x.Username == user.Username))
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Update(User userParam, int ownId, string password = null)
        {
            if(ownId == userParam.Id)
            {
                var user = _context.Users.Find(userParam.Id);

                if (user == null)
                    throw new AppException("User not found");

                // update username if it has changed
                if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
                {
                    // throw error if the new username is already taken
                    if (_context.Users.Any(x => x.Username == userParam.Username))
                        throw new AppException("Username " + userParam.Username + " is already taken");

                    user.Username = userParam.Username;
                }

                // update password if provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                _context.Users.Update(user);
                _context.SaveChanges();
            }
        }

        public void Delete(int id, int ownId)
        {
            if(IsAdmin(ownId)||ownId == id)
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }
            }
        }

        // private helper methods

        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}