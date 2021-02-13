using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
                if (member.Identifier.ToFullString() == node.Identifier.ToFullString())
                    return null;
            }

            // this probably controversial...
            if (node.ExplicitInterfaceSpecifier is { })
                node = node.WithExplicitInterfaceSpecifier(null);

            node = node.WithModifiers(TokenList(node.Modifiers.Except(node.Modifiers.Where(z => z.IsKind(SyntaxKind.AbstractKeyword)))));

            return node.Modifiers.Any(z => z.IsKind(SyntaxKind.PublicKeyword) || z.IsKind(SyntaxKind.ProtectedKeyword) || z.IsKind(SyntaxKind.PrivateKeyword))
                ? node
                : node.AddModifiers(Token(SyntaxKind.PublicKeyword));
        }

        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            foreach (var member in _relatedSyntax
               .SelectMany(z => z.Members)
               .OfType<FieldDeclarationSyntax>())
            {
                if (member.Declaration.ToFullString() == node.Declaration.ToFullString())
                    return null;
            }

            return node;
        }

        public override SyntaxNode? VisitBaseList(BaseListSyntax node)
        {
            foreach (var baseType in _relatedSyntax.SelectMany(z => ( z.BaseList ?? BaseList() ).Types))
            {
                node = node.WithTypes(SeparatedList(node.Types.Where(t => t.Type.GetSyntaxName() != baseType.Type.GetSyntaxName())));
            }

            if (node.Types.Count == 0)
                return null;
            return node;
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            foreach (var member in _relatedSyntax
               .SelectMany(z => z.Members)
               .OfType<MethodDeclarationSyntax>())
            {
                if (member.Identifier.ToFullString() == node.Identifier.ToFullString())
                    return null;
            }

            // this probably controversial...
            if (node.ExplicitInterfaceSpecifier is { })
                node = node.WithExplicitInterfaceSpecifier(null);

            node = node.WithModifiers(TokenList(node.Modifiers.Except(node.Modifiers.Where(z => z.IsKind(SyntaxKind.AbstractKeyword)))));

            return node;
        }
    }
}