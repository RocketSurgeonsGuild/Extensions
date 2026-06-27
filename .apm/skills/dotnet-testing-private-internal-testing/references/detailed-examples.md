#### Level 3: Value Output Assessment

### Question: Does the test value exceed the cost?

Assess test value:

- ✅ Can catch real business logic errors
- ✅ Provides clear failure messages
- ✅ Runs stably long-term at reasonable cost

### Recommended Action: (continued)

- If value is insufficient, look for alternative testing strategies
- Consider performance testing, integration testing, or other approaches

### Decision Matrix

| Scenario                    | Recommended Approach     | Reason                 |
| :-------------------------- | :----------------------- | :--------------------- |
| Simple private methods (< 10 lines) | Test through public methods | Low maintenance cost |
| Complex private logic (> 10 lines) | Refactor to independent class | Improve design and testability |
| Framework internal algorithms | Use InternalsVisibleTo | Need precise internal behavior testing |
| Legacy system private methods | Consider reflection testing | Difficult to refactor short-term |
| Security-related private logic | Refactor or use reflection testing | Need independent correctness verification |
| Frequently changing implementation details | Avoid direct testing | Tests become fragile |

## DO - Recommended Practices

1. **Design First**
   - ✅ Prioritize refactoring over testing private methods
   - ✅ Use dependency injection and interface abstraction
   - ✅ Apply strategy pattern to separate complex logic
   - ✅ Maintain single responsibility principle

2. **Test Public Behavior**
   - ✅ Focus on testing public API behavior
   - ✅ Test private logic indirectly through public methods
   - ✅ Use integration testing to cover complex flows

3. **Use InternalsVisibleTo Wisely**
   - ✅ Only for framework or class library development
   - ✅ Use Meziantou.MSBuild.InternalsVisibleTo to simplify configuration
   - ✅ Document why internal visibility is needed

4. **Use Reflection Cautiously**
   - ✅ Create helper methods to encapsulate reflection logic
   - ✅ Mark in test names that reflection is used
   - ✅ Regularly review whether refactoring is possible

## DON'T - Practices to Avoid

1. **Don't Over-Test Private Methods**
   - ❌ Avoid writing tests for every private method
   - ❌ Don't test simple getters/setters
   - ❌ Avoid testing pure delegation calls

2. **Don't Ignore Design Problems**
   - ❌ Don't use testing as an alternative to design problems
   - ❌ Don't break encapsulation for testing
   - ❌ Don't let tests hinder refactoring

3. **Don't Depend on Implementation Details**
   - ❌ Avoid testing call order of private methods
   - ❌ Don't validate values of private fields
   - ❌ Avoid testing frequently changing implementation details

4. **Don't Abuse InternalsVisibleTo**
   - ❌ Don't open internal for application layer code
   - ❌ Avoid excessive test project visibility
   - ❌ Don't use it to replace proper public API

## Example Reference

See `templates/` directory for complete examples:

- `internals-visible-to-examples.cs` - InternalsVisibleTo configuration examples
- `reflection-testing-examples.cs` - Reflection testing technique examples
- `strategy-pattern-refactoring.cs` - Strategy pattern refactoring examples

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 09 - Testing Private and Internal Members: Private and Internal Testing Strategies**
  - Article: https://ithelp.ithome.com.tw/articles/10374866
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day09

### Official Documentation

- [Meziantou's Blog - InternalsVisibleTo](https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm)

### Related Skills

- `unit-test-fundamentals` - Unit testing basics
- `nsubstitute-mocking` - Test doubles and mocking

## Testing Checklist

When handling private and internal member testing, confirm the following checklist items:

- [ ] Evaluated whether to refactor rather than test private methods
- [ ] Internal members really need to be open to test projects
- [ ] Using appropriate InternalsVisibleTo configuration method
- [ ] Reflection tests use helper methods for encapsulation
- [ ] Test names clearly indicate test type (e.g., using reflection)
- [ ] Strategy pattern and other design patterns considered for complex logic
- [ ] Tests won't become a hindrance to refactoring
- [ ] Test value exceeds maintenance cost
- [ ] Not overly dependent on implementation details
- [ ] Regularly review appropriateness of testing strategy
````
