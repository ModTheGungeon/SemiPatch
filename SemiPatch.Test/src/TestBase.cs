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

    public class Test {
        public static DefaultAssemblyResolver AssemblyResolver = new DefaultAssemblyResolver();
        public static bool Debug = false;
        static Test() {
            Logger.WriteConsoleDefault = Debug;
            AssemblyResolver.AddSearchDirectory(".sptest");
        }
        public struct Module : IDisposable {
            public ModuleDefinition Definition;
            public string Name;
            public PatchData PatchData;

            public Module(string name, ModuleDefinition module) {
                Definition = module;
                Name = name;
                PatchData = null;
            }

            public Module Analyzed(Module target) {
                var m = new Module(Name, Definition);
                var analyzer = new Analyzer(target.Definition, new ModuleDefinition[] { Definition });
                m.PatchData = analyzer.Analyze();
                return m;
            }

            public void Dispose() {
                Definition.Dispose();
            }
        }

        public string Name;

        public Module TargetModule;
        public List<Module> PatchModules = new List<Module>();
        public Logger Logger;
        
        public Test(string name) {
            Name = name;
            Logger = new Logger($"Test({Name})");
        }

        public struct QuickTestResult<T> {
            public T Target; 
            public T Patched;
        }

        public static QuickTestResult<T> StaticTest<T>(string name, string method_name, object[] args, Type target_type, params Type[] patch_types) {
            var test = new Test(name);
            test.Target(target_type);
            for (var i = 0; i < patch_types.Length; i++) {
                test.Patch($"patch{i + 1}", patch_types[i]);
            }
            test.DoStaticPatchRoundtrip();

            var type_name = target_type.Name;
            var target_method = test.LoadTarget().GetType(type_name).GetMethod(method_name);
            var patched_method = test.LoadPatched().GetType(type_name).GetMethod(method_name);
            return new QuickTestResult<T> {
                Target = (T)target_method.Invoke(null, args),
                Patched = (T)patched_method.Invoke(null, args)
            };
        }

        public static QuickTestResult<Type> SimpleTest(string name, Type target_type, params Type[] patch_types) {
            var test = new Test(name);
            test.Target(target_type);
            for (var i = 0; i < patch_types.Length; i++) {
                test.Patch($"patch{i + 1}", patch_types[i]);
            }
            test.DoStaticPatchRoundtrip();

            var type_name = target_type.Name;
            var post_target_type = test.LoadTarget().GetType(type_name);
            var post_patched_type = test.LoadPatched().GetType(type_name);
            return new QuickTestResult<Type> {
                Target = post_target_type,
                Patched = post_patched_type
            };
        }

        public void Target(params Type[] types) {
            var name = $"{Name}_Target";
            Logger.Debug($"Adding target '{name}' from {types.Length} types");
            var module = ModuleDefinition.CreateModule(name, new ModuleParameters {
                AssemblyResolver = AssemblyResolver,
                Kind = ModuleKind.Dll
            });
            var relinker = new Relinker();
            for (var i = 0; i < types.Length; i++) {
                var imported_type = module.ImportReference(types[i]);
                var cecil_type = imported_type.Resolve().Clone(module);
                cecil_type.Namespace = "";
                cecil_type.Attributes &= ~TypeAttributes.NestedPublic & ~TypeAttributes.NestedPrivate;
                relinker.Map(imported_type.Resolve().ToPath(), new Relinker.TypeEntry { TargetType = cecil_type });
            }
            relinker.Relink(module);
            TargetModule = new Module(name, module);
            WriteTarget();
        }

        public void Patch(string patch_name, params Type[] types) {
            var name = $"{Name}_Patch_{patch_name}";
            Logger.Debug($"Adding patch '{name}' from {types.Length} types");
            var module = ModuleDefinition.CreateModule(name, new ModuleParameters {
                AssemblyResolver = AssemblyResolver,
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
                        var target_type = path.FindIn(TargetModule.Definition);

                        attr = new CustomAttribute(
                            module.ImportReference(module.ImportReference(typeof(PatchAttribute)).Resolve().Methods[0])
                        );
                        cecil_type.CustomAttributes[i] = attr;

                        attr.ConstructorArguments.Add(new CustomAttributeArgument(
                                module.ImportReference(typeof(Type)),
                                module.ImportReference(target_type)
                        ));

                    }
                }
                relinker.Map(imported_type.Resolve().ToPath(), new Relinker.TypeEntry { TargetType = cecil_type });
            }
            relinker.Relink(module);
            PatchModules.Add(new Module(name, module));
        }

        public ReloadableModule Reloadable(string patch_name) {
            for (var i = 0; i < PatchModules.Count; i++) {
                var m = PatchModules[i];
                if (m.Name == $"{Name}_Patch_{patch_name}") {
                    return new ReloadableModule(
                        target_module: TargetModule.Definition,
                        patch_module: m.Definition,
                        patch_data: m.PatchData
                    );
                }
            }
            throw new Exception($"Could not find patch module '{patch_name}'");
        }

        public void AnalyzeAll() {
            Logger.Debug($"Analyzing {PatchModules.Count} patches");
            for (var i = 0; i < PatchModules.Count; i++) {
                Logger.Debug($"Analyzing patch '{PatchModules[i].Name}'");
                PatchModules[i] = PatchModules[i].Analyzed(TargetModule);
            }
        }

        public void WriteTarget() {
            Logger.Debug($"Writing target to disk");
            if (!Directory.Exists(".sptest")) {
                Directory.CreateDirectory(".sptest");
            }
            TargetModule.Definition.Write(Path.Combine(".sptest", TargetModule.Name + ".dll"));
        }

        public void WritePatches() {
            Logger.Debug($"Writing all patches to disk");
            if (!Directory.Exists(".sptest")) {
                Directory.CreateDirectory(".sptest");
            }

            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];
                Logger.Debug($"Writing patch '{mod.Name}' to disk");
                mod.Definition.Write(Path.Combine(".sptest", mod.Name + ".dll"));

                var reloadable = new ReloadableModule(
                    target_module: TargetModule.Definition,
                    patch_module: mod.Definition,
                    patch_data: mod.PatchData
                );

                using (var f = File.Create(Path.Combine(".sptest", mod.Name + ".spr"))) {
                    reloadable.Write(f);
                }

            }
        }

        public void ReloadFromDisk() {
            Logger.Debug($"Reloading all from disk");
            TargetModule.Dispose();
            TargetModule.Definition = ModuleDefinition.ReadModule(Path.Combine(".sptest", TargetModule.Name + ".dll"), new ReaderParameters {
                AssemblyResolver = AssemblyResolver
            });

            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];
                mod.Dispose();
                var reloadable = ReloadableModule.Read(Path.Combine(".sptest", mod.Name + ".spr"), TargetModule.Definition);
                PatchModules[i] = new Module(mod.Name, ModuleDefinition.ReadModule(
                    Path.Combine(".sptest", mod.Name + ".dll"),
                    new ReaderParameters {
                        AssemblyResolver = AssemblyResolver
                    }
                )) { PatchData = reloadable.PatchData };
            }
        }

        public void StaticPatch() {
            Logger.Debug($"Statically patching target");
            var sp = new StaticPatcher(TargetModule.Definition);
            for (var i = 0; i < PatchModules.Count; i++) {
                var mod = PatchModules[i];

                sp.LoadPatch(mod.PatchData, mod.Definition);
            }
            sp.Patch();
            TargetModule.Definition.Name = TargetModule.Name + "_Patched";
            var asm = TargetModule.Definition.Assembly;
            asm.Name = new AssemblyNameDefinition(TargetModule.Name + "_Patched", asm.Name.Version);
        }

        public void WritePatched() {
            Logger.Debug($"Writing statically patched target");
            TargetModule.Definition.Write(Path.Combine(".sptest", TargetModule.Name + "_Patched.dll"));
        }

        public Assembly LoadPatched() {
            Logger.Debug($"Loading statically patched assembly");
            var asm = Assembly.LoadFrom(Path.Combine(".sptest", TargetModule.Name + "_Patched.dll"));
            return asm;
        }

        public void DoStaticPatchRoundtrip() {
            AnalyzeAll();
            WritePatches();
            ReloadFromDisk();
            StaticPatch();
            WritePatched();
        }

        public Assembly LoadTarget() {
            Logger.Debug($"Loading unpatched target assembly");
            var asm = Assembly.LoadFrom(Path.Combine(".sptest", TargetModule.Name + ".dll"));
            return asm;
        }

        public void Dispose() {
            TargetModule.Dispose();
            for (var i = 0; i < PatchModules.Count; i++) {
                PatchModules[i].Dispose();
            }
            PatchModules.Clear();
        }
    }
}
