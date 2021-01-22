using System.Collections.Generic;

namespace WebApi.Models.Groups
{
     public class AddRmUserGroupModel
    {
        public int GroupId { get; set; }
    }

    public class AddRmUserGroupsModel
    {
        public IEnumerable<int> GroupIds { get; set; }
    }
}