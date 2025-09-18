using System.Windows.Markup;

namespace Joufflu.Shared.Extensions
{
    /// <summary>
    /// Return the list of values from an enum type.
    /// </summary>
    public class EnumValuesExtension : MarkupExtension
    {
        /// <summary>
        /// Ignore the X first elements
        /// </summary>
        public int IgnoreElements { get; set; } = 0;

        private readonly Type _enumType;
        public EnumValuesExtension(Type enumType)
        { _enumType = enumType; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        { return Enum.GetValues(_enumType).Cast<Enum>().Skip(IgnoreElements); }
    }
}
