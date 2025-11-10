using Microsoft.Extensions.DependencyInjection;
using PokedexApp.Translator.Queries;
using System.Reflection;

namespace PokedexApp.Translator;

public static class DependencyInjection
{
    public static IServiceCollection AddTranslator(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddHttpClient<GetTranslatedDescriptionQueryHandler>();
        return services;
    }
}
