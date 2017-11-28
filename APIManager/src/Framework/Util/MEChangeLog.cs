using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Framework.Logging;
using Framework.Model;
using Framework.Context;

namespace Framework.Util
{
    /// <summary>
    /// The MEChangeLog class is used to create and maintain a log of change notes for a modelling element. The log is maintained in RTF format.
    /// </summary>
    internal class MEChangeLog
    {
        // We use this header to create valid RTF-formatted Documentation sections.
        private const string _DocHeader =
@"{\rtf1\ansi\deflang3081\ftnbj\uc1\deff0
{\fonttbl{\f0 \fswiss Arial;}{\f1 \fnil Century Gothic;}{\f2 \fmodern Courier New;}}
{\colortbl\red0\green0\blue0 ;\red255\green255\blue255 ;\red0\green0\blue0 ;}
{\stylesheet{\f0\fs24 Normal;}{\cs1 Default Paragraph Font;}}{\*\revtbl{Unknown;}}";

        // We use this header to communicate with our repository.
        private const string _LogHeader =
@"{\rtf1\ansi\deflang3081\ftnbj\uc1\deff0
{\fonttbl{\f0 \fswiss Arial;}{\f1 \fnil Century Gothic;}{\f2 \fmodern Courier New;}}
{\colortbl\red0\green0\blue0 ;\red255\green255\blue255 ;\red0\green0\blue0 ;}
{\stylesheet{\f0\fs24 Normal;}{\cs1 Default Paragraph Font;}}{\*\revtbl{Unknown;}}
\paperw12240\paperh15840\margl1800\margr1800\margt1440\margb1440\headery720\footery720\htmautsp1\nogrowautofit\deftab720\formshade\nofeaturethrottle1\dntblnsbdb\fet4\aendnotes\aftnnrlc\pgbrdrhead\pgbrdrfoot 
\sectd\pgwsxn12240\pghsxn15840\guttersxn0\marglsxn1800\margrsxn1800\margtsxn1440\margbsxn1440\headery720\footery720\sbkpage\pgncont\pgndec 
\plain\plain\f0\fs24\sa1\ql\plain\f0\fs24\hich\f0\dbch\f0\loch\f0\fs24\b Documentation:\plain\f0\fs24\par";

        // We use this header to attach documentation- and change-log sections.
        private const string _AuditHeader = 
@"\plain\f0\fs24\hich\f0\dbch\f0\loch\f0\fs24\b Change Log:\plain\f0\fs24\par 
\plain\f0\fs24\hich\f0\dbch\f0\loch\f0\fs24\b Date:\tab\tab Author:\tab\tab\tab\tab Text:\plain\f0\fs24\par";

        private const string _LogRow = " @DATE@\\tab @AUTHOR@\\tab @TEXT@\\par ";
        private const string _LogTrailer = "}";
        private const string _HDRTOKEN = "{\\rtf1\\ansi\\deflang";
        private const string _LINESTOKEN = "Text:\\plain\\f0\\fs24\\par";
        private const string _DOCTOKEN = "Documentation:\\plain\\f0\\fs24\\par";
        private const string _EODTOKEN = "\\plain\\f0\\fs24\\hich\\f0\\dbch\\f0\\loch\\f0\\fs24\\b Change Log:";
        private const string _EOLTOKEN = "\\par ";
        private const string _DOCHEADER = "";

        private List<string> _lines;        // The change log.
        private string _documentation;      // The documentation section.

        /// <summary>
        /// Getter and setter for the documentation. We can only manipulate this as a single string, which might be RTF-formatted.
        /// </summary>
        internal string Documentation
        {
            get { return this._documentation; }
            set { this._documentation = value; }
        }

