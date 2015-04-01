using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bulk_Image_Watermark
{
    public enum DragOperations
    { Move, Rotate}

    public class WatermarkLabelAdorner : Adorner
    //adorner for label of text watermark
    {
        private Point draggingStartPosition;
        private Point draggingEndPosition;

        //drag event
        public delegate void DragEventContainer(WatermarkLabelAdorner sender, DragOperations operation, double diffX, double diffY);
        public event DragEventContainer OnDrag;

        private Ellipse corner;
        private VisualCollection visualChildren;

        public WatermarkLabelAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            //handlers for drugging
            this.MouseLeftButtonDown += AdornerMouseLeftButtonDown;
            this.MouseLeftButtonUp += AdornerMouseLeftButtonUp;
            this.Cursor = Cursors.Hand;

            visualChildren = new VisualCollection(this);
            //rotation ellipse on right top ange
            corner = new Ellipse();
            
            //mouse enter and leave handlers to swith off/on adorner body mouse pressed handlers
            corner.MouseEnter += AdornerEllipseMouseEnter;
            corner.MouseLeave += AdornerEllipseMouseLeave;

            corner.Cursor = Cursors.SizeNESW;
            corner.Width = 14;
            corner.Height = 14;
            corner.Fill = Brushes.Silver;
            corner.Stroke = Brushes.Navy;
            corner.StrokeThickness = 0.5;
            corner.MouseLeftButtonDown += AdornerMouseLeftButtonDown;
            corner.MouseLeftButtonUp += AdornerMouseLeftButtonUp;
            visualChildren.Add(corner);
        }

        private void AdornerEllipseMouseEnter(object sender, System.EventArgs e)
        {
            //disable adorner body mouse pressed handlers to avoid movement instead of rotation
            this.MouseLeftButtonDown -= AdornerMouseLeftButtonDown;
            this.MouseLeftButtonUp -= AdornerMouseLeftButtonUp;            
        }

        private void AdornerEllipseMouseLeave(object sender, System.EventArgs e)
        {
            this.MouseLeftButtonDown += AdornerMouseLeftButtonDown;
            this.MouseLeftButtonUp += AdornerMouseLeftButtonUp;
        }

        private void AdornerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //remember mouse position before drugging
        {
            var draggableControl = sender as FrameworkElement;
            FrameworkElement o = this.Parent as FrameworkElement;
            draggingStartPosition = e.GetPosition(o);
            draggableControl.CaptureMouse();
        }

        private void AdornerMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement o = this.Parent as FrameworkElement;
            draggingEndPosition = e.GetPosition(o);
            var draggable = sender as FrameworkElement;

            //check if user drags adorner body or corner and generate event with corresponding parameter
            DragOperations op;
            if (sender.GetType() == typeof(WatermarkLabelAdorner))
                //it is adorner body - need to move
                op = DragOperations.Move;
            else
                //it is corner - need to rotate
                op = DragOperations.Rotate;

            OnDrag(this, op, draggingEndPosition.X - draggingStartPosition.X, draggingEndPosition.Y - draggingStartPosition.Y);
            draggingStartPosition = new Point(0,0);
            draggingEndPosition = new Point(0, 0);

            draggable.ReleaseMouseCapture();
            e.Handled = true;
        }

        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            //place rotation ellipse
            corner.Arrange(new Rect(desiredWidth - corner.Width / 2, 0 - corner.Height / 2, corner.Width, corner.Height));
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //draw border rectangle
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 0.5);
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.Transparent), renderPen, new Rect(adornedElementRect.TopLeft, adornedElementRect.BottomRight));
        }
    }


}
