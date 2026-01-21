
namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; }
        public StringValueAttribute(string stringValue)
        {
            StringValue = stringValue;
        }
    }
}
