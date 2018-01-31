using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TouchRemote.UI.Controls
{
    internal class SimpleSlider : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(float), typeof(SimpleSlider), new UIPropertyMetadata(OnPropChanged));

        public float Value
        {
            get
            {
                return (float)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SimpleSlider), new UIPropertyMetadata(OnPropChanged));

        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        private static void OnPropChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            var @this = @object as SimpleSlider;
            @this.Render();
        }

        #endregion

        private readonly DrawingGroup _BackingStore = new DrawingGroup();

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Render();
            drawingContext.DrawDrawing(_BackingStore);
        }

        private void Render()
        {
            var drawingContext = _BackingStore.Open();
            Render(drawingContext);
            drawingContext.Close();
        }

        private void Render(DrawingContext context)
        {
            var fillRect = new Rect();
            if (Orientation == Orientation.Vertical)
            {
                var height = ActualHeight * Value;
                fillRect.X = 0;
                fillRect.Y = ActualHeight - height;
                fillRect.Height = height;
                fillRect.Width = ActualWidth;
            }
            else
            {
                fillRect.X = fillRect.Y = 0;
                fillRect.Height = ActualHeight;
                fillRect.Width = ActualWidth * Value;
            }
            var bgBrush = new SolidColorBrush(Color.FromArgb(51, 0, 0, 0));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));
            context.DrawRectangle(Foreground, null, fillRect);
        }
    }
}
