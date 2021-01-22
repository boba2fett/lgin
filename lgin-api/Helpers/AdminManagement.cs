using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApi.Services;
using WebApi.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace WebApi.Helpers
{
    public interface IAdminManagement
    {
        void check();
    }

    public class AdminManagement : IAdminManagement
    {
        protected readonly IConfiguration Configuration;
        private DataContext _context;
        public AdminManagement(IConfiguration configuration, DataContext context)
        {
            Configuration = configuration;
            _context = context;
        }
        public void check()
        {
            if(!_context.Users.Any())
            {
                Console.WriteLine("Configure Admin Imperator with Password ThisIsMadnessNoThisIsSparta!");
                UserService.CreatePasswordHash("ThisIsMadnessNoThisIsSparta!",out var passwordHash, out var passwordSalt);
                User admin = new User
                {
                    Id = 0,
                    Username = "Imperator",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Groups=new List<Group>()
                };
                Group adminGroup = new Group
                {
                    Id = 0,
                    Name = "Admin"
                };
                // UserGroup ug = new UserGroup()
                // {
                //     UserId=0,
                //     GroupId=0
                // }
                admin.Groups.Add(adminGroup);
                _context.Groups.Add(adminGroup);
                _context.Users.Add(admin);
                _context.SaveChanges();
            }
        }
    }
}