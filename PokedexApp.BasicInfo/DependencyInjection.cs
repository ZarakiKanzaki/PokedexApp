using Microsoft.Extensions.DependencyInjection;
using PokedexApp.BasicInfo.Dto;
using PokedexApp.BasicInfo.Entities;
using PokedexApp.BasicInfo.Utilities;
using PokedexApp.BasicInfo.Utilities.Converters;
using System.Reflection;

namespace PokedexApp.BasicInfo;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<IConverter<PokemonSpecies, Pokemon>, PokemonSpeciesToPokemonConverter>();
        return services;
    }
}
