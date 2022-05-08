using Microsoft.AspNetCore.Mvc;
using ProducerAPI.Models;
using ProducerAPI.Services;

namespace ProducerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _UsuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _UsuarioService = usuarioService;
        }

        [HttpPost("cadastrar")]
        public async Task<ActionResult> CadastrarUsuario(UsuarioNovo usuario)
        {
            await _UsuarioService.CadastrarUsuarioNovo(usuario);
            return Ok();
        }
    }
}
