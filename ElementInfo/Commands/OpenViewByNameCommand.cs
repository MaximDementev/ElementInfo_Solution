using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Neuroptera.Contracts.PluginLogging;
using Neuroptera.Plugins.ElementInfo.Constants;
using Neuroptera.Plugins.ElementInfo.Utils;
using Neuroptera.Revit.Contracts.PluginLogging;
using System;
using System.Linq;
using System.Windows;

namespace Neuroptera.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenViewByNameCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var journal = PluginOperationJournal.Start(
                ElementInfoOperations.PluginId,
                ElementInfoOperations.OpenViewByName,
                doc.Title);

            try
            {
                journal.Step("Запуск команды открытия вида по имени");

                var uiDoc = commandData.Application.ActiveUIDocument;

                if (doc.IsFamilyDocument)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        "Это семейство. Информацию можно получить только из файла проекта.",
                        "Откройте файл проекта и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.OpenViewByName,
                        doc);
                    return Result.Failed;
                }

                journal.Step("Получение текста для анализа");
                string textToAnalyze = GetValidatedTextForAnalysis(journal);
                if (string.IsNullOrWhiteSpace(textToAnalyze))
                {
                    journal.Step("Пользователь отменил ввод текста");
                    return Result.Cancelled;
                }

                string viewName = TextParser.ExtractViewName(textToAnalyze);
                if (string.IsNullOrWhiteSpace(viewName))
                {
                    RevitPluginErrorHandling.ShowValidation(
                        Messages.INVALID_VIEW_FORMAT,
                        "Скопируйте текст с полем «Активный вид» или введите имя вида вручную.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.OpenViewByName,
                        doc);
                    return Result.Cancelled;
                }

                journal.Step("Поиск вида", viewName);
                var targetView = FindViewByName(doc, viewName);
                if (targetView == null)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        Messages.VIEW_NOT_FOUND,
                        "Проверьте имя вида в тексте и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.OpenViewByName,
                        doc);
                    return Result.Cancelled;
                }

                journal.Step("Переключение на вид", targetView.Name);
                OpenView(uiDoc, targetView);

                string successMessage = string.Format(Messages.VIEW_OPENED, viewName);
                DialogHelper.ShowSuccess(successMessage);
                journal.Complete(successMessage);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                RevitPluginErrorHandling.Handle(ex, journal);
                return Result.Failed;
            }
        }

        private string GetValidatedTextForAnalysis(PluginOperationJournal journal)
        {
            string textToAnalyze = null;

            try
            {
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(clipboardText))
                {
                    string viewName = TextParser.ExtractViewName(clipboardText);
                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        journal.Step("Использован текст из буфера обмена");
                        return clipboardText;
                    }

                    textToAnalyze = DialogHelper.ShowInvalidTextDialog(clipboardText, Messages.INVALID_VIEW_FORMAT);
                }
                else
                {
                    textToAnalyze = DialogHelper.ShowTextInputDialog();
                }
            }
            catch
            {
                textToAnalyze = DialogHelper.ShowTextInputDialog();
            }

            return textToAnalyze;
        }

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

        private void OpenView(UIDocument uiDoc, View view)
        {
            try
            {
                uiDoc.ActiveView = view;
            }
            catch
            {
            }
        }
    }
}
