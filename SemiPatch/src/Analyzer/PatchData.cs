using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchData {
        public ModuleDefinition TargetModule;
        public IList<ModuleDefinition> PatchModules;
        public IList<PatchTypeData> Types;

        public PatchData(ModuleDefinition target, IList<ModuleDefinition> patches) {
            TargetModule = target;
            PatchModules = patches;
            Types = new List<PatchTypeData>();
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append("Target Assembly: ").Append(TargetModule.Name);
            s.Append("\n").Append(indent);
            s.Append("Patch Assemblies: [");
            for (var i = 0; i < PatchModules.Count; i++) {
                s.Append(PatchModules[i].Name);
                if (i < PatchModules.Count - 1) s.Append(", ");
            }
            s.Append("]");
            s.Append("\n").Append(indent);
            s.Append("Types:\n");
            for (var i = 0; i < Types.Count; i++) {
                s.Append(Types[i].ToString(indent + "\t")).Append("\n").Append(indent);
                if (i < Types.Count - 1) s.Append("\n");
            }
            return s.ToString();
        }

        public override string ToString() => ToString("");

        public void Serialize(BinaryWriter writer) {
            writer.Write(TargetModule.Assembly.FullName);
            writer.Write(PatchModules.Count);
            foreach (var mod in PatchModules) {
                writer.Write(mod.Assembly.FullName);
            }
            writer.Write(Types.Count);
            foreach (var type in Types) type.Serialize(writer);
        }

        public static PatchData Deserialize(BinaryReader reader) {
            var target_module_name = reader.ReadString();
            var target_asm = System.Reflection.Assembly.ReflectionOnlyLoad(target_module_name);
            var target_module = ModuleDefinition.ReadModule(target_asm.Location);
            var patch_modules_count = reader.ReadInt32();
            var patch_modules = new List<ModuleDefinition>();
            var patch_module_map = new Dictionary<string, ModuleDefinition>();
            for (var i = 0; i < patch_modules_count; i++) {
                var patch_asm = System.Reflection.Assembly.ReflectionOnlyLoad(reader.ReadString());
                var patch_module = ModuleDefinition.ReadModule(patch_asm.Location);
                patch_modules.Add(patch_module);
                patch_module_map[patch_module.Assembly.FullName] = patch_module;
            }
            var data = new PatchData(target_module, patch_modules);
            var types_count = reader.ReadInt32();
            for (var i = 0; i < types_count; i++) {
                data.Types.Add(PatchTypeData.Deserialize(target_module, patch_module_map, reader));
            }
            return data;
        }

        public void WriteInsertList(TextWriter writer) {
            foreach (var type in Types) {
                foreach (var method in type.Methods) {
                    if (!method.IsInsert) continue;
                    writer.WriteLine($"[{type.PatchType.Module.Assembly.FullName};{type.PatchType};{type.TargetType}]M {method.PatchSignature} --> {method.TargetSignature}");
                }
            }
        }
    }
}
