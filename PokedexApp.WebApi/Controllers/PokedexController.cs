using MediatR;
using Microsoft.AspNetCore.Mvc;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.Translator.Queries;

namespace PokedexApp.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokedexController(ILogger<PokedexController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<PokedexController> _logger = logger;
        private readonly IMediator _mediator = mediator;

       [HttpGet("pokemon/{name}")]
        public async Task<IActionResult> GetPokemonByName(string name) 
            => Ok(await _mediator.Send(new GetPokemonByNameQuery(name)));

        [HttpGet("/pokemon/translated/{name}")]
        public async Task<IActionResult> GetTranslatedPokemonByName(string name)
        { 
            var pokemon =  await _mediator.Send(new GetPokemonByNameQuery(name));
            var translatedDescription = await _mediator.Send(new GetTranslatedDescriptionQuery(pokemon));
            pokemon.Description = translatedDescription;
            return Ok(pokemon);
        }
    }
}
