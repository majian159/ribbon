using System;

namespace Rabbit.Feign.Reflective
{
    public class ClientDescriptor
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public IMethodDescriptorTable MethodDescriptorTable { get; set; }
    }
}