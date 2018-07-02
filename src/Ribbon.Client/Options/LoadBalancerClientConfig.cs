namespace Ribbon.Client.Options
{
    public class LoadBalancerClientConfig
    {
        public LoadBalancerClientConfig()
        {
            ClientTypeName = "Ribbon.Client.Http.RibbonHttpClient,Ribbon.Client.Http";
        }

        public string ClientTypeName { get; set; }
    }
}