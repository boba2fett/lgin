using System.Collections.Generic;

namespace WebApi.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<UserGroup> UserGroup { get; set; }
    }
}