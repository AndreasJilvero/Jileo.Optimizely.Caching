using System.Text.Encodings.Web;
using EPiServer.Framework.Cache;
using EPiServer.Personalization.VisitorGroups;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Jileo.Optimizely.Caching.TagHelper;

public class ContentAreaCacheTagHelper : AbstractCacheTagHelper
{
    private readonly ILogger<ContentAreaCacheTagHelper> _logger;

    public ContentAreaCacheTagHelper(CacheTagHelperMemoryCacheFactory factory,
        HtmlEncoder htmlEncoder,
        IVisitorGroupRoleRepository visitorGroupRoleRepository,
        IObjectInstanceCache cache,
        ILogger<ContentAreaCacheTagHelper> logger)
        : base(factory, htmlEncoder, visitorGroupRoleRepository, cache)
    {
        _logger = logger;
        
        // Some expiration must be set, otherwise there's a default sliding expiration of 30 seconds.
        ExpiresSliding = TimeSpan.FromHours(1);
    }

    public Type[] DependentTypes { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var varyBy = VaryBy; // VaryBy is set in ExtractSettingsFromChildContent, thus save it here and re-use this value later.
        var key = VersionRef.CreateKey(context, WithPathAndQuery());
        var version = VersionRef.Versions.GetOrAdd(key, id => new VersionRef(id, DependentTypes));

        try
        {
            await ExtractSettingsFromChildContent(context, output, ViewContext.HttpContext, version);

            var matchingRoles = string.Join(",", version.GetVisitorGroups().Where(HasAccess));

            VaryBy = $"{varyBy}-{WithPathAndQuery()}-{matchingRoles}-{version.GetValue()}";

            await base.ProcessAsync(context, output);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not serve contents of content-area-cache tag.");

            VaryBy = null;
            ExpiresSliding = TimeSpan.FromSeconds(30);

            // Clear any already existing output from this tag helper
            output.SuppressOutput();

            await base.ProcessAsync(context, new UncachedTagHelperOutput(output.TagName, output.Attributes, output.GetChildContentAsync));
        }
    }
}