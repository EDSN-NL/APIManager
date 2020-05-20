using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Events.Util
{
    class DeployApplicationListEvent : MenuEventImplementation
    {
        // Configuration properties used by this module:
        private const string _CMDBStagingPackageName            = "CMDBStagingPackageName";
        private const string _ApplicationListRootPath           = "ApplicationListRootPath";
        private const string _CMDBApplicationStereotype         = "CMDBApplicationStereotype";
        private const string _ArchimateApplicationStereotype    = "ArchimateApplicationStereotype";


        /// <summary>
        /// This event is valid when we're either at the root of the CMDB staging area, or one of it's direct decendants...
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() 
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEPackage currentPackage = context.CurrentPackage;
            string rootName = context.GetConfigProperty(_CMDBStagingPackageName);
            return currentPackage != null && (currentPackage.Name == rootName || (currentPackage.Parent != null && currentPackage.Parent.Name == rootName));
        }

        /// <summary>
        /// Specialty event that is used to copy the CMDB master application list to our chosen Reference Data destination package. We copy ALL
        /// Archimate Application components to a new location and assign our specialty "CMDBApplication" stereotype, which is an Archimate specialization
        /// that adds all CMDB metadata.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.DeployApplicationListEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            MEPackage deployToPkg = null;

            try
            {
                // We must be called either from the CMDB staging root, or one of it's children. When called from the root, we will process all
                // sub-packages and when called from a sub-package, we will process only that package.
                MEPackage currentPackage = context.CurrentPackage;
                string rootName = context.GetConfigProperty(_CMDBStagingPackageName);
                string destinationPkg = context.GetConfigProperty(_ApplicationListRootPath);
                string pkgName = destinationPkg.Substring(destinationPkg.LastIndexOf(":") + 1);
                string pkgPath = destinationPkg.Substring(0, destinationPkg.LastIndexOf(":"));
                List<MEClass> newClasses = new List<MEClass>();
                ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
                string userName;

                deployToPkg = ModelSlt.GetModelSlt().FindPackage(pkgPath, pkgName);
                if (deployToPkg == null)
                {
                    MessageBox.Show("We could not find our deployment destination '" + destinationPkg + "', aborting!", 
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (deployToPkg.IsLocked(out userName) == MEPackage.LockStatus.Locked)
                {
                    MessageBox.Show("Our deployment destination '" + destinationPkg + "', is locked by user '" + userName + "'; unable to continue!",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!deployToPkg.Lock())
                {
                    MessageBox.Show("Unable to lock deployment destination '" + destinationPkg + "'; aborting!",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (currentPackage.Name == rootName)
                {
                    Logger.WriteInfo("Plugin.Application.Events.Util.DeployApplicationListEvent.handleEvent >> Called from staging root, process all sub-packages...");
                    List<MEPackage> children = new List<MEPackage>();
                    int totalClasses = 0;

                    foreach (MEPackage child in currentPackage.Packages)
                    {
                        children.Add(child);
                        totalClasses += child.ClassCount;
                    }
                    panel.ShowPanel("Processing '" + totalClasses + "' classes in all packages under '" + rootName + "'...", totalClasses);

                    foreach (MEPackage child in children)
                    {
                        panel.WriteInfo(0, "Processing package: '" + child.Name + "'...");
                        foreach (MEClass newClass in ProcessPackage(deployToPkg, child)) newClasses.Add(newClass);
                    }
                    panel.Done();
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.Events.Util.DeployApplicationListEvent.handleEvent >> Called from child, processing package '" + currentPackage.Name + "'...");
                    int totalClasses = currentPackage.ClassCount;
                    panel.ShowPanel("Processing '" + totalClasses + "' classes in package '" + currentPackage.Name + "'...", totalClasses);
                    foreach (MEClass newClass in ProcessPackage(deployToPkg, currentPackage)) newClasses.Add(newClass);
                    panel.Done();
                }
                if (newClasses.Count > 0)
                {
                    Logger.WriteInfo("Plugin.Application.Events.Util.DeployApplicationListEvent.handleEvent >> We have created new classes, add to diagram...");
                    Diagram deploymentDiagram = deployToPkg.FindDiagram(deployToPkg.Name);
                    if (deploymentDiagram != null)
                    {
                        deploymentDiagram.AddClassList(newClasses);
                        deploymentDiagram.Redraw();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.Events.Util.DeployApplicationListEvent.handleEvent >> Caught an exception: '" + exc.ToString() + "'.");
            }
            finally
            {
                if (deployToPkg != null) deployToPkg.Unlock();
            }
        }

        /// <summary>
        /// Process all classes within the specified package and send the contents to the specified deploymentPackage.
        /// Note that we do NOT recursively process sub-packages.
        /// </summary>
        /// <param name="deploymentPackage">Will receive all processed classes.</param>
        /// <param name="thisPackage">Package to be processed.</param>
        /// <returns>A list of classes that have been created (instead of updated)</returns>
        private List<MEClass> ProcessPackage (MEPackage deploymentPackage, MEPackage thisPackage)
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.DeployApplicationListEvent.processPackage >> Processing Package '" + thisPackage.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string targetStereotype = context.GetConfigProperty(_CMDBApplicationStereotype);
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            List<MEClass> newClassList = new List<MEClass>();
            panel.WriteInfo(1, "Processing package '" + thisPackage.Name + "'...");

            // We use 'find' instead of 'get', since this might perform better...
            foreach (MEClass currClass in thisPackage.FindClasses(null, context.GetConfigProperty(_ArchimateApplicationStereotype)))
            {
                try
                {
                    panel.WriteInfo(2, "Deploying Application: '" + currClass.Name + "'...");
                    List<Tuple<string, string>> tagList = new List<Tuple<string, string>>();
                    string classID = currClass.GetTag("app_id");
                    string appID = currClass.AliasName;
                    if (string.IsNullOrEmpty(classID) || classID.Contains("NULL")) classID = null;

                    tagList.Add(new Tuple<string, string>("activeIndicator", currClass.GetTag("app_actief")));
                    tagList.Add(new Tuple<string, string>("currentVersion", currClass.GetTag("app_versie")));
                    tagList.Add(new Tuple<string, string>("department", currClass.GetTag("app_afdeling")));
                    tagList.Add(new Tuple<string, string>("deploymentType", currClass.GetTag("app_type")));
                    tagList.Add(new Tuple<string, string>("expertName", currClass.GetTag("app_expert")));
                    tagList.Add(new Tuple<string, string>("ID", !string.IsNullOrEmpty(classID)? classID: "NULL"));
                    tagList.Add(new Tuple<string, string>("importance", currClass.GetTag("app_belang")));
                    tagList.Add(new Tuple<string, string>("lastChangedDateTime", currClass.GetTag("app_lastchange")));
                    tagList.Add(new Tuple<string, string>("lastVersion", currClass.GetTag("app_oude_versie")));
                    tagList.Add(new Tuple<string, string>("ownerName", currClass.GetTag("app_eigenaar")));
                    tagList.Add(new Tuple<string, string>("source", currClass.GetTag("app_bron")));
                    tagList.Add(new Tuple<string, string>("status", currClass.GetTag("app_status")));
                    tagList.Add(new Tuple<string, string>("synonym", currClass.GetTag("app_synoniem")));

                    MEClass myClass = null;
                    bool processThis = true;
                    // Application ID's are not consistent! Therefor, we FIRST check against the Alias (CMDB APP-ID). 
                    // If that has a value, we try to locate the class using that value. When we don't have an APP-ID, we
                    // check against the DIS number, which is in classID. If that also is NULL, we use the name of the application.
                    if (!string.IsNullOrEmpty(appID) && !appID.Contains("NULL"))
                    {
                        List<MEClass> resultList = deploymentPackage.FindClasses(appID, targetStereotype, true);
                        if (resultList.Count == 1) myClass = resultList[0];     // There should at MOST be one application with this ID!
                        if (resultList.Count > 1)
                        {
                            string applicationNames = string.Empty;
                            bool firstOne = true;
                            foreach (MEClass appClass in resultList)
                            {
                                applicationNames += firstOne ? appClass.Name : ", " + appClass.Name;
                                firstOne = false;
                            }
                            if (MessageBox.Show("APP-ID '" + appID + "' exists '" + resultList.Count + "' times in destination package '" +
                                                deploymentPackage.Name + "' for applications: '" + applicationNames + "'! Application has NOT been updated.",
                                                "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel) return newClassList;
                            processThis = false;
                        }
                    }
                    else if (classID != null)
                    {
                        List<MEClass> resultList = deploymentPackage.FindClassesByTag("ID", classID);
                        if (resultList.Count == 1) myClass = resultList[0];     // There should at MOST be one application with this ID!
                        if (resultList.Count > 1)
                        {
                            string applicationNames = string.Empty;
                            bool firstOne = true;
                            foreach (MEClass appClass in resultList)
                            {
                                applicationNames += firstOne ? appClass.Name : ", " + appClass.Name;
                                firstOne = false;
                            }
                            if (MessageBox.Show("DIS-ID '" + classID + "' exists '" + resultList.Count + "' times in destination package '" +
                                                deploymentPackage.Name + "' for applications: '" + applicationNames + "'! Application has NOT been updated.",
                                                "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel) return newClassList;
                            processThis = false;
                        }
                    }

                    if (processThis)
                    {
                        Tuple<MEClass, bool> response = deploymentPackage.CreateOrUpdateClass(currClass.Name, currClass.AliasName, targetStereotype,
                                                                                              currClass.Annotation, tagList, myClass);
                        //response.Item1.BuildNumber = currClass.BuildNumber;   // We must copy the "phase" attribute as well. 
                        if (response.Item2) newClassList.Add(response.Item1);   // When Item2 == true we have created a new class instead of just updated one.
                    }
                }
                catch (Exception exc)
                {
                    panel.WriteError(2, "Processing failed because: " + exc.ToString());
                }
                panel.IncreaseBar(1);
            }
            return newClassList;
        }
    }
}
