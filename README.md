# Jileo.Optimizely.Caching
A cache tag helper for ContentArea content

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
  @Html.PropertyFor(x => x.ContentArea)
</content-area-cache>
```
