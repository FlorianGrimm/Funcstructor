// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors
{
    using System;

    /// <summary>
    /// Provides configuration for IFuncstructor.
    /// </summary>
    public interface IFuncstructorConfigurable : IFuncstructor
    {
        /// <summary>
        /// Gets the parent named <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>The parent or NULL.</returns>
        IFuncstructorConfigurable GetParent(string name);

        /// <summary>
        /// Register a Func.
        /// </summary>
        /// <param name="returnType">The result type.</param>
        /// <param name="key">The key often null.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="funcstructor">The func.</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        bool Register(Type returnType, object key, Type[] parameterTypes, object funcstructor, bool overwrite = false);

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        bool Register<T>(object key, Func<T> funcstructor, bool overwrite = false);

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="P1">The parameter type.</typeparam>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        bool Register<P1, T>(object key, Func<P1, T> funcstructor, bool overwrite = false);

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="P1">The 1st parameter type.</typeparam>
        /// <typeparam name="P2">The 2cd parameter type.</typeparam>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        bool Register<P1, P2, T>(object key, Func<P1, P2, T> funcstructor, bool overwrite = false);

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="P1">The 1st parameter type.</typeparam>
        /// <typeparam name="P2">The 2cd parameter type.</typeparam>
        /// <typeparam name="P3">The 3rd parameter type.</typeparam>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        bool Register<P1, P2, P3, T>(object key, Func<P1, P2, P3, T> funcstructor, bool overwrite = false);
    }
}
