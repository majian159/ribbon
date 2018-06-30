namespace Ribbon.LoadBalancer
{
    public interface IRule
    {
        ILoadBalancer LoadBalancer { get; set; }

        Server Choose(object key);
    }
}