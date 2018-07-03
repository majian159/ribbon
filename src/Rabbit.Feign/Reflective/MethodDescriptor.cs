using System;
using System.Collections.Generic;
using Rabbit.Feign.Codec;

namespace Rabbit.Feign.Reflective
{
    public class MethodDescriptor
    {
        public string RequestMethod { get; set; }
        public TemplateString RequestLine { get; set; }
        public ParameterDescriptor[] Parameters { get; set; }
        public IDictionary<string, TemplateString> Headers { get; set; }
        public IEncoder Encoder { get; set; }
        public IDecoder Decoder { get; set; }
    }

    public class ParameterDescriptor
    {
        public Type ParameterType { get; set; }
        public string ParameterName { get; set; }
        public object[] Attributes { get; set; }
        public string Name { get; set; }
    }
}