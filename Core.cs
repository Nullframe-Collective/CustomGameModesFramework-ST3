using MelonLoader;
using System.Collections.Concurrent;
using System.Reflection;

[assembly: MelonInfo(typeof(CustomGameModesFramework.Core), "CustomGameModesFramework", "1.1.4", "Nullframe Collective")]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]
[assembly: MelonColor(255, 0, 128, 255)]

namespace CustomGameModesFramework
{
    public class Core : MelonMod
    {
        public static bool IsInitialized { get; private set; }
        public static bool AssetsReady => IsInitialized;
        public static string Error => null;
        private readonly ConcurrentQueue<Assembly> _assemblies = new();
        private readonly HashSet<Assembly> _loaded = [];

        public override void OnInitializeMelon()
        {
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _assemblies.Enqueue(assembly);
            }

            LoadExternalGameModes();
            IsInitialized = true;
            LoggerInstance.Msg("Initialized. FishNet game modes are ready.");
        }

        public override void OnUpdate()
        {
            LoadExternalGameModes();
            GameModeRuntime.Update();
            GameSettingsIntegration.Initialize();
            GameModeMenu.Refresh();
        }

        public override void OnGUI() => GameModeMenu.DrawError();

        public override void OnDeinitializeMelon()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoaded;
            GameModeRuntime.Leave();
            GameSettingsIntegration.Dispose();
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            _assemblies.Enqueue(args.LoadedAssembly);
            GameSettingsIntegration.AssemblyLoaded();
        }

        private void LoadExternalGameModes()
        {
            while (_assemblies.TryDequeue(out Assembly assembly))
            {
                if (!_loaded.Add(assembly) || assembly.IsDynamic) continue;

                string name = assembly.GetName().Name;
                if (name.StartsWith("Unity") || name.StartsWith("Il2Cpp") || name.StartsWith("System") || name is "mscorlib" or "netstandard") continue;

                foreach (Type type in LoadableTypes(assembly, name))
                {
                    if (type.IsAbstract || type.ContainsGenericParameters || !typeof(CustomGameMode).IsAssignableFrom(type)) continue;

                    try
                    {
                        GameModeManager.Instance.RegisterGameMode((CustomGameMode)Activator.CreateInstance(type));
                        MelonLogger.Msg($"Registered game mode: {type.FullName}");
                    }
                    catch (Exception exception)
                    {
                        MelonLogger.Error($"Cannot register {type.FullName}: {exception.InnerException?.Message ?? exception.Message}");
                    }
                }
            }
        }

        private static Type[] LoadableTypes(Assembly assembly, string name)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                MelonLogger.Warning($"Some types in {name} could not be loaded: {exception.Message}");
                return exception.Types.Where(type => type != null).ToArray();
            }
            catch (Exception exception)
            {
                MelonLogger.Warning($"Cannot inspect {name}: {exception.Message}");
                return Array.Empty<Type>();
            }
        }
    }
}
