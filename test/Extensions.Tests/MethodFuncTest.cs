using System.Collections.Generic;
using System.Linq;
using Rocket.Surgery.Extensions.Tests.Fixtures;

namespace Rocket.Surgery.Extensions.Tests
{
    public class MethodFuncTest
    {
        public virtual void Execute() { }
        public virtual void Execute(IConfigured1 i1) { }
        public virtual void Execute(IInstance instance) { }
        public virtual void Execute(IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }
        public virtual void Execute(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { }

        public virtual bool ExecuteReturn() { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1) { return true; }
        public virtual bool ExecuteReturn(IInstance instance) { return true; }
        public virtual bool ExecuteReturn(IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }
        public virtual bool ExecuteReturn(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return true; }

        public virtual IEnumerable<IReturn> ExecuteEnumerable() { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IInstance instance) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInstance instance, IInjected1 n1, IInjected2 n2, IInjected3 n3 = null) { return Enumerable.Empty<IReturn>(); }
    }
}
