using System.Windows;
using System.Windows.Media;

namespace Bariole;

// For more complexe / faster ? https://github.com/tree-sitter/tree-sitter
// Could be a good stard : https://github.com/danipen/TextMateSharp
public class SyntaxTextBlock : FrameworkElement
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(SyntaxTextBlock), new PropertyMetadata(default(string)));

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // Your custom rendering code here
        // Example: Draw a circle
        drawingContext.DrawEllipse(
            Brushes.Blue,
            new Pen(Brushes.Black, 2),
            new Point(50, 50),
            40, 40);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Return your desired size
        return new Size(100, 100);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }
}