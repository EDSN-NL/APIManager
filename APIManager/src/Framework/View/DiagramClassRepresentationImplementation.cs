namespace Framework.View
{
    /// <summary>
    /// Helper class that defines how a Class must be represented on a given diagram. Must be implemented by a tool-specific derived class.
    /// </summary>
    internal abstract class DiagramClassRepresentationImplementation
    {
        /// <summary>
        /// Applies the current representation settings to the class on the diagram. This allows a series of independent changes to be made,
        /// which are then persisted on a single call to 'Apply'.
        /// </summary>
        internal abstract void Apply();

        /// <summary>
        /// Returns the class background color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>Background color as an RGB value.</returns>
        internal abstract byte[] GetBackgroundColor();

        /// <summary>
        /// Returns the class border color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>Border color as an RGB value.</returns>
        internal abstract byte[] GetBorderColor();

        /// <summary>
        /// Returns the border line width in pixels.
        /// </summary>
        /// <returns>Border line width in pixels.</returns>
        internal abstract int GetBorderLineWidth();

        /// <summary>
        /// Returns a new interface object of the class that is associated with this representation.
        /// </summary>
        /// <returns>Class that is associated with this representation.</returns>
        internal abstract Model.MEClass GetClass();

        /// <summary>
        /// Returns the font color as a set of three bytes (R, G, B).
        /// </summary>
        /// <returns>font color as an RGB value.</returns>
        internal abstract byte[] GetFontColor();

        /// <summary>
        /// Returns the font characteristics (Bold, Italic or Underline) as an enumerated type.
        /// </summary>
        /// <returns>Font characteristics identifier</returns>
        internal abstract DiagramClassRepresentation.Characteristics GetFontCharacteristics();

        /// <summary>
        /// Returns the name of the current font.
        /// </summary>
        /// <returns>Font name.</returns>
        internal abstract string GetFontName();

        /// <summary>
        /// Applies a new font name.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <param name="newName">Font name.</param>
        internal abstract void SetFontName(string newName);

        /// <summary>
        /// Returns the current font size in points.
        /// </summary>
        /// <returns>Font size in points.</returns>
        internal abstract int GetFontSize();

        /// <summary>
        /// Reload the current settings of the representation from the associated diagram.
        /// </summary>
        internal abstract void Refresh();

        /// <summary>
        /// Set the class background color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetBackgroundColor(byte[] newColor);

        /// <summary>
        /// Set the class background color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetBackgroundDefaultColor();

        /// <summary>
        /// Set the class background color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetBorderColor(byte[] newColor);

        /// <summary>
        /// Set the class border color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetBorderDefaultColor();

        /// <summary>
        /// Set the border line width in pixels.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetBorderLineWidth(int newLineWidth);

        /// <summary>
        /// Applies (new) font characteristics (Normal, Bold, Italics, Underline or any combination of these).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetFontCharacteristics(DiagramClassRepresentation.Characteristics newSettings);

        /// <summary>
        /// Set the font color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetFontColor(byte[] newColor);

        /// <summary>
        /// Set the font color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        internal abstract void SetFontDefaultColor();

        /// <summary>
        /// Applies a new font name.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// Specify -1 to set the default size.
        /// </summary>
        /// <param name="newName">Font name.</param>
        internal abstract void SetFontSize(int newSize);
    }
}
