using Joufflu.Data.DnD;
using Joufflu.Data.Shared;
using Joufflu.Data.Shared.Builders;
using NJsonSchema;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            border.Child = new TextBlock()
            {
                Text = testClass.Name,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

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

        public GenericObject Root { get; private set; }

        public DataSamples()
        {
            DropHandler = new CustomDropHandler();
            DragHandler = new CustomDragHandler(this);

            Root = BuilderFromType.ConvertObject(Tests.ValueWithSub);

            var schema1 = JsonSchema.FromSampleJson("""
                                      {
                                      	"tata": "string",
                                      	"boobo": true
                                      }
                                      """);
            var schema2 = JsonSchema.FromSampleJson("""
                                                    {
                                                    	"tata": 54,
                                                    	"somethingelse": ["toto"]
                                                    }
                                                    """);
            
            var mergedSchema = new JsonSchema
            {
                Description = "Merged schema allowing either schema1 or schema2",
            };

            mergedSchema.Properties.Add("previous", new JsonSchemaProperty
            {
                AnyOf = { schema1 }
            });
            mergedSchema.Properties.Add("context", new JsonSchemaProperty
            {
                AnyOf = { schema2 }
            });
            
            var schemaJson = mergedSchema.ToJson();
            var schemaSample = mergedSchema.ToSampleJson().ToString();
            
            InitializeComponent();
        }

        #region UI events
        private void Paging_PagingChange(int pageNumber, int capacity)
        { 
            PagingDataGrid.ItemsSource = Tests.Values.Skip((pageNumber - 1) * capacity).Take(capacity); 
        }
        #endregion
    }
}
