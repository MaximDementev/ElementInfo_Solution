using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MagicEntry.Plugins.ElementInfo.Utils
{
    // Утилитарный класс для парсинга текста и извлечения информации
    public static class TextParser
    {
        #region Regex Patterns
        private static readonly Regex IdPattern = new Regex(@"ID:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ViewPattern = new Regex(@"Активный вид:\s*(.+?)(?:\n|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        #endregion

        #region Public Methods

        // Извлекает все ID элементов из текста
        public static List<int> ExtractElementIds(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<int>();

            var matches = IdPattern.Matches(text);
            var ids = new List<int>();

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int id))
                {
                    ids.Add(id);
                }
            }

            return ids.Distinct().ToList();
        }

        // Извлекает имя активного вида из текста
        public static string ExtractViewName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = ViewPattern.Match(text);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        #endregion
    }
}
