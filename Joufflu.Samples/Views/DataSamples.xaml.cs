using Joufflu.Data.DnD;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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

    /// <summary>
    /// Logique d'interaction pour DataSamples.xaml
    /// </summary>
    public partial class DataSamples : UserControl
    {
        public int Total => Tests.Values.Count;
        public List<TestClass> DnDValues { get; private set; } = Tests.Values;

        public DragHandler DragHandler { get; private set; }
        public CustomDropHandler DropHandler { get; private set; }

        public DataSamples()
        {
            DragHandler = new DragHandler(this);
            DropHandler = new CustomDropHandler();
            InitializeComponent();
            PagingDataGrid.ItemsSource = Tests.Values.Take(5);
        }

        #region UI events
        private void Paging_PagingChange(int pageNumber, int capacity)
        {
            PagingDataGrid.ItemsSource = Tests.Values.Skip((pageNumber - 1) * capacity).Take(capacity);
        }
        #endregion
    }
}
