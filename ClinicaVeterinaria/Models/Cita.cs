using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaVeterinaria.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de la cita")]
        public DateTime FechaCita { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public int MascotaId { get; set; }

        [ForeignKey(nameof(MascotaId))]
        public Mascota Mascota { get; set; } = null!;

        [Required]
        public int ServicioVeterinarioId { get; set; }

        [ForeignKey(nameof(ServicioVeterinarioId))]
        public ServicioVeterinario ServicioVeterinario { get; set; } = null!;
    }
}
