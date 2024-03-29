using System.Collections.Generic;

namespace project.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        // Additional properties to hold all roles available in the system for assignment
        public IList<string> AvailableRoles { get; set; }
        // This will be true if the user is awaiting approval
        public bool IsAwaitingApproval { get; set; }
        public string SelectedRole { get; set; }
    }
}