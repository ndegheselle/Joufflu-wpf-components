using System.Windows.Markup;

namespace Joufflu.Shared.Extensions
{
    /// <summary>
    /// Return the list of values from an enum type.
    /// </summary>
    public class EnumValuesExtension : MarkupExtension
    {
        public Type EnumType { get; set; }

        public EnumValuesExtension(Type enumType)
        { EnumType = enumType; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        { return Enum.GetValues(EnumType); }
    }
}
