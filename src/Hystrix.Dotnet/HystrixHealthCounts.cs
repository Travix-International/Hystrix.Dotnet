namespace Hystrix.Dotnet
{
    public class HystrixHealthCounts
    {
        private readonly long totalCount;
        private readonly long errorCount;
        private readonly int errorPercentage;

        public HystrixHealthCounts(long totalCount, long errorCount, int errorPercentage) {
            this.totalCount = totalCount;
            this.errorCount = errorCount;
            this.errorPercentage = errorPercentage;
        }

        public long GetTotalRequests() {
            return totalCount;
        }

        public long GetErrorCount() {
            return errorCount;
        }

        public int GetErrorPercentage() {
            return errorPercentage;
        }
    }
}
