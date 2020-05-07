using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.Cecil;
using BindingFlags = System.Reflection.BindingFlags;

namespace SemiPatch {
    /// <summary>
    /// Represents a unique signature of a type or type member. In the case of
    /// type members, this signature is only guaranteed to be unique within the
    /// scope of its declaring type. For an identifier that's unique at the level
    /// of the assembly, see <see cref="TypePath"/> or <see cref="MemberPath"/>.
    /// </summary>
    public struct Signature {
        private readonly string _Value;
        public readonly string Name;

        internal Signature(string value, string name) { _Value = value; Name = name; }

        // cecil
        public Signature(MethodReference method, bool skip_first_arg = false, string forced_name = null) : this(method.BuildSignature(skip_first_arg, forced_name), method.Name) { }
        public Signature(TypeReference type) : this(type.BuildSignature(), type.Name) { }
        public Signature(FieldReference field, string forced_name = null) : this(field.BuildSignature(forced_name: forced_name), forced_name ?? field.Name) { }
        public Signature(PropertyReference prop, string forced_name = null) : this(prop.BuildSignature(forced_name: forced_name), forced_name ?? prop.Name) { }

        // reflection
        public Signature(Type type) : this(type.BuildSignature(), type.Name) { }
        public Signature(System.Reflection.MethodBase method, bool skip_first_arg = false, string forced_name = null) : this(method.BuildSignature(skip_first_arg, forced_name), forced_name ?? method.Name) { }
        public Signature(System.Reflection.FieldInfo field, string forced_name = null) : this(field.BuildSignature(forced_name: forced_name), forced_name ?? field.Name) { }


        public override string ToString() {
            return _Value;
        }

        public override int GetHashCode() {
            return _Value.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (!(obj is Signature) || (obj is null)) return false;
            if (obj is string) {
                return _Value == (string)obj;
            }
            return ((Signature)obj)._Value == _Value;
        }

        public static bool operator==(Signature a, Signature b) {
            return a._Value == b._Value;
        }

        public static bool operator !=(Signature a, Signature b) {
            return a._Value != b._Value;
        }

        public static bool operator ==(Signature a, string b) {
            return a._Value == b;
        }

        public static bool operator !=(Signature a, string b) {
            return a._Value != b;
        }

        public static Signature FromInterface(IMemberDefinition member, string forced_name = null) {
            if (member is MethodDefinition) return new Signature((MethodDefinition)member, forced_name: forced_name);
            if (member is FieldDefinition) return new Signature((FieldDefinition)member, forced_name: forced_name);
            if (member is PropertyDefinition) return new Signature((PropertyDefinition)member, forced_name: forced_name);
            throw new InvalidOperationException($"Unsupported IMemberDefinition in Signature.FromInterface: {member?.GetType().Name ?? "<null>"}");
        }
    }

    /// <summary>
    /// Represents an identifier and path to a type member. Unique within
    /// the context of the declaring assembly.
    /// </summary>
    public abstract class MemberPath {
        public string Namespace;
        protected string _TypeName;
        protected IList<string> _TypeNames;
        public Signature Signature;

        public abstract MemberType Type { get; }

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

        private static void _InitTypePathRecursive(IList<string> list, Type type) {
            if (type == null) return;
            _InitTypePathRecursive(list, type.DeclaringType);
            list.Add(type.Name);
        }

        private void _InitTypePath(Type type) {
            if (type.DeclaringType == null) _TypeName = type.Name;
            else {
                _TypeNames = new List<string>();
                _InitTypePathRecursive(_TypeNames, type);
            }
        }

        public abstract IMemberDefinition FindIn(ModuleDefinition mod);
        public abstract System.Reflection.MemberInfo FindIn(System.Reflection.Assembly asm);

        protected Exception PathSearchException() {
            return new MemberPathSearchException(this);
        }

