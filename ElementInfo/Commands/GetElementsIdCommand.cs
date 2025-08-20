using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MagicEntry.Core.Interfaces;
using MagicEntry.Core.Models;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    // Команда для получения ID выбранных элементов
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetElementsIdCommand : IExternalCommand, IPlugin
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

        // Выполнение команды получения ID элементов
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument.Document;
                var selection = commandData.Application.ActiveUIDocument.Selection;

                // Получаем уже выбранные элементы
                var preSelectedElements = GetPreSelectedElements(selection, doc);
                var elementIds = new HashSet<ElementId>();

                // Добавляем предварительно выбранные элементы
                if (preSelectedElements.Any())
                {
                    foreach (var element in preSelectedElements)
                    {
                        elementIds.Add(element.Id);
                    }
                }

                // Если нет предварительно выбранных элементов, запускаем выбор
                if (!elementIds.Any())
                {
                    var selectedElements = SelectElementsWithBox(selection, doc);
                    if (selectedElements != null && selectedElements.Any())
                    {
                        foreach (var element in selectedElements)
                        {
                            elementIds.Add(element.Id);
                        }
                    }
                }

                // Если элементы не выбраны, выходим
                if (!elementIds.Any())
                {
                    DialogHelper.ShowError(Messages.NO_ELEMENTS_SELECTED);
                    return Result.Cancelled;
                }

                // Показываем диалог с ID элементов
                ShowElementsIdDialog(elementIds, selection, doc);

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

        // Получает предварительно выбранные элементы
        private List<Element> GetPreSelectedElements(Selection selection, Document doc)
        {
            try
            {
                var selectedIds = selection.GetElementIds();
                return selectedIds.Select(id => doc.GetElement(id)).Where(e => e != null).ToList();
            }
            catch
            {
                return new List<Element>();
            }
        }

        // Выбирает элементы рамкой
        private List<Element> SelectElementsWithBox(Selection selection, Document doc)
        {
            try
            {
                var elementRefs = selection.PickObjects(ObjectType.Element, Messages.SELECT_ELEMENTS_INSTRUCTION);
                return elementRefs.Select(elementRef => doc.GetElement(elementRef)).ToList();
            }
            catch
            {
                return null;
            }
        }

        // Показывает диалог с ID элементов
        private void ShowElementsIdDialog(HashSet<ElementId> elementIds, Selection selection, Document doc)
        {
            string idsText = FormatElementIds(elementIds);

            using (var dialog = new ElementsIdDialog(idsText))
            {
                while (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Retry)
                {
                    // Пользователь нажал "Выбрать еще элементы"
                    var additionalElements = SelectElementsWithBox(selection, doc);
                    if (additionalElements != null && additionalElements.Any())
                    {
                        var newIds = additionalElements.Select(e => e.Id).ToList();

                        // Форматируем только новые ID и добавляем к существующему тексту
                        string additionalText = string.Join(" ", newIds.Select(id => $"ID: {id.IntegerValue}"));
                        dialog.AppendText(additionalText);
                    }
                }
            }
        }

        // Форматирует ID элементов в требуемый формат
        private string FormatElementIds(HashSet<ElementId> elementIds)
        {
            return string.Join(" ", elementIds.OrderBy(id => id.IntegerValue).Select(id => $"ID: {id.IntegerValue}"));
        }

        #endregion
    }
}
