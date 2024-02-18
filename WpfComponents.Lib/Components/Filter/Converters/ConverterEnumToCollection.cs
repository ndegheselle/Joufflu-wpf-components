using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfComponents.Lib.Logic.Helpers;

namespace UltraFiltre.Wpf.Lib.Converters
{
    /* Exemple d'utilisation
     <ComboBox
        ItemsSource="{Binding PropAvecEnumEnType, Converter={StaticResource ConverterEnumToCollection}, Mode=OneTime}"
        SelectedValue="{Binding PropAvecEnumEnType, Mode=TwoWay}"
        DisplayMemberPath="Description"
        SelectedValuePath="Valeur"
        Grid.Column="1" />
     */
    public class ConverterEnumToCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var targetEnum = (Enum) value;
            return Enum.GetValues(targetEnum.GetType())
                .Cast<Enum>()
                .Select(e => new {Valeur = e, Description = e.GetDescription()})
                .ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { return null; }
    }
}
