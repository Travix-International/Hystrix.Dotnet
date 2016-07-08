using System;
using System.Threading;

namespace Hystrix.Dotnet
{
    public class HystrixRollingPercentile
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixRollingPercentile));

        private readonly CircularArray<RollingPercentileBucket> buckets;
        private readonly DateTimeProvider dateTimeProvider;
        private readonly int timeInMilliseconds;
        private readonly int numberOfBuckets;
        private readonly int bucketDataLength;
        private readonly IHystrixConfigurationService configurationService;
        private readonly int bucketSizeInMilliseconds;

        internal CircularArray<RollingPercentileBucket> Buckets { get { return buckets; } }

        private volatile PercentileSnapshot currentPercentileSnapshot = new PercentileSnapshot(0);

        public HystrixRollingPercentile(int timeInMilliseconds, int numberOfBuckets, int bucketDataLength, IHystrixConfigurationService configurationService)
            :this(new DateTimeProvider(), timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationService)
        {
        }

        [Obsolete("This constructor is only use for testing in order to inject a DateTimeProvider mock")]
        public HystrixRollingPercentile(DateTimeProvider dateTimeProvider, int timeInMilliseconds, int numberOfBuckets, int bucketDataLength, IHystrixConfigurationService configurationService)
        {
            if (timeInMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException("timeInMilliseconds", "Parameter timeInMilliseconds needs to be greater than 0");
            }
            if (numberOfBuckets <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfBuckets", "Parameter numberOfBuckets needs to be greater than 0");
            }
            if (timeInMilliseconds % numberOfBuckets != 0)
            {
                throw new ArgumentOutOfRangeException("timeInMilliseconds", "Parameter timeInMilliseconds needs to be an exact multiple of numberOfBuckets");
            }
            if (bucketDataLength < 100)
            {
                throw new ArgumentOutOfRangeException("bucketDataLength", "Parameter bucketDataLength needs to be greater than or equal to 100");
            }
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }

            this.dateTimeProvider = dateTimeProvider;
            this.timeInMilliseconds = timeInMilliseconds;
            this.numberOfBuckets = numberOfBuckets;
            this.bucketDataLength = bucketDataLength;
            this.configurationService = configurationService;

            bucketSizeInMilliseconds = this.timeInMilliseconds / this.numberOfBuckets;

            buckets = new CircularArray<RollingPercentileBucket>(this.numberOfBuckets);
        }

        /**
         * Add value (or values) to current bucket.
         * 
         * @param value
         *            Value to be stored in current bucket such as execution latency in milliseconds
         */
        public void AddValue(params int[] values)
        {
            /* no-op if disabled */
            if (!configurationService.GetMetricsRollingPercentileEnabled())
            {
                return;
            }

            foreach (var v in values)
            {
                try
                {
                    GetCurrentBucket().Data.AddValue(v);
                }
                catch (Exception)
                {
                    //Log.Error("Failed to add value: " + v, e);
                }
            }
        }

        /**
         * Compute a percentile from the underlying rolling buckets of values.
         * <p>
         * For performance reasons it maintains a single snapshot of the sorted values from all buckets that is re-generated each time the bucket rotates.
         * <p>
         * This means that if a bucket is 5000ms, then this method will re-compute a percentile at most once every 5000ms.
         * 
         * @param percentile
         *            value such as 99 (99th percentile), 99.5 (99.5th percentile), 50 (median, 50th percentile) to compute and retrieve percentile from rolling buckets.
         * @return int percentile value
         */
        public int GetPercentile(double percentile)
        {
            /* no-op if disabled */
            if (!configurationService.GetMetricsRollingPercentileEnabled())
            {
                return -1;
            }

            // force logic to move buckets forward in case other requests aren't making it happen
            GetCurrentBucket();

            // fetch the current snapshot
            return GetCurrentPercentileSnapshot().GetPercentile(percentile);
        }

        /**
         * This returns the mean (average) of all values in the current snapshot. This is not a percentile but often desired so captured and exposed here.
         * 
         * @return mean of all values
         */
        public int GetMean()
        {
            /* no-op if disabled */
            if (!configurationService.GetMetricsRollingPercentileEnabled())
            {
                return -1;
            }

            // force logic to move buckets forward in case other requests aren't making it happen
            GetCurrentBucket();

            // fetch the current snapshot
            return GetCurrentPercentileSnapshot().GetMean();
        }

        /**
         * This will retrieve the current snapshot or create a new one if one does not exist.
         * <p>
         * It will NOT include data from the current bucket, but all previous buckets.
         * <p>
         * It remains cached until the next bucket rotates at which point a new one will be created.
         */
        private PercentileSnapshot GetCurrentPercentileSnapshot()
        {
            return currentPercentileSnapshot;
        }

        private readonly object newBucketLock = new object();

        private RollingPercentileBucket GetCurrentBucket()
        {
            long currentTime = dateTimeProvider.GetCurrentTimeInMilliseconds();

            /* a shortcut to try and get the most common result of immediately finding the current bucket */

            /**
             * Retrieve the latest bucket if the given time is BEFORE the end of the bucket window, otherwise it returns NULL.
             * 
             * NOTE: This is thread-safe because it's accessing 'buckets' which is a LinkedBlockingDeque
             */
            RollingPercentileBucket currentBucket = buckets.GetTail();
            if (currentBucket != null && currentTime < currentBucket.WindowStart + bucketSizeInMilliseconds)
            {
                // if we're within the bucket 'window of time' return the current one
                // NOTE: We do not worry if we are BEFORE the window in a weird case of where thread scheduling causes that to occur,
                // we'll just use the latest as long as we're not AFTER the window
                return currentBucket;
            }

            /* if we didn't find the current bucket above, then we have to create one */

            /**
             * The following needs to be synchronized/locked even with a synchronized/thread-safe data structure such as LinkedBlockingDeque because
             * the logic involves multiple steps to check existence, create an object then insert the object. The 'check' or 'insertion' themselves
             * are thread-safe by themselves but not the aggregate algorithm, thus we put this entire block of logic inside synchronized.
             * 
             * I am using a tryLock if/then (http://download.oracle.com/javase/6/docs/api/java/util/concurrent/locks/Lock.html#tryLock())
             * so that a single thread will get the lock and as soon as one thread gets the lock all others will go the 'else' block
             * and just return the currentBucket until the newBucket is created. This should allow the throughput to be far higher
             * and only slow down 1 thread instead of blocking all of them in each cycle of creating a new bucket based on some testing
             * (and it makes sense that it should as well).
             * 
             * This means the timing won't be exact to the millisecond as to what data ends up in a bucket, but that's acceptable.
             * It's not critical to have exact precision to the millisecond, as long as it's rolling, if we can instead reduce the impact synchronization.
             * 
             * More importantly though it means that the 'if' block within the lock needs to be careful about what it changes that can still
             * be accessed concurrently in the 'else' block since we're not completely synchronizing access.
             * 
             * For example, we can't have a multi-step process to add a bucket, remove a bucket, then update the sum since the 'else' block of code
             * can retrieve the sum while this is all happening. The trade-off is that we don't maintain the rolling sum and let readers just iterate
             * bucket to calculate the sum themselves. This is an example of favoring write-performance instead of read-performance and how the tryLock
             * versus a synchronized block needs to be accommodated.
             */
            if (Monitor.TryEnter(newBucketLock))
            {
                try
                {
                    if (buckets.GetTail() == null)
                    {
                        // the list is empty so create the first bucket
                        RollingPercentileBucket newBucket = new RollingPercentileBucket(currentTime, bucketDataLength);
                        buckets.Add(newBucket);
                        return newBucket;
                    }
                    else
                    {
                        // We go into a loop so that it will create as many buckets as needed to catch up to the current time
                        // as we want the buckets complete even if we don't have transactions during a period of time.
                        for (int i = 0; i < numberOfBuckets; i++)
                        {
                            // we have at least 1 bucket so retrieve it
                            RollingPercentileBucket lastBucket = buckets.GetTail();
                            if (currentTime < lastBucket.WindowStart + bucketSizeInMilliseconds)
                            {
                                // if we're within the bucket 'window of time' return the current one
                                // NOTE: We do not worry if we are BEFORE the window in a weird case of where thread scheduling causes that to occur,
                                // we'll just use the latest as long as we're not AFTER the window
                                return lastBucket;
                            }
                            else if (currentTime - (lastBucket.WindowStart + bucketSizeInMilliseconds) > timeInMilliseconds)
                            {
                                // the time passed is greater than the entire rolling counter so we want to clear it all and start from scratch
                                Reset();
                                // recursively call getCurrentBucket which will create a new bucket and return it
                                return GetCurrentBucket();
                            }
                            else
                            { // we're past the window so we need to create a new bucket
                                RollingPercentileBucket[] allBuckets = buckets.GetArray();
                                // create a new bucket and add it as the new 'last' (once this is done other threads will start using it on subsequent retrievals)
                                buckets.Add(new RollingPercentileBucket(lastBucket.WindowStart + bucketSizeInMilliseconds, bucketDataLength));
                                // we created a new bucket so let's re-generate the PercentileSnapshot (not including the new bucket)
                                currentPercentileSnapshot = new PercentileSnapshot(allBuckets);
                            }
                        }
                        // we have finished the for-loop and created all of the buckets, so return the lastBucket now
                        return buckets.GetTail();
                    }
                }
                finally
                {
                    Monitor.Exit(newBucketLock);
                }
            }
            
            currentBucket = buckets.GetTail();
            if (currentBucket != null)
            {
                // we didn't get the lock so just return the latest bucket while another thread creates the next one
                return currentBucket;
            }
                
            // the rare scenario where multiple threads raced to create the very first bucket
            // wait slightly and then use recursion while the other thread finishes creating a bucket
            Thread.Sleep(5);

            return GetCurrentBucket();
        }

        /**
         * Force a reset so that percentiles start being gathered from scratch.
         */
        public void Reset()
        {
            /* no-op if disabled */
            if (!configurationService.GetMetricsRollingPercentileEnabled())
            {
                return;
            }

            // clear buckets so we start over again
            buckets.Clear();
        }
    }
}
