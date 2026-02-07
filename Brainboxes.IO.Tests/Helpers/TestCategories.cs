namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// Test category constants for organizing and filtering tests.
    /// Use with [TestCategory] attribute.
    /// </summary>
    /// <example>
    /// [TestMethod]
    /// [TestCategory(TestCategories.Unit)]
    /// public void MyTest() { }
    /// </example>
    public static class TestCategories
    {
        /// <summary>
        /// Unit tests that do not require hardware.
        /// Can be run anywhere without device connectivity.
        /// </summary>
        public const string Unit = "Unit";

        /// <summary>
        /// Integration tests that require hardware devices.
        /// Will skip (Assert.Inconclusive) if no devices configured.
        /// </summary>
        public const string Integration = "Integration";

        /// <summary>
        /// Tests requiring specific Brainboxes hardware devices.
        /// Subset of Integration tests.
        /// </summary>
        public const string Hardware = "Hardware";

        /// <summary>
        /// Thread safety and concurrency tests.
        /// Tests race conditions and concurrent access patterns.
        /// </summary>
        public const string ThreadSafety = "ThreadSafety";

        /// <summary>
        /// Performance and latency tests.
        /// Measures throughput and timing characteristics.
        /// </summary>
        public const string Performance = "Performance";

        /// <summary>
        /// Protocol-level tests (ASCII, Modbus).
        /// Tests command parsing and response handling.
        /// </summary>
        public const string Protocol = "Protocol";

        /// <summary>
        /// Connection management tests.
        /// Tests connect/disconnect, timeouts, availability.
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// IO line and device state tests.
        /// Tests reading/writing digital and analog lines.
        /// </summary>
        public const string IOLine = "IOLine";

        /// <summary>
        /// Event handling tests.
        /// Tests event subscription, polling, and callbacks.
        /// </summary>
        public const string Events = "Events";

        /// <summary>
        /// Error handling tests.
        /// Tests exception cases, invalid inputs, error responses.
        /// </summary>
        public const string ErrorHandling = "ErrorHandling";
    }
}
