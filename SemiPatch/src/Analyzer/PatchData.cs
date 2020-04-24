﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class PatchData {
        public readonly ModuleDefinition TargetModule;
        public readonly IList<ModuleDefinition> PatchModules;
        public readonly IList<PatchTypeData> Types;

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

        public static PatchData Deserialize(BinaryReader reader, Dictionary<string, ModuleDefinition> fallback_patch_module_map = null) {
            var target_module_name = reader.ReadString();

            var target_asm = System.Reflection.Assembly.ReflectionOnlyLoad(target_module_name);
            var target_module = ModuleDefinition.ReadModule(target_asm.Location);
            var patch_modules_count = reader.ReadInt32();
            var patch_modules = new List<ModuleDefinition>();
            var patch_module_map = new Dictionary<string, ModuleDefinition>();
            for (var i = 0; i < patch_modules_count; i++) {
                var patch_asm_name = reader.ReadString();
                var patch_asm = System.Reflection.Assembly.ReflectionOnlyLoad(patch_asm_name);
                ModuleDefinition patch_module = null;
                if (patch_asm == null || patch_asm_name != patch_asm.FullName) {
                    fallback_patch_module_map?.TryGetValue(patch_asm_name, out patch_module);
                    if (patch_module == null) {
                        throw new Exception($"Failed loading assembly (version must match exactly): '{patch_asm_name}'");
                    }
                } else patch_module = ModuleDefinition.ReadModule(patch_asm.Location);
                patch_modules.Add(patch_module);
                patch_module_map[patch_asm_name] = patch_module;
            }
            var data = new PatchData(target_module, patch_modules);
            var types_count = reader.ReadInt32();
            for (var i = 0; i < types_count; i++) {
                data.Types.Add(PatchTypeData.Deserialize(target_module, patch_module_map, reader));
            }
            return data;
        }

        public static PatchData ReadFrom(string path, Dictionary<string, ModuleDefinition> fallback_patch_module_map = null) {
            using (var r = new BinaryReader(File.OpenRead(path))) {
                return Deserialize(r, fallback_patch_module_map);
            }
        }

        public void WriteInsertList(TextWriter writer) {
            foreach (var type in Types) {
                foreach (var method in type.Methods) {
                    if (!method.IsInsert) continue;
                    writer.WriteLine($"[{type.PatchType.Module.Assembly.FullName};{type.PatchType};{type.TargetType}]M {method.PatchPath} --> {method.TargetPath}");
                }
            }
        }

        public override int GetHashCode() {
            var x = TargetModule.Assembly.FullName.GetHashCode();
            x ^= PatchModules.Count;
            for (var i = 0; i < PatchModules.Count; i++) {
                x ^= PatchModules[i].Assembly.FullName.GetHashCode();
            }
            x ^= Types.Count;
            for (var i = 0; i < Types.Count; i++) {
                x ^= Types[i].GetHashCode();
            }
            return x;
        }

        public bool Equals(PatchData other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is PatchData)) return false;
            return Equals((PatchData)obj);
        }

        public static bool operator==(PatchData a, PatchData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator!=(PatchData a, PatchData b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return !a.Equals(b);
        }
    }
}