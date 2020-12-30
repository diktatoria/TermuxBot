using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TermuxBot.API
{
    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        public PluginAssemblyLoadContext(string contextName, string pluginFolder)
            : base(contextName, false)
        {
            this.PluginFolder = pluginFolder;
        }

        /// <summary>
        /// When overridden in a derived class, allows an assembly to be resolved and loaded based on its <see cref="T:System.Reflection.AssemblyName" />.
        /// </summary>
        /// <param name="assemblyName">The object that describes the assembly to be loaded.</param>
        /// <returns>
        /// The loaded assembly, or <see langword="null" />.
        /// </returns>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Do not load API twice...
            if (assemblyName.Name == "TermuxBot.API")
            {
                return Assembly.GetAssembly(typeof(Plugin));
            }

            string pluginFolderPath = Path.Combine(this.PluginFolder, assemblyName.Name + ".dll");
            string path = String.Empty;

            if (File.Exists(pluginFolderPath))
            {
                var dependencyResolver = new AssemblyDependencyResolver(pluginFolderPath);
                path = dependencyResolver.ResolveAssemblyToPath(assemblyName);
            }

            if (String.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return base.Load(assemblyName);
            }

            return Assembly.LoadFile(path);
        }

        /// <summary>
        /// Loads the unmanaged DLL.
        /// </summary>
        /// <param name="unmanagedDllName">Name of the unmanaged DLL.</param>
        /// <returns></returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var dependencyResolver = new AssemblyDependencyResolver(Path.Combine(this.PluginFolder, unmanagedDllName));
            string path = dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            if (String.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return base.LoadUnmanagedDll(unmanagedDllName);
            }

            try
            {
                Assembly.LoadFile(path);
            }
            catch (Exception)
            {
                // Nothing to do for now, maybe log this later
            }

            return IntPtr.Zero;
        }

        public string PluginFolder { get; }
    }
}