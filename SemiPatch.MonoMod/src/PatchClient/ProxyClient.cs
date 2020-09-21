using System;
using System.IO;
using ModTheGungeon;
using Mono.Cecil;
using MonoMod;
using System.Collections.Generic;

namespace SemiPatch {
    public class ProxyClient : Client {
        public Logger Logger;
        private IList<ReloadableModule> _Modules;
        public ModuleDefinition ProxyModule;

        public ProxyClient(ModuleDefinition target_module) : base(target_module) {
            Logger = new Logger($"{nameof(StaticClient)}({TargetModule.Name})");
            _Modules = new List<ReloadableModule>();
        }

        public override AddModuleResult AddModule(ReloadableModule module) {
            _Modules.Add(module);
            return AddModuleResult.Success;
        }

        private string _GetProxyName() {
            return $"SEMIPATCH PROXY FOR {TargetModule.Assembly.FullName}";
        }

        private void _ProcessPatchMethodData(TypeDefinition target_type, PatchMethodData method) {
            // if ignored, shouldn't appear in proxy - if not insert,
            // it will already be there since we copy from target first
            if (method.EffectivelyIgnored || !method.IsInsert) return;

            // we can just copy the method with no body,
            // since ReceiveOriginal methods will never be inserts

            var patch_method = method.PatchMember as MethodDefinition;
            var new_method = patch_method.Clone(target_type, ProxyModule, strip_body: true);
            new_method.Attributes |= MethodAttributes.PInvokeImpl;
            new_method.IsPInvokeImpl = true;
            new_method.Name = method.EffectiveName;

            target_type.Methods.Add(new_method);

            if (method.SideEffect != null) {
                method.SideEffect.Apply(new_method, target_type.Module);
            }
        }

        private void _ProcessPatchFieldData(TypeDefinition target_type, PatchFieldData field) {
            // same as above
            if (field.EffectivelyIgnored || !field.IsInsert) return;

            // even simpler than methods
            var patch_field = field.PatchMember as FieldDefinition;
            var new_field = patch_field.Clone(target_type, ProxyModule);

            target_type.Fields.Add(new_field);
        }

        private void _ProcessPatchPropertyData(TypeDefinition target_type, PatchPropertyData prop) {
            if (prop.EffectivelyIgnored || !prop.IsInsert) return;

            var patch_prop = prop.PatchMember as PropertyDefinition;
            target_type.Properties.Add(patch_prop.Clone(target_type, ProxyModule));
        }

        private void _ProcessPatchTypeData(PatchTypeData type) {
            var target_type_path = type.TargetType.ToPath();
            var proxy_type = target_type_path.FindIn(ProxyModule);

            for (var i = 0; i < type.Methods.Count; i++) {
                _ProcessPatchMethodData(proxy_type, type.Methods[i]);
            }

            for (var i = 0; i < type.Fields.Count; i++) {
                _ProcessPatchFieldData(proxy_type, type.Fields[i]);
            }

            for (var i = 0; i < type.Properties.Count; i++) {
                _ProcessPatchPropertyData(proxy_type, type.Properties[i]);
            }

            // injections already covered by methods&fields
        }

        private void _ProcessPatchData(PatchData patch) {
            for (var i = 0; i < patch.Types.Count; i++) {
                _ProcessPatchTypeData(patch.Types[i]);
            }
        }

        private bool _TypeFilter(TypeDefinition type) {
            if (type.FullName == "<Module>") return true;
            return false;
        }

        public override CommitResult Commit() {
            ProxyModule = ModuleDefinition.CreateModule(_GetProxyName(), ModuleKind.Dll);

            // first: copy all types from target to proxy, but strip the actual code
            // from methods
            for (var i = 0; i < TargetModule.Types.Count; i++) {
                var type = TargetModule.Types[i];
                if (type.FullName == "<Module>") continue;

                var new_type = type.Clone(ProxyModule, exclude: _TypeFilter, strip_method_bodies: true);
                ProxyModule.Types.Add(new_type);
            }

            // second: for each reloadablemodule, implement the patchdata as dummy
            // fields/methods
            // inserting types is not supported by SP (it's pointless)

            for (var i = 0; i < _Modules.Count; i++) {
                var rm = _Modules[i];

                _ProcessPatchData(rm.PatchData);
            }

            return CommitResult.Success;
        }

        public void WriteResult(Stream stream) {
            if (ProxyModule == null)
                throw new InvalidOperationException($"You must call Commit() first to write the result");

            ProxyModule.Write(stream);
        }

        public void WriteResult(string path) {
            if (ProxyModule == null)
                throw new InvalidOperationException($"You must call Commit() first to write the result");

            ProxyModule.Write(path);
        }

        public override void Dispose() {}
    }
}
