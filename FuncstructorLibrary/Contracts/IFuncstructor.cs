// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors
{
    /// <summary>
    /// Provides factories.
    /// </summary>
    public interface IFuncstructor
    {
        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <returns>A new instance.</returns>
        T Resolve<T>(object key);

        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="P1">The type of the parameter</typeparam>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <param name="p1">The parameter.</param>
        /// <returns>A new instance.</returns>
        T Resolve<P1, T>(object key, P1 p1);

        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="P1">The type of the 1st parameter.</typeparam>
        /// <typeparam name="P2">The type of the 2cd parameter.</typeparam>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <param name="p1">The 1st parameter.</param>
        /// <param name="p2">The 2cd parameter.</param>
        /// <returns>A new instance.</returns>
        T Resolve<P1, P2, T>(object key, P1 p1, P2 p2);

        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="P1">The type of the 1st parameter.</typeparam>
        /// <typeparam name="P2">The type of the 2cd parameter.</typeparam>
        /// <typeparam name="P3">The type of the 3rd parameter.</typeparam>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <param name="p1">The 1st parameter.</param>
        /// <param name="p2">The 2cd parameter.</param>
        /// <param name="p3">The 3rd parameter.</param>
        /// <returns>A new instance.</returns>
        T Resolve<P1, P2, P3, T>(object key, P1 p1, P2 p2, P3 p3);
    }
}