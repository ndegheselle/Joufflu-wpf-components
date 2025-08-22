using Joufflu.Data.DnD;
using Joufflu.Data.Schema;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Usuel.Shared.Data;

namespace Joufflu.Samples.Views
{
    public class CustomDropHandler : DropHandler<TestClass>
    {
        public ObservableCollection<TestClass> Dropped { get; private set; } = [];

        protected override void ApplyDrop(TestClass? data, DragEventArgs e)
        {
            if (data == null)
                return;
            Dropped.Add(data);
        }
    }

    public class CustomDragHandler : AdornerDragHandler
    {
        public CustomDragHandler(FrameworkElement parent) : base(parent)
        {
        }

        protected override FrameworkElement? CreateAdornerContent(object data)
        {
            if (data is not TestClass testClass)
                return null;

            Border border = new Border()
            {
                CornerRadius = new CornerRadius(2),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                Height = 64,
                Width = 64
            };
            border.Child = new TextBlock() { Text = testClass.Name, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

            return border; 
        }
    }

    /// <summary>
    /// Logique d'interaction pour DataSamples.xaml
    /// </summary>
    public partial class DataSamples : UserControl
    {
        public int Total => Tests.Values.Count;

        public List<TestClass> DnDValues { get; private set; } = Tests.Values;

        public CustomDragHandler DragHandler { get; private set; }

        public CustomDropHandler DropHandler { get; private set; }

        public SchemaObjectUi DataRoot { get; private set; }

        public DataSamples()
        {
            SchemaObjectUi obj = new SchemaObjectUi("afdsfg");
            DataRoot = new SchemaObjectUi("Root schema");
            DataRoot
                .AddValue("fdsfdsf")
                .AddValue("bvcbvc")
                .Add(obj)
                .AddValue("liuluil");

            obj.AddValue("tataqds");

            DropHandler = new CustomDropHandler();
            DragHandler = new CustomDragHandler(this);
            InitializeComponent();
            PagingDataGrid.ItemsSource = Tests.Values.Take(5);
        }

        #region UI events
        private void Paging_PagingChange(int pageNumber, int capacity)
        { PagingDataGrid.ItemsSource = Tests.Values.Skip((pageNumber - 1) * capacity).Take(capacity); }

        void HandleMouseMove(object sender, MouseEventArgs e)
        {
            Debug.WriteLine(e.GetPosition(this));
        }
        #endregion
    }
}
