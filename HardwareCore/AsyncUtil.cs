using System;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareCore
{
    /// <summary>
    /// Helper class to run async methods within a sync process.
    /// </summary>
    public static class AsyncUtil
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
        /// USAGE: AsyncUtil.RunSync(() => AsyncMethod());
        /// </summary>
        /// <param name="task">Task method to execute</param>
        public static void RunSync(Func<Task> task)
            => _taskFactory
                .StartNew(task)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}