using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class SwitchGenerator
{
    public static SwitchStatementSyntax GenerateSwitchStatement(IReadOnlyList<ResolvedSourceLocation> items)
    {
        var lineNumberIdentifier = IdentifierName("lineNumber");
        var switchStatement = SwitchStatement(lineNumberIdentifier);
        foreach (var lineGrouping in items.GroupBy(x => x.Location.LineNumber))
        {
            // disallow list?
            var location = lineGrouping.First().Location;
            var lineSwitchSection = createNestedSwitchSections(
                    lineGrouping.ToArray(),
                    InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ParseName("System.IO.Path"), IdentifierName("GetFileName"))
                        )
                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("filePath"))))),
                    x => x.Location.FileName,
                    generateFilePathSwitchStatement,
                    value => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))
                )
               .AddLabels(
                    CaseSwitchLabel(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(lineGrouping.Key)))
                       .WithKeyword(
                            Token(
                                TriviaList(Comment($"// FilePath: {location.FilePath.Replace("\\", "/")} Expression: {location.ExpressionHash}")),
                                SyntaxKind.CaseKeyword,
                                TriviaList()
                            )
                        )
                );

            switchStatement = switchStatement.AddSections(lineSwitchSection);
        }

        return switchStatement;
    }

    private static SwitchSectionSyntax createNestedSwitchSections<T>(
        IReadOnlyList<ResolvedSourceLocation> blocks,
        ExpressionSyntax identifier,
        Func<ResolvedSourceLocation, T> regroup,
        Func<IGrouping<T, ResolvedSourceLocation>, SwitchSectionSyntax> next,
        Func<T, LiteralExpressionSyntax> literalFactory
    )
    {
        if (blocks is [var localBlock])
        {
            return SwitchSection()
                  .AddStatements([.. ParseStatements(localBlock.Expression)])
                  .AddStatements(BreakStatement());
        }

        var section = SwitchStatement(identifier);
        foreach (var item in blocks.GroupBy(regroup))
        {
            var location = item.First().Location;
            var newSection = next(item)
               .AddLabels(
                    CaseSwitchLabel(literalFactory(item.Key))
                       .WithKeyword(
                            Token(
                                TriviaList(Comment($"// FilePath: {location.FilePath.Replace("\\", "/")} Expression: {location.ExpressionHash}")),
                                SyntaxKind.CaseKeyword,
                                TriviaList()
                            )
                        )
                );
            section = section.AddSections(
                newSection
            );
        }

        return SwitchSection().AddStatements(section, BreakStatement());
    }

    private static SwitchSectionSyntax generateFilePathSwitchStatement(IGrouping<string, ResolvedSourceLocation> innerGroup) => createNestedSwitchSections(
        innerGroup.GroupBy(z => z.Location.ExpressionHash).Select(z => z.First()).ToArray(),
        InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("ICompiledTypeProvider"),
                    IdentifierName("GetArgumentExpressionHash")
                )
            )
           .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("argumentExpression"))))),
        x => x.Location.ExpressionHash,
        generateExpressionHashSwitchStatement,
        value =>
            LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(value)
            )
    );

    private static SwitchSectionSyntax generateExpressionHashSwitchStatement(IGrouping<string, ResolvedSourceLocation> innerGroup) => SwitchSection()
       .AddStatements([.. ParseStatements(innerGroup.FirstOrDefault()?.Expression ?? "")])
       .AddStatements(BreakStatement());

    private static IEnumerable<StatementSyntax> ParseStatements(string expression)
    {
        var tree = ParseSyntaxTree(expression, new CSharpParseOptions(kind: SourceCodeKind.Script));
        return tree
              .GetRoot()
              .DescendantNodes()
              .OfType<BlockSyntax>()
              .SelectMany(z => z.Statements);
    }
}
