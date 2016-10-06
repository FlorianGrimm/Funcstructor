// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors
{
    /// <summary>
    /// If a <see cref="FuncstructorConfigurationAttribute"/> reference the implemention a instance will be created and Register will be called.
    /// </summary>
    public interface IFuncstructorConfiguration
    {
        /// <summary>
        /// Should register the funcstructors of that assembly.
        /// </summary>
        /// <param name="funcstructor">The funcstructor that should be used for registration.</param>
        void Register(IFuncstructorConfigurable funcstructor);
    }
}
