
using ClinicaVeterinaria.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicaVeterinaria.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ClinicaVeterinaria.Data.ApplicationUser> _userManager;

        public ReportesController(
            ApplicationDbContext context,
            UserManager<ClinicaVeterinaria.Data.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            return View(usuarios);
        }

        // 1. REPORTE GENERAL DE CITAS
        public async Task<IActionResult> CitasGeneral()
        {
            var citas = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .Text("REPORTE GENERAL DE CITAS")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Mascota").Bold();
                            header.Cell().Element(CellStyle).Text("Servicio").Bold();
                            header.Cell().Element(CellStyle).Text("Fecha").Bold();
                            header.Cell().Element(CellStyle).Text("Estado").Bold();
                        });

                        foreach (var cita in citas)
                        {
                            table.Cell().Element(CellStyle)
                                .Text(cita.Mascota.Nombre);

                            table.Cell().Element(CellStyle)
                                .Text(cita.ServicioVeterinario.Nombre);

                            table.Cell().Element(CellStyle)
                                .Text(cita.FechaCita.ToString("dd/MM/yyyy"));

                            table.Cell().Element(CellStyle)
                                .Text(cita.Estado);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Clínica Veterinaria - Reporte generado ");
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });
                });
            });

            return File(
                pdf.GeneratePdf(),
                "application/pdf",
                "Reporte_General_Citas.pdf");
        }

        // 2. REPORTE DE CITAS POR USUARIO
        public async Task<IActionResult> CitasPorUsuario(string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction(nameof(Index));

            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario == null)
                return NotFound();

            var citas = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .Where(c => c.Mascota.UsuarioId == usuarioId)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .Text("REPORTE DE CITAS POR USUARIO")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(column =>
                    {
                        column.Item()
                            .PaddingBottom(15)
                            .Text($"Cliente: {usuario.NombreCompleto}")
                            .FontSize(13)
                            .Bold();

                        column.Item()
                            .PaddingBottom(15)
                            .Text($"Correo: {usuario.Email}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Mascota").Bold();
                                header.Cell().Element(CellStyle).Text("Servicio").Bold();
                                header.Cell().Element(CellStyle).Text("Fecha").Bold();
                                header.Cell().Element(CellStyle).Text("Estado").Bold();
                            });

                            foreach (var cita in citas)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(cita.Mascota.Nombre);

                                table.Cell().Element(CellStyle)
                                    .Text(cita.ServicioVeterinario.Nombre);

                                table.Cell().Element(CellStyle)
                                    .Text(cita.FechaCita.ToString("dd/MM/yyyy"));

                                table.Cell().Element(CellStyle)
                                    .Text(cita.Estado);
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return File(
                pdf.GeneratePdf(),
                "application/pdf",
                $"Citas_{usuario.NombreCompleto}.pdf");
        }

        // 3. SERVICIOS MÁS SOLICITADOS
        public async Task<IActionResult> ServiciosMasSolicitados()
        {
            var servicios = await _context.ServiciosVeterinarios
                .Select(s => new
                {
                    s.Nombre,
                    s.Precio,
                    TotalCitas = s.Citas.Count()
                })
                .OrderByDescending(s => s.TotalCitas)
                .ToListAsync();

            var pdf = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(40);

                    page.Header()
                        .Text("SERVICIOS MÁS SOLICITADOS")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("#").Bold();
                            header.Cell().Element(CellStyle).Text("Servicio").Bold();
                            header.Cell().Element(CellStyle).Text("Precio").Bold();
                            header.Cell().Element(CellStyle).Text("Citas").Bold();
                        });

                        int posicion = 1;

                        foreach (var servicio in servicios)
                        {
                            table.Cell().Element(CellStyle)
                                .Text(posicion.ToString());

                            table.Cell().Element(CellStyle)
                                .Text(servicio.Nombre);

                            table.Cell().Element(CellStyle)
                                .Text($"Bs. {servicio.Precio:F2}");

                            table.Cell().Element(CellStyle)
                                .Text(servicio.TotalCitas.ToString());

                            posicion++;
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return File(
                pdf.GeneratePdf(),
                "application/pdf",
                "Servicios_Mas_Solicitados.pdf");
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}

