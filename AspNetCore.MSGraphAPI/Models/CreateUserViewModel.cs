using System.ComponentModel.DataAnnotations;

namespace AspNetCore.MSGraphAPI.Models
{
    public class CreateUserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string UserPrincipalName { get; set; }
        [Required]
        public string Password { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string Domain { get; set; }
    }
}
