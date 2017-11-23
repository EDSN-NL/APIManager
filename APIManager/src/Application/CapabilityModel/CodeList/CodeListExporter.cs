using Framework.Logging;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// Base class for a family of CodeList Exporter types. Only defined to act as abstract base class for future manipulations.
    /// </summary>
    internal abstract class CodeListExporter: CapabilityProcessor
    {
        protected CodeListExporter(): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListExporter >> Creating new exporter.");
        }
    }
}
