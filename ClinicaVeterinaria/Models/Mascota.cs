using ClinicaVeterinaria.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaVeterinaria.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria.")]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [Required(ErrorMessage = "La raza es obligatoria.")]
        [StringLength(50)]
        public string Raza { get; set; } = string.Empty;

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey(nameof(UsuarioId))]
        public ApplicationUser Usuario { get; set; } = null!;

        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}
