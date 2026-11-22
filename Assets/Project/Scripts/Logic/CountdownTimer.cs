namespace Labyrinthino.Logic
{
    /// <summary>
    /// Simple countdown that is advanced manually so it can be unit tested.
    /// </summary>
    public sealed class CountdownTimer
    {
        /// <summary>Seconds left, never below zero.</summary>
        public float Remaining { get; private set; }

        /// <summary>True while the timer is counting down.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Sets the remaining time and starts the countdown.
        /// </summary>
        /// <param name="seconds">New remaining time in seconds.</param>
        public void Start(float seconds)
        {
            Remaining = seconds;
            IsRunning = true;
        }

        /// <summary>
        /// Stops the countdown without changing the remaining time.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Advances the timer.
        /// </summary>
        /// <param name="deltaSeconds">Elapsed seconds since the previous call.</param>
        /// <returns>True exactly once, on the tick when the time reaches zero.</returns>
        public bool Tick(float deltaSeconds)
        {
            if (!IsRunning)
            {
                return false;
            }

            Remaining -= deltaSeconds;
            if (Remaining > 0f)
            {
                return false;
            }

            Remaining = 0f;
            IsRunning = false;
            return true;
        }
    }
}
