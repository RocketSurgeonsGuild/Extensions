using System;

namespace XUnitTestProject1
{
    public class MethodNotFoundException : Exception
    {
        public string[] MethodNames { get; }

        public MethodNotFoundException(string[] methodNames) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames))
        {
            MethodNames = methodNames;
        }

        public MethodNotFoundException(string[] methodNames, Exception innerException) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames), innerException)
        {
            MethodNames = methodNames;
        }
    }
}