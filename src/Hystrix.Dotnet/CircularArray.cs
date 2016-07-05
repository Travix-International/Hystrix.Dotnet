using System;
using System.Collections.Concurrent;

namespace Hystrix.Dotnet
{
    internal class CircularArray<T>
    {
        private readonly int maximumSize;

        public int MaximumSize { get { return maximumSize; } }

        public int Length { get { return internalQueue.Count; } }

        private readonly ConcurrentQueue<T> internalQueue = new ConcurrentQueue<T>();

        public CircularArray(int maximumSize)
        {
            if (maximumSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumSize", "Parameter maximumSize needs to be greater than 0");
            }

            this.maximumSize = maximumSize;
        }

        public T GetTail()
        {
            if (internalQueue.Count == 0)
            {
                return default(T);
            }

            var localArray = internalQueue.ToArray();

            return localArray[localArray.Length - 1];
        }

        public T[] GetArray()
        {
            // try to remove old items if length is still beyond maximumSize
            while (internalQueue.Count > maximumSize)
            {
                T result;
                internalQueue.TryDequeue(out result);
            }
            
            return internalQueue.ToArray();
        }

        public void Add(T value)
        {
            internalQueue.Enqueue(value);

            // try to remove old items if length goes beyond maximumSize
            while (internalQueue.Count > maximumSize)
            {
                T result;
                internalQueue.TryDequeue(out result);
            }
        }

        public void Clear()
        {
            /*
             * it should be very hard to not succeed the first pass thru since this is typically is only called from
             * a single thread protected by a tryLock, but there is at least 1 other place (at time of writing this comment)
             * where reset can be called from (CircuitBreaker.markSuccess after circuit was tripped) so it can
             * in an edge-case conflict.
             * 
             * Instead of trying to determine if someone already successfully called clear() and we should skip
             * we will have both calls reset the circuit, even if that means losing data added in between the two
             * depending on thread scheduling.
             * 
             * The rare scenario in which that would occur, we'll accept the possible data loss while clearing it
             * since the code has stated its desire to clear() anyways.
             */

            while (internalQueue.Count > 0)
            {
                T result;
                internalQueue.TryDequeue(out result);
            }
        }
    }
}