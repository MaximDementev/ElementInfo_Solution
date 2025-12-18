using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace MagicEntry.Plugins.ElementInfo.Services
{
    // Интерфейс сервиса для получения информации об элементах
    public interface IElementInfoService
    {
        // Получает общую информацию о документе
        string GetGeneralDocumentInfo(Document doc, View currentView, string userName);
        
        //Получение дополнительной информации о текущем документе
        string GetExtraDocumentInfo(Document doc, View currentView, string userName);
        
        // Получает путь к документу
        string GetDocumentPath(Document doc);

        // Получает предварительно выбранные элементы
        List<Element> GetPreSelectedElements(UIDocument uidoc);

        // Получает информацию об элементах в формате "полная информация"
        string GetElementsFullInfo(List<Element> elements, Document doc);

        // Получает информацию об элементах в формате "только ID"
        string GetElementsIds(IEnumerable<ElementId> elementIds);
    }
}
