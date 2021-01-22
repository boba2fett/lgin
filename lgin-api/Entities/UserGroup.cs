using System;
using System.Collections.Generic;

namespace WebApi.Entities
{
    public class UserGroup
    {
        public DateTime MemberSince {get; set;}
        public int UserId { get; set; }
        public User User { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}