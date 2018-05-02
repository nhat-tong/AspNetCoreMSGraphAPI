using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AspNetCore.MSGraphAPI.Models
{
    public class CreateGroupMemberViewModel
    {
        public string GroupId { get; set; }
        public IEnumerable<SelectListItem> GroupListItem { get; set; }
        public List<string> SelectedGroups { get; set; }
    }
}
