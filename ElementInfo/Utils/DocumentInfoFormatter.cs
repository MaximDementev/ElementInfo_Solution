using Autodesk.Revit.DB;
using Neuroptera.Plugins.ElementInfo.Constants;
using Neuroptera.Plugins.ElementInfo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroptera.Plugins.ElementInfo.Utils
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
                sb.AppendLine(string.Format($"{i + 1}", Messages.ELEMENT_SEPARATOR));

                string worksetName = GetWorksetName(element, doc);
                sb.AppendLine(string.Format(Messages.WORKSET_TEMPLATE, worksetName));
                sb.AppendLine(string.Format(Messages.NAME_TEMPLATE, element.Name ?? "Без имени"));
                sb.AppendLine(string.Format(Messages.ID_TEMPLATE, element.Id.IntegerValue));
            }

            return sb.ToString();
        }

        // Форматирует таблицы в текст для буфера обмена
        public static string FormatTablesAsText(IEnumerable<DocumentInfoTable> tables)
        {
            if (tables == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var table in tables)
            {
                if (table == null || table.Headers == null || table.Headers.Count == 0)
                    continue;

                if (!string.IsNullOrEmpty(table.Title))
                {
                    sb.AppendLine(table.Title);
                }

                sb.AppendLine(string.Join("\t", table.Headers));

                if (table.Rows != null)
                {
                    foreach (var row in table.Rows)
                    {
                        sb.AppendLine(string.Join("\t", row));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
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
