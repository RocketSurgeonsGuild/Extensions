using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Experimental.Analyzers
{
    class TypeSymbolVisitor : SymbolVisitor
    {
        private readonly GeneratorExecutionContext _context;
        private readonly ImmutableArray<AttributeData> _attributeData;
        private readonly ImmutableDictionary<string, TypeDeclarationSyntax> _interfaces;
        private readonly Dictionary<string, bool> _values = new Dictionary<string, bool>();

        public static void GetTypes(
            GeneratorExecutionContext context,
            CSharpCompilation compilation,
            ImmutableArray<AttributeData> interfaces
        )
        {
            var visitor = new TypeSymbolVisitor(context, interfaces);
            visitor.VisitNamespace(compilation.GlobalNamespace);
            compilation.Assembly?.Accept(visitor);
            foreach (var symbol in compilation.References.Select(compilation.GetAssemblyOrModuleSymbol).Where(z => z != null))
                symbol?.Accept(visitor);
        }

        public TypeSymbolVisitor(GeneratorExecutionContext context, ImmutableArray<AttributeData> attributeData)
        {
            _context = context;
            _attributeData = attributeData;
            _interfaces = attributeData
               .Select(
                    attr =>
                    {
                        var syntax = ParseSyntaxTree((string)attr.ConstructorArguments[0].Value!)
                           .GetRoot()
                           .DescendantNodesAndSelf()
                           .OfType<TypeDeclarationSyntax>()
                           .Single();
                        return ( data: attr, syntax );
                    }
                )
               .ToImmutableDictionary(z => z.syntax.GetFullMetadataName(), z => z.syntax);
        }

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
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Struct)
            {
                if (!symbol.CanBeReferencedByName || symbol.DeclaringSyntaxReferences is { Length: 0 })
                    return;
                if (_values.ContainsKey(symbol.GetFullMetadataName()))
                    return;
                _values[symbol.GetFullMetadataName()] = true;

                TypeDeclarationSyntax? declarationSyntax = null;
                var namespaces = new List<UsingDirectiveSyntax>();

                var syntaxes = symbol.DeclaringSyntaxReferences
                   .Select(x => x.GetSyntax())
                   .OfType<TypeDeclarationSyntax>()
                   .ToImmutableArray();

                foreach (var @interface in GetMixins(symbol))
                {
                    TypeDeclarationSyntaxes(
                        _context,
                        @interface,
                        _interfaces,
                        ref syntaxes,
                        namespaces,
                        ref declarationSyntax
                    );
                }


                static void TypeDeclarationSyntaxes(
                    GeneratorExecutionContext context,
                    INamedTypeSymbol namedTypeSymbol,
                    ImmutableDictionary<string, TypeDeclarationSyntax> interfaces,
                    ref ImmutableArray<TypeDeclarationSyntax> syntaxes,
                    List<UsingDirectiveSyntax> namespaces,
                    ref TypeDeclarationSyntax? declarationSyntax
                )
                {
                    if (!interfaces.TryGetValue(namedTypeSymbol.GetFullMetadataName(), out var interfaceSyntax))
                        return;

                    foreach (var syntax in syntaxes)
                    {
                        if (!syntax.Modifiers.Any(z => z.IsKind(SyntaxKind.PartialKeyword)))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(Diagnostics.MustBePartial, syntax.Identifier.GetLocation(), syntax.GetFullMetadataName())
                            );
                        }
                    }

                    declarationSyntax ??= syntaxes[0]
                       .WithMembers(List(ImmutableArray<MemberDeclarationSyntax>.Empty))
                       .WithAttributeLists(List(ImmutableArray<AttributeListSyntax>.Empty))
                       .WithConstraintClauses(List(Array.Empty<TypeParameterConstraintClauseSyntax>()))
                       .WithBaseList(null);

                    var rewriter = new InterfaceSyntaxRewriter(syntaxes);
                    namespaces.AddRange(interfaceSyntax.SyntaxTree.GetCompilationUnitRoot().Usings);
                    interfaceSyntax = (TypeDeclarationSyntax)rewriter.Visit(interfaceSyntax);
                    declarationSyntax = (TypeDeclarationSyntax)declarationSyntax
                       .AddMembers(interfaceSyntax.Members.ToArray())
                       .AddAttributeLists(interfaceSyntax.AttributeLists.ToArray())
                       .AddBaseListTypes(interfaceSyntax.BaseList is null ? Array.Empty<BaseTypeSyntax>() : @interfaceSyntax.BaseList.Types.ToArray());
                    syntaxes = syntaxes.Add(declarationSyntax);

                    foreach (var symbol in GetMixins(namedTypeSymbol))
                    {
                        TypeDeclarationSyntaxes(context, symbol, interfaces, ref syntaxes, namespaces, ref declarationSyntax);
                    }
                }


                if (declarationSyntax is { })
                {
                    if (declarationSyntax.BaseList?.Types.Any() == true)
                    {
                        var baseTypes =
                            declarationSyntax.BaseList.Types
                               .GroupBy(z => z.Type.GetSyntaxName())
                               .Select(z => z.First())
                               .ToArray();
                        if (baseTypes.Length > 0)
                        {
                            declarationSyntax = declarationSyntax.WithBaseList(BaseList(SeparatedList(baseTypes)));
                        }
                    }

                    if (declarationSyntax is { BaseList: { Types: { Count: 0 } } })
                    {
                        declarationSyntax = declarationSyntax.WithBaseList(null);
                    }

                    var root = declarationSyntax.ReparentDeclaration(_context, (TypeDeclarationSyntax)symbol.DeclaringSyntaxReferences[0].GetSyntax());

                    var cu = CompilationUnit(
                        List<ExternAliasDirectiveSyntax>(),
                        List(ImmutableArray<UsingDirectiveSyntax>.Empty),
                        List<AttributeListSyntax>(),
                        SingletonList<MemberDeclarationSyntax>(
                            _context.Compilation.GlobalNamespace.ToDisplayString() == symbol.ContainingNamespace.ToDisplayString()
                                ? root
                                : NamespaceDeclaration(ParseName(symbol.ContainingNamespace.ToDisplayString()))
                                   .WithMembers(SingletonList<MemberDeclarationSyntax>(root))
                        )
                    ).AddUsings(
                        namespaces
                           .GroupBy(z => z.Name.ToFullString().Trim())
                           .Select(z => z.First())
                           .ToArray()
                    );
                    // if (!SymbolEqualityComparer.Default.Equals(_context.Compilation.GlobalNamespace, symbol.ContainingNamespace))
                    // {
                    //     cu = cu.AddUsings(UsingDirective(ParseName(symbol.ContainingNamespace.ToString())));
                    // }

                    _context.AddSource($"Extension_{symbol.ToDisplayString()}.cs", cu.NormalizeWhitespace().GetText(Encoding.UTF8));
                }
            }

            Accept(symbol.GetMembers());

            static IEnumerable<INamedTypeSymbol> GetMixins(INamedTypeSymbol symbol) => symbol.AllInterfaces
               .Concat(
                    symbol.GetAttributes()
                       .Where(z => z.AttributeClass?.Name is "AutoImplementAttribute" or "MixinAttribute")
                       .Where(z => z.ConstructorArguments is { Length: 1 })
                       .Where(z => z.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol })
                       .Select(z => z.ConstructorArguments[0].Value!)
                       .OfType<INamedTypeSymbol>()
                );
        }
    }
}