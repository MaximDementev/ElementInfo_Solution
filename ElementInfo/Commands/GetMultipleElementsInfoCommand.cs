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
    public class GetMultipleElementsInfoCommand : IExternalCommand
    {
        private readonly IElementInfoService _elementInfoService;

        public GetMultipleElementsInfoCommand()
        {
            _elementInfoService = new ElementInfoService();
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var journal = PluginOperationJournal.Start(
                ElementInfoOperations.PluginId,
                ElementInfoOperations.GetMultipleElementsInfo,
                doc.Title);

            try
            {
                journal.Step("Запуск команды получения информации об элементах");

                var uidoc = commandData.Application.ActiveUIDocument;
                var currentView = uidoc.ActiveView;

                if (doc.IsFamilyDocument)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        "Это семейство. Информацию можно получить только из файла проекта.",
                        "Откройте файл проекта и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.GetMultipleElementsInfo,
                        doc);
                    return Result.Failed;
                }

                journal.Step("Сбор общей информации о документе");
                string generalInfo = _elementInfoService.GetGeneralDocumentInfo(
                    doc,
                    currentView,
                    commandData.Application.Application.Username);

                var selectedElements = _elementInfoService.GetPreSelectedElements(uidoc);
                if (!selectedElements.Any())
                {
                    journal.Step("Выбор элементов пользователем");
                    selectedElements = SelectMultipleElements(uidoc.Selection, doc);
                }
                else
                {
                    journal.Step("Использованы предварительно выбранные элементы", selectedElements.Count.ToString());
                }

                string fullInfo;
                if (selectedElements == null || !selectedElements.Any())
                {
                    fullInfo = generalInfo + "\nЭлементы не выбраны";
                    journal.Step("Элементы не выбраны");
                }
                else
                {
                    journal.Step("Формирование информации об элементах", selectedElements.Count.ToString());
                    string elementsInfo = _elementInfoService.GetElementsFullInfo(selectedElements, doc);
                    fullInfo = generalInfo + elementsInfo;
                }

                journal.Step("Отображение окна с информацией");
                var window = new InfoDisplayWindow("Информация об элементах");
                window.SetText(fullInfo);
                window.ShowDialog();

                journal.Complete("Информация об элементах показана");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                RevitPluginErrorHandling.Handle(ex, journal);
                return Result.Failed;
            }
        }

        private List<Element> SelectMultipleElements(Selection selection, Document doc)
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
