# Contributing to HttpClient.Resilience

Thank you for your interest in contributing to HttpClient.Resilience! This guide will help you get started.

## Getting Started

### Prerequisites

- Git
- .NET 8.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Setting Up the Development Environment

1. Fork the repository on GitHub
2. Clone your fork locally:

   ```bash
   git clone https://github.com/YOUR_USERNAME/HttpClient.Resilience.git
   cd HttpClient.Resilience
   ```

3. Restore dependencies and build:

   ```bash
   dotnet restore
   dotnet build
   ```

4. Run tests to ensure everything works:

   ```bash
   dotnet test
   ```

## Making Changes

### Branch Strategy

- Create a new branch for each feature or bug fix
- Use descriptive branch names:
  - `feature/add-timeout-configuration`
  - `fix/circuit-breaker-race-condition`
  - `docs/update-configuration-examples`

### Code Style

We follow standard .NET coding conventions:

- Use PascalCase for public members
- Use camelCase for private fields and local variables
- Add XML documentation for public APIs
- Keep methods focused and small
- Write descriptive variable and method names

### Writing Tests

- All new features must include tests
- Aim for high test coverage
- Use descriptive test names that explain the scenario
- Follow the Arrange-Act-Assert pattern
- Use FluentAssertions for assertions

Example:

```csharp
[Fact]
public void AddResilience_WithNullOptions_ShouldThrowArgumentNullException()
{
    // Arrange
    var services = new ServiceCollection();
    var builder = services.AddHttpClient("test");

    // Act & Assert
    var exception = Assert.Throws<ArgumentNullException>(() =>
        builder.AddResilience((HttpClientOptions)null));

    exception.ParamName.Should().Be("options");
}
```

### Documentation

- Update documentation for any API changes
- Add examples for new features
- Update the changelog for breaking changes
- Ensure all public APIs have XML documentation

## Pull Request Process

### Before Submitting

1. Ensure all tests pass:

   ```bash
   dotnet test
   ```

2. Check for code style issues:

   ```bash
   dotnet format --verify-no-changes
   ```

3. Update documentation if needed
4. Add or update tests for your changes

### Pull Request Guidelines

1. **Title**: Use a clear, descriptive title
   - ✅ "Add support for custom timeout configuration"
   - ❌ "Fix stuff"

2. **Description**: Include:
   - What changes were made and why
   - Any breaking changes
   - Links to related issues
   - Screenshots/examples if applicable

3. **Scope**: Keep PRs focused on a single feature or fix
4. **Tests**: Include tests for new functionality
5. **Documentation**: Update docs for user-facing changes

### Pull Request Template

```markdown
## Description
Brief description of the changes.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes

## Checklist
- [ ] My code follows the style guidelines of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
```

## Development Guidelines

### API Design Principles

1. **Simplicity First**: Default configuration should work for 80% of use cases
2. **Explicit Configuration**: Advanced options should be clearly documented
3. **Fail Fast**: Invalid configurations should throw exceptions early
4. **Backward Compatibility**: Avoid breaking changes when possible
5. **Consistent Patterns**: Follow existing conventions in the codebase

### Performance Considerations

- Minimize allocations in hot paths
- Use `ValueTask` where appropriate
- Consider thread safety for shared resources
- Profile performance for critical paths

### Error Handling

- Use specific exception types for different error scenarios
- Provide clear, actionable error messages
- Include context information in exceptions
- Handle edge cases gracefully

## Types of Contributions

### Bug Reports

When reporting bugs, please include:

- Clear description of the issue
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, etc.)
- Minimal code sample that demonstrates the issue

### Feature Requests

For new features:

- Explain the use case and problem you're solving
- Provide examples of how the feature would be used
- Consider backward compatibility implications
- Discuss alternative approaches

### Documentation Improvements

- Fix typos and grammar
- Add missing examples
- Improve clarity of explanations
- Update outdated information

### Code Contributions

- Bug fixes
- New features
- Performance improvements
- Code refactoring

## Release Process

We follow semantic versioning (SemVer):

- **Major** (1.0.0): Breaking changes
- **Minor** (0.1.0): New features, backward compatible
- **Patch** (0.0.1): Bug fixes, backward compatible

## Community Guidelines

### Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them succeed
- Focus on constructive feedback
- Assume good intentions

### Getting Help

- Check existing issues and documentation first
- Ask questions in GitHub Discussions
- Join our community discussions
- Provide context when asking for help

## Recognition

Contributors are recognized in:

- Release notes for significant contributions
- GitHub contributors page
- Special thanks for major features or fixes

## Legal

By contributing to HttpClient.Resilience, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to HttpClient.Resilience! Your help makes this project better for everyone. 🎉
