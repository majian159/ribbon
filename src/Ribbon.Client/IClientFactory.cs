namespace Ribbon.Client
{
    public interface IClientFactory
    {
        IClient CreateClient(string name);
    }
}