        /// <summary>
        /// Create a new instance, receiving as argument an existing logger entry in RTF format.
        /// If omitted, a new log is created.
        /// </summary>
        /// <param name="contents">Existing tracelog in RTF format.</param>
        internal MEChangeLog (string contents = null)
        {            
            this._lines = new List<string>();
            this._documentation = string.Empty;

            // Validate the contents string, if it does not contain our header, treat as empty content....
            if (!string.IsNullOrEmpty(contents) && contents.Contains(_HDRTOKEN))
            {
                // Remove the header and trailer and split into documentation section and changelog lines...
                contents = RemoveLineSeparators(contents);
                string startDocumentation = contents.Substring(contents.IndexOf(_DOCTOKEN) + _DOCTOKEN.Length);
                this._documentation = startDocumentation.Substring(0, startDocumentation.IndexOf(_EODTOKEN)).Trim();
                string lines = startDocumentation.Substring(startDocumentation.IndexOf(_LINESTOKEN) + _LINESTOKEN.Length);
                while (lines.Length > 20)    // Must have at least a date field and an author in it, so this is an easy check.
                {
                    this._lines.Add(lines.Substring(0, lines.IndexOf(_EOLTOKEN) + _EOLTOKEN.Length));
                    lines = lines.Substring(lines.IndexOf(_EOLTOKEN) + _EOLTOKEN.Length);
                }
            }
        }

        /// <summary>
        /// Create a new logging entry. The method will automatically add the current date to the entry.
        /// </summary>
        /// <param name="author">Name of author of the entry.</param>
        /// <param name="text">Entry text.</param>
        internal void AddEntry(string author, string text)
        {
            string newLine = _LogRow;

            // We attempt to align text depending on the width of the Author field. We have 3 tab-stops to test. If the name is 
            // much shorter, we have to add some additional tabs to align stuff. If the name is too long, too bad ;-)
            int extraTabs = 3 - (author.Length / 8);
            for (int i = 0; i < extraTabs; i++, author += "\\tab") ;
            newLine = newLine.Replace("@AUTHOR@", author);
            newLine = newLine.Replace("@DATE@", DateTime.Now.Date.ToString("dd-MM-yyyy"));
            newLine = newLine.Replace("@TEXT@", text);
            this._lines.Add(newLine);
        }

