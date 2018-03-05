using System;
using System.IO;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Base class for a family of capability-specific processing objects. Capability Processors are passed down the hierarchy when
    /// the capability tree is 'handled'. Depending on the type of capability and the desired processing effect, other processors 
    /// might be chosen.
    /// </summary>
    internal abstract class CapabilityProcessor
    {
        // Configuration properties shared by processors...
        protected const string _FileNameTag = "FileNameTag";
        protected const string _PathNameTag = "PathNameTag";

        private string _capabilityClass;  // Is used to group capability processors together based on specific types of capabilities.

        protected Capability _currentCapability;    // Capability that is currently being processed.
        protected Service _currentService;          // Service that is currently being processed.

        /// <summary>
        /// Getter and setter for the class name. We can't pass this through the constructor since we require a default constructor 
        /// for dynamic invocation of instances.
        /// </summary>
        internal string CapabilityClass
        {
            get { return this._capabilityClass; }
            set { this._capabilityClass = value; }
        }

        /// <summary>
        /// Getter and setter for the capability that we want to process. Typically, this is passed through the 'ProcessCapability' method.
        /// However, in some cases we want to process only selective parts of a Capability without going through the entire hierarchy.
        /// In those cases, we can force the 'Current Capability' by assigning it explicitly.
        /// </summary>
        internal Capability CurrentCapability
        {
            get { return this._currentCapability; }
            set { this._currentCapability = value; }
        }

        /// <summary>
        /// Default constructor creates a new instance with the specified capability class name. Classes are used to group capability
        /// processors together for specific purposes. 
        /// </summary>
        protected CapabilityProcessor()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityProcessor >> Creating new processor");
            this._currentCapability = null;
            this._currentService = null;
        }

        /// <summary>
        /// Derived classes must implement processor-specific methods that construct the appropriate filenames for the current
        /// combination of capability and processor type or service and processor type respectively.
        /// The default implementations simply return an empty string!
        /// </summary>
        /// <returns>Filename or null if not feasible.</returns>
        internal virtual string GetCapabilityFilename()   { return string.Empty; }
        internal virtual string GetServiceFilename()      { return string.Empty; }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal abstract string GetID();

        /// <summary>
        /// Since processors are dealt-out as references managed by the CapabilityProcessor manager, we need a separate init. function to 
        /// assure that a processor is properly initialized before use. Derived types MUST implement this method if they have stuff to
        /// initialize and MUST call the base-class initialize method beforehand.
        /// </summary>
        internal virtual void Initialize()
        {
            this._currentCapability = null;
            this._currentService = null;
        }

        /// <summary>
        /// Performs the actual processing on the specified capability. Note that, since we're using a SINGLE capability processor object
        /// to process a possibly large number of capabilities, the processor MUST be stateless between stages!
        /// The function must be properly implemented by derived implementations. This default implementation simply returns false.
        /// </summary>
        /// <param name="capability">The current capability instance that is being processed.</param>
        /// <param name="stage">The processing stage we're in.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal virtual bool ProcessCapability(Capability capability, ProcessingStage stage) { return false; }

        /// <summary>
        /// Performs the actual processing on the service that owns the capabilities. This is invoked in case we're processing a collection
        /// of capabilities, starting at the service. Note that, since we're using a SINGLE capability processor object
        /// to process a possibly large number of capabilities, the processor MUST be stateless between stages!
        /// Sequencer stages will be: (1) processService - (2) 'N' x processCapability
        /// The function must be properly implemented by derived implementations. This default implementation simply returns false.
        /// </summary>
        /// <param name="service">The current service instance that is being processed.</param>
        /// <param name="stage">The processing stage we're in.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal virtual bool ProcessService(Service service, ProcessingStage stage) { return false; }

        /// <summary>
        /// After processing a capability, this method can be invoked to retrieve the result of processing and write this to a file. 
        /// Depending on the type of processor, the method might be overloaded by specialized processors.
        /// </summary>
        /// <returns>True on successfull completion, false on errors.</returns>
        internal virtual bool SaveProcessedCapability()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedCapability >> Saving...");

            // As a safety check, we verify that my service contains a valid absolute path. If not, we attempt
            // to create one...
            string fileName = GetCapabilityFilename();
            string pathName = string.Empty;
            bool result = false;

            if (string.IsNullOrEmpty(this._currentService.FullyQualifiedPath))
            {
                if (!this._currentService.InitializePath())
                {
                    Logger.WriteWarning("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedCapability >> Unable to set path, giving up!");
                    return false;
                }
            }

            FileStream saveStream = null;
            try
            {
                using (saveStream = new FileStream(this._currentService.FullyQualifiedPath + "/" + fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    SaveContents(saveStream);   // Actual 'write-to-stream' operation is processor specific and implemented in specialized processors...
                }
                // Next, we update the file- and path name tags in our capability class...
                this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_FileNameTag), fileName);
                this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), this._currentService.ComponentPath);
                result = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedCapability >> Error writing to '" +
                                  pathName + "\\" + fileName + "' because:\n" + exc.Message);
            }
            finally
            {
                if (saveStream != null) saveStream.Dispose();
            }
            return result;
        }

        /// <summary>
        /// After processing a service, this method can be invoked to retrieve the result of processing and write this to a file. 
        /// Depending on the type of processor, the method might be overloaded by specialized processors.
        /// </summary>
        /// <returns>True on successfull completion, false on errors.</returns>=
        internal virtual bool SaveProcessedService()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedService >> Saving...");

            // As a safety check, we verify that my service contains a valid absolute path. If not, we attempt
            // to create one...
            string fileName = GetServiceFilename();
            string pathName = string.Empty;
            bool result = false;

            // If the service contains a fully qualified path, we have a proper output structure. If not, we invoke InitializePath to create one...
            if (string.IsNullOrEmpty(this._currentService.FullyQualifiedPath))
            {
                if (!this._currentService.InitializePath())
                {
                    Logger.WriteWarning("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedService >> Unable to set path, giving up!");
                    return false;
                }
            }

            FileStream saveStream = null;
            try
            {
                saveStream = new FileStream(this._currentService.FullyQualifiedPath + "\\" + fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                SaveContents(saveStream);   // Actual 'write-to-stream' operation is processor specific and implemented in specialized processors...
                result = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CapabilityProcessor.saveProcessedService >> Error writing to '" +
                                  pathName + "\\" + fileName + "' because:\n" + exc.Message);
            }
            finally
            {
                if (saveStream != null) saveStream.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Must be implemented by specialized Processors to actually write the processed contents of the Capability to the provided 
        /// File Stream. 
        /// </summary>
        /// <param name="stream">Stream that receives processed contents.</param>
        protected abstract void SaveContents(FileStream stream);
    }
}
