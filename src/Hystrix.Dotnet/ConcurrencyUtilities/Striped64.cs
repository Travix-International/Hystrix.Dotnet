using System;
using System.Threading;

namespace Hystrix.Dotnet.ConcurrencyUtilities
{
    public abstract class Striped64
    {
        private static readonly int ProcessorCount = Environment.ProcessorCount;

        protected sealed class Cell
        {
            public static int SizeInBytes = PaddedAtomicLong.SizeInBytes + 16;

            public PaddedAtomicLong Value;

            public Cell(long x)
            {
                this.Value = new PaddedAtomicLong(x);
            }
        }

        protected volatile Cell[] Cells;
        protected AtomicLong Base = new AtomicLong(0);

        private int cellsBusy; // no need for volatile as we only update with Interlocked.CompareExchange

        private bool CasCellsBusy()
        {
            return Interlocked.CompareExchange(ref this.cellsBusy, 1, 0) == 0;
        }

        protected void LongAccumulate(long x, bool wasUncontended)
        {
            var h = GetProbe();

            var collide = false;                // True if last slot nonempty
            for (; ; )
            {
                Cell[] @as; Cell a; int n; long v;
                if ((@as = this.Cells) != null && (n = @as.Length) > 0)
                {
                    if ((a = @as[(n - 1) & h]) == null)
                    {
                        if (this.cellsBusy == 0)
                        {       // Try to attach new Cell
                            var r = new Cell(x);   // Optimistically create
                            if (this.cellsBusy == 0 && CasCellsBusy())
                            {
                                var created = false;
                                try
                                {               // Recheck under lock
                                    Cell[] rs; int m, j;
                                    if ((rs = this.Cells) != null &&
                                        (m = rs.Length) > 0 &&
                                        rs[j = (m - 1) & h] == null)
                                    {
                                        rs[j] = r;
                                        created = true;
                                    }
                                }
                                finally
                                {
                                    this.cellsBusy = 0;
                                }
                                if (created)
                                    break;
                                continue;           // Slot is now non-empty
                            }
                        }
                        collide = false;
                    }
                    else if (!wasUncontended)       // CAS already known to fail
                        wasUncontended = true;      // Continue after rehash
                    else if (a.Value.CompareAndSwap(v = a.Value.GetValue(), v + x))
                        break;
                    else if (n >= ProcessorCount || this.Cells != @as)
                        collide = false;            // At max size or stale
                    else if (!collide)
                        collide = true;
                    else if (this.cellsBusy == 0 && CasCellsBusy())
                    {
                        try
                        {
                            if (this.Cells == @as)
                            {      // Expand table unless stale
                                var rs = new Cell[n << 1];
                                for (var i = 0; i < n; ++i)
                                    rs[i] = @as[i];
                                this.Cells = rs;
                            }
                        }
                        finally
                        {
                            this.cellsBusy = 0;
                        }
                        collide = false;
                        continue;                   // Retry with expanded table
                    }
                    h = AdvanceProbe(h);
                }
                else if (this.cellsBusy == 0 && this.Cells == @as && CasCellsBusy())
                {
                    var init = false;
                    try
                    {                           // Initialize table
                        if (this.Cells == @as)
                        {
                            var rs = new Cell[2];
                            rs[h & 1] = new Cell(x);
                            this.Cells = rs;
                            init = true;
                        }
                    }
                    finally
                    {
                        this.cellsBusy = 0;
                    }
                    if (init)
                        break;
                }
                else if (this.Base.CompareAndSwap(v = this.Base.GetValue(), v + x))
                    break;                          // Fall back on using volatileBase
            }
        }

        protected static int GetProbe()
        {
            return HashCode.Value.Code;
        }

        private static int AdvanceProbe(int probe)
        {
            probe ^= probe << 13;   // xorshift
            probe ^= (int)((uint)probe >> 17);
            probe ^= probe << 5;
            HashCode.Value.Code = probe;
            return probe;
        }

        private static readonly ThreadLocal<ThreadHashCode> HashCode = new ThreadLocal<ThreadHashCode>(() => new ThreadHashCode());

        private class ThreadHashCode
        {
            public int Code = ThreadLocalRandom.Next(1, int.MaxValue);
        }

        /// <summary>
        /// Returns the size in bytes occupied by an Striped64 instance.
        /// </summary>
        /// <param name="instance">instance for whch to calculate the size.</param>
        /// <returns>The size of the instance in bytes.</returns>
        public static int GetEstimatedFootprintInBytes(Striped64 instance)
        {
            var cells = instance.Cells;
            var cellsLength = cells != null ? cells.Length : 0;
            var nonNullCells = 0;
            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    if (cell != null)
                    {
                        nonNullCells++;
                    }
                }
            }

            return AtomicLong.SizeInBytes + // base
                   sizeof(int) + // cellsBusy
                   IntPtr.Size + // cells reference
                   cellsLength * IntPtr.Size + // size of array of references to cells
                   nonNullCells * Cell.SizeInBytes; // size of non null cells
        }
    }
}