        public T FindIn<T>(ModuleDefinition mod) where T : class, IMemberDefinition {
            return FindIn(mod) as T;
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

        protected Type FindDeclaringType(System.Reflection.Assembly asm) {
            var asm_types = asm.GetTypes();
            if (_TypeName != null) {
                for (var i = 0; i < asm_types.Length; i++) {
                    var type = asm_types[i];
                    if (type.Name == _TypeName && type.Namespace == Namespace) return type;
                }
                throw PathSearchException();
            } else {
                var idx = 0;
                Type found_type = null;
                for (var i = 0; i < asm_types.Length; i++) {
                    var type = asm_types[i];
                    if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                        found_type = type;
                        break;
                    }
                }
                if (found_type == null) throw PathSearchException();
                idx += 1;
                var nested_types = found_type.GetNestedTypes();
                while (idx < _TypeNames.Count) {
                    Type new_found_type = null;
                    for (var i = 0; i < nested_types.Length; i++) {
                        var type = nested_types[i];
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

        protected MemberPath(TypeDefinition decl_type) {
            _InitTypePath(decl_type);
            Namespace = decl_type.Namespace;
        }

        protected MemberPath(Type decl_type) {
            _InitTypePath(decl_type);
            Namespace = decl_type.Namespace;
        }

        protected MemberPath() { }

        public override string ToString() {
            return $"[{DeclaringType}] {Signature}";
        }

        public bool Equals(MemberPath member) {
            if (Namespace != member.Namespace) return false;
            if (Signature != member.Signature) return false;
            if (_TypeName != member._TypeName) return false;
            if ((_TypeNames == null && member._TypeNames != null) || (_TypeNames != null && member._TypeNames == null)) return false;
            if (_TypeNames != null) {
                for (var i = 0; i < _TypeNames.Count; i++) {
                    if (_TypeNames[i] != member._TypeNames[i]) return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (!(obj is MemberPath)) return false;
            return Equals((MemberPath)obj);
        }

        public override int GetHashCode() {
            return Type.GetHashCode() ^ ToString().GetHashCode();
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
            writer.Write(Signature.Name.ToString());
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
            Signature = new Signature(reader.ReadString(), reader.ReadString());
        }

        protected void InitTypePathFrom(TypePath path) {
            Namespace = path.Namespace;
            if (path._TypeName == null && path._TypeNames == null) {
                _TypeName = path.Signature.Name;
            } else if (path._TypeName != null) {
                _TypeNames = new List<string>();
                _TypeNames.Add(path._TypeName);
                _TypeNames.Add(path.Signature.Name);
            } else {
                _TypeNames = new List<string>();
                for (var i = 0; i < path._TypeNames.Count; i++) _TypeNames.Add(path._TypeNames[i]);
                _TypeNames.Add(path.Signature.Name);
            }
            Signature = Signature;
        }

        public static bool operator ==(MemberPath a, MemberPath b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(MemberPath a, MemberPath b) {
            if (a is null && b is null) return false;
            if (a is null || b is null) return true;
            return !a.Equals(b);
        }
    }

    /// <summary>
    /// Represents an identifier and path to a method. Unique within the context
    /// of its declaring assembly.
    /// </summary>
    public class MethodPath : MemberPath {
        public MethodPath(MethodDefinition method, bool skip_first_arg = false, string forced_name = null) : base(method.DeclaringType) {
            Signature = new Signature(method, skip_first_arg, forced_name);

        }

        public MethodPath(System.Reflection.MethodBase method, bool skip_first_arg = false, string forced_name = null) : base(method.DeclaringType) {
            Signature = new Signature(method, skip_first_arg, forced_name);

        }

        internal MethodPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private MethodPath() { }

        public override MemberType Type => MemberType.Method;

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

        private System.Reflection.MethodBase _FindInType(Type type) {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            for (var i = 0; i < methods.Length; i++) {
                var method = methods[i];
                var sig = new Signature(method);
                if (sig == Signature) return method;
            }
            // ??? why is this a separate thing
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            for (var i = 0; i < constructors.Length; i++) {
                var ctor = constructors[i];
                var sig = new Signature(ctor);
                if (sig == Signature) return ctor;
            }
            throw PathSearchException();
        }

        public override IMemberDefinition FindIn(ModuleDefinition mod) {
            return _FindInType(FindDeclaringType(mod));
        }

        public override System.Reflection.MemberInfo FindIn(System.Reflection.Assembly asm) {
            return _FindInType(FindDeclaringType(asm));
        }

        public MethodPath WithDeclaringType(TypeDefinition decl_type) {
            return new MethodPath(Signature, decl_type);
        }

        public MethodPath WithDeclaringTypePath(TypePath path) {
            var p = new MethodPath();
            p.InitTypePathFrom(path);
            p.Signature = Signature;
            return p;
        }

        public MethodPath WithSignature(Signature sig) {
            var p = new MethodPath();
            p._TypeName = _TypeName;
            p._TypeNames = _TypeNames;
            p.Namespace = Namespace;
            p.Signature = sig;
            return p;
        }

        public static MethodPath Deserialize(BinaryReader reader) {
            var p = new MethodPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    /// <summary>
    /// Represents an identifier and path to a field. Unique within the context
    /// of its declaring assembly.
    /// </summary>
    public class FieldPath : MemberPath {
        public FieldPath(FieldDefinition field, string forced_name = null) : base(field.DeclaringType) {
            Signature = new Signature(field, forced_name: forced_name);
        }

        public FieldPath(System.Reflection.FieldInfo field, string forced_name = null) : base(field.DeclaringType) {
            Signature = new Signature(field, forced_name: forced_name);
        }

        internal FieldPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private FieldPath() { }

        public override MemberType Type=> MemberType.Field;

        public override MemberPath Snapshot() {
            var p = new FieldPath();
            p.CopyFrom(this);
            return p;
        }

        public override IMemberDefinition FindIn(ModuleDefinition mod) {
            var type = FindDeclaringType(mod);
            for (var i = 0; i < type.Fields.Count; i++) {
                if (new Signature(type.Fields[i]) == Signature) return type.Fields[i];
            }
            throw PathSearchException();
        }

        public override System.Reflection.MemberInfo FindIn(System.Reflection.Assembly asm) {
            var type = FindDeclaringType(asm);
            var fields = type.GetFields();
            for (var i = 0; i < fields.Length; i++) {
                if (new Signature(fields[i]) == Signature) return fields[i];
            }
            throw PathSearchException();
        }

        public FieldPath WithDeclaringType(TypeDefinition decl_type) {
            return new FieldPath(Signature, decl_type);
        }

        public FieldPath WithDeclaringTypePath(TypePath path) {
            var p = new FieldPath();
            p.InitTypePathFrom(path);
            p.Signature = Signature;
            return p;
        }

        public static FieldPath Deserialize(BinaryReader reader) {
            var p = new FieldPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    /// <summary>
    /// Represents an identifier and path to a property. Unique within the context
    /// of its declaring assembly.
    /// </summary>
    public class PropertyPath : MemberPath {
        public PropertyPath(PropertyDefinition prop, string forced_name = null) : base(prop.DeclaringType) {
            Signature = new Signature(prop, forced_name: forced_name);
        }

        internal PropertyPath(Signature sig, TypeDefinition decl_type) : base(decl_type) {
            Signature = sig;
        }

        private PropertyPath() { }

        public override MemberType Type => MemberType.Property;

        public override MemberPath Snapshot() {
            var p = new PropertyPath();
            p.CopyFrom(this);
            return p;
        }

        public override IMemberDefinition FindIn(ModuleDefinition mod) {
            var type = FindDeclaringType(mod);
            for (var i = 0; i < type.Properties.Count; i++) {
                if (new Signature(type.Properties[i]) == Signature) return type.Properties[i];
            }
            throw PathSearchException();
        }

        public override System.Reflection.MemberInfo FindIn(System.Reflection.Assembly asm) {
            throw new NotImplementedException();
        }

        public PropertyPath WithDeclaringType(TypeDefinition decl_type) {
            return new PropertyPath(Signature, decl_type);
        }

        public PropertyPath WithDeclaringTypePath(TypePath path) {
            var p = new PropertyPath();
            p.InitTypePathFrom(path);
            p.Signature = Signature;
            return p;
        }

        public static PropertyPath Deserialize(BinaryReader reader) {
            var p = new PropertyPath();
            p.InitializeFrom(reader);
            return p;
        }
    }

    /// <summary>
    /// Represents an identifier and path to a type. Unique within the context
    /// of its declaring assembly.
    /// </summary>
    public class TypePath {
        public string Namespace;
        internal string _TypeName;
        internal IList<string> _TypeNames;
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
                else if (_TypeNames != null && _TypeNames.Count > 0) {
                    for (var i = 0; i < _TypeNames.Count; i++) {
                        s.Append(_TypeNames[i]);
                        if (i < _TypeNames.Count - 1) s.Append(".");
                    }
                } else return null;
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

        private static void _InitTypePathRecursive(IList<string> list, Type type) {
            if (type == null) return;
            _InitTypePathRecursive(list, type.DeclaringType);
            list.Add(type.Name);
        }

        private void _InitTypePath(Type type) {
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

        public TypePath(Type type) {
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

        public bool Equals(TypePath type) {
            if (Namespace != type.Namespace) return false;
            if (Signature != type.Signature) return false;
            if (_TypeName != type._TypeName) return false;
            if ((_TypeNames == null && type._TypeNames != null) || (_TypeNames != null && type._TypeNames == null)) return false;
            if (_TypeNames != null) {
                for (var i = 0; i < _TypeNames.Count; i++) {
                    if (_TypeNames[i] != type._TypeNames[i]) return false;
                }
            }
            return true;
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
            writer.Write(Signature.Name.ToString());
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
            Signature = new Signature(reader.ReadString(), reader.ReadString());
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

        protected Type FindDeclaringType(System.Reflection.Assembly asm) {
            if (_TypeName == null && _TypeNames == null) return null;

            if (_TypeName != null) {
                var types = asm.GetTypes();
                for (var i = 0; i < types.Length; i++) {
                    var type = types[i];
                    if (type.Name == _TypeName && type.Namespace == Namespace) return type;
                }
                throw PathSearchException();
            } else {
                var idx = 0;
                Type found_type = null;
                var types = asm.GetTypes();
                for (var i = 0; i < types.Length; i++) {
                    var type = types[i];
                    if (type.Name == _TypeNames[idx] && type.Namespace == Namespace) {
                        found_type = type;
                        break;
                    }
                }
                if (found_type == null) throw PathSearchException();
                idx += 1;
                while (idx < _TypeNames.Count) {
                    Type new_found_type = null;
                    var nested_types = found_type.GetNestedTypes();
                    for (var i = 0; i < nested_types.Length; i++) {
                        var type = nested_types[i];
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

        public TypeDefinition FindInIncludingDependencies(ModuleDefinition mod) {
            var full_name = DeclaringType;
            if (full_name == null) full_name = Signature.Name;
            else full_name += $".{Signature.Name}";
            var type = SemiPatch.FindType(mod, full_name);
            if (type == null) throw PathSearchException();
            return type;
        }

        public Type FindIn(System.Reflection.Assembly asm) {
            var type = FindDeclaringType(asm);
            if (type == null) {
                var types = asm.GetTypes();
                for (var i = 0; i < types.Length; i++) {
                    if (new Signature(types[i]) == Signature) return types[i];
                }
            } else {
                var nested_types = type.GetNestedTypes();
                for (var i = 0; i < nested_types.Length; i++) {
                    if (new Signature(nested_types[i]) == Signature) return nested_types[i];
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

        public static bool operator ==(TypePath a, TypePath b) {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(TypePath a, TypePath b) {
            if (a is null && b is null) return false;
            if (a is null || b is null) return true;
            return !a.Equals(b);
        }
    }
}
