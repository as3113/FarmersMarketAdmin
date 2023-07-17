using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace FarmersMarketAdmin
{
    public static class WatermarkService
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(string),
            typeof(WatermarkService),
            new PropertyMetadata(null, OnWatermarkChanged));

        public static string GetWatermark(DependencyObject d) => (string)d.GetValue(WatermarkProperty);
        public static void SetWatermark(DependencyObject d, string value) => d.SetValue(WatermarkProperty, value);

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((string)e.NewValue != null)
                {
                    textBox.GotFocus += TextBox_GotFocus;
                    textBox.LostFocus += TextBox_LostFocus;
                    RefreshWatermark(textBox);
                }
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            RefreshWatermark(sender as TextBox);
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshWatermark(sender as TextBox);
        }

        private static void RefreshWatermark(TextBox textBox)
        {
            if (textBox == null)
                return;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = GetWatermark(textBox);
                textBox.FontStyle = FontStyles.Italic;
            }
            else if (textBox.Text == GetWatermark(textBox))
            {
                textBox.Text = string.Empty;
                textBox.FontStyle = FontStyles.Normal;
            }
        }
    }
}
