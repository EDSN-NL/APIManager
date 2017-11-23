using System.Web.Security.AntiXss;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// This is an extremely simple class for encoding an ASCII string to an XML compatible format. Its a wrapper around the .Net
    /// AntiXSS component, which does all the work for us. By wrapping it in a separate class, we have the freedom to change the
    /// implementation in the future if need be. Also, it fits in the framework a bit better this way.
    /// </summary>
    internal class XmlTextEncoder
    {
        /// <summary>
        /// Encodes an input string to an 'XML-compliant' version by converting illegal characters to the appropriate escape
        /// sequences.
        /// </summary>
        /// <param name="input">Text to be encoded.</param>
        /// <returns>XML-compliant version of input.</returns>
        internal static string Encode(string input)
        {
            return AntiXssEncoder.XmlEncode(input);
        }
    }
}
