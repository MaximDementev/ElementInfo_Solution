using System.Windows;

namespace MagicEntry.Plugins.ElementInfo.UI
{
    // WPF диалог для ввода текста
    public partial class TextInputDialog : Window
    {
        public string InputText { get; private set; }

        public TextInputDialog(string title, string message, string invalidText = "")
        {
            InitializeComponent();
            Title = title;
            MessageTextBlock.Text = message;

            // Если передан невалидный текст, показываем его в поле ввода
            if (!string.IsNullOrEmpty(invalidText))
            {
                InputTextBox.Text = invalidText;
                InputTextBox.SelectAll();
                InputTextBox.Focus();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
