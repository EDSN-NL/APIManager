using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel;

namespace Plugin.ApplicationBDM.Events
{
    class ColorPickerEvent : MenuEventImplementation
    {
        // Configuration properties used by this module...
        private const string _ColorTargetRoot       = "ColorTargetRoot";
        private const string _ColorTargetList       = "ColorTargetList";
        private const string _ColorTagName          = "ColorTagName";
        private const string _ColorStereotype       = "ColorStereotype";
        private const string _ColorDefnPackageName  = "ColorDefnPackageName";

        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we only have to check whether we are called
        /// from a diagram of the correct type, which is determined by the declaration package of which the diagram is a part.
        /// </summary>
        /// <returns>True on correct context, false otherwise.</returns>
        internal override bool IsValidState()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string validName = context.GetConfigProperty(_ColorDefnPackageName);
            using (Diagram myDiagram = context.CurrentDiagram)
            using (MEPackage myPackage = myDiagram.OwningPackage)
            {
                return myPackage != null && myDiagram != null && myPackage.Name == validName && myDiagram.Name == validName;
            }
        }

        /// <summary>
        /// Processes the 'Color Picker' menu click event. This event is raised when the user right-clicks on a object in the specified
        /// package (and diagram) and after user consent, uses the color info from that object to update all applicable domain packages
        /// with that color info.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorPickerEvent.handleEvent >> Processing a color picker event...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            MEClass sourceClass = context.CurrentClass;         // Contains the class that has been most recently selected by the user.
            Diagram sourceDiagram = context.CurrentDiagram;     // Contains the diagram that has been most recently selected by the user.
            DiagramClassRepresentation representation = sourceDiagram.GetRepresentation(sourceClass);

            int backgroundColor = DiagramClassRepresentation.ColorToInteger(representation.BackgroundColor);
            int fontColor = DiagramClassRepresentation.ColorToInteger(representation.FontColor);
            string domainName = sourceClass.Name;
            string stereotype = context.GetConfigProperty(_ColorStereotype);
            string colorTag = context.GetConfigProperty(_ColorTagName);
            string colorValue = backgroundColor + ":" + fontColor;

            if (MessageBox.Show("Assigning color code '" + colorValue + "' to all domain packages for domain '" + domainName + 
                                "'. Are you sure?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
                panel.ShowPanel("Assign Colors to domain '" + domainName + "'...", 5);

                Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorPickerEvent.handleEvent >> Selected background color '" + backgroundColor +
                                 "' and font color '" + fontColor + "' for domain '" + domainName + "'.");

                // Configuration contains a list of all root packages to be used to look for domain packages that need updates...
                string colorTargetRootName = context.GetConfigProperty(_ColorTargetRoot);
                foreach (string target in context.GetConfigProperty(_ColorTargetList).Split(','))
                {
                    MEPackage domainRootPackage = model.FindPackage(colorTargetRootName, target.Trim());
                    if (domainRootPackage != null)
                    {
                        Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorPickerEvent.handleEvent >> Searching for domain at level '" +
                                         colorTargetRootName + ":" + domainRootPackage.Name + "'...");
                        panel.WriteInfo(0, "Searching for domain at '" + colorTargetRootName + ":" + domainRootPackage.Name + "'...");
                        List<MEPackage> domainPackages = domainRootPackage.FindPackages(domainName, stereotype, true, true);
                        foreach (MEPackage domainPkg in domainPackages)
                        {
                            panel.WriteInfo(1, "Assigning color to '" + domainPkg.Name + "'...");
                            domainPkg.SetTag(colorTag, colorValue, true);
                        }
                        panel.IncreaseBar(1);
                    }
                }
                panel.Done();
            }
        }
    }
}
