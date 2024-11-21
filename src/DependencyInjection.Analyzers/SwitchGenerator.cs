using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Rocket.Surgery.DependencyInjection.Analyzers;

internal static class SwitchGenerator
{
    public static SwitchStatementSyntax GenerateSwitchStatement(List<(SourceLocation location, BlockSyntax block)> items)
    {
        var lineNumberIdentifier = IdentifierName("lineNumber");
        var switchStatement = SwitchStatement(lineNumberIdentifier);
        foreach (var lineGrouping in items.GroupBy(x => x.location.LineNumber))
        {
            // disallow list?
            var location = lineGrouping.First().location;
            var lineSwitchSection = createNestedSwitchSections(
                    lineGrouping.ToArray(),
                    InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ParseName("System.IO.Path"), IdentifierName("GetFileName"))
                        )
                       .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("filePath"))))),
                    x => x.location.FileName,
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
        IReadOnlyList<(SourceLocation location, BlockSyntax block)> blocks,
        ExpressionSyntax identifier,
        Func<(SourceLocation location, BlockSyntax block), T> regroup,
        Func<IGrouping<T, (SourceLocation location, BlockSyntax block)>, SwitchSectionSyntax> next,
        Func<T, LiteralExpressionSyntax> literalFactory
    )
    {
        if (blocks is [var localBlock,])
            return SwitchSection()
                  .AddStatements(localBlock.block.Statements.ToArray())
                  .AddStatements(BreakStatement());

        var section = SwitchStatement(identifier);
        foreach (var item in blocks.GroupBy(regroup))
        {
            var location = item.First().location;
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

    private static SwitchSectionSyntax generateFilePathSwitchStatement(IGrouping<string, (SourceLocation location, BlockSyntax block)> innerGroup)
    {
        return createNestedSwitchSections(
            innerGroup.GroupBy(z => z.location.ExpressionHash).Select(z => z.First()).ToArray(),
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("ICompiledTypeProvider"),
                        IdentifierName("GetArgumentExpressionHash")
                    )
                )
               .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("argumentExpression"))))),
            x => x.location.ExpressionHash,
            generateExpressionHashSwitchStatement,
            value =>
                LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(value)
                )
        );
    }

    private static SwitchSectionSyntax generateExpressionHashSwitchStatement(IGrouping<string, (SourceLocation location, BlockSyntax block)> innerGroup)
    {
        return SwitchSection()
              .AddStatements(innerGroup.FirstOrDefault().block?.Statements.ToArray() ?? Array.Empty<StatementSyntax>())
              .AddStatements(BreakStatement());
    }
}
