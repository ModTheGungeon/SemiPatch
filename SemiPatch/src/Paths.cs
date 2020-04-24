using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace SemiPatch {
    public class TypePathSearchException : Exception {
        public TypePathSearchException(TypePath path) : base($"Failed to find type path '{path}'") { }
    }

    public class MemberPathSearchException<T> : Exception {
        public MemberPathSearchException(MemberPath<T> path) : base($"Failed to find member path '{path}'") { }
    }

    public struct Signature {
        private readonly string _Value;

        internal Signature(string value) { _Value = value; }
        public Signature(MethodReference method, bool skip_first_arg = false, string forced_name = null) : this(method.BuildSignature(skip_first_arg, forced_name)) { }
        public Signature(TypeReference type) : this(type.BuildSignature()) { }
        public Signature(FieldReference field, string forced_name = null) : this(field.BuildSignature(forced_name: forced_name)) { }
        public Signature(PropertyReference prop, string forced_name = null) : this(prop.BuildSignature(forced_name: forced_name)) { }

        public override string ToString() {
            return _Value;
        }

        public override int GetHashCode() {
            return _Value.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is Signature) || (obj is null)) return false;
            return ((Signature)obj)._Value == _Value;
        }

        public static bool operator==(Signature a, Signature b) {
            return a._Value == b._Value;
        }

        public static bool operator !=(Signature a, Signature b) {
            return a._Value != b._Value;
        }

        public static Signature FromInterface(IMemberDefinition member, string forced_name = null) {
            if (member is MethodDefinition) return new Signature((MethodDefinition)member, forced_name: forced_name);
            if (member is FieldDefinition) return new Signature((FieldDefinition)member, forced_name: forced_name);
            if (member is PropertyDefinition) return new Signature((PropertyDefinition)member, forced_name: forced_name);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in Signature.FromInterface: {member?.GetType().Name ?? "<null>"}");
        }
    }

    public abstract class MemberPath {
        public string Namespace;
        protected string _TypeName;
        protected IList<string> _TypeNames;
        public Signature Signature;

        public abstract string MemberTypeName { get; }

        public IList<string> TypeNames {
            get {
                if (_TypeNames != null) return _TypeNames;
                _TypeNames = new List<string>();
                _TypeNames.Add(_TypeName);
                return _TypeNames;
            }
        }

        public string DeclaringType {
            get {
                var s = new StringBuilder();
                if (Namespace != "") {
                    s.Append(Namespace);
                    s.Append(".");
                }
                if (_TypeName != null) s.Append(_TypeName);
                else {
                    for (var i = 0; i < _TypeNames.Count; i++) {
                        s.Append(_TypeNames[i]);
                        if (i < _TypeNames.Count - 1) s.Append(".");
                    }
                }
                return s.ToString();
            }
        }

        public abstract MemberPath Snapshot();

        protected virtual void CopyFrom(MemberPath path) {
            _TypeNames = path._TypeNames;
            _TypeName = path._TypeName;
            Signature = path.Signature;
            Namespace = path.Namespace;

        }

        private static void _InitTypePathRecursive(IList<string> list, TypeDefinition type) {
            if (type == null) return;
            _InitTypePathRecursive(list, type.DeclaringType);
            list.Add(type.Name);
        }

        private void _InitTypePath(TypeDefinition type) {
            if (type.DeclaringType == null) _TypeName = type.Name;
            else {
                _TypeNames = new List<string>();
                _InitTypePathRecursive(_TypeNames, type);
            }
        }

        protected MemberPath(TypeDefinition decl_type) {
            _InitTypePath(decl_type);
            Namespace = decl_type.Namespace;
        }

        protected MemberPath() { }

        public override string ToString() {
            return $"[{DeclaringType}] {Signature}";
        }

        public bool Equals(MemberPath member) {
            return member.GetHashCode() == GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (!(obj is MemberPath)) return false;
            return Equals((MemberPath)obj);
        }

        public override int GetHashCode() {
            return MemberTypeName.GetHashCode() ^ ToString().GetHashCode();
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(Namespace);
            if (_TypeName != null) {
                writer.Write(1);
                writer.Write(_TypeName);
            } else {
                writer.Write(_TypeNames.Count);
                for (var i = 0; i < _TypeNames.Count; i++) {
                    writer.Write(_TypeNames[i]);
                }
            }
            writer.Write(Signature.ToString());
        }

        protected void InitializeFrom(BinaryReader reader) {
            Namespace = reader.ReadString();
            var type_name_count = reader.ReadInt32();
            if (type_name_count == 1) {
                _TypeName = reader.ReadString();
            } else {
                _TypeNames = new string[type_name_count];
                for (var i = 0; i < type_name_count; i++) {
                    _TypeNames[i] = reader.ReadString();
                }
            }
            Signature = new Signature(reader.ReadString());
        }
    }

    public abstract class MemberPath<T> : MemberPath {
        protected MemberPath(TypeDefinition decl_type) : base(decl_type) {}
        protected MemberPath() { }

        public abstract T FindIn(ModuleDefinition mod);

        protected Exception PathSearchException() {
            return new MemberPathSearchException<T>(this);
        }

        protected TypeDefinition FindDeclaringType(ModuleDefinition mod) {
            if (_TypeName != null) {
                for (var i = 0; i < mod.Types.Count; i++) {
                    var type = mod.Types[i];
                    if (type.Name == _TypeName && type.Namespace == Namespace) return type;
                }
                throw PathSearchException();
            } else {
                var idx = 0;
                TypeDefinition found_type = null;
                for (var i = 0; i < mod.Types.Count; i++) {
                    var type = mod.Types[i];
                    if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                        found_type = type;
                        break;
                    }
                }
                if (found_type == null) throw PathSearchException();
                idx += 1;
                while (idx < _TypeNames.Count) {
                    TypeDefinition new_found_type = null;
                    for (var i = 0; i < found_type.NestedTypes.Count; i++) {
                        var type = found_type.NestedTypes[i];
                        if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                            new_found_type = type;
                            break;
                        }
                    }
                    if (new_found_type == null) throw PathSearchException();

                    idx += 1;
                    found_type = new_found_type;
                }

                return found_type;
            }
        }
    }

    public class MethodPath : MemberPath<MethodDefinition> {
        public MethodPath(MethodDefinition method, bool skip_first_arg = false, string forced_name = null) : base(method.DeclaringType) {
            Signature = new Signature(method, skip_first_arg, forced_name);

        }

        internal MethodPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private MethodPath() { }

        public override string MemberTypeName => "Method";

        public override MemberPath Snapshot() {
            var p = new MethodPath();
            p.CopyFrom(this);
            return p;
        }

        private MethodDefinition _FindInType(TypeDefinition type) {
            for (var i = 0; i < type.Methods.Count; i++) {
                var method = type.Methods[i];
                var sig = new Signature(method);
                if (sig == Signature) return method;
            }
            throw PathSearchException();
        }

        public override MethodDefinition FindIn(ModuleDefinition mod) {
            return _FindInType(FindDeclaringType(mod));
        }

        public MethodPath WithDeclaringType(TypeDefinition decl_type) {
            return new MethodPath(Signature, decl_type);
        }

        public static MethodPath Deserialize(BinaryReader reader) {
            var p = new MethodPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    public class FieldPath : MemberPath<FieldDefinition> {
        public FieldPath(FieldDefinition field, string forced_name = null) : base(field.DeclaringType) {
            Signature = new Signature(field, forced_name: forced_name);
        }

        internal FieldPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private FieldPath() { }

        public override string MemberTypeName => "Field";

        public override MemberPath Snapshot() {
            var p = new FieldPath();
            p.CopyFrom(this);
            return p;
        }

        public override FieldDefinition FindIn(ModuleDefinition mod) {
            var type = FindDeclaringType(mod);
            for (var i = 0; i < type.Fields.Count; i++) {
                if (new Signature(type.Fields[i]) == Signature) return type.Fields[i];
            }
            throw PathSearchException();
        }

        public FieldPath WithDeclaringType(TypeDefinition decl_type) {
            return new FieldPath(Signature, decl_type);
        }

        public static FieldPath Deserialize(BinaryReader reader) {
            var p = new FieldPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    public class PropertyPath : MemberPath<PropertyDefinition> {
        public PropertyPath(PropertyDefinition prop, string forced_name = null) : base(prop.DeclaringType) {
            Signature = new Signature(prop, forced_name: forced_name);
        }

        internal PropertyPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private PropertyPath() { }

        public override string MemberTypeName => "Property";

        public override MemberPath Snapshot() {
            var p = new PropertyPath();
            p.CopyFrom(this);
            return p;
        }

        public override PropertyDefinition FindIn(ModuleDefinition mod) {
            var type = FindDeclaringType(mod);
            for (var i = 0; i < type.Properties.Count; i++) {
                if (new Signature(type.Properties[i]) == Signature) return type.Properties[i];
            }
            throw PathSearchException();
        }

        public PropertyPath WithDeclaringType(TypeDefinition decl_type) {
            return new PropertyPath(Signature, decl_type);
        }

        public static PropertyPath Deserialize(BinaryReader reader) {
            var p = new PropertyPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    public class TypePath {
        public string Namespace;
        protected string _TypeName;
        protected IList<string> _TypeNames;
        public Signature Signature;

        public IList<string> TypeNames {
            get {
                if (_TypeNames != null) return _TypeNames;
                _TypeNames = new List<string>();
                if (_TypeName != null) _TypeNames.Add(_TypeName);
                return _TypeNames;
            }
        }

        public string DeclaringType {
            get {
                var s = new StringBuilder();
                if (Namespace != "") {
                    s.Append(Namespace);
                    if (_TypeNames == null && _TypeName == null) return s.ToString();
                    s.Append(".");
                }
                if (_TypeName != null) s.Append(_TypeName);
                else {
                    for (var i = 0; i < _TypeNames.Count; i++) {
                        s.Append(_TypeNames[i]);
                        if (i < _TypeNames.Count - 1) s.Append(".");
                    }
                }
                return s.ToString();
            }
        }

        private static void _InitTypePathRecursive(IList<string> list, TypeDefinition type) {
            if (type == null) return;
            _InitTypePathRecursive(list, type.DeclaringType);
            list.Add(type.Name);
        }

        private void _InitTypePath(TypeDefinition type) {
            if (type == null) return;

            if (type.DeclaringType == null) _TypeName = type.Name;
            else {
                _TypeNames = new List<string>();
                _InitTypePathRecursive(_TypeNames, type);
            }
        }

        public TypePath(TypeDefinition type) {
            _InitTypePath(type.DeclaringType);
            Namespace = type.Namespace;
            Signature = new Signature(type);
        }

        public TypePath(Signature sig, TypeDefinition decl_type) {
            _InitTypePath(decl_type);
            Namespace = decl_type.Namespace;
            Signature = sig;
        }

        private TypePath() { }

        public override string ToString() {
            var decl_type = DeclaringType;
            if (decl_type == null) return $"[<root>] {Signature}";
            return $"[{decl_type}] {Signature}";
        }

        public bool Equals(TypePath member) {
            return member.GetHashCode() == GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (!(obj is TypePath)) return false;
            return Equals((TypePath)obj);
        }

        public override int GetHashCode() {
            return "Type".GetHashCode() ^ ToString().GetHashCode();
        }

        public void Serialize(BinaryWriter writer) {
            writer.Write(Namespace);
            if (_TypeName == null && _TypeNames == null) {
                writer.Write(0);
            } else if (_TypeNames == null) {               
                writer.Write(1);
                writer.Write(_TypeName);
            } else {
                writer.Write(_TypeNames.Count);
                for (var i = 0; i < _TypeNames.Count; i++) {
                    writer.Write(_TypeNames[i]);
                }
            }
            writer.Write(Signature.ToString());
        }

        protected void InitializeFrom(BinaryReader reader) {
            Namespace = reader.ReadString();
            var type_name_count = reader.ReadInt32();
            _TypeName = null;
            if (type_name_count == 1) {
                _TypeName = reader.ReadString();
            } else {
                _TypeNames = new string[type_name_count];
                for (var i = 0; i < type_name_count; i++) {
                    _TypeNames[i] = reader.ReadString();
                }
            }
            Signature = new Signature(reader.ReadString());
        }

        protected Exception PathSearchException() {
            return new TypePathSearchException(this);
        }

        protected TypeDefinition FindDeclaringType(ModuleDefinition mod) {
            if (_TypeName == null && _TypeNames == null) return null;

            if (_TypeName != null) {
                for (var i = 0; i < mod.Types.Count; i++) {
                    var type = mod.Types[i];
                    if (type.Name == _TypeName && type.Namespace == Namespace) return type;
                }
                throw PathSearchException();
            } else {
                var idx = 0;
                TypeDefinition found_type = null;
                for (var i = 0; i < mod.Types.Count; i++) {
                    var type = mod.Types[i];
                    if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                        found_type = type;
                        break;
                    }
                }
                if (found_type == null) throw PathSearchException();
                idx += 1;
                while (idx < _TypeNames.Count) {
                    TypeDefinition new_found_type = null;
                    for (var i = 0; i < found_type.NestedTypes.Count; i++) {
                        var type = found_type.NestedTypes[i];
                        if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                            new_found_type = type;
                            break;
                        }
                    }
                    if (new_found_type == null) throw PathSearchException();

                    idx += 1;
                    found_type = new_found_type;
                }

                return found_type;
            }
        }

        public TypeDefinition FindIn(ModuleDefinition mod) {
            var type = FindDeclaringType(mod);
            if (type == null) {
                for (var i = 0; i < mod.Types.Count; i++) {
                    if (new Signature(mod.Types[i]) == Signature) return mod.Types[i];
                }
            } else {
                for (var i = 0; i < type.NestedTypes.Count; i++) {
                    if (new Signature(type.NestedTypes[i]) == Signature) return type.NestedTypes[i];
                }
            }
            throw PathSearchException();
        }

        public TypePath WithDeclaringType(TypeDefinition decl_type) {
            return new TypePath(Signature, decl_type);
        }

        public static TypePath Deserialize(BinaryReader reader) {
            var p = new TypePath();
            p.InitializeFrom(reader);
            return p;
        }
    }
}
