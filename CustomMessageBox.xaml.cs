using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace SimpleZip
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public CustomMessageBox(string message, string title = "SimpleZip", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            CreateButtons(buttons);
        }

        private void CreateButtons(MessageBoxButton buttons)
        {
            BtnPanel.Children.Clear();

            if (buttons == MessageBoxButton.OK)
            {
                AddButton("Aceptar", MessageBoxResult.OK, "#4A6BD1");
            }
            else if (buttons == MessageBoxButton.OKCancel)
            {
                AddButton("Cancelar", MessageBoxResult.Cancel, "#E74C3C");
                AddButton("Aceptar", MessageBoxResult.OK, "#4A6BD1");
            }
            else if (buttons == MessageBoxButton.YesNo)
            {
                AddButton("No", MessageBoxResult.No, "#E74C3C");
                AddButton("Sí", MessageBoxResult.Yes, "#63D921");
            }
            else if (buttons == MessageBoxButton.YesNoCancel) 
            {
                AddButton("Cancelar", MessageBoxResult.Cancel, "#95A5A6");
                AddButton("No", MessageBoxResult.No, "#E74C3C");
                AddButton("Sí", MessageBoxResult.Yes, "#63D921");
            }
        }

        private void AddButton(string text, MessageBoxResult result, string hexColor)
        {
            Button btn = new Button
            {
                Content = text,
                Tag = result,
                Foreground = Brushes.White,
                Padding = new Thickness(20,8,20,8),
                Margin = new Thickness(8,0,0,0),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0),
                Background = (Brush) new BrushConverter().ConvertFromString(hexColor),
                Template = (ControlTemplate) Application.Current.FindResource("ModernButtonTemplate")
            };

            btn.Click += (s, e) => { Result = result; this.Close(); };
            BtnPanel.Children.Add(btn);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
