namespace Rabbit.Feign.Reflective
{
    public interface IParameterExpander
    {
        string Expand(object value);
    }
}