using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// This implements the Processor Manager, a component that creates a list of defined Capability Processor objects. These can 
    /// subsequently be retrieved by searching for their class name (a grouping of processors) and the Processor ID.
    /// </summary>
    internal sealed class ProcessorManagerSlt
    {
        // Configuration properties used by this module:
        private const string _CapabilityProcessor =  "CapabilityProcessor";

        // This is the actual Context singleton. It is created automatically on first load.
        private static readonly ProcessorManagerSlt _processorManagerSlt = new ProcessorManagerSlt();

        private bool _initialized;
        private SortedList<string, List<CapabilityProcessor>> _processorList;

        /// <summary>
        /// Public Processor Manager "factory" method. Simply returns the static instance. Also assures that the instance is initialized.
        /// </summary>
        /// <returns>Processor Manager singleton object</returns>
        internal static ProcessorManagerSlt GetProcessorManagerSlt()
        {
            _processorManagerSlt.Initialize();
            return _processorManagerSlt;
        }

        /// <summary>
        /// This method returns a CapabilityProcessor instance based on the capability class and a valid processor ID.
        /// </summary>
        /// <param name="capabilityClass">The class in which to search.</param>
        /// <param name="ID">ID of the processor we're looking for.</param>
        /// <returns>Capability processor or NULL if not found.</returns>
        internal CapabilityProcessor GetProcessorByID(string capabilityClass, string ID)
        {
            if (this._processorList.ContainsKey(capabilityClass))
            {
                foreach (CapabilityProcessor proc in this._processorList[capabilityClass])
                {
                    string capID = proc.GetID();
                    if (!string.IsNullOrEmpty(capID) && capID == ID)
                    {
                        proc.Initialize();
                        return proc;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// This method returns a CapabilityProcessor instance based on the capability class and an index in the list of
        /// processors.
        /// </summary>
        /// <param name="capabilityClass">The class in which to search.</param>
        /// <param name="index">Index to use, if out of bounds, nothing will be returned.</param>
        /// <returns>Capability processor or NULL if not found.</returns>
        internal CapabilityProcessor GetProcessorByIndex(string capabilityClass, int index)
        {
            if (index >= 0 && this._processorList.ContainsKey(capabilityClass) && this._processorList[capabilityClass].Count > index)
            {
                CapabilityProcessor proc = this._processorList[capabilityClass][index];
                proc.Initialize();
                return proc;
            }
            return null;
        }

        /// <summary>
        /// Returns the number of CapabilityProcessor instances that are registered for the given class.
        /// </summary>
        /// <param name="capabilityClass">Capability class to check.</param>
        /// <returns>Number of registered processors.</returns>
        internal int GetProcessorCount(string capabilityClass)
        {
            return (this._processorList.ContainsKey(capabilityClass)) ? this._processorList[capabilityClass].Count : 0;
        }

        /// <summary>
        /// This method returns a list of all Capability Processor identifiers for the specified class.
        /// </summary>
        /// <param name="capabilityClass">Name of Class to search.</param>
        /// <returns>List of ID's (could be empty).</returns>
        internal List<string> GetProcessorIDList(string capabilityClass)
        {
            var IDList = new List<string>();
            if (this._processorList.ContainsKey(capabilityClass))
            {
                foreach (CapabilityProcessor proc in this._processorList[capabilityClass])
                {
                    string capID = proc.GetID();
                    if (!string.IsNullOrEmpty(capID)) IDList.Add(capID);
                }
            }
            return IDList;
        }

        /// <summary>
        /// This is called from within the 'getProcessorManagerSlt' method. We use a lazy-initialization mechanism, in which the processor
        /// manager is initialized only when it's going to be used. The initialization method reads all defined processors from the configuration
        /// and creates instances for them, which are registered by 'class'. 
        /// </summary>
        private void Initialize()
        {
            if (!this._initialized)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                this._processorList = new SortedList<string, List<CapabilityProcessor>>();
                List<string> processorDefns = context.GetConfigPropertyList(_CapabilityProcessor);

                foreach (string processorDefn in processorDefns)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.ProcessorManagerSlt.initialize >> Processing: '" + processorDefn + "'...");
                    string capabilityClass = processorDefn.Substring(0, processorDefn.IndexOf(':'));
                    string processorClassFQN = processorDefn.Substring(processorDefn.IndexOf(':') + 1);

                    if (!string.IsNullOrEmpty(processorClassFQN))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.ProcessorManagerSlt.initialize >> We have processor FQN '" 
                                         + processorClassFQN + "', try to create an instance...");
                        try
                        {
                            ObjectHandle handle = Activator.CreateInstance(null, processorClassFQN);
                            var proc = handle.Unwrap() as CapabilityProcessor;
                            if (proc != null)
                            {
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.ProcessorManagerSlt.initialize >> Valid processor instance, going to register...");
                                proc.CapabilityClass = capabilityClass;
                                if (!this._processorList.ContainsKey(capabilityClass))
                                    this._processorList.Add(capabilityClass, new List<CapabilityProcessor>());
                                this._processorList[capabilityClass].Add(proc);
                            }
                            else
                            {
                                Logger.WriteWarning("Plugin.Application.CapabilityModel.ProcessorManagerSlt.initialize >> Invalid Capability Processor definition: '" + processorClassFQN + "'!");
                            }
                        }
                        catch (Exception)
                        {
                            Logger.WriteWarning("Plugin.Application.CapabilityModel.ProcessorManagerSlt.initialize >> Invalid Capability Processor definition: '" + processorClassFQN + "'!");
                        }
                    }
                    this._initialized = true;
                }
            }
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that exactly one valid object is present at all times.
        /// </summary>
        private ProcessorManagerSlt()
        {
            this._initialized = false;
            this._processorList = null;
        }
    }
}
