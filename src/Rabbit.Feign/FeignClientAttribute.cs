using System;

namespace Rabbit.Feign
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FeignClientAttribute : Attribute
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Type FallbackType { get; set; }
    }
}