public class Test<T> {
	public class NestedTest {
	}
}

public static class MainClass {
	public static void Main(string[] args) {
		var x = new Test<int>.NestedTest();

	}
}
