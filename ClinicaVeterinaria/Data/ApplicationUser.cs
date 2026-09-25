using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClinicaVeterinaria.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        public ICollection<ClinicaVeterinaria.Models.Mascota> Mascotas { get; set; }
            = new List<ClinicaVeterinaria.Models.Mascota>();
    }
}
