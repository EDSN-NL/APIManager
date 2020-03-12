using System;
using Framework.Logging;
using Framework.View;
using Framework.Model;
using SparxEA.Model;

namespace SparxEA.View
{
    /// <summary>
    /// Represents a 'diagram' within Sparx EA. Diagrams live in a Package and may contain ModelElements.
    /// </summary>
    internal sealed class EADiagramClassRepresentation : DiagramClassRepresentationImplementation
    {
        private byte[] _classBackgroundColor;       // Background color RGB in Hex format (R = [0], G = [1], B = [2]).
        private bool _classBackgroundDefaultColor;  // Enforces reset to default colors.
        private byte[] _classBorderColor;           // Border color of a class representation RGB in Hex format (R = [0], G = [1], B = [2]).
        private bool _classBorderDefaultColor;      // Enforces reset to default colors.
        private int _borderLineWidth;               // Border line width of the class.
        private byte[] _fontColor;                  // Font color RGB in Hex format (R = [0], G = [1], B = [2]).
        private bool _fontDefaultColor;             // Enforces reset to default colors.
        private bool _bold;                         // Set font to bold representation.
        private bool _italic;                       // Set font to italic representation.
        private bool _underline;                    // Set font to underline.
        private string _fontName;                   // Set font to specified name.
        private int _fontSize;                      // Set font to specified size (in points).

        private EA.Element _theClass;               // The associated class for which we want to get/set the representation.
        private EA.Diagram _diagram;                // EA Diagram representation.
        private EA.DiagramObject _diagramObject;    // Representation of theClass on the diagram.

        // Set of flags to keep track of changes (we don't want to update stuff that has not been updated)...
        private bool _changedClassBackgroundColor;
        private bool _changedClassBorderColor;
        private bool _changedBorderLineWidth;
        private bool _changedFontCharacteristics;
        private bool _changedFontName;
        private bool _changedFontSize;
        private bool _changedFontColor;

        /// <summary>
        /// Initialises a diagram class representation from the specified diagram.
        /// </summary>
        /// <param name="model">Identifies the repository to be used.</param>
        /// <param name="classID">Identifies our target class on the diagram.</param>
        /// <param name="diagramID">Identifies the diagram on which the class must be present.</param>
        internal EADiagramClassRepresentation(EAModelImplementation model, int classID, int diagramID)
        {
            this._diagram = model.Repository.GetDiagramByID(diagramID) as EA.Diagram;
            this._theClass = model.Repository.GetElementByID(classID) as EA.Element;

            if (this._theClass != null && this._diagram != null)
            {
                this._diagramObject = this._diagram.GetDiagramObjectByID(classID, string.Empty);

                if (this._diagramObject != null)
                {
                    this._classBackgroundColor = null;
                    this._classBorderColor = null;
                    this._borderLineWidth = -1;
                    this._fontColor = null;
                    this._bold = false;
                    this._italic = false;
                    this._underline = false;
                    this._fontName = string.Empty;
                    this._fontSize = -1;

                    GetCurrentRepresentation(); // Load initial state for the display representation.
                }
                else Logger.WriteError("SparxEA.View.EADiagramClassRepresentation >> Unable to retrieve DiagramObject for Class '" + 
                                       this._theClass.Name + "' on Diagram '" + this._diagram.Name + "'!");
            }
            else
            {
                string className = this._theClass != null ? this._theClass.Name : "--NO VALID CLASS--";
                string diagramName = this._diagram != null ? this._diagram.Name : "--NO VALID DIAGRAM--";
                Logger.WriteError("SparxEA.View.EADiagramClassRepresentation >> Attempt to create a representation for class '" + 
                                  className + "' on Diagram '" + diagramName + "' failed!");
            }
        }

