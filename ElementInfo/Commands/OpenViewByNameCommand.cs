using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MagicEntry.Core.Interfaces;
using MagicEntry.Core.Models;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.Utils;
using System;
using System.Linq;
using System.Windows;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenViewByNameCommand : IExternalCommand, IPlugin
    {
        #region IPlugin Implementation

        public PluginInfo Info { get; set; }
        public bool IsEnabled { get; set; }

        // Инициализация плагина
        public bool Initialize()
        {
            try
            {
                IsEnabled = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Завершение работы плагина
        public void Shutdown()
        {
            // Логика завершения работы
        }

        #endregion

        #region IExternalCommand Implementation

        // Выполнение команды поиска и открытия вида по имени
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument.Document;
                var uiDoc = commandData.Application.ActiveUIDocument;

                // Проверка: проект, не семейство, не шаблон
                if (doc.IsFamilyDocument)
                {
                    TaskDialog.Show("Ошибка", "Это семейство. Информацию можно получить только из файла проекта");
                    return Result.Failed;
                }

                // Получаем текст для анализа с валидацией
                string textToAnalyze = GetValidatedTextForAnalysis();

                if (string.IsNullOrWhiteSpace(textToAnalyze))
                {
                    return Result.Cancelled;
                }

                // Извлекаем имя вида из текста
                string viewName = TextParser.ExtractViewName(textToAnalyze);

                if (string.IsNullOrWhiteSpace(viewName))
                {
                    DialogHelper.ShowError(Messages.INVALID_VIEW_FORMAT);
                    return Result.Cancelled;
                }

                // Находим вид по имени
                var targetView = FindViewByName(doc, viewName);

                if (targetView == null)
                {
                    DialogHelper.ShowError(Messages.VIEW_NOT_FOUND);
                    return Result.Cancelled;
                }

                // Открываем найденный вид
                OpenView(uiDoc, targetView);

                // Показываем результат
                string successMessage = string.Format(Messages.VIEW_OPENED, viewName);
                DialogHelper.ShowSuccess(successMessage);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                DialogHelper.ShowError($"Ошибка при выполнении команды: {ex.Message}");
                return Result.Failed;
            }
        }

        #endregion

        #region Private Methods

        // Получает и валидирует текст для анализа
        private string GetValidatedTextForAnalysis()
        {
            string textToAnalyze = null;

            try
            {
                // Сначала пробуем получить из буфера обмена
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(clipboardText))
                {
                    // Проверяем валидность текста из буфера
                    string viewName = TextParser.ExtractViewName(clipboardText);
                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        return clipboardText;
                    }
                    else
                    {
                        // Текст из буфера невалидный, показываем WPF диалог с ошибкой
                        textToAnalyze = DialogHelper.ShowInvalidTextDialog(clipboardText, Messages.INVALID_VIEW_FORMAT);
                    }
                }
                else
                {
                    // В буфере нет текста, показываем обычный WPF диалог ввода
                    textToAnalyze = DialogHelper.ShowTextInputDialog();
                }
            }
            catch
            {
                // Ошибка при работе с буфером, показываем WPF диалог ввода
                textToAnalyze = DialogHelper.ShowTextInputDialog();
            }

            return textToAnalyze;
        }

        // Находит вид по имени
        private View FindViewByName(Document doc, string viewName)
        {
            try
            {
                var collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate && v.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase));

                return collector.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        // Открывает указанный вид
        private void OpenView(UIDocument uiDoc, View view)
        {
            try
            {
                uiDoc.ActiveView = view;
            }
            catch
            {
                // Не удалось открыть вид
            }
        }

        #endregion
    }
}
