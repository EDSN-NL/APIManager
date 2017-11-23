using System;
using System.Collections.Generic;
using Framework.Exceptions;

namespace Framework.Util
{
    internal static class Conversions
    {
        /// <summary>
        /// Simple method that translates the received string to camelCase (e.g. first character is lower case).
        /// </summary>
        /// <param name="inputString">String to be converted.</param>
        /// <returns>String with first character translated to lower case.</returns>
        internal static string ToCamelCase(string inputString)
        {
            if (!string.IsNullOrEmpty(inputString))
            {
                string result = char.ToLower(inputString[0]).ToString();
                result += inputString.Substring(1);
                return result;
            }
            else return string.Empty;
        }

        /// <summary>
        /// Simple method that translates the received string to PascalCase (e.g. first character is upper case).
        /// </summary>
        /// <param name="inputString">String to be converted.</param>
        /// <returns>String with first character translated to upper case.</returns>
        internal static string ToPascalCase(string inputString)
        {
            if (!string.IsNullOrEmpty(inputString))
            {
                string result = char.ToUpper(inputString[0]).ToString();
                result += inputString.Substring(1);
                return result;
            }
            else return string.Empty;
        }
    }

    /// <summary>
    /// Helper class that facilitates simple conversion of enumerated type values from- and to string representation.
    /// </summary>
    /// <typeparam name="TEnum">Enumerated type associated with this conversion.</typeparam>
    internal static class EnumConversions<TEnum>
    {

        /// <summary>
        /// Returns the string representation of the specified enumerated type value.
        /// </summary>
        /// <param name="enumValue">Enumerated-type value to translate.</param>
        /// <returns>String representation of enumerated type value.</returns>
        internal static string EnumToString(TEnum enumValue)
        {
            return enumValue.ToString();
        }

        /// <summary>
        /// Converts the list of enumerated-type values to a string list.
        /// </summary>
        /// <returns>List of enumerated-type values as list of strings.</returns>
        internal static List<string> GetNamesList()
        {
            List<string> enumNames = new List<string>(Enum.GetNames(typeof(TEnum)));
            return enumNames;
        }

        /// <summary>
        /// Converts the list of enumerated-type values to an array of strings.
        /// </summary>
        /// <returns>List of enumerated-type values as an array of strings.</returns>
        internal static string[] GetNamesArray()
        {
            return Enum.GetNames(typeof(TEnum));
        }

        /// <summary>
        /// Convert the specified string to the enumeration type associated with this template class. If the conversion fails, an exception is thrown.
        /// </summary>
        /// <param name="enumValue">Case-insensitive string value to convert.</param>
        /// <returns>Enumerated type value corresponding with the specified string.</returns>
        /// <exception cref="IllegalEnumException">String value is no match for the specified enumeration.</exception>
        internal static TEnum StringToEnum(string enumValue)
        {
            try { return (TEnum)Enum.Parse(typeof(TEnum), enumValue, true); }
            catch
            {
                throw new IllegalEnumException("Enumerated value '" + enumValue + "' is illegal for enumeration '" + typeof(TEnum) + "'!");
            }
        }
    }
}
