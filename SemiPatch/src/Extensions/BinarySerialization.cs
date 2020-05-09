using System;
using System.IO;

namespace SemiPatch {
    public static partial class Extensions {
        public static void WriteNullable(this BinaryWriter writer, string obj) {
            if (obj == null) writer.Write((byte)0);
            else {
                writer.Write((byte)1);
                writer.Write(obj);
            }
        }

        public static string ReadNullableString(this BinaryReader reader) {
            if (reader.ReadByte() == (byte)0) {
                return null;
            }
            return reader.ReadString();
        }

        public static void Write(this BinaryWriter writer, MemberPath path) {
            path.Serialize(writer);
        }

        public static MethodPath ReadMethodPath(this BinaryReader reader) {
            return MethodPath.Deserialize(reader);
        }

        public static FieldPath ReadFieldPath(this BinaryReader reader) {
            return FieldPath.Deserialize(reader);
        }

        public static PropertyPath ReadPropertyPath(this BinaryReader reader) {
            return PropertyPath.Deserialize(reader);
        }
    }
}
