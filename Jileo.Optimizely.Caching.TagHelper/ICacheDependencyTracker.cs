using EPiServer.Core;

namespace Jileo.Optimizely.Caching.TagHelper;

public interface ICacheDependencyTracker
{
    void Invalidate(IContent content);
}

public class CacheDependencyTracker : ICacheDependencyTracker
{
    public void Invalidate(IContent content)
    {
        foreach (var versionRef in VersionRef.GetAll())
        {
            if (HasDependentTypeOrContent(versionRef, content))
            {
                versionRef.Increment();
            }
        }
    }

    private static bool HasDependentTypeOrContent(VersionRef versionRef, IContent content)
    {
        return versionRef.GetDependentTypes().Any(type => type.IsInstanceOfType(content)) ||
               versionRef.GetDependentContent().Contains(content.ContentLink, ContentReferenceComparer.IgnoreVersion);
    }
}
