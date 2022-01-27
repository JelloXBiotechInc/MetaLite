using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using MetaLite_Viewer.Helper;
using System;

namespace MetaLite_Viewer.Model
{
    public class MouseBehaviour : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
           "MouseY", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
           "MouseX", typeof(double), typeof(MouseBehaviour), new PropertyMetadata(default(double)));

        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var pos = mouseEventArgs.GetPosition(Resource.MainWindow.mainViewer);
            double width = Resource.MainWindow.mainViewer.ActualWidth;
            double height = Resource.MainWindow.mainViewer.ActualHeight;
            if (width + height>0)
            {
                MouseX = MathHelper.Clip(pos.X * Resource.nowImageData.DataWidth / Resource.MainWindow.mainViewer.ActualWidth,0, Resource.nowImageData.DataWidth-1) ;
                MouseY = MathHelper.Clip(pos.Y * Resource.nowImageData.DataHeight / Resource.MainWindow.mainViewer.ActualHeight,0, Resource.nowImageData.DataHeight-1);
            }
            else
            {
                MouseX = MouseY = 0;
            }
            
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }
    }
}