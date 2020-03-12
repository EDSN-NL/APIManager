using System.Collections.Generic;

namespace Plugin.Application.CapabilityModel.SchemaGeneration
{
    /// <summary>
    /// This is a simple helper class that implements a Singleton for the processing cache. We need this to assure that all processed classes
    /// and classifiers are properly administered across multiple operation- and message creation passes by possibly different instances of
    /// the Schema Processor.
    /// </summary>
    internal sealed class ClassCacheSlt
    {
        // This is the actual Model singleton. It is created automatically on first load.
        private static readonly ClassCacheSlt _classCacheSlt = new ClassCacheSlt();

        // The actual Caches are implemented as sorted lists...
        private static SortedList<string, ClassifierContext>    _classifierCache;   // Keeps track of processed Classifiers.
        private static SortedList<string, string>               _classCache;        // Keeps track of processed Classes.

        /// <summary>
        /// Removes ALL contents from the cache.
        /// </summary>
        internal void Flush()
        {
            _classifierCache = new SortedList<string, ClassifierContext>();
            _classCache = new SortedList<string, string>();
        }

        /// <summary>
        /// Adds the given key/context pair to the Classifier cache. An exception will be thrown if the key is already in the cache.
        /// </summary>
        /// <param name="classifierKey">Key for the provided context.</param>
        /// <param name="context">Classifier context to add.</param>
        /// <exception cref="ArgumentNullException">Key is NULL</exception>
        /// <exception cref="ArgumentException">Duplicate key</exception>
        /// <exception cref="InvalidOperationException">Internal error.</exception>
        internal void AddClassifierContext(string classifierKey, ClassifierContext context)
        {
            _classifierCache.Add(classifierKey, context);
        }

        /// <summary>
        /// Adds the given key/value pair to the cache. An exception will be thrown if the key is already in the cache.
        /// </summary>
        /// <param name="classKey">Key for the provided qualified name.</param>
        /// <param name="name">Qualified class name.</param>
        /// <exception cref="ArgumentNullException">Key is NULL</exception>
        /// <exception cref="ArgumentException">Duplicate key</exception>
        /// <exception cref="InvalidOperationException">Internal error.</exception>
        internal void AddQualifiedClassName(string classKey, string name)
        {
            _classCache.Add(classKey, name);
        }

        /// <summary>
        /// Public Model "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>ClassCache singleton object</returns>
        internal static ClassCacheSlt GetClassCacheSlt() { return _classCacheSlt; }

        /// <summary>
        /// Returns the Classifier context indexed by the specified key. Returns NULL if key not in cache.
        /// </summary>
        /// <param name="classifierKey">Index of Classifier to return.</param>
        /// <returns>ClassifierContext or NULL if not in cache.</returns>
        internal ClassifierContext GetClassifierContext (string classifierKey)
        {
            return _classifierCache.ContainsKey(classifierKey) ? _classifierCache[classifierKey] : null;
        }

        /// <summary>
        /// Returns the qualified class name given by the specified key. Returns an empty string if key is not in cache.
        /// </summary>
        /// <param name="classKey">Index of qualified name to fetch.</param>
        /// <returns>Qualified class name or empty string if not in cache.</returns>
        internal string GetQualifiedClassName (string classKey)
        {
            return _classCache.ContainsKey(classKey) ? _classCache[classKey] : string.Empty;
        }

        /// <summary>
        /// Checks whether an element with the given key exists in the Class cache.
        /// </summary>
        /// <param name="classKey">Key to be checked.</param>
        /// <returns>True if element in cache, false otherwise.</returns>
        internal bool HasClassKey (string classKey)
        {
            return _classCache.ContainsKey(classKey);
        }

        /// <summary>
        /// Checks whether a classifier with the given key exists in the Classifier cache.
        /// </summary>
        /// <param name="classifierKey">Key to be checked.</param>
        /// <returns>True if classifier in cache, false otherwise.</returns>
        internal bool HasClassifierKey(string classifierKey)
        {
            return _classifierCache.ContainsKey(classifierKey);
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that the cache is ready for use.
        /// </summary>
        private ClassCacheSlt()
        {
            Flush();
        }
    }
}
