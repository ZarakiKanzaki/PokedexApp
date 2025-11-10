using Microsoft.Extensions.DependencyInjection;
using PokedexApp.BasicInfo.Entities;
using PokedexApp.BasicInfo.Utilities.Converters;
using PokedexApp.Common.Dto;
using PokedexApp.Common.Utilities;
using System.Reflection;
using PokedexApp.BasicInfo.Queries; // Add this if needed for AddHttpClient

namespace PokedexApp.BasicInfo;

public static class DependencyInjection
{
    public static IServiceCollection AddBasicInfo(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<IConverter<PokemonSpecies, Pokemon>, PokemonSpeciesToPokemonConverter>();
        services.AddHttpClient<GetPokemonByNameQueryHandler>();
        return services;
    }
}
