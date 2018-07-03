using System;

namespace Rabbit.Feign
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class FeignClientAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Decode404 { get; set; }
        public string Path { get; set; }
    }
}