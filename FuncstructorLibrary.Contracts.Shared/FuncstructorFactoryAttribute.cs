// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors {
    using System;

    /// <summary>
    /// TODO
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class FuncstructorFactoryAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorFactoryAttribute"/> class.
        /// </summary>
        public FuncstructorFactoryAttribute() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorFactoryAttribute"/> class.
        /// which type should be called must be a <see cref="T:IFuncstructorConfiguration"/>.
        /// </summary>
        /// <param name="factoryType">the type of the factory</param>
        public FuncstructorFactoryAttribute(Type factoryType) {
            this.FactoryType = factoryType;
        }

        /// <summary>
        /// Gets or sets the type of the factory.
        /// </summary>
        public Type FactoryType { get; set; }
    }
}
