using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks; // VERIFICAR
using LogBlazorWebApp.Configurations;
using LogBlazorWebApp.Client.Models;
using LogBlazorWebApp.Services;
using Microsoft.AspNetCore.Authorization;

// Controlador API para la autenticación de usuarios.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<IdentityUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Retorna los errores de validación
        }

        var user = await _userManager.FindByNameAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { Error = "Credenciales inválidas" });
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    // Método para registrar a un usuario.
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Return validation errors
        }
        var email = model.Email.ToLower(); // Convert email to lowercase
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors); // Return errors if user creation failed
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new { Token = token }); // Return the generated token
    }
}
