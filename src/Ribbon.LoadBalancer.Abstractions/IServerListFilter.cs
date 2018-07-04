namespace Ribbon.LoadBalancer
{
    public interface IServerListFilter<T> where T : Server
    {
        T[] GetFilteredListOfServers(T[] servers);
    }
}