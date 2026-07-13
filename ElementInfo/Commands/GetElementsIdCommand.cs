using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Neuroptera.Contracts.PluginLogging;
using Neuroptera.Plugins.ElementInfo.Constants;
using Neuroptera.Plugins.ElementInfo.Services;
using Neuroptera.Plugins.ElementInfo.UI;
using Neuroptera.Revit.Contracts.PluginLogging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neuroptera.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetElementsIdCommand : IExternalCommand
    {
        private readonly IElementInfoService _elementInfoService;

        public GetElementsIdCommand()
        {
            _elementInfoService = new ElementInfoService();
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var journal = PluginOperationJournal.Start(
                ElementInfoOperations.PluginId,
                ElementInfoOperations.GetElementsId,
                doc.Title);

            try
            {
                journal.Step("Запуск команды получения ID элементов");

                var uidoc = commandData.Application.ActiveUIDocument;

                if (doc.IsFamilyDocument)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        "Это семейство. Информацию можно получить только из файла проекта.",
                        "Откройте файл проекта и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.GetElementsId,
                        doc);
                    return Result.Failed;
                }

                var elementIds = new HashSet<ElementId>(_elementInfoService
                    .GetPreSelectedElements(uidoc)
                    .Select(e => e.Id));

                if (!elementIds.Any())
                {
                    journal.Step("Выбор элементов пользователем");
                    var selected = SelectElementsWithBox(uidoc.Selection, doc);
                    if (selected != null)
                    {
                        foreach (var e in selected)
                        {
                            elementIds.Add(e.Id);
                        }
                    }
                }

                if (!elementIds.Any())
                {
                    RevitPluginErrorHandling.ShowValidation(
                        Messages.NO_ELEMENTS_SELECTED,
                        "Выберите элементы в модели и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.GetElementsId,
                        doc);
                    return Result.Cancelled;
                }

                journal.Step("Формирование списка ID", elementIds.Count.ToString());
                string initialText = _elementInfoService.GetElementsIds(elementIds);
                var allIds = new HashSet<ElementId>(elementIds);
                string currentText = initialText;

                while (true)
                {
                    journal.Step("Отображение окна со списком ID");
                    var window = new InfoDisplayWindow("ID элементов", showSelectMoreButton: true);
                    window.SetText(currentText);
                    bool? result = window.ShowDialog();

                    if (result != true)
                    {
                        break;
                    }

                    currentText = window.GetText();

                    var additionalElements = SelectElementsWithBox(uidoc.Selection, doc);
                    if (additionalElements == null || !additionalElements.Any())
                    {
                        break;
                    }

                    journal.Step("Добавлены дополнительные элементы", additionalElements.Count.ToString());
                    foreach (var e in additionalElements)
                    {
                        allIds.Add(e.Id);
                    }

                    string newIdsText = _elementInfoService.GetElementsIds(additionalElements.Select(e => e.Id));
                    currentText = currentText.TrimEnd() + "\n" + newIdsText;
                }

                journal.Complete($"Получены ID элементов: {allIds.Count}");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                RevitPluginErrorHandling.Handle(ex, journal);
                return Result.Failed;
            }
        }

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
    }
}
