using System;
using System.Diagnostics.Contracts;

namespace Hystrix.Dotnet
{
    internal class PercentileSnapshot
    {
        private readonly int[] data;
        private readonly int length;
        private readonly int mean;

        public PercentileSnapshot(RollingPercentileBucket[] buckets) 
        {
            int lengthFromBuckets = 0;
            
            // we need to calculate it dynamically as it could have been changed by properties (rare, but possible)
            // also this way we capture the actual index size rather than the max so size the int[] to only what we need
            foreach (var bd in buckets)
            {                
                lengthFromBuckets += bd.Data.Length();
            }

            data = new int[lengthFromBuckets];
            int index = 0;
            int sum = 0;

            foreach (var bd in buckets)
            {
                PercentileBucketData pbd = bd.Data;
                int localLength = pbd.Length();
                for (int i = 0; i < localLength; i++)
                {
                    int v = pbd.List[i];
                    data[index++] = v;
                    sum += v;
                }
            }

            length = index;

            mean = length == 0 ? 0 : sum/length;

            Array.Sort(data, 0, length);
        }

        public PercentileSnapshot(params int[] data) 
        {
            this.data = data;
            length = data.Length;

            int sum = 0;

            foreach (var v in data)
            {                
                sum += v;
            }

            mean = sum / length;

            Array.Sort(this.data, 0, length);
        }

        public int GetMean() 
        {
            return mean;
        }

        /**
         * Provides percentile computation.
         */
        public int GetPercentile(double percentile) 
        {
            if (length == 0) 
            {
                return 0;
            }

            return ComputePercentile(percentile);
        }

        /**
         * @see <a href="http://en.wikipedia.org/wiki/Percentile">Percentile (Wikipedia)</a>
         * @see <a href="http://cnx.org/content/m10805/latest/">Percentile</a>
         * 
         * @param percent percentile of data desired
         * @return data at the asked-for percentile.  Interpolation is used if exactness is not possible
         */
        private int ComputePercentile(double percent) 
        {
            // Some just-in-case edge cases
            if (length <= 0) 
            {
                return 0;
            }
            if (percent <= 0.0) 
            {
                return data[0];
            }
            if (percent >= 100.0) 
            {
                return data[length - 1];
            }

            // ranking (http://en.wikipedia.org/wiki/Percentile#Alternative_methods)
            double rank = (percent / 100.0) * length;

            // linear interpolation between closest ranks
            int iLow = (int) Math.Floor(rank);
            int iHigh = (int) Math.Ceiling(rank);

            Contract.Assert(0 <= iLow && iLow <= rank && rank <= iHigh && iHigh <= length);
            Contract.Assert((iHigh - iLow) <= 1);

            if (iHigh >= length) 
            {
                // Another edge case
                return data[length - 1];
            }
            if (iLow == iHigh) 
            {
                return data[iLow];
            }

            // Interpolate between the two bounding values
            return (int) (data[iLow] + (rank - iLow) * (data[iHigh] - data[iLow]));
        }
    }
}
