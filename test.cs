using System;
using System.Reflection;

public class TestClass {
	public int TestMe(int a, string b) {
		Console.WriteLine($"this = {this}, a = {a}, b = {b}");
		return 42;
	}

	public MainClass.TestDelegateA<int, string, int> GetTestMe() {
		return (MainClass.TestDelegateA<int, string, int>)TestMe;
	}
}

public class MainClass {
	public delegate R TestDelegateA<T, U, R>(T arg1, U arg2);
	public delegate R TestDelegateB<S, T, U, R>(S self, T arg1, U arg2);

	public static void Main(String[] args) {
		var t = new TestClass();
		var testme = t.GetTestMe();
		Console.WriteLine(t);
		testme(1, "hi");
		Console.WriteLine(testme);

		TestDelegateB<TestClass, int, string, int> testme2 = (self, a, b) => {
			return self.TestMe(a, b);
		};

		testme2(t, 2, "hi2");
		Console.WriteLine(testme2);

		TestDelegateA<int, string, int> testme3 = (TestDelegateA<int, string, int>)Delegate.CreateDelegate(
			typeof(TestDelegateA<int, string, int>),
			t,
			testme2.GetMethodInfo()
		);

		testme3(3, "hi3");
	}
}
