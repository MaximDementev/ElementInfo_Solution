using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Neuroptera.Contracts.PluginLogging;
using Neuroptera.Plugins.ElementInfo.Constants;
using Neuroptera.Plugins.ElementInfo.Utils;
using Neuroptera.Revit.Contracts.PluginLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Neuroptera.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectElementsByIdCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var journal = PluginOperationJournal.Start(
                ElementInfoOperations.PluginId,
                ElementInfoOperations.SelectElementsById,
                doc.Title);

            try
            {
                journal.Step("Запуск команды выбора элементов по ID");

                var uiDoc = commandData.Application.ActiveUIDocument;

                if (doc.IsFamilyDocument)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        "Это семейство. Информацию можно получить только из файла проекта.",
                        "Откройте файл проекта и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.SelectElementsById,
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

                var elementIds = TextParser.ExtractElementIds(textToAnalyze);
                if (!elementIds.Any())
                {
                    RevitPluginErrorHandling.ShowValidation(
                        Messages.INVALID_ID_FORMAT,
                        "Укажите ID в формате «ID: число» или несколько ID подряд.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.SelectElementsById,
                        doc);
                    return Result.Cancelled;
                }

                journal.Step("Поиск элементов по ID", elementIds.Count.ToString());
                var foundElements = FindElementsById(doc, elementIds);
                if (!foundElements.Any())
                {
                    RevitPluginErrorHandling.ShowValidation(
                        Messages.ELEMENTS_NOT_FOUND,
                        "Проверьте ID в тексте и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.SelectElementsById,
                        doc);
                    return Result.Cancelled;
                }

                journal.Step("Выделение элементов в модели", foundElements.Count.ToString());
                SelectElements(uiDoc, foundElements);

                string successMessage = string.Format(Messages.ELEMENTS_SELECTED, foundElements.Count);
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
                    var elementIds = TextParser.ExtractElementIds(clipboardText);
                    if (elementIds.Any())
                    {
                        journal.Step("Использован текст из буфера обмена");
                        return clipboardText;
                    }

                    textToAnalyze = DialogHelper.ShowInvalidTextDialog(clipboardText, Messages.INVALID_ID_FORMAT);
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

        private List<Element> FindElementsById(Document doc, List<int> elementIds)
        {
            var foundElements = new List<Element>();

            foreach (int id in elementIds)
            {
                try
                {
                    var element = doc.GetElement(new ElementId(id));
                    if (element != null)
                    {
                        foundElements.Add(element);
                    }
                }
                catch
                {
                }
            }

            return foundElements;
        }

        private void SelectElements(UIDocument uiDoc, List<Element> elements)
        {
            try
            {
                var ids = elements.Select(e => e.Id).ToList();
                uiDoc.Selection.SetElementIds(ids);
            }
            catch
            {
            }
        }
    }
}
