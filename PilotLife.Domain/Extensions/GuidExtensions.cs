namespace PilotLife.Domain.Extensions;

/// <summary>
/// Extension methods for working with UUID v7 GUIDs.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Extracts the timestamp from a UUID v7 identifier.
    /// Returns the DateTimeOffset when the UUID was created.
    /// </summary>
    /// <param name="uuid">The UUID v7 to extract the timestamp from.</param>
    /// <returns>The DateTimeOffset representing when the UUID was created.</returns>
    public static DateTimeOffset GetTimestamp(this Guid uuid)
    {
        // UUID v7 stores the 48-bit Unix timestamp in milliseconds in the first 48 bits
        // .NET's Guid stores bytes in a mixed endian format:
        // - bytes[0-3]: little-endian (reversed from canonical)
        // - bytes[4-5]: little-endian (reversed from canonical)
        // - bytes[6-7]: little-endian (reversed from canonical)
        // - bytes[8-15]: big-endian (same as canonical)
        //
        // UUID v7 canonical format: | unix_ts_ms (48 bits) | ver (4) | rand_a (12) | ...
        // So we need to reverse the byte order for the first parts

        Span<byte> bytes = stackalloc byte[16];
        uuid.TryWriteBytes(bytes);

        // In .NET GUID format, the timestamp bytes are:
        // bytes[0-3] are little-endian (so reversed)
        // bytes[4-5] are little-endian (so reversed)
        // We need bytes 0-5 for the 48-bit timestamp, but properly ordered

        // Reconstruct the 48-bit timestamp considering .NET's byte layout
        // First 4 bytes are stored little-endian
        long timestampMs = ((long)bytes[3] << 40) |  // highest byte of first group
                          ((long)bytes[2] << 32) |
                          ((long)bytes[1] << 24) |
                          ((long)bytes[0] << 16) |   // lowest byte of first group
                          ((long)bytes[5] << 8) |    // highest byte of second group
                          bytes[4];                   // lowest byte of second group

        return DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
    }

    /// <summary>
    /// Validates that a GUID is UUID version 7.
    /// </summary>
    /// <param name="uuid">The GUID to validate.</param>
    /// <returns>True if the GUID is UUID version 7, false otherwise.</returns>
    public static bool IsVersion7(this Guid uuid)
    {
        // Version is stored in bits 48-51 (high nibble of byte 6 in canonical format)
        // In .NET's format, this is bytes[6-7] which are stored little-endian
        // So the version is in the high nibble of byte 7
        Span<byte> bytes = stackalloc byte[16];
        uuid.TryWriteBytes(bytes);
        return (bytes[7] >> 4) == 7;
    }
}
