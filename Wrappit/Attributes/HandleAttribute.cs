namespace Wrappit.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HandleAttribute : Attribute
{
    public string Topic { get; }
    
    public HandleAttribute(string topic)
    {
        Topic = topic;
    }   
}
