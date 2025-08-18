using Autodesk.Revit.UI;
using MagicEntry.Plugins.ElementInfo.Constants;
using System.Windows;
using System.Windows.Forms;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    // Утилитарный класс для работы с диалогами
    public static class DialogHelper
    {
        #region Public Methods

        // Показывает диалог с информацией и возможностью копирования в буфер
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
                System.Windows.Clipboard.SetText(info);
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

        // Показывает WinForms диалог для ввода текста с возможностью показа невалидного текста
        public static string ShowTextInputDialog(string title = null, string message = null, string invalidText = "")
        {
            title = title ?? Messages.ENTER_TEXT_TITLE;
            message = message ?? Messages.ENTER_TEXT_INSTRUCTION;

            using (var form = new TextInputForm(title, message, invalidText))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.InputText;
                }
            }

            return null;
        }

        // Показывает диалог с сообщением о невалидном тексте и полем для ввода
        public static string ShowInvalidTextDialog(string invalidText, string errorMessage)
        {
            string message = $"{errorMessage}\n\nВведите корректный текст:";
            return ShowTextInputDialog(Messages.INVALID_TEXT_TITLE, message, invalidText);
        }

        #endregion
    }
}
