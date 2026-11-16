using NTiInbox.Storage;

namespace NTiInbox.Connection;

/// <summary>Saved connection profiles of the current user, most recently used first.</summary>
public sealed class ConnectionProfileStore(JsonUserStore store, ICurrentUser user)
{
    /// <summary>Shape of the user file. Only profiles are kept, never a password.</summary>
    private sealed class ProfileFile
    {
        public List<ConnectionProfile> Profiles { get; set; } = [];
    }

    /// <summary>
    /// All saved profiles. Duplicates found in the file are merged and the cleaned
    /// file is written back.
    /// </summary>
    public IReadOnlyList<ConnectionProfile> List()
    {
        var stored = store.Load<ProfileFile>(user.Key).Profiles;
        var clean = Merge(stored);

        if (clean.Count != stored.Count)
        {
            store.Update<ProfileFile>(user.Key, data => data.Profiles = Merge(data.Profiles));
        }

        return clean;
    }

    /// <summary>
    /// Adds a profile, or replaces the one with the same host and user while keeping
    /// its identifier.
    /// </summary>
    public void Save(ConnectionProfile profile)
        => store.Update<ProfileFile>(user.Key, data =>
        {
            if (data.Profiles.FirstOrDefault(p => p.Matches(profile.Host, profile.User)) is { } previous)
            {
                profile.Id = previous.Id;
            }

            data.Profiles.RemoveAll(p => p.Matches(profile.Host, profile.User));
            profile.LastUsed = DateTime.UtcNow;
            data.Profiles.Add(profile);
        });

    /// <summary>Forgets a profile.</summary>
    public void Remove(ConnectionProfile profile)
        => store.Update<ProfileFile>(user.Key, data => data.Profiles.RemoveAll(p => p.Matches(profile.Host, profile.User)));

    // Newest first; without a date, the profile saved last (end of the file) counts as the newest.
    private static List<ConnectionProfile> Merge(IEnumerable<ConnectionProfile> profiles)
        => [.. profiles
            .Select((profile, index) => (Profile: profile, Index: index))
            .OrderByDescending(x => x.Profile.LastUsed)
            .ThenByDescending(x => x.Index)
            .DistinctBy(x => $"{x.Profile.Host.Trim()}|{x.Profile.User.Trim()}", StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Profile)];
}
