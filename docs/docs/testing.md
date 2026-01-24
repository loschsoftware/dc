# Unit Testing

The Dassie Compiler includes a built-in unit testing framework that allows you to write and run tests for your Dassie projects.

## Overview

The testing framework is provided through the `Dassie.Tests` namespace in the `Dassie.Core` library. Tests are defined using attributes and can be run using the `dc test` command.

## Writing Tests

### Test Module

A **test module** is a module decorated with the `<TestModule>` attribute. It serves as a container for related unit tests.

```dassie
import Dassie.Tests

<TestModule>
module MathTests = {
    # Tests go here
}
```

### Test Methods

A **test** is a method within a test module decorated with the `<Test>` attribute. The test passes if the method completes without throwing an exception.

```dassie
import Dassie.Tests
import Dassie.Core

<TestModule>
module MathTests = {
    <Test>
    Addition_TwoPlusTwo_ReturnsFour (): null = {
        result = 2 + 2
        assert (result == 4)
    }
    
    <Test>
    Division_TenByTwo_ReturnsFive (): null = {
        result = 10 / 2
        assertEqual result, 5
    }
}
```

## Assertion Functions

The `Dassie.Core` module provides assertion functions for verifying test conditions:

### assert

Checks a condition and throws an exception if the condition is false:

```dassie
import Dassie.Core

assert (x > 0)                          # Basic assertion
assert (x > 0), "x must be positive"    # With custom message
```

### assertEqual

Checks whether two values are equal:

```dassie
import Dassie.Core

assertEqual actual, expected
```

If the values are not equal, the assertion displays both the expected and actual values in the error message.

## Running Tests

### Run All Tests

To compile and run all tests in the current project:

```bash
dc test
```

### Run Tests from Specific Module

To run tests from a specific test module:

```bash
dc test -m=MyNamespace.MathTests
```

You can specify multiple modules by using the option multiple times:

```bash
dc test -m=MyNamespace.MathTests -m=MyNamespace.StringTests
```

### Run Tests from Specific Assembly

To run tests from a pre-compiled assembly without recompiling:

```bash
dc test -a=./bin/MyProject.dll
```

### Show Only Failed Tests

To filter the output to show only failed tests:

```bash
dc test --failed
```

## Test Output

When you run tests, the compiler displays results in a structured format:

```
MyProject
??? MyNamespace.MathTests
    ??? ? Addition_TwoPlusTwo_ReturnsFour
    ??? ? Division_TenByTwo_ReturnsFive
    ??? ? Division_ByZero_ThrowsException
        Error: Expected exception was not thrown

Results: 2 passed, 1 failed, 3 total
```

## Best Practices

### Naming Conventions

Use descriptive test names that follow the pattern: `MethodName_Scenario_ExpectedBehavior`

```dassie
<Test>
CalculateTotal_EmptyCart_ReturnsZero (): null = {
    cart = Cart
    assertEqual (cart.CalculateTotal), 0
}

<Test>
CalculateTotal_SingleItem_ReturnsItemPrice (): null = {
    cart = Cart
    cart.AddItem (Item "Apple", 1.50)
    assertEqual (cart.CalculateTotal), 1.50
}
```

### One Assertion Per Test

Keep tests focused by testing one thing at a time:

```dassie
# Good - one assertion per test
<Test>
Add_ReturnsCorrectSum (): null = {
    assertEqual (Calculator.Add 2, 3), 5
}

<Test>
Add_HandlesNegativeNumbers (): null = {
    assertEqual (Calculator.Add (-2), 3), 1
}

# Avoid - multiple unrelated assertions
<Test>
Add_Works (): null = {
    assertEqual (Calculator.Add 2, 3), 5
    assertEqual (Calculator.Add (-2), 3), 1
    assertEqual (Calculator.Add 0, 0), 0
}
```

### Arrange-Act-Assert Pattern

Structure your tests using the AAA pattern:

```dassie
<Test>
Withdraw_SufficientBalance_UpdatesBalance (): null = {
    # Arrange
    account = BankAccount 100.0
    
    # Act
    account.Withdraw 30.0
    
    # Assert
    assertEqual (account.Balance), 70.0
}
```

### Test Independence

Tests should not depend on each other. Each test should set up its own state:

```dassie
<TestModule>
module AccountTests = {
    <Test>
    Deposit_IncreasesBalance (): null = {
        account = BankAccount 0.0
        account.Deposit 50.0
        assertEqual (account.Balance), 50.0
    }
    
    <Test>
    Withdraw_DecreasesBalance (): null = {
        account = BankAccount 100.0  # Fresh account for this test
        account.Withdraw 30.0
        assertEqual (account.Balance), 70.0
    }
}
```

## Project Configuration

### Enable Automatic Test Running

To automatically run tests as part of your build process, you can configure a build profile:

```xml
<BuildProfiles>
  <BuildProfile Name="Test">
    <Arguments>build</Arguments>
    <PostBuildEvents>
      <BuildEvent>
        <Command>dc test</Command>
        <Critical>true</Critical>
      </BuildEvent>
    </PostBuildEvents>
  </BuildProfile>
</BuildProfiles>
```

Then run with:

```bash
dc build Test
```

## Limitations

> [!NOTE]
> The following features are not yet fully supported:
> - Test fixtures (setup/teardown methods)
> - Parameterized tests
> - Test categories/filtering
> - Running tests on project groups

## See Also

- [Command-Line Reference](./cli.md) - Full reference for the `dc test` command
- [Project Files](./projects.md) - Configuring build profiles
- [Getting Started](./getting-started.md) - Introduction to the Dassie Compiler
