using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jileo.Optimizely.Caching.TagHelper;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCachingTagHelper(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICacheDependencyTracker, CacheDependencyTracker>()
            .AddTransient<ContentAreaRenderer, CacheCollectingContentAreaRenderer>()
            .Intercept<IContentAreaLoader>((provider, defaultImpl) =>
                new CacheCollectingContentAreaLoader(defaultImpl, provider.GetRequiredService<IHttpContextAccessor>()));
    }
}
