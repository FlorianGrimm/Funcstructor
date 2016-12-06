// <copyright file="Funcstructor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Brimborium.Funcstructors {
    using System;
    using System.Collections;
    using System.Threading;

    //#pragma warning disable SA1001
    //#pragma warning disable SA1008
    //#pragma warning disable SA1309
    //#pragma warning disable SA1313
    //#pragma warning disable SA1119
    //#pragma warning disable SA1130
    //#pragma warning disable SA1300
    //#pragma warning disable SA1401
    //#pragma warning disable SA1410
    //#pragma warning disable SA1600

    /// <summary>
    /// TODO
    /// </summary>
    public sealed class Funcstructor
        : IFuncstructor
        , IFuncstructorConfigurable {
        /// <summary>
        /// The name of the funcstrucotr.
        /// </summary>
        public readonly string Name;
        private readonly bool _ContainsInstances;
        private ReaderWriterLockSlim _ReaderWriterLock;
        private Hashtable _RegistryItemState;
        private Funcstructor _Parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Funcstructor"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent - can be null</param>
        public Funcstructor(string name, Funcstructor parent) {
            this._ReaderWriterLock = new ReaderWriterLockSlim();
            this._RegistryItemState = new Hashtable();
            this.Name = name ?? string.Empty;
            this._Parent = parent;
            this._ContainsInstances = true;
        }

        private Funcstructor(string name, Funcstructor parent, bool containsInstances) {
            this._ReaderWriterLock = new ReaderWriterLockSlim();
            this._RegistryItemState = new Hashtable();
            this.Name = name ?? string.Empty;
            this._Parent = parent;
            this._ContainsInstances = containsInstances;
        }

        /// <summary>
        /// Create the default chain LateLoad-LateInstance-Root
        /// </summary>
        /// <param name="assemblyWatcherService">The service - can be null, but...</param>
        /// <returns>the root instance of the default chain.</returns>
        public static Funcstructor CreateDefault(IAssemblyWatcherService assemblyWatcherService) {
            var instanceConfiguration = new Funcstructor(FuncstructorConstant.ParentNameLateLoad, null, false);
            var instanceLateInstance = new Funcstructor(FuncstructorConstant.ParentNameLateInstance, instanceConfiguration, false);
            var instanceRoot = new Funcstructor(FuncstructorConstant.ParentNameRoot, instanceLateInstance, true);
            if ((object)assemblyWatcherService == null) {
                assemblyWatcherService.WireTo(instanceRoot);
            }

            return instanceRoot;
        }

        /// <summary>
        /// Gets the parent named <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <returns>The parent or NULL.</returns>
        public IFuncstructorConfigurable GetParent(string name) {
            if (name == null) {
                return this._Parent;
            } else {
                for (Funcstructor that = this; that != null; that = that._Parent) {
                    if (that.Name.Equals(name)) {
                        return that;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Register a Func.
        /// </summary>
        /// <param name="returnType">The result type.</param>
        /// <param name="key">The key often null.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <param name="funcstructor">The func.</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        public bool Register(Type returnType, object key, Type[] parameterTypes, object funcstructor, bool overwrite = false) {
            var l = new object[parameterTypes.Length + 2];
            l[0] = key;
            l[1] = returnType;
            for (int idx = 0; idx < parameterTypes.Length; idx++) {
                l[2 + idx] = parameterTypes[idx];
            }

            CacheKey cacheKey = new CacheKey(l);
            return this.registerAny(cacheKey, funcstructor, overwrite);
        }

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        public bool Register<T>(object key, Func<T> funcstructor, bool overwrite = false) {
            CacheKey cacheKey = new CacheKey(key, typeof(T));
            return this.registerAny(cacheKey, funcstructor, overwrite);
        }

        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <returns>A new instance.</returns>
        public T Resolve<T>(object key) {
            CacheKey cacheKey = new CacheKey(key, typeof(T));
            ItemState itemState = this.GetItemState(cacheKey);
            if ((object)itemState != null) {
                var func = (Func<T>)(itemState.Funcstructor);
                return func();
            }

            return default(T);
        }

        /// <summary>
        /// Register a Func
        /// </summary>
        /// <typeparam name="P1">The parameter type.</typeparam>
        /// <typeparam name="T">The result type often an interface..</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="funcstructor">The func that create a new instance</param>
        /// <param name="overwrite">true set; false add only if not registered</param>
        /// <returns>true if registered.</returns>
        public bool Register<P1, T>(object key, Func<P1, T> funcstructor, bool overwrite = false) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            return this.registerAny(cacheKey, funcstructor, overwrite);
        }

        /// <summary>
        /// Create a instance.
        /// </summary>
        /// <typeparam name="P1">The type of the parameter</typeparam>
        /// <typeparam name="T">The result (interface)</typeparam>
        /// <param name="key">The key normally NULL.</param>
        /// <param name="p1">The parameter.</param>
        /// <returns>A new instance.</returns>
        public T Resolve<P1, T>(object key, P1 p1) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            ItemState itemState = this.GetItemState(cacheKey);
            if ((object)itemState != null) {
                var func = (Func<P1, T>)(itemState.Funcstructor);
                return func(p1);
            }

            return default(T);
        }

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
        public bool Register<P1, P2, T>(object key, Func<P1, P2, T> funcstructor, bool overwrite = false) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            return this.registerAny(cacheKey, funcstructor, overwrite);
        }

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
        public T Resolve<P1, P2, T>(object key, P1 p1, P2 p2) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            ItemState itemState = this.GetItemState(cacheKey);
            if ((object)itemState != null) {
                var func = (Func<P1, P2, T>)(itemState.Funcstructor);
                return func(p1, p2);
            }

            return default(T);
        }

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
        public bool Register<P1, P2, P3, T>(object key, Func<P1, P2, P3, T> funcstructor, bool overwrite = false) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            return this.registerAny(cacheKey, funcstructor, overwrite);
        }

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
        public T Resolve<P1, P2, P3, T>(object key, P1 p1, P2 p2, P3 p3) {
            CacheKey cacheKey = new CacheKey(key, typeof(T), typeof(P1));
            ItemState itemState = this.GetItemState(cacheKey);
            if ((object)itemState != null) {
                var func = (Func<P1, P2, P3, T>)(itemState.Funcstructor);
                return func(p1, p2, p3);
            }

            return default(T);
        }

        /// <summary>
        /// Register the type via the AssemblyQualifiedName.
        /// </summary>
        /// <typeparam name="T">The result type - the interface</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="fqnType">The AssemblyQualifiedName.</param>
        public void RegisterLateLoad<T>(object key, string fqnType) {
            var lateLoad = this.GetParent(FuncstructorConstant.ParentNameLateLoad) ?? this;
            var itemState = new ItemStateLateLoad(key, fqnType);
            Func<T> funcLateLoad = (delegate () {
                var lateInstance = this.GetParent(FuncstructorConstant.ParentNameLateInstance) ?? this;
                Func<T> funcLateInstance = (delegate () {
                    return (T)System.Activator.CreateInstance(itemState.GetClassType());
                });
                lateInstance.Register<T>(key, funcLateInstance, false);
                return this.Resolve<T>(itemState.Key);
            });
            itemState.Funcstructor = funcLateLoad;
            this.Register(typeof(T), key, null, itemState, false);
        }

        /// <summary>
        /// Register the type via the AssemblyQualifiedName.
        /// </summary>
        /// <typeparam name="P1">The type of the parameter.</typeparam>
        /// <typeparam name="T">The result type - the interface</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="fqnType">The AssemblyQualifiedName.</param>
        public void RegisterLateLoad<P1, T>(object key, string fqnType) {
            var lateLoad = this.GetParent(FuncstructorConstant.ParentNameLateLoad) ?? this;
            var itemState = new ItemStateLateLoad(key, fqnType);
            Func<P1, T> funcLateLoad = (delegate (P1 l1) {
                var lateInstance = this.GetParent(FuncstructorConstant.ParentNameLateInstance) ?? this;
                Func<P1, T> funcLateInstance = (delegate (P1 i1) {
                    return (T)System.Activator.CreateInstance(itemState.GetClassType(), i1);
                });
                lateInstance.Register<P1, T>(key, funcLateInstance, false);
                return this.Resolve<P1, T>(itemState.Key, l1);
            });
            itemState.Funcstructor = funcLateLoad;
            this.Register(typeof(T), key, new Type[] { typeof(P1) }, itemState, false);
        }

        /// <summary>
        /// Register the type via the AssemblyQualifiedName.
        /// </summary>
        /// <typeparam name="P1">The type of the 1st parameter.</typeparam>
        /// <typeparam name="P2">The type of the 2cd parameter.</typeparam>
        /// <typeparam name="T">The result type - the interface</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="fqnType">The AssemblyQualifiedName.</param>
        public void RegisterLateLoad<P1, P2, T>(object key, string fqnType) {
            var lateLoad = this.GetParent(FuncstructorConstant.ParentNameLateLoad) ?? this;
            var itemState = new ItemStateLateLoad(key, fqnType);
            Func<P1, P2, T> funcLateLoad = (delegate (P1 l1, P2 l2) {
                var lateInstance = this.GetParent(FuncstructorConstant.ParentNameLateInstance) ?? this;
                Func<P1, P2, T> funcLateInstance = (delegate (P1 i1, P2 i2) {
                    return (T)System.Activator.CreateInstance(itemState.GetClassType(), i1, i2);
                });
                lateInstance.Register<P1, P2, T>(key, funcLateInstance, false);
                return this.Resolve<P1, P2, T>(itemState.Key, l1, l2);
            });
            itemState.Funcstructor = funcLateLoad;
            this.Register(typeof(T), key, new Type[] { typeof(P1), typeof(P2) }, itemState, false);
        }

        /// <summary>
        /// Register the type via the AssemblyQualifiedName.
        /// </summary>
        /// <typeparam name="P1">The type of the 1st parameter.</typeparam>
        /// <typeparam name="P2">The type of the 2cd parameter.</typeparam>
        /// <typeparam name="P3">The type of the 3rd parameter.</typeparam>
        /// <typeparam name="T">The result type - the interface</typeparam>
        /// <param name="key">The key often null.</param>
        /// <param name="fqnType">The AssemblyQualifiedName.</param>
        public void RegisterLateLoad<P1, P2, P3, T>(object key, string fqnType) {
            var lateLoad = this.GetParent(FuncstructorConstant.ParentNameLateLoad) ?? this;
            var itemState = new ItemStateLateLoad(key, fqnType);
            Func<P1, P2, P3, T> funcLateLoad = (delegate (P1 l1, P2 l2, P3 l3) {
                var lateInstance = this.GetParent(FuncstructorConstant.ParentNameLateInstance) ?? this;
                Func<P1, P2, P3, T> funcLateInstance = (delegate (P1 i1, P2 i2, P3 i3) {
                    return (T)System.Activator.CreateInstance(itemState.GetClassType(), i1, i2, i3);
                });
                lateInstance.Register<P1, P2, P3, T>(key, funcLateInstance, false);
                return this.Resolve<P1, P2, P3, T>(itemState.Key, l1, l2, l3);
            });
            itemState.Funcstructor = funcLateLoad;
            this.Register(typeof(T), key, new Type[] { typeof(P1), typeof(P2), typeof(P3) }, itemState, false);
        }

        private bool registerAny(CacheKey cacheKey, object funcstructor, bool overwrite = false) {
            this._ReaderWriterLock.EnterWriteLock();
            try {
                if (overwrite) {
                    var itemState = funcstructor as ItemState;
                    if (itemState != null) {
                        itemState.CacheKey = cacheKey;
                    } else {
                        itemState = new ItemState(cacheKey, funcstructor);
                    }

                    this._RegistryItemState[cacheKey] = itemState;
                    return true;
                } else {
                    if (this._RegistryItemState[cacheKey] == null) {
                        var itemState = funcstructor as ItemState;
                        if (itemState != null) {
                            itemState.CacheKey = cacheKey;
                        } else {
                            itemState = new ItemState(cacheKey, funcstructor);
                        }

                        this._RegistryItemState[cacheKey] = itemState;
                        return true;
                    }
                }
            } finally {
                this._ReaderWriterLock.ExitWriteLock();
            }

            return false;
        }

        private ItemState GetItemState(CacheKey cacheKey) {
            ItemState itemState = null;
            for (Funcstructor that = this; that != null; that = that._Parent) {
                that._ReaderWriterLock.EnterReadLock();
                try {
                    itemState = (ItemState)that._RegistryItemState[cacheKey];
                } finally {
                    that._ReaderWriterLock.ExitReadLock();
                }

                if (itemState != null) {
                    return itemState;
                }
            }

            return itemState;
        }

        internal class ItemState {
            internal CacheKey CacheKey;
            internal object Funcstructor;

            public ItemState(CacheKey cacheKey, object funcstructor) {
                this.CacheKey = cacheKey;
                this.Funcstructor = funcstructor;
            }
        }

        internal sealed class ItemStateLateLoad : ItemState {
            internal object Key;
            internal string ClassName;
            internal Type Type;

            public ItemStateLateLoad(object key, string className)
                : base(null, null) {
                this.Key = key;
                this.ClassName = className;
                this.Funcstructor = null;
            }

            internal Type GetClassType() {
                if (this.Type == null) {
                    this.Type = System.Type.GetType(this.ClassName, true);
                }

                return this.Type;
            }
        }
    }
}
