using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TypeAttributes = Mono.Cecil.TypeAttributes;
using ModTheGungeon;

namespace SemiPatch.Test {
    // later changed to SemiPatch.PatchAttribute
    public class TestPatchAttribute : Attribute {
        public TestPatchAttribute(string type) {}
    }

    public class ModuleContext : IAssemblyResolver {
        private Dictionary<string, AssemblyDefinition> _AssemblyMap = new Dictionary<string, AssemblyDefinition>();
        private Dictionary<string, ModuleDefinition> _ModuleMap = new Dictionary<string, ModuleDefinition>();
        public DefaultAssemblyResolver FallbackResolver;
        
        public ModuleContext() {
            FallbackResolver = new DefaultAssemblyResolver();
        }


        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters reader_params) {
            if (_AssemblyMap.TryGetValue(name.FullName, out AssemblyDefinition mapped)) {
                return mapped;
            }
            return FallbackResolver.Resolve(name, reader_params);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) {
            if (_AssemblyMap.TryGetValue(name.FullName, out AssemblyDefinition mapped)) {
                return mapped;
            }
            return FallbackResolver.Resolve(name);
        }

        public void Dispose() {
            FallbackResolver.Dispose();
        }

        public ModuleDefinition Read(string path) {
            return Read(path, new ReaderParameters());
        }
        
        public ModuleDefinition Read(string path, ReaderParameters reader_params) {
            reader_params.AssemblyResolver = this;
            var m = ModuleDefinition.ReadModule(
                path, reader_params
            );

            var name = m.Assembly.Name.FullName;
            if (_ModuleMap.TryGetValue(name, out ModuleDefinition mapped)) {
                m.Dispose();
                return mapped;
            }

            return _ModuleMap[name] = m;
        }

        public ModuleDefinition Create(string name, ModuleKind kind) {
            return Create(name, new ModuleParameters {
                Kind = kind
            });
        }

