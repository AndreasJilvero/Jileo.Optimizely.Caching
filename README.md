# Jileo.Optimizely.Caching
A cache tag helper for ContentArea content

The added benefit of this tag helper (in comparison to [CacheTagHelper](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/cache-tag-helper?view=aspnetcore-7.0)) is that it's capable of keeping track of what content is included in the content area. This is done by overriding `IContentAreaLoader` and `ContentAreaRenderer`. In addition, you can also specify a list of dependent types that if published, will invalidate the cached content.

## How to use

1. Add to `Startup.cs`
```
services.AddCachingTagHelper();
```
2. Listen for published content events:
```
[InitializableModule]
public class ContentAreaCacheTagHelperInitializationModule : IInitializableModule
{
    private IContentEvents _contentEvents;
    private ICacheDependencyTracker _cacheDependencyTracker;

    public void Initialize(InitializationEngine context)
    {
        _contentEvents = context.Locate.Advanced.GetRequiredService<IContentEvents>();
        _cacheDependencyTracker = context.Locate.Advanced.GetRequiredService<ICacheDependencyTracker>();

        _contentEvents.PublishedContent += OnPublishedContent;
    }

    public void Uninitialize(InitializationEngine context)
    {
        _contentEvents.PublishedContent -= OnPublishedContent;
    }

    private void OnPublishedContent(object? sender, ContentEventArgs e)
    {
        _cacheDependencyTracker.Invalidate(e.Content);
    }
}
```
3. Add `ContentAreaCacheTagHelper` to `_ViewImports.cshtml`
```
@addTagHelper Jileo.Optimizely.Caching.TagHelper.ContentAreaCacheTagHelper, Jileo.Optimizely.Caching
```
4. Use in .cshtml files
```
<content-area-cache>
    @Html.PropertyFor(x => x.MenuContentArea)
</content-area-cache>

<content-area-cache dependent-types="@(new[] { typeof(CtaBlock) })">
    @Html.PropertyFor(x => x.MainContentArea)
</content-area-cache>
```
