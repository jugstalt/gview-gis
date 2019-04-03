using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace gView.Desktop.Wpf.Controls
{
    public static class ToolBarFactory
    {
        static public ToolBar NewToolBar(bool hasGrip = true)
        {
            ToolBar toolbar = new ToolBar();

            //toolbar.Height = 26;
            toolbar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            toolbar.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            toolbar.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 188, 199, 216));
            toolbar.BorderBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(255, 213, 220, 232));

            toolbar.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            toolbar.Padding = new System.Windows.Thickness(0, 0, 0, 0);

            if (hasGrip == false)
            {

            }

            return toolbar;
        }

        static public void AppendToolButton(ToolBar toolbar, Image image, string text = "", System.Windows.RoutedEventHandler click = null)
        {
            Button button = new Button();
            AppendToolButtonProperties(button, image, text, click);
            if (button.Content == null)
                return;

            toolbar.Items.Add(button);
        }

        static public void AppendSeperator(ToolBar toolbar)
        {
            Separator sep = new Separator();
            sep.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 133, 145, 162));
            toolbar.Items.Add(sep);
        }

        static public void AppendToolButtonProperties(Button button, Image image, string text, System.Windows.RoutedEventHandler click = null)
        {
            object content = image;
            if (!String.IsNullOrEmpty(text))
            {
                content = new StackPanel();
                ((StackPanel)content).Orientation = Orientation.Horizontal;
                if (image != null)
                    ((StackPanel)content).Children.Add(image);
                var textBlock = new TextBlock();
                textBlock.Text = text;
                textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 47, 21, 62));
                ((StackPanel)content).Children.Add(textBlock);
            }
            else
            {
                button.Width = 24;
            }
            if (content != null)
                button.Content = content;

            if (click != null)
                button.Click += click;
            button.Height = 24;
        }
    }

    public static class ImageFactory
    {
        static public Image FromBitmap(System.Drawing.Image bm)
        {
            if (bm == null)
                return null;

            Image image = new Image();

            MemoryStream ms = new MemoryStream();
            bm.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            image.Source = bi;
            image.Width = bm.Width;
            image.Height = bm.Height;

            return image;
        }
    }
}
 