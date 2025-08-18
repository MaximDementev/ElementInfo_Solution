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
using System.Runtime.Remoting.Messaging;
using System.Xml.Linq;

namespace MagicEntry.Plugins.ElementInfo.Commands
{
    // Команда для сбора информации о всех выбранных элементах
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetMultipleElementsInfoCommand : IExternalCommand, IPlugin
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

        // Выполнение команды сбора информации о множественных элементах
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument.Document;
                var currentView = commandData.Application.ActiveUIDocument.ActiveView;
                var selection = commandData.Application.ActiveUIDocument.Selection;

                // Получаем общую информацию о документе
                string generalInfo = GetGeneralDocumentInfo(doc, currentView, commandData.Application.Application.Username);

                // Выбираем элементы
                var selectedElements = SelectMultipleElements(selection, doc);

                if (selectedElements == null || !selectedElements.Any())
                {
                    // Если элементы не выбраны, показываем только общую информацию (полезно для спецификаций)
                    string infoWithoutElements = generalInfo;
                    DialogHelper.ShowInfoDialogWithCopy(infoWithoutElements);
                    return Result.Succeeded;
                }

                // Форматируем полную информацию
                string elementsInfo = DocumentInfoFormatter.FormatElementsInfo(selectedElements, doc);
                string fullInfo = generalInfo + elementsInfo;

                // Показываем диалог с возможностью копирования
                DialogHelper.ShowInfoDialogWithCopy(fullInfo);

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

        // Получает общую информацию о документе
        private string GetGeneralDocumentInfo(Document doc, View currentView, string userName)
        {
            string docPath = GetDocumentPath(doc);
            string fileName = GetCleanFileName(doc.Title, userName);
            string viewName = currentView.Name;

            return DocumentInfoFormatter.FormatGeneralInfo(docPath, fileName, userName, viewName);
        }

        // Получает путь к документу с учетом центральной модели
        private string GetDocumentPath(Document doc)
        {
            try
            {
                string docPath = doc.PathName;

                if (doc.GetWorksharingCentralModelPath().ServerPath)
                {
                    ModelPath centralPath = doc.GetWorksharingCentralModelPath();
                    docPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralPath);
                }

                return docPath.Replace($"{doc.Title}.rvt", "");
            }
            catch
            {
                return doc.PathName;
            }
        }

        // Очищает имя файла от имени пользователя
        private string GetCleanFileName(string docTitle, string userName)
        {
            return docTitle.Replace($"_{userName}", "");
        }

        // Выбирает множественные элементы
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

        #endregion
    }
}
