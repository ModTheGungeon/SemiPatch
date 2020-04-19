using System;
using System.Runtime.CompilerServices;

namespace SemiPatch {
    public class PatchControl {
        // http://msdn.microsoft.com/en-us/library/ms973858.aspx#highperfmanagedapps_topic10

        protected static int _PreventInlineLoopCount = 3;

        // making this virtual will prevent inlining of calls to this method
        protected virtual void _PreventInlineStub() { }

        // and this method is going to be over 32 bytes of IL and therefore not
        // a candidate for inlining either
        protected virtual void _PreventInline() {
            // methods with complex control flow (while/switch) are not 
            // candidates for inlining

            // plus we use a public static int so compiler can't optimize
            // the while loop
            var i = _PreventInlineLoopCount;
            while (i > 0) _PreventInlineStub();

            // 15 calls, well above 32 bytes
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();
            _PreventInlineStub();

            // this method should never actually be called, if it is,
            // that is a MAJOR error and we will crash
            for (var j = 0; j < 15; j++) Console.WriteLine("!!!PATCHCONTROL CALLED AT RUNTIME!!!");
            Environment.Exit(30);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Cancel() { new PatchControl()._PreventInline(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetReturnValue(object obj) { new PatchControl()._PreventInline(); }
    }
}
