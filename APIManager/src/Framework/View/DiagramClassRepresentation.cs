using System;
using Framework.Exceptions;
using Framework.Model;

namespace Framework.View
{
    /// <summary>
    /// Interface class for Diagram representation of classes. Can be used, through the owning Diagram, to change the presentation of classes on that diagram.
    /// </summary>
    internal sealed class DiagramClassRepresentation
    {
        /// <summary>
        /// Represents the font characteristics, which is a combination of Bold, Italic and/or Underline. 'Normal' represent the situation where
        /// no special characteristics are present.
        /// </summary>
        internal enum Characteristics { Normal = 0, Underline = 1, Italic = 2, ItalicUnderline = 3, Bold = 4, BoldUnderline = 5, BoldItalic = 6, BoldItalicUnderline = 7 }

        /// <summary>
        /// Can be used to quickly set colors based on predefined color definitions.
        /// </summary>
        internal enum QuickColors { Default, Blue, LightBlue, Orange, Purple, Red, White, Yellow, LimeGreen, Black}

        private DiagramClassRepresentationImplementation _imp = null;  // The associated implementation object; does all the 'real' work.

        /// <summary>
        /// Constructor binds this implementation interface to the actual implementation object. Typically, this constructor is invoked ONLY from the
        /// Diagram that constructs the implementation.
        /// </summary>
        /// <param name="imp">Implementation object for this interface.</param>
        internal DiagramClassRepresentation(DiagramClassRepresentationImplementation imp)
        {
            this._imp = imp;
        }

