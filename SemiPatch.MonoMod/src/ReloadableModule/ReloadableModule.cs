using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using SemiPatch;
using Mono.Cecil;
using System.Collections.Generic;

namespace SemiPatch.MonoMod {
    public class CorruptedReloadableModuleException : SemiPatchException {
        public CorruptedReloadableModuleException(string section)
            : base($"Attempted to load corrupted reloadable module. Error while reading section: '{section}'.") { }
    }

    public class InvalidVersionReloadableModuleException : SemiPatchException {
        public InvalidVersionReloadableModuleException(int version)
            : base($"Attempted to load a reloadable module that is either too new or too old. Current format version is {ReloadableModule.VERSION}, but version you're trying to load is {version}.") { }
    }

    public class ReloadableModule : IDisposable {
        public enum SPRFileType {
            Embedded,
            Missing,
            CreateAtRuntime
        }

        public struct SPRFile {
            public SPRFileType Type;
            public long Offset;
            public long Length;

            public static int SizeInBytes = sizeof(long) + sizeof(long);

            public bool IsPresent => Type == SPRFileType.Embedded;

            public SPRFile(long offs, long len) {
                Type = SPRFileType.Embedded;
                Offset = offs;
                Length = len;
            }

            public static SPRFile MissingFile = new SPRFile { Type = SPRFileType.Missing };
            public static SPRFile CreateAtRuntimeFile = new SPRFile { Type = SPRFileType.CreateAtRuntime, Offset = 1 };

            public SpanStream OpenStreamTo(Stream stream) {
                if (!IsPresent) {
                    throw new InvalidOperationException($"SPRFile is missing or must be created at runtime");
                }
                return new SpanStream(stream, Offset, Length);
            }

            public void Write(BinaryWriter writer) {
                writer.Write(Offset);
                writer.Write(Length);
            }

            public static SPRFile ReadFrom(BinaryReader reader) {
                var offs = reader.ReadInt64();
                var len = reader.ReadInt64();
                if (offs == 0) return new SPRFile { Type = SPRFileType.Missing };
                if (offs == 1) {
                    throw new NotImplementedException($"SPRFileType.CreateAtRuntime not supported yet");
                    return new SPRFile { Type = SPRFileType.CreateAtRuntime, Offset = 1 };
                }
                return new SPRFile {
                    Type = SPRFileType.Embedded,
                    Offset = offs,
                    Length = len
                };
            }

            public static SpanStream TryReadStream(BinaryReader reader, Stream stream) {
                var file = ReadFrom(reader);
                if (!file.IsPresent) return null;
                return file.OpenStreamTo(stream);
            }
        }

        public const int VERSION = 0;
        public static readonly byte[] MAGIC = { (byte)'S', (byte)'P', (byte)'R' };

        private ModuleDefinition _TargetModule;

        private Stream _SourceStream;

        public readonly Stream PatchAssemblyStream;
        // MonoMod Static Generator
        public readonly Stream MMSGAssemblyStream;
        // RuntimeDetour Bootstrap
        public readonly Stream RDBSAssemblyStream;
        public readonly Stream PatchDataStream;

        private ModuleDefinition _PatchModule;
        private ModuleDefinition _MMSGModule;
        private ModuleDefinition _RDBSModule;
        private PatchData _PatchData;

        private Dictionary<string, ModuleDefinition> _ModuleMap;

        public Dictionary<string, ModuleDefinition> ModuleMap {
            get {
                if (_ModuleMap != null) return _ModuleMap;
                _ModuleMap = new Dictionary<string, ModuleDefinition>();
                _ModuleMap.Add(PatchModule.Assembly.FullName, PatchModule);
                if (_TargetModule != null) {
                    _ModuleMap.Add(_TargetModule.Assembly.FullName, _TargetModule);
                }
                return _ModuleMap;
            }
        }

        public ModuleDefinition PatchModule {
            get {
                if (_PatchModule != null) return _PatchModule;
                if (PatchAssemblyStream == null) return null;
                return _PatchModule = ModuleDefinition.ReadModule(PatchAssemblyStream);
            }
            set {
                _PatchModule = value;
            }
        }

        public ModuleDefinition MMSGModule {
            get {
                if (_MMSGModule != null) return _MMSGModule;
                if (MMSGAssemblyStream == null) return null;
                return _MMSGModule = ModuleDefinition.ReadModule(MMSGAssemblyStream);
            }
            set {
                _MMSGModule = value;
            }
        }

