using System.Xml.Linq;

namespace Usuel.Shared.Data
{
    public enum EnumDataProxyType
    {
        Object,
        String,
        Integer,
        Float,
        Boolean,
        DateTime,
        TimeSpan
    }

    public abstract class DataProxyNode
    {
        public EnumDataProxyType DataType { get; set; } = EnumDataProxyType.String;
        public DataProxyNode(EnumDataProxyType dataType = EnumDataProxyType.String)
        {
            DataType = dataType;
        }
    }

    public class DataProxyValue : DataProxyNode
    {
        public dynamic? Value { get; set; }
        public DataProxyValue(EnumDataProxyType dataType = EnumDataProxyType.String) : base(dataType)
        {}
    }

    public class DataProxyValueList : DataProxyNode
    {
        public List<DataProxyNode> Values { get; set; } = [];
        public DataProxyValueList(EnumDataProxyType dataType = EnumDataProxyType.String) : base(dataType)
        {}
    }

    public class DataProxyProperty : DataProxyNode
    {
        public string Name { get; set; }
        public DataProxyProperty(string name)
        {
            Name = name;
        }
    }

    public class DataProxyPropertyValue : DataProxyProperty
    {
        public DataProxyValue Value { get; set; }
        public DataProxyPropertyValue(string name, DataProxyValue value) : base(name)
        {
            Value = value;
        }
    }
    public class DataProxyPropertyObject : DataProxyProperty
    {
        public DataProxyObject Value { get; set; }
        public DataProxyPropertyObject(string name, DataProxyObject value) : base(name)
        {
            Value = value;
        }
    }

    public class DataProxyObject : DataProxyNode
    {
        public List<DataProxyProperty> Properties { get; private set; } = [];
        public DataProxyObject() : base(EnumDataProxyType.Object)
        {}

        public DataProxyObject AddValue(string name, DataProxyValue value)
        {
            Properties.Add(new DataProxyPropertyValue(name, value));
            return this;
        }

        public DataProxyObject AddObject(string name, DataProxyObject value)
        {
            Properties.Add(new DataProxyPropertyObject(name, value));
            return this;
        }
    }
}
