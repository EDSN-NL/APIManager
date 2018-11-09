using System;
using System.Collections.Generic;
using EA;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Context;
using SparxEA.Model;

namespace SparxEA.View
{
    /// <summary>
    /// Represents a 'diagram' within Sparx EA. Diagrams live in a Package and may contain ModelElements.
    /// </summary>
    internal sealed class EADiagramImplementation: DiagramImplementation
    {
        private const int _ClassWidth = 150;
        private const int _ClassHeight = 60;

        private EA.Diagram _diagram;    // EA Diagram representation.
        private int _top;               // Initial top coordinate for new classes.
        private int _left;              // Initial left coordinate for new classes.

        /// <summary>
        /// The internal constructor is called to initialize the diagram object.
        /// <param name="model">Reference to the tool-specific model implementation.</param>
        /// <param name="diagramID">Repository ID of the diagram.</param>
        /// </summary>
        internal EADiagramImplementation(EAModelImplementation model, int diagramID): base(model)
        {
            this._diagram = model.Repository.GetDiagramByID(diagramID);
            this._owningPackage = null;
            this._top = 1000;
            this._left = 20;
            if (this._diagram != null)
            {
                this._name = this._diagram.Name;
                this._diagramID = diagramID;
                this._globalID = this._diagram.DiagramGUID;
                this._owningPackage = new MEPackage(this._diagram.PackageID);
            }
            else
            {
                Logger.WriteError("SparxEA.View.EADiagramImplementation (id) >> Failed to retrieve EA Diagram with ID: " + diagramID);
                this._name = string.Empty;
                this._diagramID = diagramID;
                this._globalID = string.Empty;
                this._owningPackage = null;
            }
        }

        /// <summary>
        /// This diagram constructor creates a new diagram implementation based on a given EA Diagram instance.
        /// <param name="model">Reference to the tool-specific model implementation.</param>
        /// <param name="diagram">EA Diagram instance.</param>
        /// </summary>
        internal EADiagramImplementation(EAModelImplementation model, EA.Diagram diagram) : base(model)
        {
            this._diagram = diagram;
            this._owningPackage = null;
            this._top = 1000;
            this._left = 20;
            if (this._diagram != null)
            {
                this._name = this._diagram.Name;
                this._diagramID = DiagramID;
                this._globalID = this._diagram.DiagramGUID;
                this._owningPackage = new MEPackage(this._diagram.PackageID);
            }
            else
            {
                Logger.WriteError("SparxEA.View.EADiagramImplementation (diagram) >> Failed to retrieve EA Diagram!");
                this._name = string.Empty;
                this._diagramID = -1;
                this._globalID = string.Empty;
                this._owningPackage = null;
            }
        }

        /// <summary>
        /// Add a list of associations to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// The method checks whether a provided association is already present on the diagram, in which case we only update some of the
        /// properties (instead of creating a new instance).
        /// </summary>
        /// <param name="assocList">Associations to add.</param>
        internal override void AddAssociationList(List<MEAssociation> assocList)
        {
            try
            {
                foreach (MEAssociation assoc in assocList)
                {
                    Logger.WriteInfo("SparxEA.View.EADiagramImplementation.AddAssociationList >> Adding / updating association '" + assoc.ElementID + "'...");
                    bool isPresent = false;
                    for (short i = 0; i < this._diagram.DiagramLinks.Count; i++)
                    {
                        var currentLink = this._diagram.DiagramLinks.GetAt(i) as EA.DiagramLink;
                        if (currentLink.ConnectorID == assoc.ElementID)
                        {
                            Logger.WriteInfo("SparxEA.View.EADiagramImplementation.AddAssociationList >> Updating existing association...");
                            currentLink.LineStyle = EA.LinkLineStyle.LineStyleOrthogonalRounded;
                            currentLink.Update();
                            isPresent = true;
                            break;
                        }
                    }

                    if (!isPresent)
                    {
                        Logger.WriteInfo("SparxEA.View.EADiagramImplementation.AddAssociationList >> Adding association ID '" + assoc.ElementID + "'...");
                        DiagramLink diagramLink = this._diagram.DiagramLinks.AddNew("", "") as DiagramLink;
                        diagramLink.ConnectorID = assoc.ElementID;
                        diagramLink.LineStyle = EA.LinkLineStyle.LineStyleOrthogonalRounded;
                        diagramLink.Update();
                    }
                }
            }
            catch
            {
                Logger.WriteError("SparxEA.View.EADiagramImplementation.AddAssociationList >> Exception during diagram update of diagram '" + this._diagram.Name + "'!");
            }
        }

