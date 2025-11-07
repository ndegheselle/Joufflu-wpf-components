using System.Windows;
using System.Windows.Media;

namespace Bariole;

// Interesting : https://github.com/danipen/TextMateSharp
public class SyntaxTextBlock : FrameworkElement
{
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