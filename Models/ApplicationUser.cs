using Microsoft.AspNetCore.Identity;

namespace project.Models
{
    // ApplicationUser klasse die de standaard IdentityUser uitbreidt.
    // Je kunt hier aangepaste eigenschappen toevoegen die je wilt opslaan voor elke gebruiker.
    public class ApplicationUser : IdentityUser
    {
        // Voorbeeld van een extra eigenschap die je aan de gebruiker kunt toevoegen.
        // Je kunt hier meer eigenschappen toevoegen zoals FirstName, LastName, etc.
        //public string FullName { get; set; }
        // Add a flag for the approval status
        public bool IsApproved { get; set; }
        // Voeg hier extra eigenschappen toe indien nodig voor je project.
    }
}