        /// <summary>
        /// Add a list of classes to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// The method checks whether a provided class is already on the diagram, in which case that class is skipped.
        /// </summary>
        /// <param name="classList">Classes to add.</param>
        internal override void AddClassList(List<MEClass> classList)
        {
            try
            {
                foreach (MEClass cl in classList)
                {
                    // Make sure to skip the objects that are already there...
                    bool isPresent = false;
                    for (short i = 0; i < this._diagram.DiagramObjects.Count; i++)
                    {
                        if (((EA.DiagramObject)this._diagram.DiagramObjects.GetAt(i)).ElementID == cl.ElementID)
                        {
                            isPresent = true;
                            break;
                        }
                    }

                    if (!isPresent)
                    {
                        string location = "l=" + this._left + ";r=" + (this._left + _ClassWidth) + ";t=" + this._top + ";b=" + (this._top + _ClassHeight);
                        var diagramObject = this._diagram.DiagramObjects.AddNew(location, "") as DiagramObject;
                        diagramObject.ElementID = cl.ElementID;
                        diagramObject.Update();
                        if (cl.AssociatedDiagram != null) diagramObject.ShowComposedDiagram = true;
                        this._top += _ClassHeight / 2;
                        this._left += _ClassWidth / 2;
                    }
                }
            }
            catch
            {
                string classes = string.Empty;
                bool firstOne = true;
                foreach (MEClass cl in classList)
                {
                    classes += firstOne ? cl.Name : "," + cl.Name;
                    firstOne = false;
                }
                Logger.WriteError("SparxEA.View.EADiagramImplementation.AddClassList >> Exception during diagram update of diagram '" + this._diagram.Name + "' with classes '" + classes + "'!");
            }
        }

        /// <summary>
        /// Create the diagram properties note element for this diagram and add it to the left-top corner. Note that the diagram is
        /// NOT refreshed, facilitating a number of updates to be performed in sequence. Invoke the 'show', 'redraw' or 'refresh'
        /// operations to update the diagram 'on screen'.
        /// </summary>
        internal override void AddDiagramProperties()
        {
            EA.Package diaPackage = ((EAModelImplementation)this._model).Repository.GetPackageByID(this._diagram.PackageID);
            var diaProperties = diaPackage.Elements.AddNew("", "Text") as EA.Element;
            diaProperties.Subtype = 18;
            diaProperties.Update();
            var diagramObject = this._diagram.DiagramObjects.AddNew("l=5;r=245;t=5;b=80", "") as DiagramObject;
            diagramObject.ElementID = diaProperties.ElementID;
            diagramObject.Update();
        }

        /// <summary>
        /// Redraw the diagram, required after one or more 'add' operations to actually show the added elements on the diagram.
        /// Redraw does NOT perform a layout so the original diagram layout will remain untouched.
        /// </summary>
        internal override void Redraw()
        {
            this._diagram.DiagramObjects.Refresh();
            this._diagram.DiagramLinks.Refresh();
            ((EAModelImplementation)this._model).Repository.ReloadDiagram(this._diagram.DiagramID);
        }

        /// <summary>
        /// Refresh the contents of all elements that are shown on the diagram without actually redrawing the entire diagram.
        /// </summary>
        internal override void Refresh()
        {
            this._diagram.DiagramObjects.Refresh();
            this._diagram.DiagramLinks.Refresh();
            ((EAModelImplementation)this._model).Repository.RefreshOpenDiagrams(false);
        }

        /// <summary>
        /// Saves the diagram to the specified file, using specified path. Path must NOT end with a separator
        /// and the file name must NOT have an extension!
        /// The type of diagram to be created depends on current configuration settings.
        /// </summary>
        /// <param name="pathName">Absolute path to use, must NOT end with a separator!</param>
        /// <param name="baseFileName">Optional filename, without extension, when omitted, the diagram name is used instead.</param>
        internal override void Save(string pathName, string baseFileName)
        {
            var repository = ((EAModelImplementation)this._model).Repository;
            var project = repository.GetProjectInterface();

            string fileName = pathName + "\\";
            string diagramType = ContextSlt.GetContextSlt().GetStringSetting(FrameworkSettings._DiagramSaveType).ToLower();

            if (diagramType[0] != '.') diagramType = "." + diagramType;
            if (string.IsNullOrEmpty(baseFileName)) baseFileName = this._name;
            fileName += baseFileName + diagramType;

            bool result = true;
            switch (diagramType)
            {
                case ".bmp":
                case ".png":
                case ".jpg":
                case ".gif":
                case ".tga":
                    result = project.PutDiagramImageToFile(this._diagram.DiagramGUID, fileName, 1);
                    break;

                case ".pdf":
                    result = this._diagram.SaveAsPDF(fileName);
                    break;

                case ".svg":
                case ".svgz":
                    SVGExport.EAPlugin.SaveDiagramAsSvg(repository, this._diagram, fileName);
                    result = true;
                    break;

                default:
                    Logger.WriteError("SparxEA.View.EADiagramImplementation.Save >> Illegal filename '" + fileName + "', configuration error?");
                    result = true;
                    break;
            }
            if (!result)
            {
                Logger.WriteError("SparxEA.View.EADiagramImplementation.Save >> Error saving diagram because: '" + ((EAModelImplementation)this._model).Repository.GetLastError() + "'!");
            }
        }

