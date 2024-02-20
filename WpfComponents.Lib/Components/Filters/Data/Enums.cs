using System.ComponentModel;

namespace  WpfComponents.Lib.Components.Filters.Data
{
    public enum EnumConjunctionFilter
    {
        [Description("AND")]
        And,
        [Description("OR")]
        Or
    }

    public enum EnumOperatorFilter
    {
        // Equals can no be used since it's already used by object.Equals()
        [Description("equals to")]
        EqualsTo,
        [Description("contains")]
        Contains,
        [Description("start with")]
        StartsWith,
        [Description("ends with")]
        EndsWith,
        [Description("greater than")]
        GreaterThan,
        [Description("greater than or equal")]
        GreaterThanOrEqual,
        [Description("lesser than")]
        LesserThan,
        [Description("lesser than or equal")]
        LesserThanOrEqual,
        [Description("between")]
        Between,
        [Description("in")]
        In,

        [Description("not equals to")]
        NotEqualsTo,
        [Description("doesn't contain")]
        NotContains,
        [Description("doesn't start with")]
        NotStartsWith,
        [Description("doesn't ends with")]
        NotEndsWith,
        //[Description("n'est pas supérieur à")]
        //NotGreaterThan,
        //[Description("n'est pas supérieur ou égal à")]
        //NotGreaterThanOrEqual,
        //[Description("n'est pas inférieur à")]
        //NotLesserThan,
        //[Description("n'est pas inférieur ou égal à")]
        //NotLesserThanOrEqual,
        [Description("not between")]
        NotBetween,
        [Description("not in")]
        NotIn,
    }
}
