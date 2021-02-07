using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
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
            foreach (var @interface in syntaxReceiver.Interfaces)
            {
                var model = compilationWithMethod.GetSemanticModel(@interface.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(@interface);
                if (symbol is null)
                    continue;

                var writer = new QualifiedCSharpSyntaxRewriter(compilationWithMethod, compilationWithMethod.GetSemanticModel(@interface.SyntaxTree));
                var rewrittenInterface = (TypeDeclarationSyntax)writer.Visit(@interface);

                var cu = SyntaxFactory.CompilationUnit(
                    SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
                    SyntaxFactory.List(ImmutableArray<UsingDirectiveSyntax>.Empty),
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(symbol.ContainingNamespace.ToDisplayString()))
                           .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(rewrittenInterface.ReparentDeclaration(context, @interface)))
                    )
                ).AddUsings(@interface.SyntaxTree.GetCompilationUnitRoot().Usings.ToArray());

                var attr = SyntaxFactory.AttributeList(
                    SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName("Rocket.Surgery.AutoImplementSourceAttribute"),
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(cu.NormalizeWhitespace().ToFullString())
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
                content.AppendLine(attr.ToFullString());
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