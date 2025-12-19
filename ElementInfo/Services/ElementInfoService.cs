using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MagicEntry.Plugins.ElementInfo.Constants;
using MagicEntry.Plugins.ElementInfo.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEntry.Plugins.ElementInfo.Services
{
    // Сервис для получения информации об элементах и документе
    public class ElementInfoService : IElementInfoService
    {
        // Получает общую информацию о документе
        public string GetGeneralDocumentInfo(Document doc, View currentView, string userName)
        {
            var lines = new List<string>();

            string docPath = GetDocumentPath(doc);
            if (!string.IsNullOrEmpty(docPath))
                lines.Add($"Путь: {docPath}");

            string fileName = GetCleanFileName(doc.Title, userName);
            if (!string.IsNullOrEmpty(fileName))
                lines.Insert(0, $"Документ: {fileName}"); // заголовок в начало

            if (!string.IsNullOrEmpty(userName))
                lines.Add($"Пользователь: {userName}");

            if (currentView != null)
                lines.Add($"Активный вид: {currentView.Name}");

            string revitVersion = doc.Application.VersionName;
            if (!string.IsNullOrEmpty(revitVersion))
                lines.Add($"Версия Revit: {revitVersion}");

            if (!lines.Any())
                return ""; // ничего не добавляем, если нет данных

            return string.Join("\n", lines) + "\n\n";
        }


        //Получение дополнительной информации о текущем документе
        public string GetExtraDocumentInfo(Document doc, View currentView, string userName)
        {
            string result = "";

            string siteInfo = GetProjectBasePointInfo(doc);
            if (!string.IsNullOrEmpty(siteInfo))
                result += siteInfo + "\n";

            string gridsInfo = GetGridsInfo(doc);
            if (!string.IsNullOrEmpty(gridsInfo))
                result += gridsInfo + "\n";

            string levelsInfo = GetLevelsInfo(doc);
            if (!string.IsNullOrEmpty(levelsInfo))
                result += levelsInfo + "\n";

            string linksInfo = GetLinksInfo(doc);
            if (!string.IsNullOrEmpty(linksInfo))
                result += linksInfo + "\n";

            string importsInfo = GetImportsInfo(doc);
            if (!string.IsNullOrEmpty(importsInfo))
                result += importsInfo + "\n";

            return result.TrimEnd();
        }

        // --- Приватные методы ---
        private string GetProjectBasePointInfo(Document doc)
        {
            var basePoints = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>();

            BasePoint projectBasePoint = basePoints.FirstOrDefault(b => !b.IsShared);
            if (projectBasePoint == null) return null;

            XYZ position = projectBasePoint.Position;
            XYZ sharedCoords = projectBasePoint.get_BoundingBox(null)?.Min ?? position;

            return $"▶ Площадка:\n{projectBasePoint.Name} (X: {position.X:F2}, Y: {position.Y:F2}, Z: {position.Z:F2})\n" +
                   $"(Общие координаты относительной точки модели: X={sharedCoords.X:F2}, Y={sharedCoords.Y:F2}, Z={sharedCoords.Z:F2})";
        }

        private string GetGridsInfo(Document doc)
        {
            var worksetTable = doc.GetWorksetTable();
            var grids = new FilteredElementCollector(doc).OfClass(typeof(Grid)).Cast<Grid>().ToList();
            if (!grids.Any()) return null;

            var gridGroups = grids.GroupBy(g => worksetTable.GetWorkset(g.WorksetId)?.Name ?? "<Не задан>");
            string result = "\n▶ Оси по рабочим наборам:\n";

            foreach (var g in gridGroups)
            {
                int pinned = g.Count(x => x.Pinned);
                int unpinned = g.Count(x => !x.Pinned);
                int total = g.Count();
                result += $"  {g.Key}: {total} элементов (Закрепленные: {pinned}, Не закрепленные: {unpinned})\n";
            }
            return result.TrimEnd();
        }

        private string GetLevelsInfo(Document doc)
        {
            var worksetTable = doc.GetWorksetTable();
            var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (!levels.Any()) return null;

            var levelGroups = levels.GroupBy(l => worksetTable.GetWorkset(l.WorksetId)?.Name ?? "<Не задан>");
            string result = "\n▶ Уровни по рабочим наборам:\n";

            foreach (var g in levelGroups)
            {
                int pinned = g.Count(x => x.Pinned);
                int unpinned = g.Count(x => !x.Pinned);
                int total = g.Count();
                result += $"  {g.Key}: {total} элементов (Закрепленные: {pinned}, Не закрепленные: {unpinned})\n";
            }
            return result.TrimEnd();
        }

        private string GetLinksInfo(Document doc)
        {
            var worksetTable = doc.GetWorksetTable();
            string result = "\n▶ Связи:";

            var linkTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .ToList();

            foreach (var linkType in linkTypes)
            {
                var instances = linkType
                    .GetDependentElements(new ElementClassFilter(typeof(RevitLinkInstance)))
                    .Select(id => doc.GetElement(id))
                    .Cast<RevitLinkInstance>()
                    .ToList();

                if (!instances.Any())
                    continue;

                // --- ДАННЫЕ ТИПА ---
                string attachment =
                    linkType.AttachmentType == AttachmentType.Attachment
                        ? "Прикрепление"
                        : "Наложение";

                string linkFullPath = "<Не загружена>";
                var extRef = linkType.GetExternalFileReference();

                if (extRef != null)
                {
                    var modelPath = extRef.GetAbsolutePath();
                    linkFullPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                }
                result += "\n";
                result += $@"  {linkType.Name}
   Путь: {linkFullPath}
   Тип: {attachment}
";

                // --- ЭКЗЕМПЛЯРЫ ---
                foreach (var link in instances)
                {
                    string instanceName = link.Name;

                    string prefix = linkType.Name + " :";

                    if (!string.IsNullOrEmpty(instanceName) && instanceName.StartsWith(prefix))
                    {
                        instanceName = instanceName.Substring(prefix.Length).Trim();
                    }


                    string pinned = link.Pinned ? "Закреплен" : "Не закреплен";
                    string wsName = worksetTable.GetWorkset(link.WorksetId)?.Name ?? "<Не задан>";

                    result +=
                        $@"    • Экземпляр ''{instanceName}''
      {pinned}
      Рабочий набор: {wsName}";
                }

                result += "\n";

            }

            return result.TrimEnd();
        }


        private string GetImportsInfo(Document doc)
        {
            var imports = new FilteredElementCollector(doc).OfClass(typeof(ImportInstance)).Cast<ImportInstance>();
            if (!imports.Any()) return null;

            string result = "▶ Импорты (DWG / изображения / PDF):\n";
            foreach (var imp in imports)
            {
                string typeStr = imp.Category?.Name ?? "Импорт";
                string fileNameImp = imp.Name;
                string pathImp = "<не найден>";

                var typeElem = doc.GetElement(imp.GetTypeId());
                var param = typeElem?.LookupParameter("Имя файла");
                if (param != null)
                    pathImp = param.AsString() ?? "<не найден>";

                result += $"  {typeStr}: {fileNameImp}\n    Путь: {pathImp}\n";
            }
            return result.TrimEnd();
        }

        // Получает путь к документу с учетом центральной модели
        public string GetDocumentPath(Document doc)
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

        // Получает предварительно выбранные элементы
        public List<Element> GetPreSelectedElements(UIDocument uidoc)
        {
            try
            {
                var selectedIds = uidoc.Selection.GetElementIds();
                return selectedIds.Select(id => uidoc.Document.GetElement(id)).Where(e => e != null).ToList();
            }
            catch
            {
                return new List<Element>();
            }
        }

        // Получает полную информацию об элементах
        public string GetElementsFullInfo(List<Element> elements, Document doc)
        {
            return DocumentInfoFormatter.FormatElementsInfo(elements, doc);
        }

        // Получает ID элементов в формате "ID: xxx ID: yyy"
        public string GetElementsIds(IEnumerable<ElementId> elementIds)
        {
            return string.Join(" ", elementIds.OrderBy(id => id.IntegerValue).Select(id => $"ID: {id.IntegerValue}"));
        }


        #region Private methods
        // Очищает имя файла от имени пользователя
        private string GetCleanFileName(string docTitle, string userName)
        {
            return docTitle.Replace($"_{userName}", "");
        }

        ////Базовая информация о документе
        //private BaseDocumentInfo GetBaseDocumentInfo(
        //    Document doc,    View currentView,    string userName)
        //{
        //    string docPath = GetDocumentPath(doc);
        //    string fileName = GetCleanFileName(doc.Title, userName);
        //    string viewName = currentView?.Name ?? "<Нет активного вида>";

        //    return new BaseDocumentInfo(
        //        docPath,
        //        fileName,
        //        userName,
        //        viewName);
        //}

        //private IEnumerable<RevitLinkInfo> GetRevitLinksInfo(Document doc)
        //{
        //    var worksetTable = doc.GetWorksetTable();

        //    var links = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RevitLinkInstance))
        //        .Cast<RevitLinkInstance>();

        //    foreach (var link in links)
        //    {
        //        var linkType = doc.GetElement(link.GetTypeId()) as RevitLinkType;
        //        var linkDoc = link.GetLinkDocument();

        //        yield return new RevitLinkInfo
        //        {
        //            FileName = linkDoc?.Title ?? "<Не загружена>",
        //            FullPath = linkDoc?.PathName ?? "<Не загружена>",
        //            AttachmentType = linkType?.AttachmentType == AttachmentType.Attachment
        //                ? "Прикрепление"
        //                : "Наложение",
        //            IsPinned = link.Pinned ? "Закреплен" : "Не закреплен",
        //            WorksetName = worksetTable
        //                .GetWorkset(link.WorksetId)
        //                ?.Name
        //        };
        //    }
        //}


        //private GridsAndLevelsInfo GetGridsAndLevelsInfo(Document doc)
        //{
        //    var worksetTable = doc.GetWorksetTable();

        //    var grids = new FilteredElementCollector(doc)
        //        .OfClass(typeof(Grid))
        //        .Cast<Grid>()
        //        .ToList();

        //    var levels = new FilteredElementCollector(doc)
        //        .OfClass(typeof(Level))
        //        .Cast<Level>()
        //        .ToList();

        //    return new GridsAndLevelsInfo
        //    {
        //        PinnedGridsCount = grids.Count(g => g.Pinned),
        //        UnpinnedGridsCount = grids.Count(g => !g.Pinned),

        //        PinnedLevelsCount = levels.Count(l => l.Pinned),
        //        UnpinnedLevelsCount = levels.Count(l => !l.Pinned),

        //        GridWorksets = grids
        //            .Select(g => worksetTable.GetWorkset(g.WorksetId)?.Name)
        //            .Where(n => !string.IsNullOrEmpty(n))
        //            .Distinct(),

        //        LevelWorksets = levels
        //            .Select(l => worksetTable.GetWorkset(l.WorksetId)?.Name)
        //            .Where(n => !string.IsNullOrEmpty(n))
        //            .Distinct()
        //    };
        //}


        //private IEnumerable<ImportInfo> GetImportsInfo(Document doc)
        //{
        //    var imports = new FilteredElementCollector(doc)
        //        .OfClass(typeof(ImportInstance))
        //        .Cast<ImportInstance>();

        //    foreach (var import in imports)
        //    {
        //        Element type = doc.GetElement(import.GetTypeId());
        //        string path = GetImportPath(type);

        //        yield return new ImportInfo
        //        {
        //            ImportType = GetImportType(import),
        //            FileName = import.Name,
        //            FullPath = path
        //        };
        //    }
        //}


        //private string GetImportType(ImportInstance import)
        //{
        //    if (import.Category?.Id.IntegerValue ==
        //        (int)BuiltInCategory.OST_RasterImages)
        //        return "Изображение / PDF";

        //    return import.IsLinked
        //        ? "DWG (Связь)"
        //        : "DWG (Импорт)";
        //}

        //private string GetImportPath(Element importType)
        //{
        //    var param = importType?
        //        .get_Parameter(BuiltInParameter.IMPORT_SYMBOL_FILENAME);

        //    return param?.AsString() ?? "<Не найден>";
        //}

        #endregion
    }
}
