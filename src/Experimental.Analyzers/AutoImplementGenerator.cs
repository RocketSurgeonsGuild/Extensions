using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Experimental.Analyzers
{
    [Generator]
    public class AutoImplementGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        static SourceText attributeSourceText = SourceText.From(
            @"using System;
using System.Diagnostics;

namespace Rocket.Surgery
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    [Conditional(""CodeGeneration"")]
    class AutoImplementAttribute : Attribute {
        public AutoImplementAttribute() { }
        public AutoImplementAttribute(Type type) { }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false)]
    [Conditional(""CodeGeneration"")]
    class MixinAttribute : Attribute {
        public MixinAttribute() { }
        public MixinAttribute(Type type) { }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    class AutoImplementSourceAttribute : Attribute
    {
        public AutoImplementSourceAttribute(string source)
        {
            
        }
    }
}",
            Encoding.UTF8
        );

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("Attribute.cs", attributeSourceText);
            if (!( context.SyntaxReceiver is SyntaxReceiver syntaxReceiver ))
            {
                return;
            }

            var compilation = ( context.Compilation as CSharpCompilation )!;
            // Debugger.Launch();
            var parseOptions = compilation.SyntaxTrees.Select(z => z.Options).OfType<CSharpParseOptions>().First();
            var content = new StringBuilder();
            var compilationWithMethod = compilation
               .AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(
                        attributeSourceText,
                        parseOptions,
                        path: "Attribute.cs"
                    )
                );
            try
            {
                foreach (var @interface in syntaxReceiver.Interfaces)
                {
                    var model = compilationWithMethod.GetSemanticModel(@interface.SyntaxTree);
                    var symbol = model.GetDeclaredSymbol(@interface);
                    if (symbol is null)
                        continue;

                    var writer = new QualifiedCSharpSyntaxRewriter(compilationWithMethod, compilationWithMethod.GetSemanticModel(@interface.SyntaxTree));
                    var rewrittenInterface = (TypeDeclarationSyntax)writer.Visit(@interface);

                    var cu = CompilationUnit(
                        List<ExternAliasDirectiveSyntax>(),
                        List(ImmutableArray<UsingDirectiveSyntax>.Empty),
                        List<AttributeListSyntax>(),
                        SingletonList<MemberDeclarationSyntax>(
                            SymbolEqualityComparer.Default.Equals(context.Compilation.GlobalNamespace, symbol.ContainingNamespace)
                                ? rewrittenInterface.ReparentDeclaration(context, @interface)
                                : NamespaceDeclaration(ParseName(symbol.ContainingNamespace.ToDisplayString()))
                                   .WithMembers(SingletonList<MemberDeclarationSyntax>(rewrittenInterface.ReparentDeclaration(context, @interface)))
                        )
                    ).AddUsings(@interface.SyntaxTree.GetCompilationUnitRoot().Usings.ToArray());
                    if (!SymbolEqualityComparer.Default.Equals(context.Compilation.GlobalNamespace, symbol.ContainingNamespace))
                    {
                        cu = cu.AddUsings(UsingDirective(ParseName(symbol.ContainingNamespace.ToString())));
                    }

                    var attr = AttributeList(
                        AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)),
                        SingletonSeparatedList(
                            Attribute(
                                ParseName("Rocket.Surgery.AutoImplementSourceAttribute"),
                                AttributeArgumentList(
                                    SingletonSeparatedList(
                                        AttributeArgument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(cu.NormalizeWhitespace().ToFullString())
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    );
                    content.AppendLine(attr.ToFullString());
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.Error, null, e.ToString(), e.InnerException?.ToString()));
            }

            context.AddSource("AssemblyData.cs", content.ToString());
            compilationWithMethod = compilationWithMethod
               .AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(
                        content.ToString(),
                        parseOptions,
                        path: "AssemblyData.cs"
                    )
                );

            // var diagnostics = compilationWithMethod.GetDiagnostics();
            var attributeData = AttributeDataVisitor.GetAttributes(compilationWithMethod);
            TypeSymbolVisitor.GetTypes(
                context,
                compilationWithMethod,
                attributeData
            );
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<TypeDeclarationSyntax> Interfaces { get; } = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is TypeDeclarationSyntax { AttributeLists: { Count: > 0 } } type
                 && type.AttributeLists.GetAttribute("AutoImplement,Mixin") is { ArgumentList: null or { Arguments: { Count: 0 } } } attr)
                {
                    Interfaces.Add(type);
                }
            }
        }
    }
}