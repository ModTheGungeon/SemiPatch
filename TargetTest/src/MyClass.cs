using System;
namespace TargetTest {
    public class UtilityClass<T> {
        public string StrToPrint;
        //public string TestProp {
        //    get { return "Abc"; }
        //    set { }
        //}

        //public string GetOnlyProp {
        //    get { return "Get Only Prop"; }
        //}

        public UtilityClass(string str) {
            StrToPrint = str;
        }

        public int PrintString() {
            Console.WriteLine(PrepareString(StrToPrint));
            return 42;
        }

        public int Test(int a, string b) {
            Console.WriteLine(PrepareString(StrToPrint));
            return 42;
        }

        public static string PrepareString<X>(X str) {
            return $"[{str}]";
        }

        public static string PrepareString<X, Y, Z>(Z str, Y a, X b, int a1, int b1, int c1, int d1, int e1) {
            return $"[{str}]";
        }

        public void DoTest() {
            Console.WriteLine($"DoTest()");
        }
    }

    public class SmallClass {
        public string GetName(int a) {
            return $"name{a}";
        }

        public void Hello(string name) {
            var my_local = name + "!";
            Console.WriteLine($"Hello {my_local}");
            Console.WriteLine($"Hello again, {my_local}");
            Console.WriteLine($"GetName(42) result {GetName(42)}");
        }

        public string GetName() {
            return "world";
        }
    }

    public static class MyClass {
        public static bool TestBool;

        public static bool ExecuteCommand(string cmd) {
            if (cmd == "hello") {
                var c = new SmallClass();
                c.Hello(c.GetName());
                return true;
            }
            if (cmd == "quit") {
                Environment.Exit(0);
            }

            return false;
        }

        public static void Main(string[] args) {
            var u = new UtilityClass<string>("Hello, world!");
            var r = u.PrintString();
            u.DoTest();
            //Console.WriteLine(u.TestProp);
            //Console.WriteLine(u.GetOnlyProp);
            Console.WriteLine($"Result: {r}");

            while (true) {
                Console.Write("> ");
                var cmd = Console.ReadLine();
                var result = ExecuteCommand(cmd);
                if (!result) Console.WriteLine($"Unknown command: {cmd}");
            }
        }
    }
}