        /// <summary>
        /// Copies the document to the Windows Clipboard as a device independent bitmap.
        /// </summary>
        internal override void SaveToClipboard()
        {
            var repository = ((EAModelImplementation)this._model).Repository;
            var project = repository.GetProjectInterface();
            if (!project.PutDiagramImageOnClipboard(this._diagram.DiagramGUID, 1))
            {
                Logger.WriteError("SparxEA.View.EADiagramImplementation.SaveToClipboard >> Error saving diagram because: '" + ((EAModelImplementation)this._model).Repository.GetLastError() + "'!");
            }
        }

        /// <summary>
        /// Changes the color of the specified class on the diagram to the specified color.
        /// </summary>
        /// <param name="thisClass">Class to be changed.</param>
        /// <param name="color">Color to assign to the class.</param>
        internal override void SetClassColor(MEClass thisClass, Framework.View.Diagram.ClassColor color)
        {
            if (thisClass == null) return;      // No valid class, do nothing.

            int colorID = -1;
            // Color coding is 3-byte HEX in order Blue-Green-Red
            switch (color)
            {
                case Framework.View.Diagram.ClassColor.Black:
                    colorID = 0x0;
                    break;

                case Framework.View.Diagram.ClassColor.Blue:
                    colorID = 0xFFC057;
                    break;

                case Framework.View.Diagram.ClassColor.Orange:
                    colorID = 0x00C0FF;
                    break;

                case Framework.View.Diagram.ClassColor.Purple:
                    colorID = 0xFFA0FF;
                    break;

                case Framework.View.Diagram.ClassColor.Red:
                    colorID = 0x0066FF;
                    break;

                case Framework.View.Diagram.ClassColor.White:
                    colorID = 0xFFFFFF;
                    break;

                case Framework.View.Diagram.ClassColor.Yellow:
                    colorID = 0x57FFFF;
                    break;

                // Default = 'Default', 'Green' or unknown values:
                default:
                    colorID = 0xA7FEE9;
                    break;
            }

            for (short i = 0; i < this._diagram.DiagramObjects.Count; i++)
            {
                EA.DiagramObject diagramObject = ((EA.DiagramObject)this._diagram.DiagramObjects.GetAt(i));
                if (diagramObject.ElementID == thisClass.ElementID)
                {
                    Logger.WriteInfo("SparxEA.View.EADiagramImplementation.SetClassColor >> Found class '" + 
                                     thisClass.Name + "', changing color to '" + color + "'...");
                    diagramObject.BackgroundColor = colorID;
                    diagramObject.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the 'show connector stereotypes' property of the current diagram.
        /// </summary>
        /// <param name="mustShow">Set to 'true' to show connector stereotypes, 'false' otherwise.</param>
        internal override void ShowConnectorStereotypes (bool mustShow)
        {
            string diagramStyle = this._diagram.StyleEx;
            bool mustUpdate = false;
            Logger.WriteInfo("SparxEA.View.EADiagramImplementation.showConnectorStereotypes >> Existing style: " + diagramStyle);

            if (mustShow)
            {
                if (diagramStyle.Contains("HideConnStereotype=1;"))
                {
                    diagramStyle = diagramStyle.Replace("HideConnStereotype=1;", string.Empty);
                    mustUpdate = true;
                }
            }
            else
            {
                if (!diagramStyle.Contains("HideConnStereotype=1"))
                {
                    Logger.WriteInfo("SparxEA.View.EADiagramImplementation.showConnectorStereotypes >> Removing indicator...");
                    if (diagramStyle.Contains("SaveTag")) diagramStyle = diagramStyle.Replace(";SaveTag", ";HideConnStereotype=1;SaveTag");
                    else diagramStyle += "HideConnStereotype=1;";
                    mustUpdate = true;
                }
            }
            if (mustUpdate)
            {
                Logger.WriteInfo("SparxEA.View.EADiagramImplementation.showConnectorStereotypes >> New style: " + diagramStyle);
                this._diagram.StyleEx = diagramStyle;
                this._diagram.Update();
            }
        }

        /// <summary>
        /// Layout and show a (new) diagram. Must be called after creation (and optionally adding some elements) in order to
        /// actually show the diagram to the user.
        /// </summary>
        internal override void Show()
        {
            this._diagram.DiagramObjects.Refresh();
            this._diagram.DiagramLinks.Refresh();

            EA.Project project = ((EAModelImplementation)this._model).Repository.GetProjectInterface();
            project.LayoutDiagramEx(this._globalID, ConstLayoutStyles.lsLayoutDirectionDown +
                                                    ConstLayoutStyles.lsLayeringOptimalLinkLength +
                                                    ConstLayoutStyles.lsInitializeDFSOut, 4, 30, 30, true);
        }
    }
}