        public ModuleDefinition RDBSModule {
            get {
                if (_RDBSModule != null) return _RDBSModule;
                if (RDBSAssemblyStream == null) return null;
                return _RDBSModule = ModuleDefinition.ReadModule(RDBSAssemblyStream);
            }
            set {
                _RDBSModule = value;
            }
        }

        public PatchData PatchData {
            get {
                if (_PatchData != null) return _PatchData;
                if (PatchDataStream == null) return null;
                using (var reader = new BinaryReader(PatchDataStream)) {
                    return _PatchData = PatchData.Deserialize(reader, ModuleMap);
                }
            }
            set {
                _PatchData = value;
            }
        }

        public bool HasMMSG => _MMSGModule != null || MMSGAssemblyStream != null;
        public bool HasRDBS => _RDBSModule != null || RDBSAssemblyStream != null;


        public ReloadableModule(
            ModuleDefinition target_module,
            Stream patch_asm_stream,
            Stream mmsg_asm_stream,
            Stream rdbs_asm_stream,
            Stream patch_data_stream
        ) {
            _TargetModule = target_module;

            PatchAssemblyStream = patch_asm_stream;
            MMSGAssemblyStream = mmsg_asm_stream;
            RDBSAssemblyStream = rdbs_asm_stream;
            PatchDataStream = patch_data_stream;
        }

        public ReloadableModule(
            ModuleDefinition target_module,
            ModuleDefinition patch_module,
            ModuleDefinition mmsg_module,
            ModuleDefinition rdbs_module,
            PatchData patch_data
        ) {
            _TargetModule = target_module;

            _PatchModule = patch_module;
            _MMSGModule = mmsg_module;
            _RDBSModule = rdbs_module;
            _PatchData = patch_data;
        }

        public ReloadableModule(
            ModuleDefinition target_module,
            ModuleDefinition patch_module = null,
            ModuleDefinition mmsg_module = null,
            ModuleDefinition rdbs_module = null,
            PatchData patch_data = null,
            Stream patch_asm_stream = null,
            Stream mmsg_asm_stream = null,
            Stream rdbs_asm_stream = null,
            Stream patch_data_stream = null
        ) {
            _TargetModule = target_module;

            PatchAssemblyStream = patch_asm_stream;
            MMSGAssemblyStream = mmsg_asm_stream;
            RDBSAssemblyStream = rdbs_asm_stream;
            PatchDataStream = patch_data_stream;

            _PatchModule = patch_module;
            _MMSGModule = mmsg_module;
            _RDBSModule = rdbs_module;
            _PatchData = patch_data;

        }

        public static AssemblyDiff Compare(ReloadableModule a, ReloadableModule b) {
            if (a == null && b == null) return AssemblyDiff.Empty;
            if (b == null) {
                return new AssemblyDiff(
                    new CILAbsoluteDiffSource(AbsoluteDiffSourceMode.AllRemoved, a.PatchModule),
                    new SemiPatchAbsoluteDiffSource(AbsoluteDiffSourceMode.AllRemoved, a.PatchData)
                );
            }
            if (a == null) {
                return new AssemblyDiff(
                    new CILAbsoluteDiffSource(AbsoluteDiffSourceMode.AllAdded, b.PatchModule),
                    new SemiPatchAbsoluteDiffSource(AbsoluteDiffSourceMode.AllAdded, b.PatchData)
                );
            }
            return new AssemblyDiff(
                new CILDiffSource(a.PatchModule, b.PatchModule),
                new SemiPatchDiffSource(a.PatchData, b.PatchData)
            );
        }

        public static bool IsSPRMagic(byte[] ary) {
            if (ary.Length != 3) return false;
            if (ary[0] != MAGIC[0]) return false;
            if (ary[1] != MAGIC[1]) return false;
            if (ary[2] != MAGIC[2]) return false;
            return true;
        }