        /// <summary>
        /// Returns a new interface object of the class that is associated with this representation.
        /// </summary>
        /// <returns>Class that is associated with this representation.</returns>
        internal MEClass AssociatedClass
        {
            get
            {
                if (this._imp != null) return this._imp.GetClass();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the class background color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal byte[] BackgroundColor
        {
            get
            {
                if (this._imp != null) return this._imp.GetBackgroundColor();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetBackgroundColor(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the class background color as an integer color value (integer encoding of RGB bytes).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal int BackgroundColorInt
        {
            get
            {
                if (this._imp != null) return ColorToInteger(this._imp.GetBackgroundColor());
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetBackgroundColor(ColorToBytes(value));
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the class border color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal byte[] BorderColor
        {
            get
            {
                if (this._imp != null) return this._imp.GetBorderColor();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetBorderColor(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the border line width in pixels.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal int BorderLineWidth
        {
            get
            {
                if (this._imp != null) return this._imp.GetBorderLineWidth();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetBorderLineWidth(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the font color as a set of three bytes (R, G, B).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal byte[] FontColor
        {
            get
            {
                if (this._imp != null) return this._imp.GetFontColor();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetFontColor(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the font color as an integer color value (integer encoding of RGB bytes).
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal int FontColorInt
        {
            get
            {
                if (this._imp != null) return ColorToInteger(this._imp.GetFontColor());
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetFontColor(ColorToBytes(value));
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the font characteristics (Bold, Italic or Underline) as an enumerated type.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal Characteristics FontCharacteristics
        {
            get
            {
                if (this._imp != null) return this._imp.GetFontCharacteristics();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetFontCharacteristics(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the name of the current font.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal string FontName
        {
            get
            {
                if (this._imp != null) return this._imp.GetFontName();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetFontName(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Get- or set the size of the current font in points.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal int FontSize
        {
            get
            {
                if (this._imp != null) return this._imp.GetFontSize();
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetFontSize(value);
                else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
            }
        }

        /// <summary>
        /// Applies the current representation settings to the class on the diagram. This allows a series of independent changes to be made,
        /// which are then persisted on a single call to 'Apply'.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Apply()
        {
            if (this._imp != null) this._imp.Apply();
            else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
        }

        /// <summary>
        /// Helper function that translates a specified RGB integer color representation to a 3-byte R,G,B byte array. 
        /// </summary>
        /// <param name="color">32-bit numeric color representation.</param>
        /// <returns>3-byte RGB array.</returns>
        internal static byte[] ColorToBytes(int color)
        {
            byte[] conversionArray = BitConverter.GetBytes(color);
            return new byte[3] { conversionArray[0], conversionArray[1], conversionArray[2] };
        }

        /// <summary>
        /// Helper function that translates a specified RGB byte array to an integer representation.
        /// </summary>
        /// <param name="colorString">3 bytes (R,G and B) color array.</param>
        /// <returns>32-bit integer representation.</returns>
        internal static int ColorToInteger(byte[] colorString)
        {
            byte[] conversionArray = new byte[4] { 0, 0, 0, 0 };
            conversionArray[0] = colorString[0];
            conversionArray[1] = colorString[1];
            conversionArray[2] = colorString[2];
            return BitConverter.ToInt32(conversionArray, 0);
        }

        /// <summary>
        /// Reload the current settings of the representation from the associated diagram.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Refresh()
        {
            if (this._imp != null) this._imp.Refresh();
            else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
        }

        /// <summary>
        /// Convenience function that assigns a pre-defined color code.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <param name="colorCode">Color code to be assigned as background color.</param>
        internal void SetBackgroundColor(QuickColors colorCode)
        {
            BackgroundColor = QuickColorToRGB(colorCode);
        }

        /// <summary>
        /// Set the class background color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetBackgroundDefaultColor()
        {
            if (this._imp != null) this._imp.SetBackgroundDefaultColor();
            else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
        }

        /// <summary>
        /// Convenience function that assigns a pre-defined color code.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <param name="colorCode">Color code to be assigned as background color.</param>
        internal void SetBorderColor(QuickColors colorCode)
        {
            BorderColor = QuickColorToRGB(colorCode);
        }

        /// <summary>
        /// Set the class border color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetBorderDefaultColor()
        {
            if (this._imp != null) this._imp.SetBorderDefaultColor();
            else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
        }

        /// <summary>
        /// Convenience function that assigns a pre-defined color code.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <param name="colorCode">Color code to be assigned as font color.</param>
        internal void SetFontColor(QuickColors colorCode)
        {
            FontColor = QuickColorToRGB(colorCode);
        }

        /// <summary>
        /// Set the font color to the default value defined by the model.
        /// Please note that changes to the display object are not actually executed until the 'Apply' function has been called.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetFontDefaultColor()
        {
            if (this._imp != null) this._imp.SetFontDefaultColor();
            else throw new MissingImplementationException("DiagramClassRepresentationImplementation");
        }

        /// <summary>
        /// Helper function that replaces the enumerated value by an RGB code.
        /// </summary>
        /// <param name="color">Enumeration to be translated.</param>
        /// <returns>3-byte RGB value.</returns>
        private byte[] QuickColorToRGB(QuickColors color)
        {
            byte[] RGB;
            switch (color)
            {
                case QuickColors.Black:
                    RGB = new byte[] { 0, 0, 0 };
                    break;

                case QuickColors.Blue:
                    RGB = new byte[] { 0x57, 0xC0, 0xFF };
                    break;

                case QuickColors.LightBlue:
                    RGB = new byte[] { 0x87, 0xCE, 0xFA };
                    break;

                case QuickColors.Orange:
                    RGB = new byte[] { 0xFF, 0xC0, 0x00 };
                    break;

                case QuickColors.Purple:
                    RGB = new byte[] { 0xFF, 0xA0, 0xFF };
                    break;

                case QuickColors.Red:
                    RGB = new byte[] { 0xFF, 0x66, 0x00 };
                    break;

                case QuickColors.White:
                    RGB = new byte[] { 0xFF, 0xFF, 0xFF };
                    break;

                case QuickColors.Yellow:
                    RGB = new byte[] { 0xFF, 0xFF, 0x57 };
                    break;

                case QuickColors.LimeGreen:
                    RGB = new byte[] { 0xA2, 0xFF, 0xA9 };
                    break;

                // Default = 'Default', 'Green' or unknown values:
                default:
                    RGB = new byte[] { 0xE9, 0xFE, 0xA7 };
                    break;
            }
            return RGB;
        }
    }
}
