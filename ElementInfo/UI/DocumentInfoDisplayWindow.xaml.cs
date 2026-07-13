using Neuroptera.Plugins.ElementInfo.Models;
using Neuroptera.Plugins.ElementInfo.Utils;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Neuroptera.Plugins.ElementInfo.UI
{
    public partial class DocumentInfoDisplayWindow : Window
    {
        private List<DocumentInfoTable> _tables;

        public DocumentInfoDisplayWindow(string title)
        {
            InitializeComponent();
            Title = title;
        }

        public void SetTables(List<DocumentInfoTable> tables)
        {
            _tables = tables ?? new List<DocumentInfoTable>();
            TablesPanel.Children.Clear();

            foreach (var table in _tables)
            {
                if (table?.Headers == null || table.Headers.Count == 0)
                    continue;

                TablesPanel.Children.Add(new TextBlock
                {
                    Text = table.Title,
                    Style = (Style)FindResource("SectionTitle")
                });

                var dataGrid = CreateDataGrid(table);
                TablesPanel.Children.Add(dataGrid);
            }
        }

        public string GetText()
        {
            return DocumentInfoFormatter.FormatTablesAsText(_tables);
        }

        private DataGrid CreateDataGrid(DocumentInfoTable table)
        {
            var dataTable = new DataTable();

            foreach (var header in table.Headers)
                dataTable.Columns.Add(header);

            if (table.Rows != null)
            {
                foreach (var row in table.Rows)
                {
                    var values = new object[table.Headers.Count];
                    for (int i = 0; i < table.Headers.Count; i++)
                        values[i] = i < row.Count ? row[i] : string.Empty;

                    dataTable.Rows.Add(values);
                }
            }

            var dataGrid = new DataGrid
            {
                Style = (Style)FindResource("InfoDataGrid"),
                ItemsSource = dataTable.DefaultView
            };

            foreach (var header in table.Headers)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = header,
                    Binding = new System.Windows.Data.Binding($"[{header}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }

            return dataGrid;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetText());
            Close();
        }
    }
}
