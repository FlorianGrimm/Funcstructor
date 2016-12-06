namespace Brimborium.Funcstructors {
    using System;

    /// <summary>
    /// Register an assembly to load from the embedded resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyInjectionAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInjectionAttribute"/> class.
        /// </summary>
        public AssemblyInjectionAttribute() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInjectionAttribute"/> class.
        /// </summary>
        /// <param name="assemblyName">The assembly name</param>
        /// <param name="embeddedResource">The assembly embedded resource name</param>
        public AssemblyInjectionAttribute(string assemblyName, string embeddedResource) {
            this.AssemblyName = assemblyName;
            this.EmbeddedResource = embeddedResource;
        }

        /// <summary>
        /// Gets or sets assembly name.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets EmbeddedResource name.
        /// </summary>
        public string EmbeddedResource { get; set; }
    }
}
