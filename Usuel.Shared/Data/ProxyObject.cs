namespace Usuel.Shared.Data
{
    public enum EnumValueType
    {
        Object,
        String,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    public class ProxyProperty
    {
        public uint Depth { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public EnumValueType Type { get; set; }

        public ProxyProperty(string name, EnumValueType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class ProxyValue : ProxyProperty
    {
        public dynamic? Value { get; set; }

        public ProxyValue(string name, EnumValueType type, dynamic? value = null) : base(name, type)
        {
            Value = value;
        }
    }

    public class ProxyObject : ProxyProperty
    {
        public List<ProxyProperty> Properties { get; set; } = [];
        public ProxyObject(string name) : base(name, EnumValueType.Object) 
        {}

        public ProxyObject AddObject(ProxyObject obj)
        {
            obj.Depth = Depth + 1;
            Properties.Add(obj);
            return this;
        }

        public ProxyObject AddValue(string name, dynamic? value)
        {
            ProxyValue proxy = new ProxyValue(name, EnumValueType.String, value);
            proxy.Depth = Depth + 1;
            Properties.Add(proxy);
            return this;
        }
    }
}
