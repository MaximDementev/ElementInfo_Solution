using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Neuroptera.Contracts.PluginLogging;
using Neuroptera.Plugins.ElementInfo.Constants;
using Neuroptera.Plugins.ElementInfo.Services;
using Neuroptera.Plugins.ElementInfo.UI;
using Neuroptera.Revit.Contracts.PluginLogging;
using System;

namespace Neuroptera.Plugins.ElementInfo.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetDocumentInfoCommand : IExternalCommand
    {
        private readonly IElementInfoService _elementInfoService;

        public GetDocumentInfoCommand()
        {
            _elementInfoService = new ElementInfoService();
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var journal = PluginOperationJournal.Start(
                ElementInfoOperations.PluginId,
                ElementInfoOperations.GetDocumentInfo,
                doc.Title);

            try
            {
                journal.Step("Запуск команды получения информации о документе");

                var uidoc = commandData.Application.ActiveUIDocument;
                var currentView = uidoc.ActiveView;

                if (doc.IsFamilyDocument)
                {
                    RevitPluginErrorHandling.ShowValidation(
                        "Это семейство. Информацию можно получить только из файла проекта.",
                        "Откройте файл проекта и повторите команду.",
                        ElementInfoOperations.PluginId,
                        ElementInfoOperations.GetDocumentInfo,
                        doc);
                    return Result.Failed;
                }

                journal.Step("Сбор информации о документе");
                var tables = _elementInfoService.GetDocumentInfoTables(
                    doc,
                    currentView,
                    commandData.Application.Application.Username);

                journal.Step("Отображение окна с информацией");
                var window = new DocumentInfoDisplayWindow("Информация о документе");
                window.SetTables(tables);
                window.ShowDialog();

                journal.Complete("Информация о документе показана");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                RevitPluginErrorHandling.Handle(ex, journal);
                return Result.Failed;
            }
        }
    }
}
