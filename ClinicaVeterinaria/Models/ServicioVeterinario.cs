using System.ComponentModel.DataAnnotations;

namespace ClinicaVeterinaria.Models
{
    public class ServicioVeterinario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 100000, ErrorMessage = "El precio debe ser mayor que 0.")]
        public decimal Precio { get; set; }

        // Relación con Citas
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