        /// <summary>
        /// This function can be used to retrieve the documentation section of the specified class as an RTF-formatted string.
        /// </summary>
        /// <param name="thisClass">Class from which we want to retrieve documentation section.</param>
        /// <returns>RTF-formatted documentation.</returns>
        internal static string GetRTFDocumentation(MEClass thisClass)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MigrateLog(thisClass);
            string annotation = thisClass.Annotation;
            MEChangeLog newLog = (!string.IsNullOrEmpty(annotation)) ? new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF)) : new MEChangeLog();
            return _DocHeader + newLog.Documentation;
        }

        /// <summary>
        /// This function can be used to retrieve the documentation section of the specified class as a plain, ASCII-formatted list of
        /// lines. For each line in the documentation, the function returns a separate string.
        /// </summary>
        /// <param name="thisClass">Class from which we want to retrieve documentation section.</param>
        /// <returns>ASCII-formatted documentation as a list of lines. Empty list if no documentation is defined.</returns>
        internal static List<string> GetDocumentationAsTextLines(MEClass thisClass)
        {
            string plainText = string.Empty;
            var lines = new List<string>();

            // Clever trick: use the built-in RTF functionality of the RichTextBox to manipulate contents.
            using (var rtBox = new RichTextBox())
            {
                rtBox.Rtf = GetRTFDocumentation(thisClass);
                plainText = rtBox.Text;     // Consists of possibly multiple lines, separated by "\n".
                while (plainText != string.Empty)
                {
                    int eolIndex = plainText.IndexOf("\n");
                    if (eolIndex <= 0)
                    {
                        lines.Add(plainText.Trim());
                        return lines;
                    }
                    lines.Add(plainText.Substring(0, eolIndex).Trim());
                    if (plainText.Length > eolIndex + 1) plainText = plainText.Substring(eolIndex + 1);
                    else plainText = string.Empty;
                }
            }
            return lines;
        }

        /// <summary>
        /// Returns the documentation of the specified class as a single string in which each line is separated by the specified separator string.
        /// By default, lines will be separated by a Newline ("\n").
        /// </summary>
        /// <param name="thisClass">Class for which we want to retrieve the documentation.</param>
        /// <returns>Documentation as a single string, empty string in case no documentation is defined.</returns>
        internal static string GetDocumentationAsText(MEClass thisClass, string lineSeparator = "\n")
        {
            List<string> docLines = GetDocumentationAsTextLines(thisClass);
            string documentation = (docLines.Count > 0) ? docLines[0] : string.Empty;
            if (docLines.Count > 1)
            {
                // We already copied the first line, so now append all additional lines...
                for (int i = 1; i < docLines.Count; i++) documentation += lineSeparator + docLines[i];
            }
            return documentation;
        }

        /// <summary>
        /// Returns the complete log as an RTF document.
        /// </summary>
        /// <returns>Current log.</returns>
        internal string GetLog()
        {
            string logText = _LogHeader + (!string.IsNullOrEmpty(this._documentation)? " " + this._documentation: string.Empty) + _AuditHeader;
            foreach (string line in this._lines) logText += line;
            logText += _LogTrailer;
            return logText;
        }

        /// <summary>
        /// Returns the log as a plain block of text, each line terminated by the proper newline character and all formatting removed.
        /// This function ONLY returns the change log, NOT the Documentation section.
        /// </summary>
        /// <param name="prefix">An optional prefix string that will be added to each line.</param>
        /// <returns>Log as plain block of text.</returns>
        internal string GetLogAsText(string prefix = null)
        {
            string logText = string.Empty;
            if (!string.IsNullOrEmpty(prefix)) logText += prefix; 
            logText += "Date:        Author:                 Text:" + Environment.NewLine;
            bool firstLine = true;  // First line has an extra character in it.

            foreach (string line in this._lines)
            {
                const string _SEPARATOR = "\\tab ";
                const string _SEPARATOR2 = "\\tab";
                string parts = line.Substring(line.IndexOf(_SEPARATOR, StringComparison.Ordinal) + _SEPARATOR.Length);
                string author = parts.Substring(0, parts.IndexOf(_SEPARATOR2, StringComparison.Ordinal));
                parts = parts.Substring(parts.IndexOf(_SEPARATOR, StringComparison.Ordinal) + _SEPARATOR.Length);
                string text = parts.Substring(0, parts.IndexOf("\\par", StringComparison.Ordinal));

                // For some reason, we need to insert a few spaces to align stuff properly...
                string textLine = "   " + line.Substring(firstLine ? 1 : 0, 10) + "   " + author;   // Date is always 10 positions, starting from pos 2 (except first line).
                int fillerSpaces = (author.Length > 23) ? 1 : 24 - author.Length;
                for (int i = 0; i < fillerSpaces; i++, textLine += " ");
                textLine += text;
                firstLine = false;

                if (!string.IsNullOrEmpty(prefix)) logText += prefix;
                logText += textLine + Environment.NewLine;
            }
            return logText;
        }

        /// <summary>
        /// This method assures that the specified class contains an audit log that complies with our latest, RTf-based, log format. 
        /// It checks the contents of the current log and if not in proper format, migrates it to the correct format.
        /// </summary>
        /// <param name="thisClass">Class for which we want to migrate the log.</param>
        internal static void MigrateLog(MEClass thisClass)
        {
            const string pattern = @"^.*Documentation:";
            const string pattern2 = @"^.*Date:\s*Author:\s*Text:";

            Logger.WriteInfo("Framework.Util.AuditLog >> Checking class '" + thisClass.Name + "' for log entry compatibility...");
            string annotation = thisClass.Annotation;
            if (string.IsNullOrEmpty(annotation)) return;   // No annotation at all, nothing to migrate or check!

            var matcher = new Regex(pattern);
            if (!matcher.IsMatch(annotation))
            {
                // We don't comply with the latest format, check is it's the 'almost latest'...
                var transformLog = new MEChangeLog();
                matcher = new Regex(pattern2);
                if (!matcher.IsMatch(annotation))
                {
                    Logger.WriteInfo("Framework.Util.AuditLog >> Old format, migrating...");
                    transformLog.LoadASCIILog(annotation, false);
                }
                else
                {
                    Logger.WriteInfo("Framework.Util.AuditLog >> Previous format, migrating...");
                    transformLog.LoadASCIILog(annotation, true);
                }
                thisClass.Annotation = ContextSlt.GetContextSlt().TransformRTF(transformLog.GetLog(), RTFDirection.FromRTF);
                Logger.WriteInfo("Framework.Util.AuditLog >> Log migrated to latest format.");
            }
        }

        /// <summary>
        /// This method loads an RTF-formatted documentation section to the Notes property of the specified class.
        /// </summary>
        /// <param name="thisClass">The class we want to update.</param>
        /// <param name="documentation">RTF-formatted documentation section.</param>
        internal static void SetRTFDocumentation(MEClass thisClass, string documentation)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEChangeLog.MigrateLog(thisClass);      // Always make sure that the documentation section is properly formatted.
            documentation += "\\par";
            var newLog = new MEChangeLog(context.TransformRTF(thisClass.Annotation, RTFDirection.ToRTF))
            {
                Documentation = documentation
            };
            string log = newLog.GetLog();
            thisClass.Annotation = context.TransformRTF(log, RTFDirection.FromRTF);
        }

        /// <summary>
        /// This method loads an RTF-formatted documentation section to the Notes property of the specified class.
        /// </summary>
        /// <param name="thisClass">The class we want to update.</param>
        /// <param name="documentation">List of RTF-formatted documentation lines.</param>
        internal static void SetRTFDocumentation(MEClass thisClass, List<string> documentation)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEChangeLog.MigrateLog(thisClass);      // Always make sure that the documentation section is properly formatted.
            string paragraph = string.Empty;
            foreach (string line in documentation) paragraph += line + "\\par ";
            var newLog = new MEChangeLog(context.TransformRTF(thisClass.Annotation, RTFDirection.ToRTF))
            {
                Documentation = paragraph
            };
            string log = newLog.GetLog();
            thisClass.Annotation = context.TransformRTF(log, RTFDirection.FromRTF);
        }

        /// <summary>
        /// This method loads the log object with an existing, ASCII-formatted, set of log lines. It replaces the contents of 
        /// the current AuditLog object by the provided string set, so it should typically be called on a new AuditLog instance!
        /// The received input should consist of three columns: 
        /// [Date] [Author] [Remarks], each separated by arbitrary amounts of whitespace. It should NOT contain any header rows!
        /// </summary>
        /// <param name="textLog">Log input in ASCII format.</param>
        private void LoadASCIILog(string textLog, bool isRTFFormat)
        {
            const string pattern = @"(?<date>\d{2}-\d{2}-\d{4})\s*(?<name>[a-zA-Z ]+)\s*(?<text>\b.+[^\r\n])";
            MatchCollection matches = new Regex(pattern).Matches(textLog);
            this._lines = new List<string>();

            foreach (Match match in matches)
            {
                GroupCollection groups = match.Groups;
                string logLine = _LogRow;
                string author = groups["name"].Value;
                if (isRTFFormat)
                {
                    // In case of translation from 'old' RTF format, we need a slightly different template...
                    const string RTFRow = " @DATE@\\tab @AUTHOR@ @TEXT@\\par ";
                    logLine = RTFRow;
                }
                else
                {
                    // In case of translation from the old ASCII format, we need to calculate the number of tab's...
                    int extraTabs = 3 - (author.Length / 8);
                    for (int i = 0; i < extraTabs; i++, author += "\\tab") ;
                }
                logLine = logLine.Replace("@AUTHOR@", author);
                logLine = logLine.Replace("@DATE@", groups["date"].Value);
                logLine = logLine.Replace("@TEXT@", groups["text"].Value);
                this._lines.Add(logLine);
            }
        }

        /// <summary>
        /// Helper function that gets rid of all line separator from the input string since this negatively affects string search and replace...
        /// </summary>
        /// <param name="rawData">Input data.</param>
        /// <returns>Input data without line separators.</returns>
        private string RemoveLineSeparators(string rawData)
        {
            if (String.IsNullOrEmpty(rawData)) return rawData;

            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return rawData.Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(lineSeparator, string.Empty).Replace(paragraphSeparator, string.Empty);
        }
    }
}
