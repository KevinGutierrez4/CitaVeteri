
using System.ComponentModel.DataAnnotations;
using ClinicaVeterinaria.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicaVeterinaria.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
            = new List<AuthenticationScheme>();

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre completo es obligatorio.")]
            [StringLength(100)]
            [Display(Name = "Nombre completo")]
            public string NombreCompleto { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [StringLength(
                100,
                MinimumLength = 6,
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe confirmar la contraseña.")]
            [DataType(DataType.Password)]
            [Compare(
                "Password",
                ErrorMessage = "Las contraseñas no coinciden.")]
            [Display(Name = "Confirmar contraseña")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins = (
                await _signInManager
                    .GetExternalAuthenticationSchemesAsync()
            ).ToList();
        }

        public async Task<IActionResult> OnPostAsync(
            string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins = (
                await _signInManager
                    .GetExternalAuthenticationSchemesAsync()
            ).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ==========================================
            // CREAR USUARIO
            // ==========================================

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                NombreCompleto = Input.NombreCompleto
            };

            var result = await _userManager.CreateAsync(
                user,
                Input.Password);

            // ==========================================
            // SI SE CREÓ CORRECTAMENTE
            // ==========================================

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Usuario {Email} creado correctamente.",
                    Input.Email);

                // ==========================================
                // ASIGNAR ROL CLIENTE AUTOMÁTICAMENTE
                // ==========================================

                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    "Cliente");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return Page();
                }

                // ==========================================
                // INICIAR SESIÓN
                // ==========================================

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                // ==========================================
                // IR AL PANEL DEL CLIENTE
                // ==========================================

                return LocalRedirect("/Cliente");
            }

            // ==========================================
            // MOSTRAR ERRORES
            // ==========================================

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }
    }
}

