namespace Ribbon.Client.Config
{
    public static class CommonClientConfigKey
    {
        public const string MaxAutoRetries = "MaxAutoRetries";
        public const string MaxAutoRetriesNextServer = "MaxAutoRetriesNextServer";
        public const string OkToRetryOnAllOperations = "OkToRetryOnAllOperations";
        public const string LoadBalancerPingInterval = "LoadBalancerPingInterval";
        public const string LoadBalancerMaxTotalPingTime = "LoadBalancerMaxTotalPingTime";
        public const string ListOfServers = "listOfServers";

        public const string LoadBalancerRuleClassName = "LoadBalancerRuleClassName";
        public const string LoadBalancerPingClassName = "LoadBalancerPingClassName";
    }
}