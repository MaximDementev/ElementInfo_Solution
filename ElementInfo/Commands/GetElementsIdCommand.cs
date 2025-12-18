using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MagicEntry.Core.Interfaces;
using MagicEntry.Core.Models;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.Services;
using MagicEntry.Plugins.ElementInfo.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetElementsIdCommand : IExternalCommand, IPlugin
    {
        private readonly IElementInfoService _elementInfoService;

        public GetElementsIdCommand()
        {
            _elementInfoService = new ElementInfoService();
        }

        #region IPlugin Implementation

        public PluginInfo Info { get; set; }
        public bool IsEnabled { get; set; }

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

        public void Shutdown()
        {
        }

        #endregion

        #region IExternalCommand Implementation

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;

                // Проверка: проект, не семейство, не шаблон
                if (doc.IsFamilyDocument)
                {
                    TaskDialog.Show("Ошибка", "Это семейство. Информацию можно получить только из файла проекта");
                    return Result.Failed;
                }

                // Получаем уже выбранные элементы
                var elementIds = new HashSet<ElementId>(_elementInfoService
                                     .GetPreSelectedElements(uidoc)
                                     .Select(e => e.Id));

                // Если нет предварительно выбранных элементов, запускаем выбор
                if (!elementIds.Any())
                {
                    var selected = SelectElementsWithBox(uidoc.Selection, doc);
                    if (selected != null)
                        foreach (var e in selected)
                            elementIds.Add(e.Id);
                }

                if (!elementIds.Any())
                {
                    TaskDialog.Show("Ошибка", Messages.NO_ELEMENTS_SELECTED);
                    return Result.Cancelled;
                }

                // Формируем текст
                string initialText = _elementInfoService.GetElementsIds(elementIds);

                var allIds = new HashSet<ElementId>(elementIds);
                string currentText = initialText;

                while (true)
                {
                    // Показываем модальное окно с текущим текстом
                    var window = new InfoDisplayWindow("ID элементов", showSelectMoreButton: true);
                    window.SetText(currentText);
                    bool? result = window.ShowDialog(); // модально

                    if (result != true) // пользователь закрыл окно без "Выбрать ещё"
                        break;

                    // Берём текущий текст из окна (редактированный пользователем)
                    currentText = window.GetText();

                    // Дополнительный выбор элементов
                    var additionalElements = SelectElementsWithBox(uidoc.Selection, doc);
                    if (additionalElements == null || !additionalElements.Any())
                        break;

                    // Добавляем новые ID
                    foreach (var e in additionalElements)
                        allIds.Add(e.Id);

                    // Объединяем текст пользователя и новые ID
                    string newIdsText = _elementInfoService.GetElementsIds(additionalElements.Select(e => e.Id));
                    currentText = currentText.TrimEnd() + "\n" + newIdsText;
                }

                // В конце allIds содержит все выбранные элементы, currentText — итоговый текст



                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Ошибка", $"Ошибка при выполнении команды: {ex.Message}");
                return Result.Failed;
            }
        }

        #endregion

        #region Private Methods

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


        #endregion
    }
}