        public static ReloadableModule Read(string file_path, ModuleDefinition target_module) {
            Stream stream = File.Open(file_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Read(stream, target_module);
        }

        public static ReloadableModule Read(Stream stream, ModuleDefinition target_module) {
            var compression_flag = stream.ReadByte();
            if (compression_flag == 1) {
                var file_stream = stream;
                stream = new MemoryStream();
                stream.WriteByte(0);
                using (var deflate = new DeflateStream(file_stream, CompressionMode.Decompress, true)) {
                    deflate.CopyTo(stream);
                }
                file_stream.Dispose();
                stream.Position = 0;
                using (var testf = File.OpenWrite("test.xx")) {
                    stream.CopyTo(testf);
                }
                stream.Position = 1;

            }

            var reader = new BinaryReader(stream);

            var magic = reader.ReadBytes(3);
            if (!IsSPRMagic(magic)) {
                throw new CorruptedReloadableModuleException("magic");
            }
            var ver = reader.ReadInt32();
            if (ver != VERSION) {
                throw new InvalidVersionReloadableModuleException(ver);
            }

            var patch_asm_file = SPRFile.ReadFrom(reader);
            if (!patch_asm_file.IsPresent) {
                throw new CorruptedReloadableModuleException("header.patch_asm_file");
            }

            var patch_asm_stream = patch_asm_file.OpenStreamTo(stream);

            var mmsg_asm_stream = SPRFile.TryReadStream(reader, stream);
            var rdbs_asm_stream = SPRFile.TryReadStream(reader, stream);
            var patch_data_file = SPRFile.ReadFrom(reader);
            if (!patch_data_file.IsPresent) {
                throw new CorruptedReloadableModuleException("header.patch_data_file");
            }
            var patch_data_stream = patch_data_file.OpenStreamTo(stream);

            return new ReloadableModule(
                target_module,
                patch_asm_stream,
                mmsg_asm_stream,
                rdbs_asm_stream,
                patch_data_stream
            ) { _SourceStream = stream };
        }

        public void Write(Stream stream, bool compressed = true) {
            try {
                var target_stream = stream;

                if (compressed) {
                    target_stream.WriteByte(1);
                    stream = new MemoryStream();
                } else stream.WriteByte(0);

                var writer = new BinaryWriter(stream);

                writer.Write(MAGIC);
                writer.Write(VERSION);

                using (var patch_module_ms = new MemoryStream())
                using (var mmsg_module_ms = new MemoryStream())
                using (var rdbs_module_ms = new MemoryStream())
                using (var patch_data_ms = new MemoryStream()) {
                    PatchModule.Write(patch_module_ms);
                    if (MMSGModule != null) MMSGModule.Write(mmsg_module_ms);
                    if (RDBSModule != null) RDBSModule.Write(rdbs_module_ms);
                    PatchData.Serialize(new BinaryWriter(patch_data_ms));

                    var offs = writer.BaseStream.Position + (4 * SPRFile.SizeInBytes);
                    if (compressed) offs += 1;
                    // if uncompressed, compression byte is part of output stream
                    // so data starts at 1 - if compressed, the compression byte is first
                    // written directly to the file and the data is written to a
                    // MemoryStream, hence the data will start at 0

                    new SPRFile(offs, patch_module_ms.Length).Write(writer);
                    offs += patch_module_ms.Length;
                    if (MMSGModule != null) {
                        new SPRFile(offs, mmsg_module_ms.Length).Write(writer);
                        offs += mmsg_module_ms.Length;
                    } else SPRFile.MissingFile.Write(writer);
                    if (RDBSModule != null) {
                        new SPRFile(offs, rdbs_module_ms.Length).Write(writer);
                        offs += rdbs_module_ms.Length;
                    } else SPRFile.MissingFile.Write(writer);
                    new SPRFile(offs, patch_data_ms.Length).Write(writer);
                    offs += patch_data_ms.Length;

                    patch_module_ms.WriteTo(writer.BaseStream);
                    if (MMSGModule != null) mmsg_module_ms.WriteTo(writer.BaseStream);
                    if (RDBSModule != null) rdbs_module_ms.WriteTo(writer.BaseStream);
                    patch_data_ms.WriteTo(writer.BaseStream);
                }

                if (compressed) {
                    using (var deflate = new DeflateStream(target_stream, CompressionMode.Compress, true)) {
                        ((MemoryStream)stream).WriteTo(deflate);
                    }
                }
            } finally {
                if (compressed) stream.Dispose();
            }
        }

        public void Dispose() {
            PatchAssemblyStream.Dispose();
            PatchDataStream.Dispose();
            MMSGAssemblyStream?.Dispose();
            RDBSAssemblyStream?.Dispose();
            _SourceStream.Dispose();
        }
    }
}
