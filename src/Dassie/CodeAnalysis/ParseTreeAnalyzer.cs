﻿using Antlr4.Runtime.Tree;
using Dassie.Errors;
using System.Collections.Generic;

namespace Dassie.CodeAnalysis;

/// <summary>
/// Represents a code analyzer that analyzes a specific kind of parse tree.
/// </summary>
/// <typeparam name="TTree">The type of tree to analyze.</typeparam>
public class ParseTreeAnalyzer<TTree> : IAnalyzer<IParseTree> where TTree : IParseTree
{
    /// <inheritdoc/>
    public virtual string Name => nameof(ParseTreeAnalyzer<TTree>);

    /// <summary>
    /// Analyzes a list of parse trees.
    /// </summary>
    /// <param name="trees">The trees to analyze.</param>
    /// <returns>The error messages generated by the analyzer.</returns>
    public virtual List<ErrorInfo> Analyze(List<IParseTree> trees) => [];
}