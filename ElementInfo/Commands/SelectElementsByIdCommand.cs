using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MagicEntry.Core.Interfaces;
using MagicEntry.Core.Models;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectElementsByIdCommand : IExternalCommand, IPlugin
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

        // Выполнение команды поиска и выделения элементов по ID
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

                // Извлекаем ID элементов из текста
                var elementIds = TextParser.ExtractElementIds(textToAnalyze);

                if (!elementIds.Any())
                {
                    DialogHelper.ShowError(Messages.INVALID_ID_FORMAT);
                    return Result.Cancelled;
                }

                // Находим и выделяем элементы
                var foundElements = FindElementsById(doc, elementIds);

                if (!foundElements.Any())
                {
                    DialogHelper.ShowError(Messages.ELEMENTS_NOT_FOUND);
                    return Result.Cancelled;
                }

                // Выделяем найденные элементы
                SelectElements(uiDoc, foundElements);

                // Показываем результат
                string successMessage = string.Format(Messages.ELEMENTS_SELECTED, foundElements.Count);
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
                    var elementIds = TextParser.ExtractElementIds(clipboardText);
                    if (elementIds.Any())
                    {
                        return clipboardText;
                    }
                    else
                    {
                        // Текст из буфера невалидный, показываем WPF диалог с ошибкой
                        textToAnalyze = DialogHelper.ShowInvalidTextDialog(clipboardText, Messages.INVALID_ID_FORMAT);
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

        // Находит элементы по списку ID
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
                    // Элемент не найден, продолжаем поиск
                }
            }

            return foundElements;
        }

        // Выделяет элементы в документе
        private void SelectElements(UIDocument uiDoc, List<Element> elements)
        {
            try
            {
                var elementIds = elements.Select(e => e.Id).ToList();
                uiDoc.Selection.SetElementIds(elementIds);
            }
            catch
            {
                // Не удалось выделить элементы
            }
        }

        #endregion
    }
}