        public ModuleDefinition Create(string name, ModuleParameters mod_params) {
            mod_params.AssemblyResolver = this;
            var m = ModuleDefinition.CreateModule(name, mod_params);
            var full_name = m.Assembly.Name.FullName;
            if (_ModuleMap.TryGetValue(name, out ModuleDefinition _)) {
                throw new InvalidOperationException($"Module '{full_name}' already exists in this context and cannot be created.");
            }

            return _ModuleMap[full_name] = m;

        }
    }

    public class Test {
        public const string TMP_DIR = ".sptest";

        private static string _TempPath(string part) {
            if (!Directory.Exists(TMP_DIR)) {
                Directory.CreateDirectory(TMP_DIR);
            }
            return Path.Combine(TMP_DIR, part);
        }
        

        public static bool Debug = false;
        static Test() {
            Logger.WriteConsoleDefault = Debug;
        }

        public string Name;

        public ModuleContext ModuleContext;
        public ModuleDefinition TargetModule;
        private string _OriginalTargetModuleName;
        public IList<ModuleDefinition> PatchModules;

        public ModuleDefinition PatchModule {
            get {
                if (PatchModules.Count != 1) {
                    throw new InvalidOperationException("PatchModule can only be used with exactly 1 patch module");
                }

                return PatchModules[0];
            }
        }

        public Logger Logger;
        
        public Test(string name, Type target_type, params Type[] patch_types) {
            Name = name;
            Logger = new Logger($"Test({Name})");
            ModuleContext = new ModuleContext();
            ModuleContext.FallbackResolver.AddSearchDirectory(".sptest");
            TargetModule = _TypeToModule($"{Name}_Target", target_type);
            _OriginalTargetModuleName = TargetModule.Name;
            Write(TargetModule);
            PatchModules = new List<ModuleDefinition>();
            for (var i = 0; i < patch_types.Length; i++) {
                PatchModules.Add(_TypeToModule($"{Name}_Patch{i + 1}", patch_types[i]));
            }
            WritePatches();
        }

        private ModuleDefinition _TypeToModule(string name, params Type[] types) {
            Logger.Debug($"Adding patch '{name}' from {types.Length} types");
            var module = ModuleContext.Create(name, new ModuleParameters {
                Kind = ModuleKind.Dll
            });
            var relinker = new Relinker();
            for (var i = 0; i < types.Length; i++) {
                var imported_type = module.ImportReference(types[i]);
                var cecil_type = imported_type.Resolve().Clone(module);
                cecil_type.Namespace = "";
                cecil_type.Attributes &= ~TypeAttributes.NestedPublic & ~TypeAttributes.NestedPrivate;
                for (var j = 0; j < cecil_type.CustomAttributes.Count; j++) {
                    var attr = cecil_type.CustomAttributes[j];
                    if (attr.AttributeType.FullName == "SemiPatch.Test.TestPatchAttribute") {
                        var type_name = attr.ConstructorArguments[0].Value as string;
                        var sig = new Signature(type_name, null);
                        var path = new TypePath(sig, null);
                        var target_type = path.FindIn(TargetModule);
                        attr = new CustomAttribute(
                            module.ImportReference(module.ImportReference(typeof(PatchAttribute)).Resolve().Methods[0])
                        );
                        cecil_type.CustomAttributes[i] = attr;

                        attr.ConstructorArguments.Add(new CustomAttributeArgument(
                                module.ImportReference(typeof(Type)),
                                target_type
                        ));

                    }
                }
                relinker.Map(imported_type.Resolve().ToPath(), new Relinker.TypeEntry { TargetType = cecil_type });
            }
            relinker.Relink(module);
            return module;
        }

        public struct QuickTestResult<T> {
            public T Target; 
            public T Patched;
        }

        public static QuickTestResult<T> StaticTest<T>(string name, string method_name, object[] args, Type target_type, params Type[] patch_types) {
            var test = new Test(name, target_type, patch_types);
            test.StaticPatch();

            var type_name = target_type.Name;
            var target_method = test.LoadTarget().GetType(type_name).GetMethod(method_name);
            var patched_method = test.LoadPatched().GetType(type_name).GetMethod(method_name);
            return new QuickTestResult<T> {
                Target = (T)target_method.Invoke(null, args),
                Patched = (T)patched_method.Invoke(null, args)
            };
        }

        public static QuickTestResult<Type> SimpleTest(string name, Type target_type, params Type[] patch_types) {
            var test = new Test(name, target_type, patch_types);
            test.StaticPatch();

            var type_name = target_type.Name;
            var post_target_type = test.LoadTarget().GetType(type_name);
            var post_patched_type = test.LoadPatched().GetType(type_name);
            return new QuickTestResult<Type> {
                Target = post_target_type,
                Patched = post_patched_type
            };
        }

        public ReloadableModule MakeReloadable(ModuleDefinition mod) {
            var analyzer = new Analyzer(TargetModule, new ModuleDefinition[] { mod });
            var rm = new ReloadableModule(
                target_module: TargetModule,
                patch_module: mod,
                patch_data: analyzer.Analyze()
            );
            Console.WriteLine($"EQ? {rm.PatchData.TargetModule == TargetModule}");
            Console.WriteLine($"EQ2? {rm.PatchData.Types[0].TargetType.Module == TargetModule}");
            return rm;
        }

        public ModuleDefinition CreatePatchModule(string name, Type type) {
            var mod = _TypeToModule($"{Name}_{name}", type);
            return mod;
        }

        public void Write(ModuleDefinition module) {
            Logger.Debug($"Writing module '{module.Name}' to disk");
            module.Write(_TempPath(module.Name + ".dll"));
        }

        public void WritePatches() {
            Logger.Debug($"Writing all patches to disk");

            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];
                Logger.Debug($"Writing patch '{mod.Name}' to disk");
                Write(mod);
            }
        }

        public void ReloadFromDisk() {
            Logger.Debug($"Reloading all from disk");
            var name = TargetModule.Name;
            TargetModule.Dispose();
            TargetModule = ModuleContext.Read(_TempPath(name + ".dll"));

            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];
                var patch_name = mod.Name;
                mod.Dispose();
                PatchModules[i] = ModuleContext.Read(
                    _TempPath(patch_name + ".dll")
                );
            }
        }

        public void StaticPatch() {
            Logger.Debug($"Statically patching target");
            var sc = new StaticClient(TargetModule);

            var whatthefuck = TargetModule;

            Console.WriteLine($"modules: {PatchModules.Count}");
            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];
                var reloadable = MakeReloadable(mod);
                Console.WriteLine(mod);
                sc.Preload(reloadable);
            }

            sc.Commit();
            TargetModule.Name = TargetModule.Name + "_Patched";
            var asm = TargetModule.Assembly;
            asm.Name = new AssemblyNameDefinition(TargetModule.Name, asm.Name.Version);
            Write(TargetModule);
        }

        public Assembly LoadTarget() {
            Logger.Debug($"Loading original target assembly");
            var asm = Assembly.LoadFrom(_TempPath(_OriginalTargetModuleName + ".dll"));
            return asm;
        }

        public Assembly LoadPatched() {
            Logger.Debug($"Loading statically patched assembly");
            var asm = Assembly.LoadFrom(_TempPath(TargetModule.Name + ".dll"));
            return asm;
        }

        public void Dispose() {
            TargetModule.Dispose();
            TargetModule = null;
            for (var i = 0; i < PatchModules.Count; i++) {
                PatchModules[i].Dispose();
            }
            PatchModules.Clear();
        }
    }
}
