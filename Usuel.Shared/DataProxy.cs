namespace Usuel.Shared
{
    public enum EnumDataType
    {
        Object,
        List,
        String,
        Integer,
        Float,
        Boolean,
        DateTime,
        TimeSpan
    }

    public abstract class DataNode
    {
        public EnumDataType DataType { get; set; } = EnumDataType.String;
        public DataNode(EnumDataType dataType = EnumDataType.String)
        {
            DataType = dataType;
        }
    }

    public class DataValue : DataNode
    {
        public dynamic? Value { get; set; }
        public DataValue(dynamic? value = null, EnumDataType dataType = EnumDataType.String) : base(dataType)
        {
            Value = value;
        }
    }

    public class DataObject : DataNode
    {
        public Dictionary<string, DataNode> Properties { get; set; } = [];
        public DataObject() : base(EnumDataType.Object)
        {}
    }

    public class DataList : DataNode
    {
        public List<DataNode> Values { get; set; } = [];
        public DataList() : base(EnumDataType.List)
        {}
    }
}
