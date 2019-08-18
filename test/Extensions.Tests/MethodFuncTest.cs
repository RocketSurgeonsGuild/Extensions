using System.Collections.Generic;
using System.Linq;
using Rocket.Surgery.Extensions.Tests.Fixtures;

namespace Rocket.Surgery.Extensions.Tests
{
    public class MethodFuncTest
    {
        public virtual void Execute() { }   
        public virtual void Execute0(IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute1(IConfigured1 i1, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute2(IConfigured1 i1, IConfigured2 i2, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute3(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute4(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute5(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute6(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute7(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute8(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute9(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute10(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute11(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute12(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute13(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }
        public virtual void Execute14(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { }

        public virtual bool ExecuteReturn() { return true; }
        public virtual bool ExecuteReturn0(IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn1(IConfigured1 i1, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn2(IConfigured1 i1, IConfigured2 i2, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn3(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn4(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn5(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn6(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn7(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn8(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn9(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn10(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn11(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn12(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn13(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }
        public virtual bool ExecuteReturn14(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return true; }

        public virtual IEnumerable<IReturn> ExecuteEnumerable() { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IConfigured1 i1) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable(IInstance instance) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable0(IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable1(IConfigured1 i1, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable2(IConfigured1 i1, IConfigured2 i2, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable3(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable4(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable5(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable6(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable7(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable8(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable9(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable10(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable11(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable12(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable13(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
        public virtual IEnumerable<IReturn> ExecuteEnumerable14(IConfigured1 i1, IConfigured2 i2, IConfigured3 i3, IConfigured4 i4, IConfigured5 i5, IConfigured6 i6, IConfigured7 i7, IConfigured8 i8, IConfigured9 i9, IConfigured10 i10, IConfigured11 i11, IConfigured12 i12, IConfigured13 i13, IConfigured14 i14, IInjected1 n1, IInjected2 n2, IInjected3? n3 = null) { return Enumerable.Empty<IReturn>(); }
    }
}
