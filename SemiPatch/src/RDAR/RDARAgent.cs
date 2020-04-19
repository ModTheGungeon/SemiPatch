using System;
using ModTheGungeon;
using Mono.Cecil;
using System.IO.Pipes;

namespace SemiPatch.RDAR {
    public class RDARAgent {
        public string Name;
        public Logger Logger;

        public RDARAgent(string name) {
            Name = name;
            Logger = new Logger($"{nameof(RDARAgent)}({Name})");
        }

        public ModuleDefinition LoadAssembly(string path) {
            Logger.Debug($"Loading assembly: '{path}'");
            return ModuleDefinition.ReadModule(path);
        }
    }
}
