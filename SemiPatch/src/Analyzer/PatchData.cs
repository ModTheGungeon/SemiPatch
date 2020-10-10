using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    /// <summary>
    /// The root of SemiPatch metadata. Contains information on what should be
    /// patched and how.
    /// </summary>
    public class PatchData {
        /// <summary>
        /// A magic number that is used to ensure that loading outdated
        /// binary metadata files errors early on.
        /// </summary>
        public const int CURRENT_VERSION = 20;

        /// <summary>
        /// The format version of this PatchData object.
        /// </summary>
        public readonly int Version;

        /// <summary>
        /// Mono.Cecil module to be patched.
        /// </summary>
        public readonly ModuleDefinition TargetModule;

        /// <summary>
        /// Source assemblies that the patch data has been extracted
        /// from.
        /// </summary>
        public readonly IList<ModuleDefinition> PatchModules;

        /// <summary>
        /// List of type patch data.
        /// </summary>
        public readonly IList<PatchTypeData> Types;

        public PatchData(ModuleDefinition target, IList<ModuleDefinition> patches) {
            Version = CURRENT_VERSION;
            TargetModule = target;
            PatchModules = patches;
            Types = new List<PatchTypeData>();
        }

        public string ToString(string indent) {
            var s = new StringBuilder();
            s.Append("Version: ").Append(Version);
            s.Append("\n").Append(indent);
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

        /// <summary>
        /// Serializes the entire PatchData object into a stream of bytes.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        public void Serialize(BinaryWriter writer) {
            writer.Write(Version);
            writer.Write(TargetModule.Assembly.FullName);
            writer.Write(PatchModules.Count);
            foreach (var mod in PatchModules) {
                writer.Write(mod.Assembly.FullName);
            }
            writer.Write(Types.Count);
            foreach (var type in Types) type.Serialize(writer);
        }

        /// <summary>
        /// Dserializes an entire PatchData object from a stream of bytes.
        /// </summary>
        /// <returns>The deserialized PatchData object.</returns>
        /// <param name="reader">The binary reader.</param>
        /// <param name="asm_resolver">Assembly resolver for the target and patch assemblies.</param>
        public static PatchData Deserialize(BinaryReader reader, IAssemblyResolver asm_resolver) {
            var version = reader.ReadInt32();
            if (version != CURRENT_VERSION) {
                throw new PatchDataVersionMismatchException(version);
            }

            var target_module_name = reader.ReadString();

            var target_module = asm_resolver.Resolve(AssemblyNameDefinition.Parse(target_module_name)).MainModule;

            var patch_modules_count = reader.ReadInt32();
            var patch_modules = new List<ModuleDefinition>();
            var patch_module_map = new Dictionary<string, ModuleDefinition>();
            for (var i = 0; i < patch_modules_count; i++) {
                var patch_asm_name = reader.ReadString();
                var patch_module = asm_resolver.Resolve(AssemblyNameDefinition.Parse(patch_asm_name)).MainModule;
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

        /// <summary>
        /// Shorthand for reading PatchData from a binary file on disk.
        /// </summary>
        /// <returns>The deserialized PatchData.</returns>
        /// <param name="path">Path of the serialized PatchData file.</param>
        /// <param name="asm_resolver">Assembly resolver for the target and patch assemblies.</param>
        public static PatchData ReadFrom(string path, IAssemblyResolver asm_resolver) {
            using (var r = new BinaryReader(File.OpenRead(path))) {
                return Deserialize(r, asm_resolver);
            }
        }

        [Obsolete("Use SemiPatchDiffSource")]
        public void WriteInsertList(TextWriter writer) {
            foreach (var type in Types) {
                foreach (var method in type.Methods) {
                    if (!method.IsInsert) continue;
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
            if (a is null && b is null) return false;
            if (a is null || b is null) return true;
            return !a.Equals(b);
        }
    }
}
