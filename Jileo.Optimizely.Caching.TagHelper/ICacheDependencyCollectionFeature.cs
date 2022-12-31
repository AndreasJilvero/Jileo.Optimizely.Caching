using EPiServer.Core;

namespace Jileo.Optimizely.Caching.TagHelper;

public interface ICacheDependencyCollectionFeature
{
    HashSet<ContentReference> Content { get; }
    HashSet<string> VisitorGroupIds { get; }
    void AddContentAreaItem(ContentAreaItem contentAreaItem);
}

public class CacheDependencyCollectionFeature : ICacheDependencyCollectionFeature
{
    public CacheDependencyCollectionFeature()
    {
        Content = new HashSet<ContentReference>();
        VisitorGroupIds = new HashSet<string>();
    }

    public HashSet<ContentReference> Content { get; }
    public HashSet<string> VisitorGroupIds { get; }

    public void AddContentAreaItem(ContentAreaItem contentAreaItem)
    {
        Content.Add(contentAreaItem.ContentLink);

        // AllowedRoles is null if no personalization
        foreach (var role in contentAreaItem.AllowedRoles ?? new List<string>())
        {
            VisitorGroupIds.Add(role);
        }
    }
}