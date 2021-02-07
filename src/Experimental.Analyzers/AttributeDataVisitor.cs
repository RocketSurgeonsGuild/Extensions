using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Rocket.Surgery.Experimental.Analyzers
{
    class AttributeDataVisitor : SymbolVisitor
    {
        public static ImmutableArray<AttributeData> GetAttributes(CSharpCompilation compilation)
        {
            var visitor = new AttributeDataVisitor();
            visitor.VisitNamespace(compilation.GlobalNamespace);
            compilation.Assembly?.Accept(visitor);
            foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol).Where(z => z != null))
                symbol?.Accept(visitor);
            return visitor.GetAttributes();
        }

        private readonly List<AttributeData> _attributes = new List<AttributeData>();

        private void Accept<T>(IEnumerable<T> members)
            where T : ISymbol?
        {
            foreach (var member in members)
                member?.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            Accept(symbol.GetMembers());
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            symbol.GlobalNamespace.Accept(this);

            _attributes.AddRange(
                symbol
                   .GetAttributes()
                   .Where(z => z.AttributeClass?.ToDisplayString() == Constants.AutoImplementSourceAttribute)
            );
        }

        public ImmutableArray<AttributeData> GetAttributes() => _attributes.ToImmutableArray();
    }
}