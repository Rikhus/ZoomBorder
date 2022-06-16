using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TatneftSkw.Controls
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public event EventHandler MouseWheelZoomBorder;
        public event EventHandler CoordinatesChanged;


        public double ZoomValue => child != null ? GetScaleTransform(child).ScaleX : 0;
        public double AbsoluteX => child != null ? GetTranslateTransform(child).X : 0;
        public double AbsoluteY => child != null ? GetTranslateTransform(child).Y : 0;

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                //this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                //  child_PreviewMouseRightButtonDown);
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 0.5;
                st.ScaleY = 0.5;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = this.ActualWidth/2;
                tt.Y = this.ActualHeight / 2;
            }
        }

        public void PanTo(double x, double y)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                tt.X = x;
                tt.Y = y;

                if(CoordinatesChanged != null)
                {
                    CoordinatesChanged(this, EventArgs.Empty);
                }
            }
        }

        public void SetZoom(double zoomValue)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                st.ScaleX = zoomValue;
                st.ScaleY = zoomValue;
                Console.WriteLine($"{this.Name} ScaleX: {st.ScaleX} ScaleY: {st.ScaleY}");
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {               
                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && ZoomValue < .3)
                    return;

                Point relative = e.GetPosition(child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * ZoomValue + AbsoluteX;
                absoluteY = relative.Y * ZoomValue + AbsoluteY;

                zoom += ZoomValue;

                SetZoom(zoom);
                PanTo(absoluteX - relative.X * ZoomValue, absoluteY - relative.Y * ZoomValue);
                if (MouseWheelZoomBorder != null)
                {
                    MouseWheelZoomBorder(this, EventArgs.Empty);
                }
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    Vector v = start - e.GetPosition(this);
                    PanTo(origin.X - v.X, origin.Y - v.Y);
                }
            }
        }

        #endregion
    }
}
