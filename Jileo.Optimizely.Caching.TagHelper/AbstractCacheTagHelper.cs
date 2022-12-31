using System.Text.Encodings.Web;
using EPiServer.Framework.Cache;
using EPiServer.Personalization.VisitorGroups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Jileo.Optimizely.Caching.TagHelper;

public abstract class AbstractCacheTagHelper : CacheTagHelper
{
    private readonly IVisitorGroupRoleRepository _visitorGroupRoleRepository;
    private readonly IObjectInstanceCache _cache;

    protected AbstractCacheTagHelper(CacheTagHelperMemoryCacheFactory factory,
        HtmlEncoder htmlEncoder,
        IVisitorGroupRoleRepository visitorGroupRoleRepository,
        IObjectInstanceCache cache)
        : base(factory, htmlEncoder)
    {
        _visitorGroupRoleRepository = visitorGroupRoleRepository;
        _cache = cache;
    }
    
 public bool VaryByUrl { get; set; } = true;

    protected bool HasAccess(string role)
    {
        var httpContext = ViewContext.HttpContext;

        return _visitorGroupRoleRepository.TryGetRole(role, out var roleProvider) && roleProvider.IsMatch(httpContext.User, httpContext);
    }

    // Tries to render the content area, and while doing so, collect all visitor groups that are applied to any content area items.
    protected async Task ExtractSettingsFromChildContent(TagHelperContext context, TagHelperOutput output, HttpContext httpContext, VersionRef version)
    {
        var cacheKey = $"{version.Id}-{version.GetValue()}-{WithPathAndQuery()}";

        await _cache.ReadThrough(cacheKey,
            async () =>
            {
                // Something unique so that it doesn't interfere with other caches.
                VaryBy = $"{Guid.NewGuid()}";

                var collection = new CacheDependencyCollectionFeature();
                var visitorGroupRepository = httpContext.RequestServices.GetRequiredService<IVisitorGroupRepository>();
                var allGroups = visitorGroupRepository.List();

                using (new CacheCollectionScope(ViewContext.HttpContext, collection))
                {
                    await base.ProcessAsync(context, new UncachedTagHelperOutput(output.TagName, output.Attributes, output.GetChildContentAsync));
                }

                var visitorGroups = collection.VisitorGroupIds
                    .Join(allGroups, x => x, x => x.Id.ToString(), (_, visitorGroup) => visitorGroup.Name)
                    .ToList();

                version.UpdateDependentContent(collection.Content);
                version.UpdateVisitorGroups(visitorGroups);
            });
    }

    protected string WithPathAndQuery() => VaryByUrl ? PathAndQuery() : string.Empty;

    protected string PathAndQuery() => ViewContext.HttpContext.Request.GetEncodedPathAndQuery();

    protected class UncachedTagHelperOutput : TagHelperOutput
    {
        public UncachedTagHelperOutput(string tagName, TagHelperAttributeList attributes, Func<bool, HtmlEncoder, Task<TagHelperContent>> getChildContentAsync)
            : base(tagName, attributes, Wrap(getChildContentAsync))
        {
        }

        private static Func<bool, HtmlEncoder, Task<TagHelperContent>> Wrap(Func<bool, HtmlEncoder, Task<TagHelperContent>> getChildContentAsync)
        {
            return (_, encoder) => getChildContentAsync(false, encoder);
        }
    }

    private class CacheCollectionScope : IDisposable
    {
        private readonly HttpContext _httpContext;

        public CacheCollectionScope(HttpContext httpContext, CacheDependencyCollectionFeature collection)
        {
            _httpContext = httpContext;

            httpContext.Features.Set<ICacheDependencyCollectionFeature>(collection);
        }

        public void Dispose()
        {
            _httpContext.Features[typeof(ICacheDependencyCollectionFeature)] = null;
        }
    }
}