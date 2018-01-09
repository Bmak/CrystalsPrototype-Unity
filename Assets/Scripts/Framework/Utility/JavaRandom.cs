using System;

/// <summary>
/// This is a partial copy of Java's Random class
/// so we can insure that the same random numbers
/// are produced on both the client and the server
/// </summary>
public class JavaRandom
{
    private long _seed = 0;

    public JavaRandom(long seed)
    {
        SetSeed(seed);
    }

    public void SetSeed(long seed)
    {
        _seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
    }

    public int nextInt(int n)
    {
        if (n <= 0)
        {
            throw new Exception("n must be positive");
        }

        if ((n & -n) == n) // i.e., n is a power of 2
        {
            return (int) ((n*(long) next(31)) >> 31);
        }

        int bits, val;
        do
        {
            bits = next(31);
            val = bits % n;
        } while (bits - val + (n - 1) < 0);

        return val;
    }

    public int next(int bits)
    {
        _seed = (_seed*0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
        return (int) (_seed >> (48 - bits));
    }
}
