
namespace Wexflow.Core
{
    public class Tag
    {
        public string Key { get; }
        public string Value { get; }

        public Tag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
