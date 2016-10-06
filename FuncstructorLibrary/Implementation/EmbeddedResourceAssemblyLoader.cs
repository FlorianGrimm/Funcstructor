namespace Brimborium.Funcstructors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable SA1202 // Elements must be ordered by access
#pragma warning disable SA1204 // Static elements must appear before instance elements

    /// <summary>
    /// Loades embedded resourdes
    /// </summary>
    public sealed class EmbeddedResourceAssemblyLoader : IDisposable
    {
        // statics
        private static EmbeddedResourceAssemblyLoader _ActiveInstance;

        /// <summary>Activate the EmbeddedResourceAssemblyLoader.</summary>
        /// <returns>An old or new instance.</returns>
        public static EmbeddedResourceAssemblyLoader Activate()
        {
            if ((object)_ActiveInstance != null) { return _ActiveInstance; }
            // create a new one
            {
                var assemblyLoader = new EmbeddedResourceAssemblyLoader();
                EmbeddedResourceAssemblyLoader._ActiveInstance = assemblyLoader;
                assemblyLoader.Wire();
                return assemblyLoader;
            }
        }

        /// <summary>Deactivate the EmbeddedResourceAssemblyLoader.</summary>
        public static void Deactivate()
        {
            using (EmbeddedResourceAssemblyLoader._ActiveInstance)
            {
                EmbeddedResourceAssemblyLoader._ActiveInstance = null;
            }
        }

        private readonly object _Lock;
        private readonly Dictionary<string, string> _ManifestResourceNames;
        private readonly Dictionary<string, Tuple<string, Assembly>> _AssemblyNameToResource;
        private readonly Dictionary<string, System.Reflection.Assembly> _AssemblyNameToAssembly;
        private bool _Wired;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceAssemblyLoader"/> class.
        /// </summary>
        public EmbeddedResourceAssemblyLoader()
        {
            this._Wired = false;
            this._Lock = new object();
            this._AssemblyNameToResource = new Dictionary<string, Tuple<string, Assembly>>(StringComparer.OrdinalIgnoreCase);
            this._AssemblyNameToAssembly = new Dictionary<string, System.Reflection.Assembly>(StringComparer.OrdinalIgnoreCase);
            GC.SuppressFinalize(this);
            this._ManifestResourceNames = this.GetType().Assembly.GetManifestResourceNames().ToDictionary(_ => _, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EmbeddedResourceAssemblyLoader"/> class.
        /// Good bye.
        /// </summary>
        ~EmbeddedResourceAssemblyLoader() { this.Dispose(false); }

        /// <summary>
        /// Unwire if needed.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.Unwire();
        }

        /// <summary>
        /// Registers the assembly.
        /// </summary>
        /// <param name="resourceName">the embedded resource name.</param>
        /// <param name="assemblyName">the assembly name contained in the resource.</param>
        /// <param name="assembly">The assembly containing the embedded resource.</param>
        public void RegisterAssembly(string resourceName, string assemblyName, Assembly assembly)
        {
            if (!this._ManifestResourceNames.ContainsKey(resourceName))
            {
                throw new ArgumentException($"Unsupported manifestResourceName:{resourceName}.", nameof(resourceName));
            }
            if (!string.Equals(resourceName, this._ManifestResourceNames[resourceName], StringComparison.Ordinal))
            {
                throw new ArgumentException($"Different manifestResourceName:{resourceName} vs {this._ManifestResourceNames[resourceName]}.", nameof(resourceName));
            }
            string[] nameParts = assemblyName.Split(',');
            string shortName = nameParts[0];
            string versionLessName = string.Join(",", nameParts.Where(_ => !_.StartsWith(" Version=", StringComparison.Ordinal)));
            var resourceNameAssembly = new Tuple<string, Assembly>(resourceName, assembly);
            this._AssemblyNameToResource.Add(assemblyName, resourceNameAssembly);
            this._AssemblyNameToResource.Add(shortName, resourceNameAssembly);
            this._AssemblyNameToResource.Add(versionLessName, resourceNameAssembly);
        }

        /// <summary>
        /// Wire AssemblyResolve
        /// </summary>
        /// <returns>true if currently wired.</returns>
        public bool Wire()
        {
            if (!this._Wired)
            {
                this._Wired = true;
                System.AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
                GC.ReRegisterForFinalize(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Unwire AssemblyResolve
        /// </summary>
        /// <returns>true if currently unwired.</returns>
        public bool Unwire()
        {
            if (this._Wired)
            {
                this._Wired = false;
                System.AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
                GC.SuppressFinalize(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!this._Wired) { return null; }
            System.Reflection.Assembly result = null;
            string assName = args.Name;
            if (assName.StartsWith("System") || assName.StartsWith("Microsoft")) { return result; }

            string[] nameParts = assName.Split(',');
            string shortName = nameParts[0];
            string versionLessName = string.Join(",", nameParts.Where(_ => !_.StartsWith(" Version=", StringComparison.Ordinal)));
            lock (this._Lock)
            {
                if (!this._AssemblyNameToAssembly.TryGetValue(assName, out result))
                {
                    if (!this._AssemblyNameToAssembly.TryGetValue(versionLessName, out result))
                    {
                        this._AssemblyNameToAssembly.TryGetValue(shortName, out result);
                    }
                }
            }
            if (result == null)
            {
                Tuple<string, Assembly> resourceNameAssembly;
                foreach (var assemblyName in new string[] { assName, versionLessName, shortName })
                {
                    bool found;
                    lock (this._Lock)
                    {
                        found = this._AssemblyNameToResource.TryGetValue(assemblyName, out resourceNameAssembly);
                    }
                    if (this._AssemblyNameToResource.TryGetValue(assemblyName, out resourceNameAssembly))
                    {
                        try
                        {
                            var content = GetManifestResourceBytes(resourceNameAssembly);
                            if (content == null)
                            {
#warning logging needed
                                System.Diagnostics.Debug.WriteLine($"Cannot find resource '{resourceNameAssembly}' for assembly '{assemblyName}'.");
                                result = null;
                            }
                            else
                            {
                                result = System.Reflection.Assembly.Load(content);
                                break;
                            }
                        }
                        catch (Exception exception)
                        {
                            result = null;
#warning logging needed
                            System.Diagnostics.Debug.WriteLine($"Error while loading assembly '{assemblyName}' - {exception.ToString()}");
                        }
                    }
                }
                //
                if (result == null)
                {
# warning logging needed
                    System.Diagnostics.Debug.WriteLine($"Cannot load assembly '{assName}'.");
                }
                else
                {
                    var fn = result.FullName;
                    lock (this._Lock)
                    {
                        this._AssemblyNameToAssembly[fn] = result;
                        this._AssemblyNameToAssembly[assName] = result;
                        this._AssemblyNameToAssembly[shortName] = result;
                        this._AssemblyNameToAssembly[versionLessName] = result;
                    }
                }
            }
            return result;
        }

        private static byte[] GetManifestResourceBytes(Tuple<string, Assembly> resourceName)
        {
            using (var stream = resourceName.Item2.GetManifestResourceStream(resourceName.Item1))
            {
                if (stream == null)
                {
#warning logging needed
                    //EventLogger.WriteEntry($"Cannot find ManifestResourceStream {resourceName} in {ass.FullName} @ {ass.Location}.", System.Diagnostics.EventLogEntryType.Error, 7);
                    return null;
                }
                else
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
        }    
    }
}
