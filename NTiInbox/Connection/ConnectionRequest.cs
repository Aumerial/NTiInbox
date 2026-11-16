namespace NTiInbox.Connection;

/// <summary>Parameters of a sign-on. The password lives in memory only.</summary>
public sealed record ConnectionRequest(
    string Host,
    string User,
    string Password,
    bool UseSsl = false,
    bool IsProduction = false);
