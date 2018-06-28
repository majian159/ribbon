using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Ribbon.Client.Http
{
    public interface IHttpResponse : IResponse
    {
        int Status { get; }
        string StatusLine { get; }
        HttpHeaders HttpHeaders { get; }
        Stream InputStream { get; }

        bool HasContent { get; }
        HttpContent Content { get; }
    }

    public static class HttpResponseExtensions
    {
        public static T GetContent<T>(this IHttpResponse response) where T : HttpContent
        {
            return response.Content as T;
        }
    }
}