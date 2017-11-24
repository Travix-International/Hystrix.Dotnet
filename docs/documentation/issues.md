# Known issues

Unlike the original Hystrix implementation the current .Net implementation doesn't use a way to limit the maximum number of concurrent requests per command. Using the ExecuteAsync method will make efficient use of the threadpool, so it's not entirely clear whether it will give us any benefits.

Neither are retries implemented at this moment.