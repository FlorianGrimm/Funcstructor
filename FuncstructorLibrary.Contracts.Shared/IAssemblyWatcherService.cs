// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors {
    /// <summary>
    /// Listen at <see cref="System.AppDomain.AssemblyLoad"/>, looks for <see cref="IFuncstructorConfiguration"/> and call <see cref="IFuncstructorConfiguration.Register(IFuncstructorConfigurable)"/>.
    /// </summary>
    public interface IAssemblyWatcherService {
        /// <summary>
        /// When a assembly with <see cref="IFuncstructorConfiguration"/> was loaded this <paramref name="funcstructor"/> will be used.
        /// </summary>
        /// <param name="funcstructor">The funcstructor to use for <see cref="IFuncstructorConfiguration.Register(IFuncstructorConfigurable)"/></param>
        void WireTo(IFuncstructorConfigurable funcstructor);
    }
}
