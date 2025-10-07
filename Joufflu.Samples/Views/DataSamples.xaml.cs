﻿using Joufflu.Data.DnD;
using NJsonSchema;
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

        public DataSamples()
        {
            DropHandler = new CustomDropHandler();
            DragHandler = new CustomDragHandler(this);

            // Get schema
            // Display
            // Bind to value
            // Return json

            /*
            var serializerOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                Converters = { new JsonStringEnumConverter() },
                RespectNullableAnnotations = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };
            JsonNode schemaJson = serializerOptions.GetJsonSchemaAsNode(typeof(TestClassWithSub));
            */

            var schema = JsonSchema.FromType<TestClass>();
            var tata = schema.ToSampleJson();

            InitializeComponent();
            PagingDataGrid.ItemsSource = Tests.Values.Take(5);
        }

        #region UI events
        private void Paging_PagingChange(int pageNumber, int capacity)
        { PagingDataGrid.ItemsSource = Tests.Values.Skip((pageNumber - 1) * capacity).Take(capacity); }
        #endregion
    }
}
