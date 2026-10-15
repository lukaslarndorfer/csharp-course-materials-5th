using System.Numerics;
using System.Runtime.CompilerServices;

namespace PiCalc.Core;

/// <summary>
///     Provides random numbers for Monte Carlo simulations
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    ///     Provides a random pair of doubles in the range [0, 1)
    /// </summary>
    /// <returns>A pair of random floating point numbers</returns>
    public (double X, double Y) NextDoublePair();

    /// <summary>
    ///     Provides <see cref="Vector{T}.Count" /> random doubles in the range [0, 1).
    ///     Every lane of the returned vector is fed by its own independent generator.
    /// </summary>
    /// <returns>A vector of random floating point numbers</returns>
    public Vector<double> NextDoubleVector();
}

/// <summary>
///     A xoshiro256+ generator, available in a scalar and a vectorized flavour
/// </summary>
internal sealed class RandomProvider : IRandomProvider
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;
    private Vector<ulong> _v0;
    private Vector<ulong> _v1;
    private Vector<ulong> _v2;
    private Vector<ulong> _v3;

    /// <summary>
    ///     Creates a new instance of the <see cref="RandomProvider" /> class
    /// </summary>
    /// <param name="seed">Seed to use, a random one is picked if none is supplied</param>
    public RandomProvider(ulong? seed = null)
    {
        var state = seed ?? (ulong) Random.Shared.NextInt64();
        _s0 = SplitMix64(ref state);
        _s1 = SplitMix64(ref state);
        _s2 = SplitMix64(ref state);
        _s3 = SplitMix64(ref state);

        var lanes = Vector<ulong>.Count;
        Span<ulong> v0 = stackalloc ulong[lanes];
        Span<ulong> v1 = stackalloc ulong[lanes];
        Span<ulong> v2 = stackalloc ulong[lanes];
        Span<ulong> v3 = stackalloc ulong[lanes];
        for (var lane = 0; lane < lanes; lane++)
        {
            v0[lane] = SplitMix64(ref state);
            v1[lane] = SplitMix64(ref state);
            v2[lane] = SplitMix64(ref state);
            v3[lane] = SplitMix64(ref state);
        }

        _v0 = new Vector<ulong>(v0);
        _v1 = new Vector<ulong>(v1);
        _v2 = new Vector<ulong>(v2);
        _v3 = new Vector<ulong>(v3);
    }

    public (double X, double Y) NextDoublePair() => (NextDouble(), NextDouble());

    public Vector<double> NextDoubleVector()
    {
        var result = _v0 + _v3;
        var t = _v1 << 17;
        _v2 ^= _v0;
        _v3 ^= _v1;
        _v1 ^= _v2;
        _v0 ^= _v3;
        _v2 ^= t;
        _v3 = Vector.ShiftLeft(_v3, 45) | Vector.ShiftRightLogical(_v3, 64 - 45);

        return ToUnitInterval(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double NextDouble()
    {
        var result = _s0 + _s3;
        var t = _s1 << 17;
        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;
        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);

        return BitConverter.UInt64BitsToDouble((result >> 12) | ExponentBits) - 1D;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<double> ToUnitInterval(Vector<ulong> bits)
    {
        var mantissa = Vector.ShiftRightLogical(bits, 12) | new Vector<ulong>(ExponentBits);

        return mantissa.As<ulong, double>() - Vector<double>.One;
    }

    private const ulong ExponentBits = 0x3FF0000000000000UL;

    private static ulong SplitMix64(ref ulong state)
    {
        // SplitMix64 is the canonical way to turn a single seed into a full xoshiro state
        state += 0x9E3779B97F4A7C15UL;
        var z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;

        return z ^ (z >> 31);
    }
}
