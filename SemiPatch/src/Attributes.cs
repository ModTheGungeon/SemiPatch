using System;
namespace SemiPatch {
    /// <summary>
    /// Marks a class as a patch class. This causes the SemiPatch analyzer to become
    /// aware of the tagged type and enables you to specify how the target class
    /// should be altered using the other available attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchAttribute : Attribute {
        /// <param name="type">
        /// Determines the target class, which is the one that will be altered
        /// using data from the tagged patch class. Use a <c>typeof(...)</c>
        /// expression here, e.g. <c>[PatchAttribute(typeof(Foo))]</c>.
        /// </param>
        public PatchAttribute(Type type) {
            Type = type;
        }

        public Type Type;
    }

    /// <summary>
    /// Marks a member of a patch class (<see cref="PatchAttribute"/>) to be
    /// inserted, as opposed to changing an existing member. A new member will be
    /// created in the target class with the data and properties of one tagged
    /// with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class InsertAttribute : Attribute {

    }

    /// <summary>
    /// Marks a member of a patch class (<see cref="PatchAttribute"/>) to be
    /// completely ignored by SemiPatch. Note that if you tag a property with this,
    /// the compiler-generated getter and setter methods as well as the backing field
    /// if one exists will also be ignored.
    /// 
    /// This attribute can also be used on a patch class itself. Doing so will
    /// cause SemiPatch to ignore the entire class, which might come in handy
    /// during debugging or experimenting.
    /// 
    /// If all you want is to just access an existing member on the target class,
    /// it is preferrable to use <see cref="ProxyAttribute"/> over
    /// <see cref="IgnoreAttribute"/>. The former will ensure that you don't
    /// accidentally try to access members that don't exist.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreAttribute : Attribute {

    }

    /// <summary>
    /// Specifies that a member of a patch class (<see cref="PatchAttribute"/>)
    /// has a different name than the one actually defined, or in the case of
    /// <see cref="InsertAttribute"/> that the member should be renamed when
    /// adding. If this attribute is used, this member will be renamed to the
    /// provided name in the final product (at runtime), and any reference to it
    /// will be appropriately renamed as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class TargetNameAttribute : Attribute {
        /// <param name="name">The desired final name for this member.</param>
        public TargetNameAttribute(string name) { }
    }

    /// <summary>
    /// Specifies that a member of a patch class (<see cref="PatchAttribute"/>)
    /// shall receive the ability to call its original version.
    /// 
    /// This is done through
    /// enforcing a special first argument in the method. This argument must have 
    /// as its type one of the different forms of the <see cref="Orig{}"/> and
    /// <see cref="VoidOrig"/> delegates. There are 30 forms each (60 in total).
    /// 
    /// This special delegate type must represent the signature of the target
    /// method exactly, otherwise analysis will fail. It can be called like a
    /// normal delegate within the method, and it can be passed to other methods
    /// or used in other ways like any normal delegate.
    /// 
    /// For example, if patching method:
    /// 
    /// <code>public string SetAge(string name, int number) {...}</code>
    /// 
    /// a normal (overwriting) patch method would look like this:
    /// 
    /// <code>public string SetAge(string name, int number) {...}</code>
    /// 
    /// or like this:
    /// 
    /// <code>
    /// [TargetName("SetAge")]
    /// public string MySetAgePatch(string name, int number) {...}
    /// </code>
    /// 
    /// However, with this attribute, the patch method looks like this:
    /// 
    /// <code>
    /// [ReceiveOriginal]
    /// public string SetAge(Orig&lt;string, int, string&gt; orig, string name, int number) {...}
    /// </code>
    /// 
    /// Then, <c>orig</c> can be freely used, for example to alter the return
    /// value of the method:
    /// 
    /// <code>
    /// [ReceiveOriginal]
    /// public string SetAge(Orig&lt;string, int, string&gt; orig, string name, int number) {
    ///     return "foobar" + orig(name, number)
    /// }
    /// </code>
    /// 
    /// If the target method returns <c>void</c>, a variation of the
    /// <see cref="VoidOrig"/> type must be used, and the return type isn't included
    /// in the generic parameters for that type. In any other case, a variation of
    /// <see cref="Orig{}"/> must be used, and the return type is always the last
    /// generic parameter.
    /// 
    /// If static patching is used to inject this method, the method may be heavily
    /// rewritten. In the case of the MonoMod static patch generator, patch methods
    /// tagged with this attribute lose their special first argument. Any invocations
    /// of the <c>orig</c> delegate become direct invocations of the old method,
    /// while more uncommon usage of the delegate (such as passing it to other methods)
    /// is detected. If any such usage is found, a new local variable appears in the
    /// method body, which contains a real instance of the delegate type and is 
    /// initialized with the target method. A reference to this local variable
    /// then replaces any such reference to the <c>orig</c> argument.
    /// 
    /// This means that with the MonoMod static patcher, directly calling the
    /// orig delegate within a patch method tagged with <c>ReceiveOriginal</c>
    /// will always be optimized down to a simple and direct method call. At the
    /// same time, the behavior of the code will stay the same no matter what.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ReceiveOriginalAttribute : Attribute {
    }

    /// <summary>
    /// Specifies that a member of a patch class (<see cref="PatchAttribute"/>)
    /// shall behave as a proxy. This is similar to the behavior of
    /// <see cref="IgnoreAttribute"/>, however, there is one key difference -
    /// while ignored members are ignored completely, proxied members will be 
    /// ignored only once the analyzer validates that the target member exists.
    /// 
    /// It is preferrable to use <see cref="ProxyAttribute"/> over
    /// <see cref="IgnoreAttribute"/> in all cases, unless you have a very good
    /// reason for not doing so.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class ProxyAttribute : Attribute {
    }

    /// <summary>
    /// Specifies that a constructor of a patch class (<see cref="PatchAttribute"/>)
    /// shall be treated the same as a method.
    /// 
    /// The default behavior is SemiPatch is to ignore all constructors. Use this
    /// attribute if you want to patch constructors. All other attributes that work
    /// on methods will also work on constructors tagged with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class TreatLikeMethodAttribute : Attribute {
    }

    /// <summary>
    /// Specifies that the method of a patch class (<see cref="PatchAttribute"/>)
    /// shall be treated as the getter method of a property.
    /// 
    /// To minimize confusion, SemiPatch does not allow you to patch properties
    /// by specifying properties in the patch class - instead, you have to define
    /// methods tagged with <see cref="GetterAttribute"/> and/or <see cref="SetterAttribute"/>.
    /// 
    /// This is superior to specifying the compiler-generated getter method directly
    /// (e.g. <c>get_SomeProperty</c>), as it allows you to name the patch
    /// method something different and also validates the existence of the targetted
    /// property.
    /// 
    /// Note that all other attributes that work on normal methods also work on
    /// methods tagged with this attribute - <see cref="InsertAttribute"/>,
    /// <see cref="ProxyAttribute"/>, <see cref="ReceiveOriginalAttribute"/> etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class GetterAttribute : Attribute {
        /// <param name="prop">Name of the property to patch the getter of.</param>
        public GetterAttribute(string prop) { }
    }

    /// <summary>
    /// Specifies that the method of a patch class (<see cref="PatchAttribute"/>)
    /// shall be treated as the setter method of a property.
    /// 
    /// To minimize confusion, SemiPatch does not allow you to patch properties
    /// by specifying properties in the patch class - instead, you have to define
    /// methods tagged with <see cref="GetterAttribute"/> and/or <see cref="SetterAttribute"/>.
    /// 
    /// This is superior to specifying the compiler-generated setter method directly
    /// (e.g. <c>set_SomeProperty</c>), as it allows you to name the patch
    /// method something different and also validates the existence of the targetted
    /// property.
    /// 
    /// Note that all other attributes that work on normal methods also work on
    /// methods tagged with this attribute - <see cref="InsertAttribute"/>,
    /// <see cref="ProxyAttribute"/>, <see cref="ReceiveOriginalAttribute"/> etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SetterAttribute : Attribute {
        /// <param name="prop">Name of the property to patch the setter of.</param>
        public SetterAttribute(string prop) { }
    }
}
