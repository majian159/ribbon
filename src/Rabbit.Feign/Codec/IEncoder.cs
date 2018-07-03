using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Feign.Codec
{
    public interface IEncoder
    {
        Task EncodeAsync(object obj, Type bodyType, HttpRequestMessage request);
    }
}