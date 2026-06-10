// NUnit + Unity TestTools stubs for compile-checking test code.
using System;

namespace NUnit.Framework
{
    public class TestAttribute : Attribute { }
    public class SetUpAttribute : Attribute { }
    public class TearDownAttribute : Attribute { }
    public class TestFixtureAttribute : Attribute { }
    public class UnityTestAttribute : Attribute { }

    public static class Assert
    {
        public static void IsTrue(bool condition, string message = null) { }
        public static void IsFalse(bool condition, string message = null) { }
        public static void IsNull(object obj, string message = null) { }
        public static void IsNotNull(object obj, string message = null) { }
        public static void AreEqual(object expected, object actual, string message = null) { }
        public static void AreEqual(float expected, float actual, float delta, string message = null) { }
        public static void AreEqual(double expected, double actual, double delta, string message = null) { }
        public static void AreNotEqual(object expected, object actual, string message = null) { }
        public static void Greater(IComparable a, IComparable b, string message = null) { }
        public static void GreaterOrEqual(IComparable a, IComparable b, string message = null) { }
        public static void Less(IComparable a, IComparable b, string message = null) { }
        public static void LessOrEqual(IComparable a, IComparable b, string message = null) { }
        public static void Fail(string message = null) { }
        public static void Pass(string message = null) { }
        public static void Throws<T>(Action code) where T : Exception { }
    }
}

namespace UnityEngine.TestTools
{
    public class UnityTestAttribute : Attribute { }
    public static class LogAssert
    {
        public static void Expect(LogType type, string message) { }
        public static bool ignoreFailingMessages { get; set; }
        public static void NoUnexpectedReceived() { }
    }
}
