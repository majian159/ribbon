namespace Ribbon.Client
{
    public class RetryHandlerConfig
    {
        public uint MaxAutoRetries { get; set; } = 0;
        public uint MaxAutoRetriesNextServer { get; set; } = 1;
        public bool OkToRetryOnAllOperations { get; set; } = false;
    }
}