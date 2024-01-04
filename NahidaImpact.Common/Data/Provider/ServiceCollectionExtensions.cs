using Microsoft.Extensions.DependencyInjection;

namespace NahidaImpact.Common.Data.Provider;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseLocalAssets(this IServiceCollection services)
    {
        return services.AddSingleton<IAssetProvider, LocalAssetProvider>();
    }
}
