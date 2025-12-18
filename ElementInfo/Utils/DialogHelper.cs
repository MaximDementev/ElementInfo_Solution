using Autodesk.Revit.UI;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.UI;
using System.Windows;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    public static class DialogHelper
    {
        // Показывает диалог с информацией и возможностью копирования в буфер (устаревший метод для обратной совместимости)
        public static bool ShowInfoDialogWithCopy(string info)
        {
            var dialog = new TaskDialog(Messages.INFORMATION_TITLE)
            {
                MainInstruction = info,
                MainContent = Messages.COPY_TO_CLIPBOARD_QUESTION,
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes,
                FooterText = Messages.FOOTER_LINK
            };

            var result = dialog.Show();

            if (result == TaskDialogResult.Yes)
            {
                Clipboard.SetText(info);
                return true;
            }

            return false;
        }

        // Показывает диалог с ошибкой
        public static void ShowError(string message)
        {
            TaskDialog.Show(Messages.ERROR_TITLE, message);
        }

        // Показывает диалог с успешным результатом
        public static void ShowSuccess(string message)
        {
            TaskDialog.Show(Messages.SUCCESS_TITLE, message);
        }

        public static string ShowTextInputDialog(string title = null, string message = null, string invalidText = "")
        {
            title = title ?? Messages.ENTER_TEXT_TITLE;
            message = message ?? Messages.ENTER_TEXT_INSTRUCTION;

            var dialog = new TextInputDialog(title, message, invalidText);

            if (dialog.ShowDialog() == true)
            {
                return dialog.InputText;
            }

            return null;
        }

        // Показывает диалог с сообщением о невалидном тексте и полем для ввода
        public static string ShowInvalidTextDialog(string invalidText, string errorMessage)
        {
            string message = $"{errorMessage}\n\nВведите корректный текст:";
            return ShowTextInputDialog(Messages.INVALID_TEXT_TITLE, message, invalidText);
        }
    }
}
