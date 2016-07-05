using System;
using System.Collections.Generic;

namespace Hystrix.Dotnet
{
    public class HystrixCommandException:AggregateException
    {
        public HystrixCommandException()
        {
        }

        public HystrixCommandException(IEnumerable<Exception> innerExceptions)
            :base(innerExceptions)
        {
        }
    }
}