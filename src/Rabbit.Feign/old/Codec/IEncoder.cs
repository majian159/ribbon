using System.Net.Http;
using Ribbon.Client.Http;
using System.Threading.Tasks;

namespace Rabbit.Feign.Codec
{
    public interface IEncoder
    {
        Task EncodeAsync(object body, HttpRequestMessage request);
    }
}