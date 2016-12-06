// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors {
    using System;

    /// <summary>
    /// Defines a type which implements <see cref="T:IFuncstructorConfiguration"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class FuncstructorConfigurationAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorConfigurationAttribute"/> class.
        /// </summary>
        public FuncstructorConfigurationAttribute() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorConfigurationAttribute"/> class.
        /// which type should be called must be a <see cref="T:IFuncstructorConfiguration"/>.
        /// </summary>
        /// <param name="configurationType">the type registers the factories.</param>
        public FuncstructorConfigurationAttribute(Type configurationType) {
            this.ConfigurationType = configurationType;
        }

        /// <summary>
        /// Gets or sets the type that will configure the factories.
        /// </summary>
        public Type ConfigurationType { get; set; }
    }
}