        /// <summary>
        /// Applies the current representation settings to the class on the diagram. This allows a series of independent changes to be made,
        /// which are then persisted on a single call to 'Apply'.
        /// </summary>
        internal override void Apply()
        {
            if (this._diagramObject != null)
            {
                bool anyChanges = false;
                byte[] convertToIntArray = new byte[4] { 0, 0, 0, 0 };     // An integer contains 4 bytes, not three!
                if (this._changedClassBackgroundColor)
                {
                    convertToIntArray[0] = this._classBackgroundColor[0];
                    convertToIntArray[1] = this._classBackgroundColor[1];
                    convertToIntArray[2] = this._classBackgroundColor[2];
                    this._diagramObject.BackgroundColor = this._classBackgroundColor != null ? BitConverter.ToInt32(convertToIntArray, 0) : -1;
                    this._changedClassBackgroundColor = false;
                    anyChanges = true;
                }
                else if (this._classBackgroundDefaultColor)
                {
                    this._diagramObject.BackgroundColor = -1;
                    this._classBackgroundDefaultColor = false;
                    anyChanges = true;
                }

                if (this._changedClassBorderColor)
                {
                    convertToIntArray[0] = this._classBorderColor[0];
                    convertToIntArray[1] = this._classBorderColor[1];
                    convertToIntArray[2] = this._classBorderColor[2];
                    this._diagramObject.BorderColor = this._classBorderColor != null ? BitConverter.ToInt32(convertToIntArray, 0) : -1;
                    this._changedClassBorderColor = false;
                    anyChanges = true;
                }
                else if (this._classBorderDefaultColor)
                {
                    this._diagramObject.BorderColor = -1;
                    this._classBorderDefaultColor = false;
                    anyChanges = true;
                }

                if (this._changedBorderLineWidth)
                {
                    this._diagramObject.BorderLineWidth = this._borderLineWidth;
                    this._changedBorderLineWidth = false;
                    anyChanges = true;
                }

                if (this._changedFontColor)
                {
                    convertToIntArray[0] = this._fontColor[0];
                    convertToIntArray[1] = this._fontColor[1];
                    convertToIntArray[2] = this._fontColor[2];
                    this._diagramObject.FontColor = this._fontColor != null ? BitConverter.ToInt32(convertToIntArray, 0) : -1;
                    this._changedFontColor = false;
                    anyChanges = true;
                }
                else if (this._fontDefaultColor)
                {
                    this._diagramObject.FontColor = -1;
                    this._fontDefaultColor = false;
                    anyChanges = true;
                }

                if (this._changedFontCharacteristics)
                {
                    this._diagramObject.FontBold = this._bold;
                    this._diagramObject.FontItalic = this._italic;
                    this._diagramObject.FontUnderline = this._underline;
                    this._changedFontCharacteristics = false;
                    anyChanges = true;
                }

                if (this._changedFontName)
                {
                    if (this._fontName != string.Empty) this._diagramObject.fontName = this._fontName;
                    this._changedFontName = false;
                    anyChanges = true;
                }

                if (this._changedFontSize)
                {
                    this._diagramObject.fontSize = this._fontSize;
                    this._changedFontSize = false;
                    anyChanges = true;
                }

                if (anyChanges) this._diagramObject.Update();
            }
            else
            {
                Logger.WriteInfo("SparxEA.View.EADiagramClassRepresentation.Apply >> No diagram object available to update!");
                return;

            }
        }

        /// <summary>
        /// Returns the class background color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>Background color as an RGB value.</returns>
        internal override byte[] GetBackgroundColor()
        {
            return this._classBackgroundColor;
        }

        /// <summary>
        /// Returns the class border color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>Border color as an RGB value.</returns>
        internal override byte[] GetBorderColor()
        {
            return this._classBorderColor;
        }

        /// <summary>
        /// Returns the border line width in pixels.
        /// </summary>
        /// <returns>Border line width in pixels.</returns>
        internal override int GetBorderLineWidth()
        {
            return this._borderLineWidth;
        }

        /// <summary>
        /// Returns a new interface object of the class that is associated with this representation.
        /// </summary>
        /// <returns>Class that is associated with this representation.</returns>
        internal override MEClass GetClass()
        {
            return new MEClass(this._theClass.ElementID);
        }

        /// <summary>
        /// Returns the font color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>font color as an RGB value.</returns>
        internal override byte[] GetFontColor()
        {
            return this._fontColor;
        }

        /// <summary>
        /// Returns the font characteristics (Bold, Italic or Underline) as an enumerated type.
        /// </summary>
        /// <returns>Font characteristics identifier</returns>
        internal override DiagramClassRepresentation.Characteristics GetFontCharacteristics()
        {
            int val = 0;
            if (this._bold) val += 4;
            if (this._italic) val += 2;
            if (this._underline) val += 1;
            return (DiagramClassRepresentation.Characteristics)val;
        }

        /// <summary>
        /// Returns the name of the current font.
        /// </summary>
        /// <returns>Font name.</returns>
        internal override string GetFontName()
        {
            return this._fontName;
        }

        /// <summary>
        /// Applies a new font name.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <param name="newName">Font name.</param>
        internal override void SetFontName(string newName)
        {
            if (newName != this._fontName)
            {
                this._fontName = newName;
                this._changedFontName = true;
            }
        }

        /// <summary>
        /// Returns the current font size in points.
        /// </summary>
        /// <returns>Font size in points.</returns>
        internal override int GetFontSize()
        {
            return this._fontSize;
        }

        /// <summary>
        /// Reload the current settings of the representation from the associated diagram.
        /// </summary>
        internal override void Refresh()
        {
            GetCurrentRepresentation();
        }

