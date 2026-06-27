### Issue 1: xunit.abstractions not found

**Error**: `The type or namespace name 'Abstractions' does not exist`

**Solution**: Remove `using Xunit.Abstractions;`, related types have moved to `Xunit` namespace.

### Issue 2: Custom DataAttribute not working

In xUnit 3.x `DataAttribute` method signature has changed: `GetData(MethodInfo)` -> `GetDataAsync(MethodInfo, DisposalTracker)`, return type changed to `Task<IReadOnlyCollection<ITheoryDataRow>>`.

### Issue 3: IDE cannot discover tests

Confirm IDE version meets requirements:

- Visual Studio 2022 17.8+
- Rider 2023.3+
- VS Code (latest)

If still issues, temporarily disable Microsoft Testing Platform:

```xml
<PropertyGroup>
  <EnableMicrosoftTestingPlatform>false</EnableMicrosoftTestingPlatform>
</PropertyGroup>
```text

---

## Upgrade Checklist

### Before Upgrade

- [ ] Confirm target framework version (.NET 8+ or .NET Framework 4.7.2+)
- [ ] Check project file format (SDK-style)
- [ ] Identify all async void test methods
- [ ] Check IAsyncLifetime implementations
- [ ] Evaluate dependency package compatibility
- [ ] Create backup branch

### During Upgrade

- [ ] Update package references (use `xunit.v3`)
- [ ] Remove `xunit.abstractions` references
- [ ] Modify OutputType to Exe
- [ ] Fix all async void test methods
- [ ] Update using statements
- [ ] Refactor custom attributes (if any)
- [ ] Verify compilation success
- [ ] Run all tests

### Post-Upgrade Verification

- [ ] Functional completeness testing
- [ ] Performance benchmark comparison
- [ ] CI/CD Pipeline validation
- [ ] Documentation update
- [ ] Team training

---

## IDE and Tool Support

### IDE Version Requirements

| IDE | Minimum Version |
| --- | --------------- |
| Visual Studio | 2022 17.8+ |
| VS Code | Latest |
| Rider | 2023.3+ |

### Microsoft Testing Platform

xUnit 3.x enables Microsoft Testing Platform by default:

```xml
<PropertyGroup>
  <EnableMicrosoftTestingPlatform>true</EnableMicrosoftTestingPlatform>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```text

---

## Performance Improvements

Performance improvements brought by xUnit 3.x:

1. **Independent process execution**: Tests run in separate processes, better isolation
2. **Improved parallel algorithm**: Smarter load balancing
3. **Faster startup time**: Executable runs directly
4. **Better memory isolation**: Reduced interference between tests

---

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 26 - xUnit Upgrade Guide: Transition from 2.9.x to 3.x**
  - Article: https://ithelp.ithome.com.tw/articles/10377477
  - Sample code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day26

### Official Documentation

- [xUnit.net Official Website](https://xunit.net/)
- [xUnit v3 New Features Documentation](https://xunit.net/docs/getting-started/v3/whats-new)
- [xUnit 2.x -> 3.x Official Migration Guide](https://xunit.net/docs/getting-started/v3/migration)
- [xunit.v3 NuGet Package](https://www.nuget.org/packages/xunit.v3)
````
