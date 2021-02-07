using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rocket.Surgery.Experimental.Analyzers
{
    class QualifiedCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly CSharpCompilation _compilation;
        private readonly SemanticModel _semanticModel;

        public QualifiedCSharpSyntaxRewriter(
            CSharpCompilation compilation,
            SemanticModel semanticModel
        )
        {
            _compilation = compilation;
            _semanticModel = semanticModel;

        }

        public override SyntaxNode? VisitAttribute(AttributeSyntax node) => node.IsAttribute("AutoImplement") || node.IsAttribute("Mixin") ? null : base.VisitAttribute(node);

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
        {
            if (node is { Attributes: { Count: 0 } })
            {
                return null;
            }

            if (base.VisitAttributeList(node) is AttributeListSyntax als && als is { Attributes: { Count: 0 } })
                return null;

            return base.VisitAttributeList(node);
        }

        public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var type = _semanticModel.GetDeclaredSymbol(node);
            return base.VisitPropertyDeclaration(
                type is null
                    ? node
                    : node.WithType(
                        SyntaxFactory.ParseTypeName(type.Type.ToDisplayString())
                    )
            );
        }

        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = _semanticModel.GetDeclaredSymbol(node);

            return base.VisitFieldDeclaration(
                type is null
                    ? node
                    : node.WithDeclaration(
                        node.Declaration.WithType(SyntaxFactory.ParseTypeName(type.ToDisplayString()))
                    )
            );
        }

        public override SyntaxNode? VisitSimpleBaseType(SimpleBaseTypeSyntax node)
        {
            var type = _semanticModel.GetDeclaredSymbol(node);

            return base.VisitSimpleBaseType(
                type is null
                    ? node
                    : node.WithType(SyntaxFactory.ParseTypeName(type.ToDisplayString()))
            );
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var type = _semanticModel.GetDeclaredSymbol(node.ReturnType);
            return base.VisitMethodDeclaration(type is null ? node : node.WithReturnType(SyntaxFactory.ParseTypeName(type.ToDisplayString())));
        }

        public override SyntaxNode? VisitParameter(ParameterSyntax node)
        {
            var type = _semanticModel.GetDeclaredSymbol(node);
            return base.VisitParameter(
                type is null
                    ? node
                    : node.WithType(
                        SyntaxFactory.ParseTypeName(type.Type.ToDisplayString())
                    )
            );
        }
    }
}