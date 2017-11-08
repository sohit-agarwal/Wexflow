using System;
using System.Threading;
using System.Diagnostics;

namespace Wexflow.Core
{
    /// <summary>
    /// Wexflow timer.
    /// </summary>
    public class WexflowTimer
    {
        /// <summary>
        /// Timer callback.
        /// </summary>
        public TimerCallback TimerCallback { get; set; }
        /// <summary>
        /// State.
        /// </summary>
        public object State { get; set; }
        /// <summary>
        /// Period.
        /// </summary>
        public TimeSpan Period { get; set; }

		private bool _doWork;

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="timerCallback">Timer callback.</param>
        /// <param name="state">State.</param>
        /// <param name="period">Period.</param>
        public WexflowTimer(TimerCallback timerCallback, object state, TimeSpan period)
        {
            TimerCallback = timerCallback;
            State = state;
            Period = period;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
			_doWork = true;

            var thread = new Thread(() =>
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    while(_doWork)
                    {
                        if (stopwatch.ElapsedMilliseconds >= Period.TotalMilliseconds)
                        {
                            stopwatch.Reset();
                            stopwatch.Start();
                            TimerCallback.Invoke(State);
                        }
                        Thread.Sleep(100);
                    }
                });

            thread.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
		public void Stop()
		{
			_doWork = false;
		}
    }
}
