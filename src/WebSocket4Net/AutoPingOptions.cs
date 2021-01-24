namespace WebSocket4Net
{
    public class AutoPingOptions
    {
        /// <summary>
        /// The interval the client send ping to server
        /// </summary>
        /// <value>in seconds</value>
        public int AutoPingInterval { get; set; }

        /// <summary>
        /// How long we expect receive pong after ping is sent
        /// </summary>
        /// <value>in seconds</value>
        public int ExpectedPongDelay { get; set; }

        public AutoPingOptions(int interval, int expectPongDelay)
        {
            AutoPingInterval = interval;
            ExpectedPongDelay = expectPongDelay;
        }
    }
}