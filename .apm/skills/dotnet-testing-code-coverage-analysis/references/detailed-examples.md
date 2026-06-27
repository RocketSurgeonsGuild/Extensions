
4. **Team Collaboration**
   - Establish testing standards and processes
   - Code Review includes test checks

## CI/CD Integration

Coverage analysis can be integrated into CI/CD Pipeline, use `dotnet test --collect:"XPlat Code Coverage"` in GitHub Actions with `reportgenerator` to generate reports; in Azure DevOps use `DotNetCoreCLI@2` task with `PublishCodeCoverageResults@1`.

> For complete YAML configuration examples, see [references/cicd-integration.md](references/cicd-integration.md)

## FAQ and Solutions

### Q1: Coverage shows 0%?

### Checklist

1. Confirm `coverlet.collector` package is installed
2. Check if runsettings configuration is correct
3. Confirm tests actually executed
4. Check if exclusion settings are too broad

### Q2: Can't see coverage in Visual Studio?

### Solution

- Community/Professional versions: Install Fine Code Coverage extension
- Enterprise version: Use built-in feature
- Confirm coverage collection is enabled

### Q3: VS Code can't display coverage?

### Solution: (continued)

1. Confirm C# Dev Kit is installed
2. Re-run "Run Coverage Test"
3. Check if lcov file is generated
4. Try reloading window

### Q4: How to improve coverage?

### Strategy

1. Identify uncovered key code (red areas)
2. Supplement boundary condition tests
3. Test all conditional branches
4. Add exception scenario tests
5. Consider refactoring overly complex methods

## Template Files

See template files in same directory:

- `templates/runsettings-template.xml` - Coverage configuration template
- `templates/coverage-workflow.md` - Complete workflow instructions

## Checklist

When configuring code coverage, confirm the following items:

- [ ] `coverlet.collector` package installed
- [ ] Can execute `dotnet test --collect:"XPlat Code Coverage"`
- [ ] Tool can properly display coverage results
- [ ] Understand true meaning of coverage numbers (not KPI)
- [ ] Excluded unnecessary code (e.g., auto-generated code)
- [ ] Team understands coverage is auxiliary tool not target
- [ ] Focus on test quality rather than coverage numbers

## Related Skills

- `unit-test-fundamentals` - Unit testing basics and FIRST principles
- `xunit-project-setup` - xUnit test project setup
- `test-naming-conventions` - Test naming conventions

## Core Philosophy

> **Code coverage is a means, not an end.**
>
> Focus not on how high the number is, but on:
>
> - Whether critical business logic is tested
> - Whether tests truly validate expected behavior
> - Whether it provides confidence during refactoring

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 06 - Code Coverage Code Coverage Practical Guide**
  - Article: https://ithelp.ithome.com.tw/articles/10374467
  - Sample Code: None (this chapter is conceptual explanation)

### Official Documentation

- [.NET Unit Testing Best Practices](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices)
- [Unit Testing with Code Coverage](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage)
- [dotnet-coverage Tool](https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage)
- [VS Code Testing](https://code.visualstudio.com/docs/editor/testing)

### Tools

- [Fine Code Coverage](https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage2022)
- [Coverlet](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)

### Related Skills

- `unit-test-fundamentals` - Unit testing basics and FIRST principles
- `xunit-project-setup` - xUnit project setup
````
