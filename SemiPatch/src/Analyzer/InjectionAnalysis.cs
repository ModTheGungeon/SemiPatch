using System;
using System.Collections.Generic;
using ModTheGungeon;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SemiPatch {
    internal struct InjectionSearchResult {
        public bool IsSuccess;
        public PatchInjectData InjectData;
        public InjectSearchFailureException FailureException;

        public PatchInjectData Unwrap() {
            if (!IsSuccess) throw FailureException;
            return InjectData;
        }

        public static InjectionSearchResult Failure(InjectSearchFailureException ex)
            => new InjectionSearchResult {
                IsSuccess = false,
                FailureException = ex
            };

        public static InjectionSearchResult Failure(MethodPath handler_path)
            => new InjectionSearchResult {
                IsSuccess = false,
                FailureException = GetDefaultFailureException(handler_path)
            };

        public static InjectionSearchResult Success(PatchInjectData data)
            => new InjectionSearchResult {
                IsSuccess = true,
                InjectData = data
            };

        public static InjectSearchFailureException GetDefaultFailureException(MethodPath handler_path) {
            return new InjectSearchFailureException(handler_path, $"Finding a viable spot to insert injection handler '{handler_path}' failed due to an unknown error.");
        }
    }

    internal interface IInjectionAnalysisHandler {
        InjectPosition DefaultPosition { get; }

        // target always has a body when this method is called
        InjectionSearchResult GetPatchData(
            InjectPosition position,
            ModuleDefinition target_module, ModuleDefinition patch_module,
            MethodDefinition target, MethodDefinition handler,
            InjectAttribute.ArgumentHandler args
        );
    }

    internal struct HeadInjectionAnalysisHandler : IInjectionAnalysisHandler {
        public InjectPosition DefaultPosition => InjectPosition.Before;

        public InjectionSearchResult GetPatchData(
            InjectPosition position,
            ModuleDefinition target_module, ModuleDefinition patch_module,
            MethodDefinition target, MethodDefinition handler,
            InjectAttribute.ArgumentHandler args
        ) {
            args.IndexArgUnused();
            args.PathArgUnused();

            return InjectionSearchResult.Success(new PatchInjectData(target, handler, position == InjectPosition.Before ? 0 : 1));
        }
    }

    internal struct TailInjectionAnalysisHandler : IInjectionAnalysisHandler {
        public InjectPosition DefaultPosition => InjectPosition.Before;

        public InjectionSearchResult GetPatchData(
            InjectPosition position,
            ModuleDefinition target_module, ModuleDefinition patch_module,
            MethodDefinition target, MethodDefinition handler,
            InjectAttribute.ArgumentHandler args
        ) {
            if (position == InjectPosition.After) {
                throw new InvalidInjectPositionException(
                    handler.ToPath(),
                    InjectQuery.Tail,
                    position
                );
            }

            args.IndexArgUnused();
            args.PathArgUnused();

            var last_idx = target.Body.Instructions.Count - 1;
            if (target.Body.Instructions[last_idx].OpCode == OpCodes.Ret) {
                last_idx -= 1;
            }
            // last instr is always ret, but just in case I check

            return InjectionSearchResult.Success(new PatchInjectData(target, handler, last_idx));
        }
    }

    internal struct MethodCallInjectionAnalysisHandler : IInjectionAnalysisHandler {
        public InjectPosition DefaultPosition => InjectPosition.Before;
        public static Logger Logger = new Logger(nameof(MethodCallInjectionAnalysisHandler));

        public MethodDefinition ResolveCall(ModuleDefinition target_module, MethodDefinition handler, string call) {
            // TODO: make this faster/more idiot proof?
            Logger.Debug($"Resolving call: '{call}'");
            if (call.IndexOf('[') != 0) throw new InvalidCallInjectFormatException(handler.ToPath(), call);
            var end_prefix_idx = call.IndexOf(']');
            if (end_prefix_idx == -1) throw new InvalidCallInjectFormatException(handler.ToPath(), call); ;

            var prefix = call.Substring(1, end_prefix_idx - 1);
            var method_sig = call.Substring(end_prefix_idx + 2);

            Logger.Debug($"Full name of declaring type: '{prefix}'");
            Logger.Debug($"Method signature: '{method_sig}'");

            var decl_type = GlobalModuleLoader.FindType(target_module, prefix);
            Logger.Debug($"Found type: '{decl_type}'");
            if (decl_type == null) throw new MissingCallInjectTargetException(handler.ToPath(), call);

            for (var i = 0; i < decl_type.Methods.Count; i++) {
                var candidate_method = decl_type.Methods[i];
                var candidate_sig = new Signature(candidate_method);

                if (candidate_sig == method_sig) return candidate_method;
            }

            throw new MissingCallInjectTargetException(handler.ToPath(), call);
        }

        public InjectionSearchResult GetPatchData(
            InjectPosition position,
            ModuleDefinition target_module, ModuleDefinition patch_module,
            MethodDefinition target, MethodDefinition handler,
            InjectAttribute.ArgumentHandler args
        ) {
            MethodDefinition call;
            try {
                call = ResolveCall(target_module, handler, args.Path);
            } catch (InjectSearchFailureException e) {
                return InjectionSearchResult.Failure(e);
            }

            var found_idx = 0;
            var required_idx = args.Index;  

            var instrs = target.Body.Instructions;
            for (var i = 0; i < instrs.Count; i++) {
                var instr = instrs[i];

                if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt) {
                    var method_ref = instr.Operand as MethodReference;
                    var resolved = method_ref?.Resolve();
                    if (resolved != null && call.IsSame(resolved)) {
                        if (found_idx == required_idx) {
                            var instrs_idx = i + 1;

                            if (position == InjectPosition.Before) {
                                var prev = instr.Previous;
                                var stack_count = 0;
                                while (prev != null) {
                                    stack_count += prev.ComputeStackDelta();
                                    instrs_idx -= 1;

                                    if (stack_count == call.Parameters.Count) {
                                        break;
                                    }

                                    prev = prev.Previous;
                                }
                            }

                            return InjectionSearchResult.Success(
                                new PatchInjectData(target, handler, instrs_idx)
                            );
                        } 
                        found_idx += 1;
                    }
                }
            }

            return InjectionSearchResult.Failure(new CallSearchMissingInInjectTargetException(
                handler.ToPath(), call.ToPath(), found_idx, required_idx
            ));
        }
    }
}
