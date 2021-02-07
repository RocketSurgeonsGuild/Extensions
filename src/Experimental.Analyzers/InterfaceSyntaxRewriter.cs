using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.Experimental.Analyzers
{
    class InterfaceSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly ImmutableArray<TypeDeclarationSyntax> _relatedSyntax;

        public InterfaceSyntaxRewriter(
            ImmutableArray<TypeDeclarationSyntax> relatedSyntax
        )
        {
            _relatedSyntax = relatedSyntax;
        }

        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            if (node.IsAttribute("AutoImplement,Mixin"))
                return node;
            return null;
        }


        public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            foreach (var member in _relatedSyntax
               .SelectMany(z => z.Members)
               .OfType<PropertyDeclarationSyntax>())
            {
                if (member.Identifier.IsEquivalentTo(node.Identifier))
                    return null;
            }

            node = node.WithModifiers(TokenList(node.Modifiers.Except(node.Modifiers.Where(z => z.IsKind(SyntaxKind.AbstractKeyword)))));

            return node.Modifiers.Any(z => z.IsKind(SyntaxKind.PublicKeyword) || z.IsKind(SyntaxKind.ProtectedKeyword) || z.IsKind(SyntaxKind.PrivateKeyword))
                ? node : node.AddModifiers(Token(SyntaxKind.PublicKeyword));
        }

        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach (var member in _relatedSyntax
               .SelectMany(z => z.Members)
               .OfType<FieldDeclarationSyntax>())
            {
                if (member.Declaration.IsEquivalentTo(node.Declaration))
                    return null;
            }

            return node;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            foreach (var member in _relatedSyntax
               .SelectMany(z => z.Members)
               .OfType<MethodDeclarationSyntax>())
            {
                if (member.Identifier.IsEquivalentTo(node.Identifier))
                    return null;
            }

            node = node.WithModifiers(TokenList(node.Modifiers.Except(node.Modifiers.Where(z => z.IsKind(SyntaxKind.AbstractKeyword)))));

            return node;
        }
    }
}