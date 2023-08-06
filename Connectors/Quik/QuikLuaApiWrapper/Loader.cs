using System.Diagnostics;
using System.Reflection;
using Quik.Grpc;
using Tools;
using static System.Net.Mime.MediaTypeNames;

namespace Quik
{
    public class Loader
    {
        /// <summary>
        /// Entry Point. This method gets called by lua wrapper of the Quik trading terminal
        /// </summary>
        /// <param name="L">Pointer to lua state struct</param>
        /// <returns></returns>
        public static int Initialize(IntPtr luaStack)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolutionFailed;            

            Quik.Instance.Initialize(luaStack);
            DI.RegisterInstance<IQuik>(Quik.Instance);
            //Task.Run(ConsoleIO.Main);
            Task.Run(GrpcServer.Launch);
            return 1;
        }

        private static Assembly? OnAssemblyResolutionFailed(object? sender, ResolveEventArgs args)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var assemblyNameLength = args.Name.IndexOf(',');
            var assemblyName = args.Name[..assemblyNameLength];

            var pathToAssembly = Path.Combine(currentDir, $"{assemblyName}.dll");

            for (int i = 0; i < 3; i++)
            {
                if (File.Exists(pathToAssembly))
                {
                    return Assembly.LoadFile(pathToAssembly);
                }
                else
                {
                    Debug.Assert(false, $"Assembly {args.Name} not found in {currentDir}");
                }
            }

            return null;
        }
    }
}
