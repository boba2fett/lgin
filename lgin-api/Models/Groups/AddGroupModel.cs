using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Groups
{
    public class AddGroupModel
    {
        [Required]
        public string Name { get; set; }
    }
}