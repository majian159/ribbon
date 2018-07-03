using System;

namespace Rabbit.Feign.Reflective
{
    public interface IParameterExpanderLocator
    {
        IParameterExpander Get(Type type);
    }
}