        /// <summary>
        /// Set the class background color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetBackgroundColor(byte[] newColor)
        {
            if (newColor[0] != this._classBackgroundColor[0] ||
                newColor[1] != this._classBackgroundColor[1] ||
                newColor[2] != this._classBackgroundColor[2])
            {
                this._classBackgroundColor = newColor;
                this._changedClassBackgroundColor = true;
                this._classBackgroundDefaultColor = false;
            }
        }

        /// <summary>
        /// Set the class background color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetBackgroundDefaultColor()
        {
            this._classBackgroundDefaultColor = true;
            this._changedClassBackgroundColor = false;
        }

        /// <summary>
        /// Set the class background color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetBorderColor(byte[] newColor)
        {
            if (newColor[0] != this._classBorderColor[0] ||
                newColor[1] != this._classBorderColor[1] ||
                newColor[2] != this._classBorderColor[2])
            {
                this._classBorderColor = newColor;
                this._classBorderDefaultColor = false;
                this._changedClassBorderColor = true;
            }
        }

        /// <summary>
        /// Set the class border color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetBorderDefaultColor()
        {
            this._classBorderDefaultColor = true;
            this._changedClassBorderColor = false;
        }

        /// <summary>
        /// Set the border line width in pixels.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetBorderLineWidth(int newLineWidth)
        {
            if (newLineWidth != this._borderLineWidth)
            {
                this._borderLineWidth = newLineWidth;
                this._changedBorderLineWidth = true;
            }
        }

        /// <summary>
        /// Applies (new) font characteristics (Normal, Bold, Italics, Underline or any combination of these).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetFontCharacteristics(DiagramClassRepresentation.Characteristics newSettings)
        {
            if (GetFontCharacteristics() == newSettings) return;    // New settings match existing settings, perform no actions!

            this._bold = ((int)newSettings & 4) != 0;
            this._italic = ((int)newSettings & 2) != 0;
            this._underline = ((int)newSettings & 1) != 0;
            this._changedFontCharacteristics = true;
        }

        /// <summary>
        /// Set the font color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetFontColor(byte[] newColor)
        {
            if (newColor[0] != this._fontColor[0] ||
                newColor[1] != this._fontColor[1] ||
                newColor[2] != this._fontColor[2])
            {
                this._fontColor = newColor;
                this._changedFontColor = true;
                this._fontDefaultColor = false;
            }
        }

        /// <summary>
        /// Set the font color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal override void SetFontDefaultColor()
        {
            this._fontDefaultColor = true;
            this._changedFontColor = false;
        }

        /// <summary>
        /// Applies a new font name.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// Specify -1 to set the default size.
        /// </summary>
        /// <param name="newName">Font name.</param>
        internal override void SetFontSize(int newSize)
        {
            if (newSize != this._fontSize)
            {
                this._fontSize = newSize;
                this._changedFontSize = true;
            }
        }

        /// <summary>
        /// Helper function that loads the current settings from the diagram.
        /// </summary>
        private void GetCurrentRepresentation()
        {
            if (this._diagramObject != null)
            {
                int color = this._diagramObject.BackgroundColor;
                // On little endian machines, BitConverter.GetBytes returns the color code in RGB order.
                // The 253,250,247 represents the UML 'default' color of a given class without any stereotypes.
                this._classBackgroundColor = color == -1? new byte[] { 253, 250, 247 }: BitConverter.GetBytes(color);
                color = this._diagramObject.BorderColor;
                this._classBorderColor = color == -1 ? new byte[] { 0, 0, 0 }: BitConverter.GetBytes(color);
                color = this._diagramObject.FontColor;
                this._fontColor = color == -1 ? new byte[] { 0, 0, 0 } : BitConverter.GetBytes(color);
                this._borderLineWidth = this._diagramObject.BorderLineWidth;
                this._bold = this._diagramObject.FontBold;
                this._italic = this._diagramObject.FontItalic;
                this._underline = this._diagramObject.FontUnderline;
                this._fontName = this._diagramObject.fontName as string;
                if (this._fontName == null) this._fontName = string.Empty;
                this._fontSize = this._diagramObject.fontSize;

                // These will change to true whenever the user invokes the associated 'Set' operation...
                this._changedClassBackgroundColor = false;
                this._changedClassBorderColor = false; ;
                this._changedBorderLineWidth = false;
                this._changedFontColor = false;
                this._changedFontCharacteristics = false;
                this._changedFontName = false;
                this._changedFontSize = false;

                // For these, 'false' indicates that we should NOT change the colors to their default values, this does not imply that
                // the color ARE at a default value ;-)
                this._classBackgroundDefaultColor = false;
                this._classBorderDefaultColor = false;
                this._fontDefaultColor = false;
            }
            else
            {
                Logger.WriteInfo("SparxEA.View.EADiagramClassRepresentation.GetCurrentRepresentation >> No diagram object available to read!");
            }
        }
    }
}
