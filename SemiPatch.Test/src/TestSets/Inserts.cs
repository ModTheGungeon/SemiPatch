using SemiPatch;
using System;

namespace SemiPatch.Test {
    public static partial class TestSets {
        public class SimpleInsertsTarget {
            public string Foo;
            public int Bar;
            public char Baz => 'a';
        }

        [TestPatch("SimpleInsertsTarget")]
        public class SimpleInsertsPatch1 {
            [Insert]
            public string NewFoo;

            [Insert]
            public object InsertedProperty => "foo";

            [Insert]
            public void Test() {
                Console.WriteLine("hello");
            }
        }

        [TestPatch("SimpleInsertsTarget")]
        public class SimpleInsertsPatch2 {
            [Insert]
            public string NewFoo2;

            [Insert]
            public object InsertedProperty2 => "foo";

            [Insert]
            public void Test2() {
                Console.WriteLine("hello");
            }
        }

    }
}
