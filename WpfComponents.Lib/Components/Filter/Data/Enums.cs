using System.ComponentModel;

namespace UltraFiltre.Lib
{
    public enum EnumConjonctionFiltre
    {
        [Description("ET")]
        And,
        [Description("OU")]
        Or
    }

    public enum EnumComparaisonFiltre
    {
        // Equals ne peut pas être utilisé en tant que nom à cause de object.Equals()
        [Description("égal à")]
        EqualsTo,
        [Description("contient")]
        Contains,
        [Description("commence par")]
        StartsWith,
        [Description("fini par")]
        EndsWith,
        [Description("supérieur à")]
        GreaterThan,
        [Description("supérieur ou égal à")]
        GreaterThanOrEqual,
        [Description("inférieur à")]
        LesserThan,
        [Description("inférieur ou égal à")]
        LesserThanOrEqual,
        [Description("entre")]
        Between,
        [Description("dans")]
        In,

        [Description("différent de")]
        NotEqualsTo,
        [Description("ne contient pas")]
        NotContains,
        [Description("ne commence pas par")]
        NotStartsWith,
        [Description("ne fini pas par")]
        NotEndsWith,
        //[Description("n'est pas supérieur à")]
        //NotGreaterThan,
        //[Description("n'est pas supérieur ou égal à")]
        //NotGreaterThanOrEqual,
        //[Description("n'est pas inférieur à")]
        //NotLesserThan,
        //[Description("n'est pas inférieur ou égal à")]
        //NotLesserThanOrEqual,
        [Description("n'est pas entre")]
        NotBetween,
        [Description("n'est pas dans")]
        NotIn,
    }
}
