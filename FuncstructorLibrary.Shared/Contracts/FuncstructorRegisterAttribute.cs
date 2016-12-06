// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors {
    using System;

    /// <summary>
    /// Register this method as funcstructor for Key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class FuncstructorRegisterAttribute : Attribute {
        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorRegisterAttribute"/> class.
        /// </summary>
        public FuncstructorRegisterAttribute() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuncstructorRegisterAttribute"/> class.
        /// </summary>
        /// <param name="key">TODO</param>
        public FuncstructorRegisterAttribute(object key) {
            this.Key = key;
        }

        /// <summary>
        /// Gets or sets the key for registration.
        /// </summary>
        public object Key { get; set; }
    }
}
