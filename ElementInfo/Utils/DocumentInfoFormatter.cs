using Autodesk.Revit.DB;
using MagicEntry.Plugins.ElementInfo.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    // Утилитарный класс для форматирования информации о документе и элементах
    public static class DocumentInfoFormatter
    {
        #region Public Methods

        // Форматирует общую информацию о документе
        public static string FormatGeneralInfo(string docPath, string fileName, string userName, string viewName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(Messages.LOCATION_TEMPLATE, docPath));
            sb.AppendLine(string.Format(Messages.CURRENT_FILE_TEMPLATE, fileName));
            sb.AppendLine(string.Format(Messages.USER_TEMPLATE, userName));
            sb.AppendLine(string.Format(Messages.ACTIVE_VIEW_TEMPLATE, viewName));
            sb.AppendLine();

            return sb.ToString();
        }

        // Форматирует информацию о списке элементов
        public static string FormatElementsInfo(List<Element> elements, Document doc)
        {
            if (elements == null || !elements.Any())
                return string.Empty;

            var sb = new StringBuilder();

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                sb.AppendLine(string.Format($"{ i + 1}", Messages.ELEMENT_SEPARATOR));

                string worksetName = GetWorksetName(element, doc);
                sb.AppendLine(string.Format(Messages.WORKSET_TEMPLATE, worksetName));
                sb.AppendLine(string.Format(Messages.NAME_TEMPLATE, element.Name ?? "Без имени"));
                sb.AppendLine(string.Format(Messages.ID_TEMPLATE, element.Id.IntegerValue));
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        // Получает имя рабочего набора элемента
        private static string GetWorksetName(Element element, Document doc)
        {
            try
            {
                int worksetId = element.WorksetId.IntegerValue;
                if (worksetId > 0)
                {
                    var workset = doc.GetWorksetTable().GetWorkset(new WorksetId(worksetId));
                    return workset?.Name ?? "Неизвестный";
                }
                return "Без рабочего набора";
            }
            catch
            {
                return "Неизвестный";
            }
        }

        #endregion
    }
}
