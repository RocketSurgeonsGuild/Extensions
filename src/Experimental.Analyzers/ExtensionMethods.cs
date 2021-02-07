using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace Rocket.Surgery.Experimental.Analyzers
{
    public static class ExtensionMethods
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> AttributeNames = new();


        public static TypeDeclarationSyntax ReparentDeclaration(
            this TypeDeclarationSyntax classToNest,
            GeneratorExecutionContext context,
            TypeDeclarationSyntax source
        )
        {
            var parent = source.Parent;
            while (parent is TypeDeclarationSyntax parentSyntax)
            {
                classToNest = parentSyntax
                   .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
                   .WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>())
                   .WithConstraintClauses(SyntaxFactory.List<TypeParameterConstraintClauseSyntax>())
                   .WithBaseList(null)
                   .AddMembers(classToNest);

                if (!parentSyntax.Modifiers.Any(z => z.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MustBePartial, parentSyntax.Identifier.GetLocation(), parentSyntax.GetFullMetadataName()));
                }

                parent = parentSyntax?.Parent;
            }

            return classToNest;
        }


        public static string GetFullName(this TypeDeclarationSyntax? source)
        {
            if (source is null)
                return string.Empty;

            var sb = new StringBuilder(source.Identifier.Text);

            var parent = source.Parent;
            while (parent is { })
            {
                if (parent is TypeDeclarationSyntax tds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, tds.Identifier.Text);
                }
                else if (parent is NamespaceDeclarationSyntax nds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, nds.Name.ToString());
                    break;
                }

                parent = parent.Parent;
            }

            return sb.ToString();
        }


        public static string GetFullMetadataName(this TypeDeclarationSyntax? source)
        {
            if (source is null)
                return string.Empty;

            var sb = new StringBuilder(source.Identifier.Text);

            var parent = source.Parent;
            while (parent is { })
            {
                if (parent is TypeDeclarationSyntax tds)
                {
                    sb.Insert(0, '+');
                    sb.Insert(0, tds.Identifier.Text);
                }
                else if (parent is NamespaceDeclarationSyntax nds)
                {
                    sb.Insert(0, '.');
                    sb.Insert(0, nds.Name.ToString());
                    break;
                }

                parent = parent.Parent;
            }

            return sb.ToString();
        }

        public static string GetFullMetadataName(this ISymbol? s)
        {
            if (s == null || IsRootNamespace(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                s = s.ContainingSymbol;
            }

            return sb.ToString();

            static bool IsRootNamespace(ISymbol symbol) => symbol is INamespaceSymbol s && s.IsGlobalNamespace;
        }

        public static string? GetSyntaxName(this TypeSyntax typeSyntax)
        {
            return typeSyntax switch
            {
                SimpleNameSyntax sns     => sns.Identifier.Text,
                QualifiedNameSyntax qns  => qns.Right.Identifier.Text,
                NullableTypeSyntax nts   => nts.ElementType.GetSyntaxName() + "?",
                PredefinedTypeSyntax pts => pts.Keyword.Text,
                ArrayTypeSyntax ats      => ats.ElementType.GetSyntaxName() + "[]",
                TupleTypeSyntax tts      => "(" + tts.Elements.Select(z => $"{z.Type.GetSyntaxName()}{z.Identifier.Text}") + ")",
                _                        => null // there might be more but for now... throw new NotSupportedException(typeSyntax.GetType().FullName)
            };
        }

        private static HashSet<string> GetNames(string attributePrefixes)
        {
            if (!AttributeNames.TryGetValue(attributePrefixes, out var names))
            {
                names = new HashSet<string>(attributePrefixes.Split(',').SelectMany(z => new[] { z, z + "Attribute" }));
                AttributeNames.TryAdd(attributePrefixes, names);
            }

            return names;
        }

        public static bool ContainsAttribute(this AttributeListSyntax list, string attributePrefixes) // string is comma separated
        {
            if (list is { Attributes: { Count: 0 } })
                return false;
            var names = GetNames(attributePrefixes);

            foreach (var item in list.Attributes)
            {
                if (item.Name.GetSyntaxName() is { } n && names.Contains(n))
                    return true;
            }

            return false;
        }

        public static bool IsAttribute(this AttributeSyntax attribute, string attributePrefixes) // string is comma separated
        {
            var names = GetNames(attributePrefixes);
            if (attribute.Name.GetSyntaxName() is { } n && names.Contains(n))
                return true;
            return false;
        }

        public static bool ContainsAttribute(this in SyntaxList<AttributeListSyntax> list, string attributePrefixes) // string is comma separated
        {
            if (list is { Count: 0 })
                return false;
            var names = GetNames(attributePrefixes);

            foreach (var item in list)
            {
                foreach (var attribute in item.Attributes)
                {
                    if (attribute.Name.GetSyntaxName() is { } n && names.Contains(n))
                        return true;
                }
            }

            return false;
        }

        public static AttributeSyntax? GetAttribute(this in SyntaxList<AttributeListSyntax> list, string attributePrefixes) // string is comma separated
        {
            foreach (var item in list)
            {
                if (item.GetAttribute(attributePrefixes) is { } attr)
                    return attr;
            }

            return null;
        }

        public static AttributeSyntax? GetAttribute(this AttributeListSyntax list, string attributePrefixes) // string is comma separated
        {
            if (list is { Attributes: { Count: 0 } })
                return null;
            var names = GetNames(attributePrefixes);

            foreach (var item in list.Attributes)
            {
                if (item.Name.GetSyntaxName() is { } n && names.Contains(n))
                    return item;
            }

            return null;
        }
    }
}