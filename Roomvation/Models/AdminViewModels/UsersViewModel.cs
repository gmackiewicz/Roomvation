using System.Collections.Generic;

namespace Roomvation.Models.AdminViewModels
{
    public class UsersViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}