using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using EPiServer.Web.Internal;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Templating;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Jileo.Optimizely.Caching.TagHelper;

public class CacheCollectingContentAreaRenderer : ContentAreaRenderer
{
    protected CacheCollectingContentAreaRenderer(IContentRenderer contentRenderer,
        ITemplateResolver templateResolver,
        IContentAreaItemAttributeAssembler attributeAssembler,
        IContentRepository contentRepository,
        IContentAreaLoader contentAreaLoader,
        IContextModeResolver contextModeResolver,
        ContentAreaRenderingOptions contentAreaRenderingOptions,
        ModelExplorerFactory modelExplorerFactory,
        IModelTemplateTagResolver modelTemplateTagResolver)
        : base(contentRenderer, templateResolver, attributeAssembler, contentRepository, contentAreaLoader, contextModeResolver, contentAreaRenderingOptions,
            modelExplorerFactory, modelTemplateTagResolver)
    {
    }

    public override void Render(IHtmlHelper htmlHelper, ContentArea? contentArea)
    {
        var httpContext = htmlHelper.ViewContext.HttpContext;
        var feature = httpContext.Features.Get<ICacheDependencyCollectionFeature>();

        if (feature != null)
        {
            // Use Items to not only base visitor groups collection on what the current user sees (i.e. FilteredItems)
            foreach (var contentAreaItem in contentArea?.Items ?? new List<ContentAreaItem>())
            {
                feature.AddContentAreaItem(contentAreaItem);
            }
        }

        base.Render(htmlHelper, contentArea);
    }

    protected override void RenderContentAreaItem(IHtmlHelper htmlHelper, ContentAreaItem? contentAreaItem, string templateTag, string htmlTag, string cssClass)
    {
        if (contentAreaItem == null)
        {
            base.RenderContentAreaItem(htmlHelper, contentAreaItem, templateTag, htmlTag, cssClass);
            
            return;
        }
        
        var httpContext = htmlHelper.ViewContext.HttpContext;
        var feature = httpContext.Features.Get<ICacheDependencyCollectionFeature>();

        if (feature != null)
        {
            feature.AddContentAreaItem(contentAreaItem);

            var contentRenderingContext = httpContext.ContentRenderingContext();

            if (contentRenderingContext?.ParentContext != null)
            {
                feature.Content.Add(contentRenderingContext.ParentContext.ContentLink.ToReferenceWithoutVersion());
            }
        }

        base.RenderContentAreaItem(htmlHelper, contentAreaItem, templateTag, htmlTag, cssClass);
    }

    protected override TemplateModel ResolveTemplate(IHtmlHelper htmlHelper, IContent content, IEnumerable<string> templateTags)
    {
        var httpContext = htmlHelper.ViewContext.HttpContext;
        var feature = httpContext.Features.Get<ICacheDependencyCollectionFeature>();

        if (feature != null)
        {
            var contentAreaItems = content.Property.OfType<PropertyContentArea>()
                .Select(x => x.Value)
                .OfType<ContentArea>()
                .SelectMany(x => x.Items);

            foreach (var contentAreaItem in contentAreaItems)
            {
                feature.AddContentAreaItem(contentAreaItem);
            }
        }

        return base.ResolveTemplate(htmlHelper, content, templateTags);
    }
}
