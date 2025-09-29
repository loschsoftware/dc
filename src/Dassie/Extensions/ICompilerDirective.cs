using Antlr4.Runtime;

namespace Dassie.Extensions;

/// <summary>
/// Represents the metadata attached to the callsite of a compiler directive.
/// </summary>
public record DirectiveContext
{
    /// <summary>
    /// The arguments passed to the directive.
    /// </summary>
    public object[] Arguments { get; internal set; }

    /// <summary>
    /// The name of the source document containing the compiler directive.
    /// </summary>
    public string DocumentName { get; internal set; }

    /// <summary>
    /// The physical line number that contains the compiler directive.
    /// </summary>
    public int LineNumber { get; internal set; }

    /// <summary>
    /// The parser rule representing the directive callsite.
    /// </summary>
    public ParserRuleContext Rule { get; internal set; }
}

/// <summary>
/// Represents a custom compiler directive enabled by the <c>${...}</c> syntax.
/// </summary>
public interface ICompilerDirective
{
    /// <summary>
    /// The name of the compiler directive.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// If <see langword="true"/>, all arguments will be passed as strings implicitly.
    /// </summary>
    public virtual bool IgnoreArgumentTypes => false;

    /// <summary>
    /// The method that is called when the compiler directive is invoked.
    /// </summary>
    /// <param name="context">An object of type <see cref="DirectiveContext"/> representing the metadata associated with the directive invocation.</param>
    /// <returns>The return value of the invocation.</returns>
    public object Invoke(DirectiveContext context);
}