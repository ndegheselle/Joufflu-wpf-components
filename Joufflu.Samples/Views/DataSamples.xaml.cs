using Joufflu.Data.DnD;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Joufflu.Samples.Views
{
    public class CustomDropHandler : DropHandler<TestClass>
    {
        public ObservableCollection<TestClass> Dropped { get; private set; } = [];

        protected override void ApplyDrop(TestClass? data)
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
        { return data is TestClass testClass ? new TextBlock { Text = testClass.Name, Background= new SolidColorBrush(Colors.Red) } : null; }
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

        public DataSamples()
        {
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

        private DragAdorner? _adorner;
        private int _adornerCount = 1;
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_adorner == null)
            {
                _adorner = new DragAdorner(this, new TextBlock() { Text="Test", Background=new SolidColorBrush(Colors.Red)}, new Point(50,50));
                AdornerLayer.GetAdornerLayer(this).Add(_adorner);
            }
            else
            {
                _adorner.UpdatePosition(new Point(50*_adornerCount, 50));
            }

            _adornerCount++;
        }
    }
}
