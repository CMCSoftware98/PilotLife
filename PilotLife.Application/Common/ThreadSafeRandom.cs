namespace PilotLife.Application.Common;

/// <summary>
/// Provides thread-safe random number generation using ThreadLocal storage.
/// Each thread gets its own Random instance with a unique seed.
/// </summary>
public static class ThreadSafeRandom
{
    private static int _seedCounter = Environment.TickCount;

    private static readonly ThreadLocal<Random> LocalRandom = new(() =>
        new Random(Interlocked.Increment(ref _seedCounter)));

    /// <summary>
    /// Gets the thread-local Random instance.
    /// </summary>
    public static Random Instance => LocalRandom.Value!;

    /// <summary>
    /// Returns a non-negative random integer less than the specified maximum.
    /// </summary>
    public static int Next(int maxValue) => Instance.Next(maxValue);

    /// <summary>
    /// Returns a random integer within the specified range.
    /// </summary>
    public static int Next(int minValue, int maxValue) => Instance.Next(minValue, maxValue);

    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0.
    /// </summary>
    public static double NextDouble() => Instance.NextDouble();
}
