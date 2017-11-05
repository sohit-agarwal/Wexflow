namespace Wexflow.Core
{
    public class Setting
    {        
        public string Name { get; set; }   
        public string Value { get; set; }
        public Attribute[] Attributes { get; set; }

        public Setting(string name, string value, Attribute[] attributes)
        {
            Name = name;
            Value = value;
            Attributes = attributes;
        }
    }
}