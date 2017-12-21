using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// A collection of utility functions used by the REST components.
    /// </summary>
    internal class RESTUtil
    {
        /// <summary>
        /// Parses a resource name and breaks it into lowercase words, separated by "-". It assumes that the name is in PascalCase.
        /// If the resource name starts with a '[', we consider it a keyword, in which case the name is not translated at all. We also do
        /// not check the remainder of the name!
        /// </summary>
        /// <param name="resourceName">Name to be translated.</param>
        /// <returns>Assigned role name in proper 'REST format'.</returns>
        internal static string GetAssignedRoleName(string resourceName)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTUtil.GetAssignedRoleName >> Parsing resource '" + resourceName + "'...");

            // Resource names between angle brackets are considered 'keywords' and are not translated.
            if (resourceName[0] == '[') return resourceName;
            if (resourceName[0] == '{')
            {
                // Identifier type, extract the first part, and translate as expected (but leave the '{' and '}')...
                return "{" + GetAssignedRoleName(resourceName.Substring(1, resourceName.IndexOf('(') - 1)) + "}";
            }

            string assignedRole = resourceName[0].ToString().ToLower();
            for (int i = 1; i < resourceName.Length; i++)
            {
                if (char.IsUpper(resourceName[i]))
                {
                    if (i < resourceName.Length - 1 && char.IsUpper(resourceName[i + 1]))
                    {
                        // A series of upper-case characters are considered an abbreviation and will be treated as a single word.
                        // And if the previous char was lowercase, we must insert a '-' inbetween...
                        if (char.IsLower(resourceName[i-1])) assignedRole += "-";
                        while (i < resourceName.Length && char.IsUpper(resourceName[i]))
                        {
                            // An upper-case abbreviation can be followed by a PascalCase word, in that case, we stop copying
                            // when the next letter is lowercase and treat the last uppercase letter as the start of the next word.
                            if (i < resourceName.Length - 1 && char.IsLower(resourceName[i + 1])) assignedRole += "-";
                            assignedRole += char.ToLower(resourceName[i++]);
                        }
                        if (i < resourceName.Length && char.IsLower(resourceName[i])) assignedRole += resourceName[i];
                    }
                    else
                    {
                        assignedRole += "-";
                        assignedRole += char.ToLower(resourceName[i]);
                    }
                }
                else assignedRole += resourceName[i];
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTUtil.GetAssignedRoleName >> Returning: '" + assignedRole + "'.");
            return assignedRole;
        }

        /// <summary>
        /// Utility function that retrieves the list of REST Header parameters from Configuration.
        /// </summary>
        /// <returns>Header Parameter declarations or empty list in case of no definitions (or errors).</returns>
        internal static List<RESTParameterDeclaration> GetHeaderParameters()
        {
            string parameterEncoding = ContextSlt.GetContextSlt().GetStringSetting(FrameworkSettings._RESTHeaderParameters);
            var parameterList = new List<RESTParameterDeclaration>();
            while (parameterEncoding != string.Empty)
            {
                if (parameterEncoding[0] == '{')
                {
                    int endOfParameter = parameterEncoding.IndexOf("}");
                    string singleParameter = parameterEncoding.Substring(1, endOfParameter - 1);
                    parameterEncoding = (endOfParameter + 1 < parameterEncoding.Length) ? parameterEncoding.Substring(endOfParameter + 1) : string.Empty;
                    RESTParameterDeclaration param = RESTParameterDeclaration.Deserialize(singleParameter);
                    parameterList.Add(param);
                }
                else
                {
                    Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RESTUtil.GetHeaderParameters >> Parameter configuration appears to be corrupt, no result!");
                    break;
                }
            }
            return parameterList;
        }

        /// <summary>
        /// Utility function that writes a list of REST Header Parameters to Configuration. This list will replace any list that is already stored.
        /// Each parameter will be stored in the configuration as a Base64-encoded string setting, enclosed in '{ }'...
        /// </summary>
        /// <param name="parameterList">List of REST Header Parameters to be stored.</param>
        internal static void SetHeaderParameters(List<RESTParameterDeclaration> parameterList)
        {
            string parameterEncoding = string.Empty;
            foreach (RESTParameterDeclaration param in parameterList)
            {
                parameterEncoding += "{" + RESTParameterDeclaration.Serialize(param) + "}";
            }
            ContextSlt.GetContextSlt().SetStringSetting(FrameworkSettings._RESTHeaderParameters, parameterEncoding);
        }
    }
}
