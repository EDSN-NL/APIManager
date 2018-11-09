using System;
using Framework.Logging;

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
        /// If the name is entirely in uppercase, we translate it to full lower case without changes.
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

            if (IsAllUpper(resourceName)) return resourceName.ToLower();

            string assignedRole = resourceName[0].ToString().ToLower(); // This will receive the translated name.
            for (int i = 1; i < resourceName.Length; i++)
            {
                if (char.IsUpper(resourceName[i]))
                {
                    if (i < resourceName.Length - 1 && char.IsUpper(resourceName[i + 1]))
                    {
                        // A series of upper-case characters are considered an abbreviation and will be treated as a single word.
                        // And if the previous char was lowercase, we must insert a '-' inbetween...
                        if (!char.IsUpper(resourceName[i-1])) assignedRole += "-";
                        while (i < resourceName.Length && char.IsUpper(resourceName[i]))
                        {
                            // An upper-case abbreviation can be followed by a PascalCase word, in that case, we stop copying
                            // when the next letter is lowercase and treat the last uppercase letter as the start of the next word.
                            // Exception: when the next character is not a letter, this should count as the start of the next word,
                            // e.g. "EAN18" should become "ean-18" and NOT "ea-n18"...
                            if (i < resourceName.Length - 1 && char.IsLower(resourceName[i + 1])) assignedRole += "-";
                            assignedRole += char.ToLower(resourceName[i++]);
                        }
                        if (i < resourceName.Length && !char.IsLetter(resourceName[i])) assignedRole += "-";
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
        /// Simple helper function that checks whether a given string is all uppercase. 
        /// It returns false when detecting the first non-uppercase character, so it quits on non-alphabetic characters.
        /// </summary>
        /// <param name="input">String to check.</param>
        /// <returns>True when string consist entirely of uppercase characters, false when not.</returns>
        private static bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++) if (!Char.IsUpper(input[i])) return false;
            return true;
        }
    }
}
