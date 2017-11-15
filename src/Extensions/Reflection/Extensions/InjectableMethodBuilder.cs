﻿using System;

namespace XUnitTestProject1
{
    public static class InjectableMethodBuilder
    {
        public static InjectableMethodBuilder<TContainer> Create<TContainer>(string methodName)
        {
            return new InjectableMethodBuilder<TContainer>().ForMethod(methodName);
        }
        public static InjectableMethodBuilder<object> Create(Type container, string methodName)
        {
            return new InjectableMethodBuilder<object>(container).ForMethod(methodName);
        }
    }
}