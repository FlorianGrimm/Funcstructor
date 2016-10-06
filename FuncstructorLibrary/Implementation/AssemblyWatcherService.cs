// Funcstructor
// MIT License
// Copyright (c) 2016 Florian Grimm

namespace Brimborium.Funcstructors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

#pragma warning disable SA1306
#pragma warning disable SA1501

    /// <summary>
    /// Watch for assembly load and calls Register to the wired.
    /// </summary>
    public class AssemblyWatcherService : IAssemblyWatcherService
    {
        private bool _IsWired;
        private IFuncstructorConfigurable[] _FuncstructorConfigurables;
        private bool _CallFuncstructorConfigurations;
        private List<IFuncstructorConfiguration> _FuncstructorConfigurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyWatcherService"/> class.
        /// </summary>
        public AssemblyWatcherService()
        {
            this._FuncstructorConfigurables = new IFuncstructorConfigurable[0];
            this._FuncstructorConfigurations = new List<IFuncstructorConfiguration>();
        }

        /// <summary>
        /// When a assembly with <see cref="IFuncstructorConfiguration"/> was loaded this <paramref name="funcstructor"/> will be used.
        /// </summary>
        /// <param name="funcstructor">The funcstructor to use for <see cref="IFuncstructorConfiguration.Register(IFuncstructorConfigurable)"/></param>
        public void WireTo(IFuncstructorConfigurable funcstructor)
        {
            if (funcstructor == null) { throw new ArgumentNullException(nameof(funcstructor)); }
            lock (this)
            {
                var l = this._FuncstructorConfigurables.ToList();
                l.Add(funcstructor);
                this._FuncstructorConfigurables = l.ToArray();
            }

            this.Wire();
        }

        /// <summary>
        /// Wires AssemblyLoad to this.
        /// </summary>
        public void Wire()
        {
            if (this._IsWired) { return; }
            this._IsWired = true;
            System.AppDomain.CurrentDomain.AssemblyLoad += this.CurrentDomain_AssemblyLoad;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies().ToArray())
            {
                this.ScanAssembly(assembly);
            }

            this.CallFuncstructorConfigurations();
        }

        /// <summary>
        /// Unwires AssemblyLoad to this.
        /// </summary>
        public void Unwire()
        {
            if (!this._IsWired) { return; }
            this._IsWired = false;
            System.AppDomain.CurrentDomain.AssemblyLoad -= this.CurrentDomain_AssemblyLoad;
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var assembly = args.LoadedAssembly;
            this.ScanAssembly(assembly);
            this.CallFuncstructorConfigurations();
        }

        private void ScanAssembly(System.Reflection.Assembly assembly)
        {
            var name = assembly.GetName();
            var assembly_Name = name.Name;
            if (assembly_Name.StartsWith("System.")) { return; }
            if (assembly_Name.StartsWith("Microsoft.")) { return; }
            var publicKeyToken = name.GetPublicKeyToken();
            if (publicKeyToken != null)
            {
                if ((publicKeyToken[0] == 0xb7)
                    && (publicKeyToken[1] == 0x7a)
                    && (publicKeyToken[2] == 0x5c)
                    && (publicKeyToken[3] == 0x56)
                    && (publicKeyToken[4] == 0x19)
                    && (publicKeyToken[5] == 0x34)
                    && (publicKeyToken[6] == 0xe0)
                    && (publicKeyToken[7] == 0x89))
                {
                    return;
                }

                if ((publicKeyToken[0] == 0xb0)
                    && (publicKeyToken[1] == 0x3f)
                    && (publicKeyToken[2] == 0x5f)
                    && (publicKeyToken[3] == 0x7f)
                    && (publicKeyToken[4] == 0x11)
                    && (publicKeyToken[5] == 0xd5)
                    && (publicKeyToken[6] == 0x0a)
                    && (publicKeyToken[7] == 0x3a))
                {
                    return;
                }
            }

            var hefezopfContractsAssemblyName = typeof(FuncstructorConfigurationAttribute).Assembly.GetName().Name;
            bool found = false;
            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (string.Equals(referencedAssemblyName.Name, hefezopfContractsAssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                }
            }

            if (!found) { return; }

            // look for FuncstructorConfigurationAttribute
            {
                var attrs = assembly.GetCustomAttributes<FuncstructorConfigurationAttribute>();
                foreach (var attr in attrs)
                {
                    var configurationType = attr.ConfigurationType;
                    var constructor = configurationType.GetConstructor(Type.EmptyTypes);
                    var instance = (IFuncstructorConfiguration)constructor.Invoke(new object[0]);
                    this._FuncstructorConfigurations.Add(instance);
                }
            }

            // look for FuncstructorFactoryAttribute
            {
                var attrs = assembly.GetCustomAttributes<FuncstructorFactoryAttribute>();
                foreach (var attr in attrs)
                {
                    var factoryType = attr.FactoryType;
                    var methods = factoryType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    foreach (var method in methods)
                    {
                        var methodAttrs = method.GetCustomAttributes<FuncstructorRegisterAttribute>(false);
                        foreach (var methodAttr in methodAttrs)
                        {
                            // method.Invoke(null, parameters)
                            this._FuncstructorConfigurations.Add(new MethodFuncstructorConfiguration(method, methodAttr.Key));
                        }
                    }
                }
            }

            // look for AssemblyInjectionAttribute
            {
                var attrs = assembly.GetCustomAttributes<AssemblyInjectionAttribute>();
                foreach (var attr in attrs)
                {
                    EmbeddedResourceAssemblyLoader.Activate().RegisterAssembly(
                        attr.EmbeddedResource,
                        attr.AssemblyName,
                        assembly);
                }
            }
        }

        private void CallFuncstructorConfigurations()
        {
            if (this._CallFuncstructorConfigurations) { return; }
            //
            this._CallFuncstructorConfigurations = true;
            try
            {
                while (this._FuncstructorConfigurations.Count > 0)
                {
                    var funcstructorConfigurations = System.Threading.Interlocked.Exchange(
                        ref this._FuncstructorConfigurations,
                        new List<IFuncstructorConfiguration>());
                    System.Threading.Interlocked.MemoryBarrier();
                    if (funcstructorConfigurations.Count > 0)
                    {
                        var funcstructorConfigurables = this._FuncstructorConfigurables;
                        var funcstructorConfigurablesLength = funcstructorConfigurables.Length;
                        foreach (var funcstructorConfiguration in funcstructorConfigurations)
                        {
                            for (int idxFunc = 0; idxFunc < funcstructorConfigurablesLength; idxFunc++)
                            {
                                IFuncstructorConfigurable funcstructorConfigurable = funcstructorConfigurables[idxFunc];
                                funcstructorConfiguration.Register(funcstructorConfigurable);
                            }
                        }
                    }
                }
            }
            finally
            {
                this._CallFuncstructorConfigurations = false;
            }
        }

        /// <summary>
        /// Helper for dynamic method binding - FuncstructorRegisterAttribute.
        /// </summary>
        internal class MethodFuncstructorConfiguration : IFuncstructorConfiguration
        {
            private object _Key;
            private MethodInfo _MethodInfo;

            /// <summary>
            /// Initializes a new instance of the <see cref="MethodFuncstructorConfiguration"/> class.
            /// </summary>
            /// <param name="methodInfo">The decorated method.</param>
            /// <param name="key">The Key in the attribute.</param>
            internal MethodFuncstructorConfiguration(MethodInfo methodInfo, object key)
            {
                this._MethodInfo = methodInfo;
                this._Key = key;
            }

            /// <summary>
            /// The decorated method should be registed.
            /// </summary>
            /// <param name="funcstructor">The given funcstructor to configure.</param>
            public void Register(IFuncstructorConfigurable funcstructor)
            {
                var returnType = this._MethodInfo.ReturnType;
                var methodParameters = this._MethodInfo.GetParameters();
                var parameterTypes = new Type[methodParameters.Length];
                var genericParameterTypes = new Type[methodParameters.Length + 1];
                for (int idx = 0; idx < methodParameters.Length; idx++)
                {
                    var methodParameter = methodParameters[idx];
                    var parameterType = methodParameter.ParameterType;
                    parameterTypes[idx] = parameterType;
                    genericParameterTypes[idx] = parameterType;
                }

                genericParameterTypes[methodParameters.Length] = returnType;
                //
                Type typeFunc;
                switch (methodParameters.Length)
                {
                    case 0:
                        typeFunc = typeof(Func<>);
                        break;
                    case 1:
                        typeFunc = typeof(Func<,>);
                        break;
                    case 2:
                        typeFunc = typeof(Func<,,>);
                        break;
                    case 3:
                        typeFunc = typeof(Func<,,,>);
                        break;
                    case 4:
                        typeFunc = typeof(Func<,,,,>);
                        break;
                    case 5:
                        typeFunc = typeof(Func<,,,,,>);
                        break;
                    case 6:
                        typeFunc = typeof(Func<,,,,,,>);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var typeFuncT = typeFunc.MakeGenericType(genericParameterTypes);
                var delegateT = Delegate.CreateDelegate(typeFuncT, this._MethodInfo);
                funcstructor.Register(returnType, this._Key, parameterTypes, delegateT, false);
            }
        }
    }
}
