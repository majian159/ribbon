using Rabbit.Feign.Codec;

namespace Rabbit.Feign.Options
{
    public class FeignClientOptions
    {
        public string Path { get; set; }
        public IEncoder Encoder { get; set; }
        public IDecoder Decoder { get; set; }
    }
}