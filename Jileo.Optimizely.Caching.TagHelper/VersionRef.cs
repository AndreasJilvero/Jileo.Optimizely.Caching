using System.Collections.Concurrent;
using EPiServer.Core;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Jileo.Optimizely.Caching.TagHelper;

public class VersionRef
{
    public static readonly ConcurrentDictionary<string, VersionRef> Versions = new ConcurrentDictionary<string, VersionRef>();

    private readonly HashSet<Type> _dependentTypes;

    private int _value;
    private HashSet<ContentReference> _dependentContent;
    private HashSet<string> _visitorGroups;

    public VersionRef(string id)
    {
        _dependentTypes = new HashSet<Type>();
        _dependentContent = new HashSet<ContentReference>();
        _visitorGroups = new HashSet<string>();

        Id = id;
    }

    public VersionRef(string id, params Type[] dependentTypes) : this(id)
    {
        _dependentTypes = new HashSet<Type>(dependentTypes ?? Type.EmptyTypes);
    }

    public string Id { get; }

    public int GetValue() => _value;

    // This will trigger a new cached version.
    public void Increment() => Interlocked.Increment(ref _value);

    // If any of these types are published, the corresponding cache should be updated.
    public IEnumerable<Type> GetDependentTypes()
    {
        return _dependentTypes;
    }

    // If any of these content instances are published, the corresponding cache should be updated.
    public IEnumerable<ContentReference> GetDependentContent()
    {
        return _dependentContent;
    }

    public IEnumerable<string> GetVisitorGroups()
    {
        return _visitorGroups;
    }

    public void UpdateDependentContent(IEnumerable<ContentReference> contentLinks)
    {
        Interlocked.Exchange(ref _dependentContent, new HashSet<ContentReference>(contentLinks));
    }

    public void UpdateVisitorGroups(IEnumerable<string> visitorGroups)
    {
        Interlocked.Exchange(ref _visitorGroups, new HashSet<string>(visitorGroups));
    }

    public static IEnumerable<VersionRef> GetAll()
    {
        return Versions.Values;
    }

    public static string CreateKey(TagHelperContext context, string urlCacheKey)
    {
        return $"{context.UniqueId}-{urlCacheKey}";
    }
}