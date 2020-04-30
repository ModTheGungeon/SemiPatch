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
        public void Hello(string name) {
            Console.WriteLine($"Hello {name}!");
        }
    }

    public static class MyClass {
        public static bool TestBool;

        public static bool ExecuteCommand(string cmd) {
            if (cmd == "hello") {
                var c = new SmallClass();
                c.Hello("world");
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
