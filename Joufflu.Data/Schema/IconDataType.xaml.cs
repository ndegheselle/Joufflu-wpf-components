using Joufflu.Shared.Resources.Fonts;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Schema
{
    public partial class IconDataType : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
            nameof(Type),
            typeof(EnumDataType),
            typeof(IconDataType),
            new PropertyMetadata(EnumDataType.String, (d, e) => ((IconDataType)d).OnTypeChanged()));

        /// <summary>
        /// Type of the value represented by this icon.
        /// </summary>
        public EnumDataType Type
        {
            get { return (EnumDataType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string Icon { get; private set; } = IconFont.Quotes;
        public bool WithText { get; set; } = true;

        public IconDataType()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Change the icon based on the type of value.
        /// </summary>
        private void OnTypeChanged()
        {
            Icon = Type switch
            {
                EnumDataType.Object => IconFont.BracketsCurly,
                EnumDataType.Array => IconFont.BracketsSquare,
                EnumDataType.String => IconFont.Quotes,
                EnumDataType.Decimal => IconFont.Hash,
                EnumDataType.Boolean => IconFont.Check,
                EnumDataType.DateTime => IconFont.Calendar,
                EnumDataType.TimeSpan => IconFont.Clock,
                _ => IconFont.QuestionMark
            };
        }
    }
}
