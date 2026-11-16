using System.Globalization;
using Aumerial.Data.Nti;

namespace NTiInbox.SystemApis;

/// <summary>Retrieves the information of a user profile (QSYRUSRI).</summary>
public static class UserInfoApi
{
    /// <summary>
    /// Position of the eight special authorities in the CHAR(15) field of the USRI0300
    /// format. Each position is Y or N.
    /// </summary>
    private static readonly string[] SpecialAuthorityNames =
    {
        "*ALLOBJ", "*SECADM", "*JOBCTL", "*SPLCTL", "*SAVSYS", "*SERVICE", "*AUDIT", "*IOSYSCFG"
    };

    /// <summary>
    /// Retrieves user profile information through the QSYRUSRI API (USRI0300 format).
    /// This API is not part of messaging: it describes the connected profile and finds
    /// its message queue.
    /// </summary>
    /// <param name="userProfile">Profile name, or *CURRENT for the job profile.</param>
    /// <returns>The profile information.</returns>
    public static UserInformation RetrieveUserInformation(this NTiConnection conn, string userProfile)
    {
        conn.EnsureOpen();

        int bufferSize = 1024;

        var parameters = new List<NTiProgramParameter>
        {
            new NTiProgramParameter(new byte[bufferSize]).AsOutput(),
            new NTiProgramParameter(bufferSize),
            new NTiProgramParameter("USRI0300", 8),
            new NTiProgramParameter(userProfile, 10),
            ApiErrorCode.Create()
        };

        conn.CallProgram("QSYS", "QSYRUSRI", parameters);

        ApiErrorCode.ThrowIfError(parameters[4], "QSYRUSRI");

        var buffer = parameters[0];

        var information = new UserInformation
        {
            UserProfileName = buffer.GetString(8, 10).Trim(),
            PreviousSignOn = ParseSignOn(buffer.GetString(18, 13)),
            InvalidSignOnAttempts = buffer.GetInt(32),
            Status = buffer.GetString(36, 10).Trim(),
            UserClass = buffer.GetString(73, 10).Trim(),
            SpecialAuthorities = ReadSpecialAuthorities(buffer.GetString(83, 15)),
            GroupProfile = buffer.GetString(98, 10).Trim(),
            CurrentLibrary = buffer.GetString(138, 10).Trim(),
            LimitCapabilities = buffer.GetString(188, 10).Trim(),
            TextDescription = buffer.GetString(198, 50).Trim(),
            StorageUsed = buffer.GetInt(284),
            JobDescriptionName = buffer.GetString(289, 10).Trim(),
            JobDescriptionLibrary = buffer.GetString(299, 10).Trim(),
            MessageQueueName = buffer.GetString(324, 10).Trim(),
            MessageQueueLibrary = buffer.GetString(334, 10).Trim(),
            MessageQueueDeliveryMethod = buffer.GetString(344, 10).Trim(),
            MessageQueueSeverity = buffer.GetInt(356),
            OutputQueueName = buffer.GetString(360, 10).Trim(),
            OutputQueueLibrary = buffer.GetString(370, 10).Trim()
        };

        return information;
    }

    /// <summary>
    /// Reads the special authorities field: one position per authority, Y or N.
    /// </summary>
    private static List<string> ReadSpecialAuthorities(string field)
    {
        var authorities = new List<string>();

        for (int i = 0; i < SpecialAuthorityNames.Length && i < field.Length; i++)
        {
            if (field[i] is 'Y' or 'y')
            {
                authorities.Add(SpecialAuthorityNames[i]);
            }
        }

        return authorities;
    }

    /// <summary>
    /// Converts the last sign-on date, in CYYMMDDHHMMSS form.
    /// The field is blank when the profile never signed on.
    /// </summary>
    private static DateTime? ParseSignOn(string value)
    {
        var stamp = (value ?? string.Empty).Trim();

        if (stamp.Length < 13)
        {
            return null;
        }

        var century = stamp[0] == '1' ? 2000 : 1900;

        if (!int.TryParse(stamp.Substring(1, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            || !int.TryParse(stamp.Substring(3, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || !int.TryParse(stamp.Substring(5, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var day)
            || !int.TryParse(stamp.Substring(7, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var hour)
            || !int.TryParse(stamp.Substring(9, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var minute)
            || !int.TryParse(stamp.Substring(11, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var second))
        {
            return null;
        }

        if (month is < 1 or > 12 || day is < 1 or > 31)
        {
            return null;
        }

        try
        {
            return new DateTime(century + year, month, day, hour, minute, second);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
