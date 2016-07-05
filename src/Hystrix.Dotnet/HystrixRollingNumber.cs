using System;
using System.Threading;

namespace Hystrix.Dotnet
{
    public class HystrixRollingNumber
    {
        private readonly DateTimeProvider dateTimeProvider;
        private readonly int timeInMilliseconds;
        private readonly int numberOfBuckets;
        private readonly int bucketSizeInMillseconds;

        private readonly CircularArray<RollingNumberBucket> buckets;
        private readonly CumulativeSum cumulativeSum = new CumulativeSum();

        public int TimeInMilliseconds
        {
            get { return timeInMilliseconds; }
        }

        public int NumberOfBuckets
        {
            get { return numberOfBuckets; }
        }

        public int BucketSizeInMillseconds
        {
            get { return bucketSizeInMillseconds; }
        }

        public HystrixRollingNumber(int timeInMilliseconds, int numberOfBuckets)
            :this(new DateTimeProvider(), timeInMilliseconds, numberOfBuckets)
        {
        }

        [Obsolete("This constructor is only use for testing in order to inject a DateTimeProvider mock")]
        public HystrixRollingNumber(DateTimeProvider dateTimeProvider, int timeInMilliseconds, int numberOfBuckets)
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

            this.dateTimeProvider = dateTimeProvider;
            this.timeInMilliseconds = timeInMilliseconds;
            this.numberOfBuckets = numberOfBuckets;

            bucketSizeInMillseconds = timeInMilliseconds / numberOfBuckets;

            buckets = new CircularArray<RollingNumberBucket>(numberOfBuckets);
        }

        public void Increment(HystrixRollingNumberEvent type)
        {
            GetCurrentBucket().GetAdder(type).Increment();
        }

        public void Add(HystrixRollingNumberEvent type, long value)
        {
            GetCurrentBucket().GetAdder(type).Add(value);
        }

        public void UpdateRollingMax(HystrixRollingNumberEvent type, long value)
        {
            GetCurrentBucket().GetMaxUpdater(type).Update(value);
        }

        public void Reset()
        {
            // if we are resetting, that means the lastBucket won't have a chance to be captured in CumulativeSum, so let's do it here
            RollingNumberBucket lastBucket = buckets.GetTail();
            if (lastBucket != null)
            {
                cumulativeSum.AddBucket(lastBucket);
            }

            // clear buckets so we start over again
            buckets.Clear();
        }

        public long GetCumulativeSum(HystrixRollingNumberEvent type)
        {
            return GetValueOfLatestBucket(type) + cumulativeSum.Get(type);
        }

        public long GetRollingSum(HystrixRollingNumberEvent type)
        {
            RollingNumberBucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
            {
                return 0;
            }

            long sum = 0;
            foreach (var bucket in buckets.GetArray())
            {
                sum += bucket.GetAdder(type).GetValue();
            }
            return sum;
        }

        public long GetValueOfLatestBucket(HystrixRollingNumberEvent type)
        {
            RollingNumberBucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
            {
                return 0;
            }

            // we have bucket data so we'll return the lastBucket
            return lastBucket.Get(type);
        }

        public long[] GetValues(HystrixRollingNumberEvent type)
        {
            RollingNumberBucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
            {
                return new long[0];
            }
            
            // get buckets as an array (which is a copy of the current state at this point in time)
            RollingNumberBucket[] bucketArray = buckets.GetArray();

            // we have bucket data so we'll return an array of values for all buckets
            long[] values = new long[bucketArray.Length];
            int i = 0;
            foreach (var bucket in buckets.GetArray())
            {
                if (type.IsCounter()) 
                {
                    values[i++] = bucket.GetAdder(type).GetValue();
                } 
                else if (type.IsMaxUpdater()) 
                {
                    values[i++] = bucket.GetMaxUpdater(type).Max();
                }
            }
            return values;
        }

        public long GetRollingMaxValue(HystrixRollingNumberEvent type)
        {
            long[] values = GetValues(type);
            if (values.Length == 0) {
                return 0;
            }

            Array.Sort(values);
            return values[values.Length - 1];
        }

        private readonly object newBucketLock = new object();

        private RollingNumberBucket GetCurrentBucket()
        {
            long currentTime = dateTimeProvider.GetCurrentTimeInMilliseconds();

            /* a shortcut to try and get the most common result of immediately finding the current bucket */

            /**
             * Retrieve the latest bucket if the given time is BEFORE the end of the bucket window, otherwise it returns NULL.
             * 
             * NOTE: This is thread-safe because it's accessing 'buckets' which is a LinkedBlockingDeque
             */
            RollingNumberBucket currentBucket = buckets.GetTail();
            if (currentBucket != null && currentTime < currentBucket.WindowStart + bucketSizeInMillseconds)
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
                        RollingNumberBucket newBucket = new RollingNumberBucket(currentTime);
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
                            RollingNumberBucket lastBucket = buckets.GetTail();
                            if (currentTime < lastBucket.WindowStart + bucketSizeInMillseconds)
                            {
                                // if we're within the bucket 'window of time' return the current one
                                // NOTE: We do not worry if we are BEFORE the window in a weird case of where thread scheduling causes that to occur,
                                // we'll just use the latest as long as we're not AFTER the window
                                return lastBucket;
                            }
                            else if (currentTime - (lastBucket.WindowStart + bucketSizeInMillseconds) > timeInMilliseconds)
                            {
                                // the time passed is greater than the entire rolling counter so we want to clear it all and start from scratch
                                Reset();
                                // recursively call getCurrentBucket which will create a new bucket and return it
                                return GetCurrentBucket();
                            }
                            else
                            { // we're past the window so we need to create a new bucket
                                // create a new bucket and add it as the new 'last'
                                buckets.Add(new RollingNumberBucket(lastBucket.WindowStart + bucketSizeInMillseconds));
                                // add the lastBucket values to the cumulativeSum
                                cumulativeSum.AddBucket(lastBucket);
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
    }
}
