using System.Collections.Generic;

namespace Neuroptera.Plugins.ElementInfo.Models
{
    public class DocumentInfoTable
    {
        public string Title { get; set; }
        public IReadOnlyList<string> Headers { get; set; }
        public IReadOnlyList<IReadOnlyList<string>> Rows { get; set; }
    }
}
