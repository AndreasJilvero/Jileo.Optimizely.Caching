using EPiServer.Core;
using EPiServer.Web;
using Microsoft.AspNetCore.Http;

namespace Jileo.Optimizely.Caching.TagHelper;

public class CacheCollectingContentAreaLoader : IContentAreaLoader
{
    private readonly IContentAreaLoader _defaultImpl;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CacheCollectingContentAreaLoader(IContentAreaLoader defaultImpl, IHttpContextAccessor httpContextAccessor)
    {
        _defaultImpl = defaultImpl;
        _httpContextAccessor = httpContextAccessor;
    }

    public IContent Get(ContentAreaItem contentAreaItem)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var feature = httpContext?.Features.Get<ICacheDependencyCollectionFeature>();

        feature?.AddContentAreaItem(contentAreaItem);

        return _defaultImpl.Get(contentAreaItem);
    }

    public DisplayOption LoadDisplayOption(ContentAreaItem contentAreaItem)
    {
        return _defaultImpl.LoadDisplayOption(contentAreaItem);
    }
}
