using System.Collections.Generic;
using Rabbit.Feign.Codec;
using Rabbit.Feign.Reflective;
using Ribbon.Client.Http;

namespace Rabbit.Feign.Options
{
    class RequestTemplate
    {
        public TemplateString Url { get; set; }
        public IDictionary<string,TemplateString> Headers { get; set; }
    }
    public class FeignRequestOptions
    {
        public HttpRequest RequestTemplate { get; set; }
        public IEncoder Encoder { get; set; }
        public IDecoder Decoder { get; set; }
    }
}