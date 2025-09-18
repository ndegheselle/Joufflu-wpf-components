using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.Data.Schema
{
    /// Logique d'interaction pour SchemaEdit.xaml
    /// </summary>
    public partial class SchemaEdit : UserControl
    {
        public SchemaObject Root { get; set; }

        public bool IsReadOnly { get; set; }

        public SchemaEdit()
        {
            Root = new SchemaObject();

            var sub = new SchemaObject();
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            Root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean });
            Root.Add("toto", new SchemaArray());
            Root.Add("titi", sub);

            InitializeComponent();
        }
    }
}
