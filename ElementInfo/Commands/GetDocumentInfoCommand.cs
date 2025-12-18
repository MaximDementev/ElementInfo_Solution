using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MagicEntry.Core.Interfaces;
using MagicEntry.Core.Models;
using MagicEntry.Plugins.ElementInfo.Services;
using MagicEntry.Plugins.ElementInfo.UI;
using System;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    // Команда для отображения информации о текущем документе
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetDocumentInfoCommand : IExternalCommand, IPlugin
    {
        private readonly IElementInfoService _elementInfoService;

        public GetDocumentInfoCommand()
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
                var currentView = uidoc.ActiveView;

                // Проверка: проект, не семейство, не шаблон
                if (doc.IsFamilyDocument)
                {
                    TaskDialog.Show("Ошибка", "Это семейство. Информацию можно получить только из файла проекта");
                    return Result.Failed;
                }

                // Получаем общую информацию о документе
                string documenExtratInfo = _elementInfoService.GetGeneralDocumentInfo(doc, currentView, commandData.Application.Application.Username);
                documenExtratInfo += _elementInfoService.GetExtraDocumentInfo(doc, currentView, commandData.Application.Application.Username);

                // Показываем WPF окно с информацией
                var window = new InfoDisplayWindow("Информация о документе");
                window.SetText(documenExtratInfo);
                window.ShowDialog();

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
    }
}
