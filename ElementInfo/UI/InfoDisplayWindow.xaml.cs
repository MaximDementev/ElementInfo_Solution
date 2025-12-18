using System.Windows;
using System.Windows.Documents;

namespace MagicEntry.Plugins.ElementInfo.UI
{
    public partial class InfoDisplayWindow : Window
    {
        public InfoDisplayWindow(string title, bool showSelectMoreButton = false)
        {
            InitializeComponent();
            Title = title;

            SelectMoreButton.Visibility = showSelectMoreButton ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetText(string text)
        {
            InfoRichTextBox.Document.Blocks.Clear();
            InfoRichTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        public string GetText()
        {
            return new TextRange(InfoRichTextBox.Document.ContentStart, InfoRichTextBox.Document.ContentEnd).Text;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetText());
            Close();
        }

        private void SelectMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем окно и возвращаем управление в команду
            DialogResult = true; // true = пользователь хочет дополнительный выбор элементов
            Close();
        }

    }

}
