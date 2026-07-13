using System.Reflection;
using Neuroptera.Contracts;
using Neuroptera.Revit.Contracts;
using Neuroptera.Revit.Contracts.Descriptors;
using Neuroptera.Revit.Contracts.PluginLogging;
using Neuroptera.Plugins.ElementInfo.Commands;
using Neuroptera.Plugins.ElementInfo.Constants;

namespace Neuroptera.Plugins.ElementInfo;

public sealed class ElementInfoPlugin : INeuropteraPlugin
{
    public PluginDefinition GetDefinition() =>
        new()
        {
            Commands = new[]
            {
                new PluginCommandDescriptor
                {
                    CommandKey = "GetMultipleElementsInfo",
                    CommandTypeName = typeof(GetMultipleElementsInfoCommand).FullName!
                },
                new PluginCommandDescriptor
                {
                    CommandKey = "GetElementsId",
                    CommandTypeName = typeof(GetElementsIdCommand).FullName!
                },
                new PluginCommandDescriptor
                {
                    CommandKey = "OpenViewByName",
                    CommandTypeName = typeof(OpenViewByNameCommand).FullName!
                },
                new PluginCommandDescriptor
                {
                    CommandKey = "SelectElementsById",
                    CommandTypeName = typeof(SelectElementsByIdCommand).FullName!
                },
                new PluginCommandDescriptor
                {
                    CommandKey = "GetDocumentInfo",
                    CommandTypeName = typeof(GetDocumentInfoCommand).FullName!
                }
            }
        };

    public void OnInitialized(IPluginContext context)
    {
        RevitPluginReporting.Configure(context, Assembly.GetExecutingAssembly());
        context.Logger.Info($"ElementInfo v{GetVersion()} загружен.");
    }

    public void OnShutdown(IPluginContext context)
    {
        context.Logger.Info("ElementInfo остановлен.");
        RevitPluginReporting.Unregister(context);
    }

    internal static string GetVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
}
