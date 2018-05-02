using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.MSGraphAPI.Models
{
    public class CreateMemberOrOwnerViewModel
    {
        public string ActionName { get; set; }
        public string GroupId { get; set; }
        public IEnumerable<SelectListItem> ListItem { get; set; }
        [Display(Name = "Select a user")]
        public List<string> SelectedItems { get; set; }
    }
}
