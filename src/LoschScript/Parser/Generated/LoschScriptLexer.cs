//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Jonas\Source\Repos\lsc\src\LoschScript\Parser\LoschScriptLexer.g4 by ANTLR 4.12.0

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace LoschScript.Parser {

	using System.Collections.Generic;
	using System.Linq;

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.12.0")]
[System.CLSCompliant(false)]
public partial class LoschScriptLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		INDENT=1, DEDENT=2, Ws=3, NewLine=4, Single_Line_Comment=5, Delimited_Comment=6, 
		Import=7, Export=8, Assembly=9, Type=10, Module=11, Global=12, Local=13, 
		Internal=14, Static=15, Protected=16, Sealed=17, Infix=18, Inline=19, 
		Var=20, Val=21, True=22, False=23, Of=24, Open_Paren=25, Close_Paren=26, 
		Open_Bracket=27, Close_Bracket=28, Open_Brace=29, Close_Brace=30, Comma=31, 
		Double_Comma=32, Dot=33, Double_Dot=34, Double_Dot_Question_Mark=35, Dot_Equals=36, 
		Colon=37, Underscore=38, Single_Quote=39, Double_Quote=40, Question_Mark=41, 
		Exclamation_Mark=42, Exclamation_Question=43, Exclamation_Colon=44, At_Sign=45, 
		Dollar_Sign=46, Caret=47, Percent_Caret=48, Bar=49, Double_Bar=50, Bar_Equals=51, 
		Double_Bar_Equals=52, Ampersand=53, Double_Ampersand=54, Double_Ampersand_Equals=55, 
		Ampersand_Equals=56, Less_Than=57, Greater_Than=58, Less_Equals=59, Greater_Equals=60, 
		Double_Equals=61, Exclamation_Equals=62, Plus=63, Plus_Equals=64, Minus=65, 
		Minus_Equals=66, Asterisk=67, Asterisk_Equals=68, Double_Asterisk=69, 
		Double_Asterisk_Equals=70, Slash=71, Slash_Equals=72, Percent=73, Percent_Equals=74, 
		Equals=75, Tilde=76, Tilde_Equals=77, Double_Less_Than=78, Double_Less_Than_Equals=79, 
		Double_Greater_Than=80, Double_Greater_Than_Equals=81, Arrow_Right=82, 
		Arrow_Left=83, Double_Backtick=84, Identifier=85, Integer_Literal=86, 
		Hex_Integer_Literal=87, Binary_Integer_Literal=88, Real_Literal=89, Character_Literal=90, 
		String_Literal=91, Verbatim_String_Literal=92;
	public const int
		Comments_Channel=2;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN", "Comments_Channel"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"Ws", "NewLine", "Single_Line_Comment", "Delimited_Comment", "Import", 
		"Export", "Assembly", "Type", "Module", "Global", "Local", "Internal", 
		"Static", "Protected", "Sealed", "Infix", "Inline", "Var", "Val", "True", 
		"False", "Of", "Open_Paren", "Close_Paren", "Open_Bracket", "Close_Bracket", 
		"Open_Brace", "Close_Brace", "Comma", "Double_Comma", "Dot", "Double_Dot", 
		"Double_Dot_Question_Mark", "Dot_Equals", "Colon", "Underscore", "Single_Quote", 
		"Double_Quote", "Question_Mark", "Exclamation_Mark", "Exclamation_Question", 
		"Exclamation_Colon", "At_Sign", "Dollar_Sign", "Caret", "Percent_Caret", 
		"Bar", "Double_Bar", "Bar_Equals", "Double_Bar_Equals", "Ampersand", "Double_Ampersand", 
		"Double_Ampersand_Equals", "Ampersand_Equals", "Less_Than", "Greater_Than", 
		"Less_Equals", "Greater_Equals", "Double_Equals", "Exclamation_Equals", 
		"Plus", "Plus_Equals", "Minus", "Minus_Equals", "Asterisk", "Asterisk_Equals", 
		"Double_Asterisk", "Double_Asterisk_Equals", "Slash", "Slash_Equals", 
		"Percent", "Percent_Equals", "Equals", "Tilde", "Tilde_Equals", "Double_Less_Than", 
		"Double_Less_Than_Equals", "Double_Greater_Than", "Double_Greater_Than_Equals", 
		"Arrow_Right", "Arrow_Left", "Double_Backtick", "Identifier", "Integer_Literal", 
		"Hex_Integer_Literal", "Binary_Integer_Literal", "Real_Literal", "Character_Literal", 
		"String_Literal", "Verbatim_String_Literal", "Integer_Suffix", "ExponentPart", 
		"CommonCharacter", "SimpleEscapeSequence", "HexEscapeSequence", "Whitespace", 
		"UnicodeClassZS", "InputCharacter", "IdentifierOrKeyword", "IdentifierStartCharacter", 
		"IdentifierPartCharacter", "LetterCharacter", "DecimalDigitCharacter", 
		"ConnectingCharacter", "CombiningCharacter", "FormattingCharacter", "UnicodeEscapeSequence", 
		"HexDigit", "UnicodeClassLU", "UnicodeClassLL", "UnicodeClassLT", "UnicodeClassLM", 
		"UnicodeClassLO", "UnicodeClassNL", "UnicodeClassMN", "UnicodeClassMC", 
		"UnicodeClassCF", "UnicodeClassPC", "UnicodeClassND"
	};


		// Initializing `pendingDent` to true means any whitespace at the beginning
		// of the file will trigger an INDENT, which will probably be a syntax error,
		// as it is in Python.
		private bool pendingDent = true;

		private int indentCount = 0;

		private LinkedList<IToken> tokenQueue = new();

		private Stack<int> indentStack = new();

		private IToken initialIndentToken = null;

		private int getSavedIndent() => indentStack.Count == 0 ? 0 : indentStack.Peek();

		private CommonToken createToken(int type, string text, IToken next) {
			CommonToken token = new(type, text);

			if (initialIndentToken != null) {
				token.StartIndex = initialIndentToken.StartIndex;
				token.Line = initialIndentToken.Line;
				token.Column = initialIndentToken.Column;
				token.StopIndex = next.StartIndex - 1;
			}

			return token;
		}
		
		public override IToken NextToken() {

			// Return tokens from the queue if it is not empty.
			if (tokenQueue.Count != 0) { var rv = tokenQueue.First(); tokenQueue.RemoveFirst(); return rv; }

			// Grab the next token and if nothing special is needed, simply return it.
			// Initialize `initialIndentToken` if needed.
			IToken next = base.NextToken();
			//NOTE: This could be an appropriate spot to count whitespace or deal with
			//NEWLINES, but it is already handled with custom actions down in the
			//lexer rules.
			if (pendingDent && null == initialIndentToken && NewLine != next.Type) { initialIndentToken = next; }
			if (null == next || next.Channel == Hidden || NewLine == next.Type) { return next; }

			// Handle EOF. In particular, handle an abrupt EOF that comes without an
			// immediately preceding NEWLINE.
			if (next.Type == Eof) {
				indentCount = 0;
				// EOF outside of `pendingDent` state means input did not have a final
				// NEWLINE before end of file.
				if (!pendingDent) {
					initialIndentToken = next;
					tokenQueue.AddLast(createToken(NewLine, "NewLine", next));
				}
			}

			// Before exiting `pendingDent` state queue up proper INDENTS and DEDENTS.
			while (indentCount != getSavedIndent()) {
				if (indentCount > getSavedIndent()) {
					indentStack.Push(indentCount);
					tokenQueue.AddLast(createToken(INDENT, "INDENT" + indentCount, next));
				} else {
					indentStack.Pop();
					tokenQueue.AddLast(createToken(DEDENT, "DEDENT" + getSavedIndent(), next));
				}
			}
			pendingDent = false;
			tokenQueue.AddLast(next);

			var returnValue = tokenQueue.First();

			tokenQueue.RemoveFirst();

			return returnValue;
		}


	public LoschScriptLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public LoschScriptLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, null, null, null, "'import'", "'export'", "'assembly'", 
		"'type'", "'module'", "'global'", "'local'", "'internal'", "'static'", 
		"'protected'", "'sealed'", "'infix'", "'inline'", "'var'", "'val'", "'true'", 
		"'false'", "'of'", "'('", "')'", "'['", "']'", "'{'", "'}'", "','", "',,'", 
		"'.'", "'..'", "'..?'", "'.='", "':'", "'_'", "'''", "'\"'", "'?'", "'!'", 
		"'!?'", "'!:'", "'@'", "'$'", "'^'", "'%^'", "'|'", "'||'", "'|='", "'||='", 
		"'&'", "'&&'", "'&&='", "'&='", "'<'", "'>'", "'<='", "'>='", "'=='", 
		"'!='", "'+'", "'+='", "'-'", "'-='", "'*'", "'*='", "'**'", "'**='", 
		"'/'", "'/='", "'%'", "'%='", "'='", "'~'", "'~='", "'<<'", "'<<='", "'>>'", 
		"'>>='", "'->'", "'<-'", "'``'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "INDENT", "DEDENT", "Ws", "NewLine", "Single_Line_Comment", "Delimited_Comment", 
		"Import", "Export", "Assembly", "Type", "Module", "Global", "Local", "Internal", 
		"Static", "Protected", "Sealed", "Infix", "Inline", "Var", "Val", "True", 
		"False", "Of", "Open_Paren", "Close_Paren", "Open_Bracket", "Close_Bracket", 
		"Open_Brace", "Close_Brace", "Comma", "Double_Comma", "Dot", "Double_Dot", 
		"Double_Dot_Question_Mark", "Dot_Equals", "Colon", "Underscore", "Single_Quote", 
		"Double_Quote", "Question_Mark", "Exclamation_Mark", "Exclamation_Question", 
		"Exclamation_Colon", "At_Sign", "Dollar_Sign", "Caret", "Percent_Caret", 
		"Bar", "Double_Bar", "Bar_Equals", "Double_Bar_Equals", "Ampersand", "Double_Ampersand", 
		"Double_Ampersand_Equals", "Ampersand_Equals", "Less_Than", "Greater_Than", 
		"Less_Equals", "Greater_Equals", "Double_Equals", "Exclamation_Equals", 
		"Plus", "Plus_Equals", "Minus", "Minus_Equals", "Asterisk", "Asterisk_Equals", 
		"Double_Asterisk", "Double_Asterisk_Equals", "Slash", "Slash_Equals", 
		"Percent", "Percent_Equals", "Equals", "Tilde", "Tilde_Equals", "Double_Less_Than", 
		"Double_Less_Than_Equals", "Double_Greater_Than", "Double_Greater_Than_Equals", 
		"Arrow_Right", "Arrow_Left", "Double_Backtick", "Identifier", "Integer_Literal", 
		"Hex_Integer_Literal", "Binary_Integer_Literal", "Real_Literal", "Character_Literal", 
		"String_Literal", "Verbatim_String_Literal"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "LoschScriptLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static LoschScriptLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	public override void Action(RuleContext _localctx, int ruleIndex, int actionIndex) {
		switch (ruleIndex) {
		case 0 : Ws_action(_localctx, actionIndex); break;
		case 1 : NewLine_action(_localctx, actionIndex); break;
		}
	}
	private void Ws_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0: 
			Channel = Hidden;
			if (pendingDent) {
				indentCount += Text.Length;
			}
		 break;
		}
	}
	private void NewLine_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 1: 
				if (pendingDent) {
					Channel = Hidden;
				}

				pendingDent = true;
				indentCount = 0;
				initialIndentToken = null;
			 break;
		}
	}

	private static int[] _serializedATN = {
		4,0,92,893,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,42,
		7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,7,48,2,49,
		7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,2,55,7,55,2,56,
		7,56,2,57,7,57,2,58,7,58,2,59,7,59,2,60,7,60,2,61,7,61,2,62,7,62,2,63,
		7,63,2,64,7,64,2,65,7,65,2,66,7,66,2,67,7,67,2,68,7,68,2,69,7,69,2,70,
		7,70,2,71,7,71,2,72,7,72,2,73,7,73,2,74,7,74,2,75,7,75,2,76,7,76,2,77,
		7,77,2,78,7,78,2,79,7,79,2,80,7,80,2,81,7,81,2,82,7,82,2,83,7,83,2,84,
		7,84,2,85,7,85,2,86,7,86,2,87,7,87,2,88,7,88,2,89,7,89,2,90,7,90,2,91,
		7,91,2,92,7,92,2,93,7,93,2,94,7,94,2,95,7,95,2,96,7,96,2,97,7,97,2,98,
		7,98,2,99,7,99,2,100,7,100,2,101,7,101,2,102,7,102,2,103,7,103,2,104,7,
		104,2,105,7,105,2,106,7,106,2,107,7,107,2,108,7,108,2,109,7,109,2,110,
		7,110,2,111,7,111,2,112,7,112,2,113,7,113,2,114,7,114,2,115,7,115,2,116,
		7,116,2,117,7,117,2,118,7,118,1,0,4,0,241,8,0,11,0,12,0,242,1,0,1,0,1,
		1,1,1,1,1,1,1,1,1,3,1,252,8,1,1,2,1,2,5,2,256,8,2,10,2,12,2,259,9,2,1,
		2,1,2,1,3,1,3,1,3,1,3,5,3,267,8,3,10,3,12,3,270,9,3,1,3,1,3,1,3,1,3,1,
		3,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,6,
		1,6,1,6,1,6,1,6,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,
		8,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,
		1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,12,1,12,1,12,1,12,1,12,1,12,1,12,
		1,13,1,13,1,13,1,13,1,13,1,13,1,13,1,13,1,13,1,13,1,14,1,14,1,14,1,14,
		1,14,1,14,1,14,1,15,1,15,1,15,1,15,1,15,1,15,1,16,1,16,1,16,1,16,1,16,
		1,16,1,16,1,17,1,17,1,17,1,17,1,18,1,18,1,18,1,18,1,19,1,19,1,19,1,19,
		1,19,1,20,1,20,1,20,1,20,1,20,1,20,1,21,1,21,1,21,1,22,1,22,1,23,1,23,
		1,24,1,24,1,25,1,25,1,26,1,26,1,27,1,27,1,28,1,28,1,29,1,29,1,29,1,30,
		1,30,1,31,1,31,1,31,1,32,1,32,1,32,1,32,1,33,1,33,1,33,1,34,1,34,1,35,
		1,35,1,36,1,36,1,37,1,37,1,38,1,38,1,39,1,39,1,40,1,40,1,40,1,41,1,41,
		1,41,1,42,1,42,1,43,1,43,1,44,1,44,1,45,1,45,1,45,1,46,1,46,1,47,1,47,
		1,47,1,48,1,48,1,48,1,49,1,49,1,49,1,49,1,50,1,50,1,51,1,51,1,51,1,52,
		1,52,1,52,1,52,1,53,1,53,1,53,1,54,1,54,1,55,1,55,1,56,1,56,1,56,1,57,
		1,57,1,57,1,58,1,58,1,58,1,59,1,59,1,59,1,60,1,60,1,61,1,61,1,61,1,62,
		1,62,1,63,1,63,1,63,1,64,1,64,1,65,1,65,1,65,1,66,1,66,1,66,1,67,1,67,
		1,67,1,67,1,68,1,68,1,69,1,69,1,69,1,70,1,70,1,71,1,71,1,71,1,72,1,72,
		1,73,1,73,1,74,1,74,1,74,1,75,1,75,1,75,1,76,1,76,1,76,1,76,1,77,1,77,
		1,77,1,78,1,78,1,78,1,78,1,79,1,79,1,79,1,80,1,80,1,80,1,81,1,81,1,81,
		1,82,3,82,552,8,82,1,82,1,82,3,82,556,8,82,1,83,1,83,3,83,560,8,83,1,83,
		1,83,5,83,564,8,83,10,83,12,83,567,9,83,1,83,5,83,570,8,83,10,83,12,83,
		573,9,83,1,83,3,83,576,8,83,1,84,1,84,3,84,580,8,84,1,84,1,84,1,84,5,84,
		585,8,84,10,84,12,84,588,9,84,1,84,4,84,591,8,84,11,84,12,84,592,1,84,
		3,84,596,8,84,1,85,1,85,3,85,600,8,85,1,85,1,85,1,85,5,85,605,8,85,10,
		85,12,85,608,9,85,1,85,4,85,611,8,85,11,85,12,85,612,1,85,3,85,616,8,85,
		1,86,1,86,3,86,620,8,86,1,86,1,86,5,86,624,8,86,10,86,12,86,627,9,86,1,
		86,5,86,630,8,86,10,86,12,86,633,9,86,3,86,635,8,86,1,86,1,86,1,86,5,86,
		640,8,86,10,86,12,86,643,9,86,1,86,5,86,646,8,86,10,86,12,86,649,9,86,
		1,86,3,86,652,8,86,1,86,3,86,655,8,86,1,86,1,86,5,86,659,8,86,10,86,12,
		86,662,9,86,1,86,5,86,665,8,86,10,86,12,86,668,9,86,1,86,1,86,1,86,3,86,
		673,8,86,3,86,675,8,86,3,86,677,8,86,1,87,1,87,1,87,3,87,682,8,87,1,87,
		1,87,1,88,1,88,1,88,5,88,689,8,88,10,88,12,88,692,9,88,1,88,1,88,1,89,
		1,89,1,89,1,89,1,89,1,89,5,89,702,8,89,10,89,12,89,705,9,89,1,89,1,89,
		1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,3,90,720,8,90,1,
		91,1,91,3,91,724,8,91,1,91,1,91,5,91,728,8,91,10,91,12,91,731,9,91,1,91,
		5,91,734,8,91,10,91,12,91,737,9,91,1,92,1,92,1,92,3,92,742,8,92,1,93,1,
		93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,93,1,
		93,1,93,1,93,1,93,1,93,1,93,1,93,3,93,766,8,93,1,94,1,94,1,94,1,94,1,94,
		1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,1,94,
		1,94,1,94,1,94,1,94,1,94,1,94,3,94,793,8,94,1,95,1,95,3,95,797,8,95,1,
		96,1,96,1,97,1,97,1,98,1,98,5,98,805,8,98,10,98,12,98,808,9,98,1,99,1,
		99,3,99,812,8,99,1,100,1,100,1,100,1,100,1,100,3,100,819,8,100,1,101,1,
		101,1,101,1,101,1,101,1,101,1,101,3,101,828,8,101,1,102,1,102,3,102,832,
		8,102,1,103,1,103,3,103,836,8,103,1,104,1,104,1,104,3,104,841,8,104,1,
		105,1,105,3,105,845,8,105,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,
		106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,1,106,
		1,106,3,106,867,8,106,1,107,3,107,870,8,107,1,108,1,108,1,109,1,109,1,
		110,1,110,1,111,1,111,1,112,1,112,1,113,1,113,1,114,1,114,1,115,1,115,
		1,116,1,116,1,117,1,117,1,118,1,118,1,268,0,119,1,3,3,4,5,5,7,6,9,7,11,
		8,13,9,15,10,17,11,19,12,21,13,23,14,25,15,27,16,29,17,31,18,33,19,35,
		20,37,21,39,22,41,23,43,24,45,25,47,26,49,27,51,28,53,29,55,30,57,31,59,
		32,61,33,63,34,65,35,67,36,69,37,71,38,73,39,75,40,77,41,79,42,81,43,83,
		44,85,45,87,46,89,47,91,48,93,49,95,50,97,51,99,52,101,53,103,54,105,55,
		107,56,109,57,111,58,113,59,115,60,117,61,119,62,121,63,123,64,125,65,
		127,66,129,67,131,68,133,69,135,70,137,71,139,72,141,73,143,74,145,75,
		147,76,149,77,151,78,153,79,155,80,157,81,159,82,161,83,163,84,165,85,
		167,86,169,87,171,88,173,89,175,90,177,91,179,92,181,0,183,0,185,0,187,
		0,189,0,191,0,193,0,195,0,197,0,199,0,201,0,203,0,205,0,207,0,209,0,211,
		0,213,0,215,0,217,0,219,0,221,0,223,0,225,0,227,0,229,0,231,0,233,0,235,
		0,237,0,1,0,28,2,0,9,9,32,32,4,0,10,10,13,13,133,133,8232,8232,1,0,48,
		57,2,0,88,88,120,120,2,0,66,66,98,98,1,0,48,49,3,0,100,100,109,109,115,
		115,6,0,10,10,13,13,39,39,92,92,133,133,8232,8233,6,0,10,10,13,13,34,34,
		92,92,133,133,8232,8233,1,0,34,34,2,0,98,98,115,115,2,0,108,108,117,117,
		2,0,69,69,101,101,2,0,43,43,45,45,2,0,9,9,11,12,9,0,32,32,160,160,5760,
		5760,6158,6158,8192,8198,8200,8202,8239,8239,8287,8287,12288,12288,4,0,
		10,10,13,13,133,133,8232,8233,3,0,48,57,65,70,97,102,82,0,65,90,192,214,
		216,222,256,310,313,327,330,381,385,386,388,395,398,401,403,404,406,408,
		412,413,415,416,418,425,428,435,437,444,452,461,463,475,478,494,497,500,
		502,504,506,562,570,571,573,574,577,582,584,590,880,882,886,895,902,906,
		908,929,931,939,975,980,984,1006,1012,1015,1017,1018,1021,1071,1120,1152,
		1162,1229,1232,1326,1329,1366,4256,4293,4295,4301,7680,7828,7838,7934,
		7944,7951,7960,7965,7976,7983,7992,7999,8008,8013,8025,8031,8040,8047,
		8120,8123,8136,8139,8152,8155,8168,8172,8184,8187,8450,8455,8459,8461,
		8464,8466,8469,8477,8484,8493,8496,8499,8510,8511,8517,8579,11264,11310,
		11360,11364,11367,11376,11378,11381,11390,11392,11394,11490,11499,11501,
		11506,42560,42562,42604,42624,42650,42786,42798,42802,42862,42873,42886,
		42891,42893,42896,42898,42902,42925,42928,42929,65313,65338,81,0,97,122,
		181,246,248,255,257,375,378,384,387,389,392,402,405,411,414,417,419,421,
		424,429,432,436,438,447,454,460,462,499,501,505,507,569,572,578,583,659,
		661,687,881,883,887,893,912,974,976,977,981,983,985,1011,1013,1119,1121,
		1153,1163,1215,1218,1327,1377,1415,7424,7467,7531,7543,7545,7578,7681,
		7837,7839,7943,7952,7957,7968,7975,7984,7991,8000,8005,8016,8023,8032,
		8039,8048,8061,8064,8071,8080,8087,8096,8103,8112,8116,8118,8119,8126,
		8132,8134,8135,8144,8147,8150,8151,8160,8167,8178,8180,8182,8183,8458,
		8467,8495,8505,8508,8509,8518,8521,8526,8580,11312,11358,11361,11372,11377,
		11387,11393,11500,11502,11507,11520,11557,11559,11565,42561,42605,42625,
		42651,42787,42801,42803,42872,42874,42876,42879,42887,42892,42894,42897,
		42901,42903,42921,43002,43866,43876,43877,64256,64262,64275,64279,65345,
		65370,6,0,453,459,498,8079,8088,8095,8104,8111,8124,8140,8188,8188,33,
		0,688,705,710,721,736,740,748,750,884,890,1369,1600,1765,1766,2036,2037,
		2042,2074,2084,2088,2417,3654,3782,4348,6103,6211,6823,7293,7468,7530,
		7544,7615,8305,8319,8336,8348,11388,11389,11631,11823,12293,12341,12347,
		12542,40981,42237,42508,42623,42652,42653,42775,42783,42864,42888,43000,
		43001,43471,43494,43632,43741,43763,43764,43868,43871,65392,65439,234,
		0,170,186,443,451,660,1514,1520,1522,1568,1599,1601,1610,1646,1647,1649,
		1747,1749,1788,1791,1808,1810,1839,1869,1957,1969,2026,2048,2069,2112,
		2136,2208,2226,2308,2361,2365,2384,2392,2401,2418,2432,2437,2444,2447,
		2448,2451,2472,2474,2480,2482,2489,2493,2510,2524,2525,2527,2529,2544,
		2545,2565,2570,2575,2576,2579,2600,2602,2608,2610,2611,2613,2614,2616,
		2617,2649,2652,2654,2676,2693,2701,2703,2705,2707,2728,2730,2736,2738,
		2739,2741,2745,2749,2768,2784,2785,2821,2828,2831,2832,2835,2856,2858,
		2864,2866,2867,2869,2873,2877,2913,2929,2947,2949,2954,2958,2960,2962,
		2965,2969,2970,2972,2986,2990,3001,3024,3084,3086,3088,3090,3112,3114,
		3129,3133,3212,3214,3216,3218,3240,3242,3251,3253,3257,3261,3294,3296,
		3297,3313,3314,3333,3340,3342,3344,3346,3386,3389,3406,3424,3425,3450,
		3455,3461,3478,3482,3505,3507,3515,3517,3526,3585,3632,3634,3635,3648,
		3653,3713,3714,3716,3722,3725,3735,3737,3743,3745,3747,3749,3751,3754,
		3755,3757,3760,3762,3763,3773,3780,3804,3807,3840,3911,3913,3948,3976,
		3980,4096,4138,4159,4181,4186,4189,4193,4208,4213,4225,4238,4346,4349,
		4680,4682,4685,4688,4694,4696,4701,4704,4744,4746,4749,4752,4784,4786,
		4789,4792,4798,4800,4805,4808,4822,4824,4880,4882,4885,4888,4954,4992,
		5007,5024,5108,5121,5740,5743,5759,5761,5786,5792,5866,5873,5880,5888,
		5900,5902,5905,5920,5937,5952,5969,5984,5996,5998,6000,6016,6067,6108,
		6210,6212,6263,6272,6312,6314,6389,6400,6430,6480,6509,6512,6516,6528,
		6571,6593,6599,6656,6678,6688,6740,6917,6963,6981,6987,7043,7072,7086,
		7087,7098,7141,7168,7203,7245,7247,7258,7287,7401,7404,7406,7409,7413,
		7414,8501,8504,11568,11623,11648,11670,11680,11686,11688,11694,11696,11702,
		11704,11710,11712,11718,11720,11726,11728,11734,11736,11742,12294,12348,
		12353,12438,12447,12538,12543,12589,12593,12686,12704,12730,12784,12799,
		13312,19893,19968,40908,40960,40980,40982,42124,42192,42231,42240,42507,
		42512,42527,42538,42539,42606,42725,42999,43009,43011,43013,43015,43018,
		43020,43042,43072,43123,43138,43187,43250,43255,43259,43301,43312,43334,
		43360,43388,43396,43442,43488,43492,43495,43503,43514,43518,43520,43560,
		43584,43586,43588,43595,43616,43631,43633,43638,43642,43695,43697,43709,
		43712,43714,43739,43740,43744,43754,43762,43782,43785,43790,43793,43798,
		43808,43814,43816,43822,43968,44002,44032,55203,55216,55238,55243,55291,
		63744,64109,64112,64217,64285,64296,64298,64310,64312,64316,64318,64433,
		64467,64829,64848,64911,64914,64967,65008,65019,65136,65140,65142,65276,
		65382,65391,65393,65437,65440,65470,65474,65479,65482,65487,65490,65495,
		65498,65500,2,0,5870,5872,8544,8559,3,0,2307,2307,2366,2368,2377,2380,
		3,0,173,173,1536,1539,1757,1757,6,0,95,95,8255,8256,8276,8276,65075,65076,
		65101,65103,65343,65343,37,0,48,57,1632,1641,1776,1785,1984,1993,2406,
		2415,2534,2543,2662,2671,2790,2799,2918,2927,3046,3055,3174,3183,3302,
		3311,3430,3439,3558,3567,3664,3673,3792,3801,3872,3881,4160,4169,4240,
		4249,6112,6121,6160,6169,6470,6479,6608,6617,6784,6793,6800,6809,6992,
		7001,7088,7097,7232,7241,7248,7257,42528,42537,43216,43225,43264,43273,
		43472,43481,43504,43513,43600,43609,44016,44025,65296,65305,947,0,1,1,
		0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,
		1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,
		0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,
		1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,
		0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,
		1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,
		0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,
		1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,1,0,0,
		0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,0,0,101,
		1,0,0,0,0,103,1,0,0,0,0,105,1,0,0,0,0,107,1,0,0,0,0,109,1,0,0,0,0,111,
		1,0,0,0,0,113,1,0,0,0,0,115,1,0,0,0,0,117,1,0,0,0,0,119,1,0,0,0,0,121,
		1,0,0,0,0,123,1,0,0,0,0,125,1,0,0,0,0,127,1,0,0,0,0,129,1,0,0,0,0,131,
		1,0,0,0,0,133,1,0,0,0,0,135,1,0,0,0,0,137,1,0,0,0,0,139,1,0,0,0,0,141,
		1,0,0,0,0,143,1,0,0,0,0,145,1,0,0,0,0,147,1,0,0,0,0,149,1,0,0,0,0,151,
		1,0,0,0,0,153,1,0,0,0,0,155,1,0,0,0,0,157,1,0,0,0,0,159,1,0,0,0,0,161,
		1,0,0,0,0,163,1,0,0,0,0,165,1,0,0,0,0,167,1,0,0,0,0,169,1,0,0,0,0,171,
		1,0,0,0,0,173,1,0,0,0,0,175,1,0,0,0,0,177,1,0,0,0,0,179,1,0,0,0,1,240,
		1,0,0,0,3,251,1,0,0,0,5,253,1,0,0,0,7,262,1,0,0,0,9,276,1,0,0,0,11,283,
		1,0,0,0,13,290,1,0,0,0,15,299,1,0,0,0,17,304,1,0,0,0,19,311,1,0,0,0,21,
		318,1,0,0,0,23,324,1,0,0,0,25,333,1,0,0,0,27,340,1,0,0,0,29,350,1,0,0,
		0,31,357,1,0,0,0,33,363,1,0,0,0,35,370,1,0,0,0,37,374,1,0,0,0,39,378,1,
		0,0,0,41,383,1,0,0,0,43,389,1,0,0,0,45,392,1,0,0,0,47,394,1,0,0,0,49,396,
		1,0,0,0,51,398,1,0,0,0,53,400,1,0,0,0,55,402,1,0,0,0,57,404,1,0,0,0,59,
		406,1,0,0,0,61,409,1,0,0,0,63,411,1,0,0,0,65,414,1,0,0,0,67,418,1,0,0,
		0,69,421,1,0,0,0,71,423,1,0,0,0,73,425,1,0,0,0,75,427,1,0,0,0,77,429,1,
		0,0,0,79,431,1,0,0,0,81,433,1,0,0,0,83,436,1,0,0,0,85,439,1,0,0,0,87,441,
		1,0,0,0,89,443,1,0,0,0,91,445,1,0,0,0,93,448,1,0,0,0,95,450,1,0,0,0,97,
		453,1,0,0,0,99,456,1,0,0,0,101,460,1,0,0,0,103,462,1,0,0,0,105,465,1,0,
		0,0,107,469,1,0,0,0,109,472,1,0,0,0,111,474,1,0,0,0,113,476,1,0,0,0,115,
		479,1,0,0,0,117,482,1,0,0,0,119,485,1,0,0,0,121,488,1,0,0,0,123,490,1,
		0,0,0,125,493,1,0,0,0,127,495,1,0,0,0,129,498,1,0,0,0,131,500,1,0,0,0,
		133,503,1,0,0,0,135,506,1,0,0,0,137,510,1,0,0,0,139,512,1,0,0,0,141,515,
		1,0,0,0,143,517,1,0,0,0,145,520,1,0,0,0,147,522,1,0,0,0,149,524,1,0,0,
		0,151,527,1,0,0,0,153,530,1,0,0,0,155,534,1,0,0,0,157,537,1,0,0,0,159,
		541,1,0,0,0,161,544,1,0,0,0,163,547,1,0,0,0,165,551,1,0,0,0,167,559,1,
		0,0,0,169,579,1,0,0,0,171,599,1,0,0,0,173,676,1,0,0,0,175,678,1,0,0,0,
		177,685,1,0,0,0,179,695,1,0,0,0,181,719,1,0,0,0,183,721,1,0,0,0,185,741,
		1,0,0,0,187,765,1,0,0,0,189,792,1,0,0,0,191,796,1,0,0,0,193,798,1,0,0,
		0,195,800,1,0,0,0,197,802,1,0,0,0,199,811,1,0,0,0,201,818,1,0,0,0,203,
		827,1,0,0,0,205,831,1,0,0,0,207,835,1,0,0,0,209,840,1,0,0,0,211,844,1,
		0,0,0,213,866,1,0,0,0,215,869,1,0,0,0,217,871,1,0,0,0,219,873,1,0,0,0,
		221,875,1,0,0,0,223,877,1,0,0,0,225,879,1,0,0,0,227,881,1,0,0,0,229,883,
		1,0,0,0,231,885,1,0,0,0,233,887,1,0,0,0,235,889,1,0,0,0,237,891,1,0,0,
		0,239,241,7,0,0,0,240,239,1,0,0,0,241,242,1,0,0,0,242,240,1,0,0,0,242,
		243,1,0,0,0,243,244,1,0,0,0,244,245,6,0,0,0,245,2,1,0,0,0,246,247,5,13,
		0,0,247,252,5,10,0,0,248,252,7,1,0,0,249,250,5,8233,0,0,250,252,6,1,1,
		0,251,246,1,0,0,0,251,248,1,0,0,0,251,249,1,0,0,0,252,4,1,0,0,0,253,257,
		5,35,0,0,254,256,3,195,97,0,255,254,1,0,0,0,256,259,1,0,0,0,257,255,1,
		0,0,0,257,258,1,0,0,0,258,260,1,0,0,0,259,257,1,0,0,0,260,261,6,2,2,0,
		261,6,1,0,0,0,262,263,5,35,0,0,263,264,5,91,0,0,264,268,1,0,0,0,265,267,
		9,0,0,0,266,265,1,0,0,0,267,270,1,0,0,0,268,269,1,0,0,0,268,266,1,0,0,
		0,269,271,1,0,0,0,270,268,1,0,0,0,271,272,5,93,0,0,272,273,5,35,0,0,273,
		274,1,0,0,0,274,275,6,3,2,0,275,8,1,0,0,0,276,277,5,105,0,0,277,278,5,
		109,0,0,278,279,5,112,0,0,279,280,5,111,0,0,280,281,5,114,0,0,281,282,
		5,116,0,0,282,10,1,0,0,0,283,284,5,101,0,0,284,285,5,120,0,0,285,286,5,
		112,0,0,286,287,5,111,0,0,287,288,5,114,0,0,288,289,5,116,0,0,289,12,1,
		0,0,0,290,291,5,97,0,0,291,292,5,115,0,0,292,293,5,115,0,0,293,294,5,101,
		0,0,294,295,5,109,0,0,295,296,5,98,0,0,296,297,5,108,0,0,297,298,5,121,
		0,0,298,14,1,0,0,0,299,300,5,116,0,0,300,301,5,121,0,0,301,302,5,112,0,
		0,302,303,5,101,0,0,303,16,1,0,0,0,304,305,5,109,0,0,305,306,5,111,0,0,
		306,307,5,100,0,0,307,308,5,117,0,0,308,309,5,108,0,0,309,310,5,101,0,
		0,310,18,1,0,0,0,311,312,5,103,0,0,312,313,5,108,0,0,313,314,5,111,0,0,
		314,315,5,98,0,0,315,316,5,97,0,0,316,317,5,108,0,0,317,20,1,0,0,0,318,
		319,5,108,0,0,319,320,5,111,0,0,320,321,5,99,0,0,321,322,5,97,0,0,322,
		323,5,108,0,0,323,22,1,0,0,0,324,325,5,105,0,0,325,326,5,110,0,0,326,327,
		5,116,0,0,327,328,5,101,0,0,328,329,5,114,0,0,329,330,5,110,0,0,330,331,
		5,97,0,0,331,332,5,108,0,0,332,24,1,0,0,0,333,334,5,115,0,0,334,335,5,
		116,0,0,335,336,5,97,0,0,336,337,5,116,0,0,337,338,5,105,0,0,338,339,5,
		99,0,0,339,26,1,0,0,0,340,341,5,112,0,0,341,342,5,114,0,0,342,343,5,111,
		0,0,343,344,5,116,0,0,344,345,5,101,0,0,345,346,5,99,0,0,346,347,5,116,
		0,0,347,348,5,101,0,0,348,349,5,100,0,0,349,28,1,0,0,0,350,351,5,115,0,
		0,351,352,5,101,0,0,352,353,5,97,0,0,353,354,5,108,0,0,354,355,5,101,0,
		0,355,356,5,100,0,0,356,30,1,0,0,0,357,358,5,105,0,0,358,359,5,110,0,0,
		359,360,5,102,0,0,360,361,5,105,0,0,361,362,5,120,0,0,362,32,1,0,0,0,363,
		364,5,105,0,0,364,365,5,110,0,0,365,366,5,108,0,0,366,367,5,105,0,0,367,
		368,5,110,0,0,368,369,5,101,0,0,369,34,1,0,0,0,370,371,5,118,0,0,371,372,
		5,97,0,0,372,373,5,114,0,0,373,36,1,0,0,0,374,375,5,118,0,0,375,376,5,
		97,0,0,376,377,5,108,0,0,377,38,1,0,0,0,378,379,5,116,0,0,379,380,5,114,
		0,0,380,381,5,117,0,0,381,382,5,101,0,0,382,40,1,0,0,0,383,384,5,102,0,
		0,384,385,5,97,0,0,385,386,5,108,0,0,386,387,5,115,0,0,387,388,5,101,0,
		0,388,42,1,0,0,0,389,390,5,111,0,0,390,391,5,102,0,0,391,44,1,0,0,0,392,
		393,5,40,0,0,393,46,1,0,0,0,394,395,5,41,0,0,395,48,1,0,0,0,396,397,5,
		91,0,0,397,50,1,0,0,0,398,399,5,93,0,0,399,52,1,0,0,0,400,401,5,123,0,
		0,401,54,1,0,0,0,402,403,5,125,0,0,403,56,1,0,0,0,404,405,5,44,0,0,405,
		58,1,0,0,0,406,407,5,44,0,0,407,408,5,44,0,0,408,60,1,0,0,0,409,410,5,
		46,0,0,410,62,1,0,0,0,411,412,5,46,0,0,412,413,5,46,0,0,413,64,1,0,0,0,
		414,415,5,46,0,0,415,416,5,46,0,0,416,417,5,63,0,0,417,66,1,0,0,0,418,
		419,5,46,0,0,419,420,5,61,0,0,420,68,1,0,0,0,421,422,5,58,0,0,422,70,1,
		0,0,0,423,424,5,95,0,0,424,72,1,0,0,0,425,426,5,39,0,0,426,74,1,0,0,0,
		427,428,5,34,0,0,428,76,1,0,0,0,429,430,5,63,0,0,430,78,1,0,0,0,431,432,
		5,33,0,0,432,80,1,0,0,0,433,434,5,33,0,0,434,435,5,63,0,0,435,82,1,0,0,
		0,436,437,5,33,0,0,437,438,5,58,0,0,438,84,1,0,0,0,439,440,5,64,0,0,440,
		86,1,0,0,0,441,442,5,36,0,0,442,88,1,0,0,0,443,444,5,94,0,0,444,90,1,0,
		0,0,445,446,5,37,0,0,446,447,5,94,0,0,447,92,1,0,0,0,448,449,5,124,0,0,
		449,94,1,0,0,0,450,451,5,124,0,0,451,452,5,124,0,0,452,96,1,0,0,0,453,
		454,5,124,0,0,454,455,5,61,0,0,455,98,1,0,0,0,456,457,5,124,0,0,457,458,
		5,124,0,0,458,459,5,61,0,0,459,100,1,0,0,0,460,461,5,38,0,0,461,102,1,
		0,0,0,462,463,5,38,0,0,463,464,5,38,0,0,464,104,1,0,0,0,465,466,5,38,0,
		0,466,467,5,38,0,0,467,468,5,61,0,0,468,106,1,0,0,0,469,470,5,38,0,0,470,
		471,5,61,0,0,471,108,1,0,0,0,472,473,5,60,0,0,473,110,1,0,0,0,474,475,
		5,62,0,0,475,112,1,0,0,0,476,477,5,60,0,0,477,478,5,61,0,0,478,114,1,0,
		0,0,479,480,5,62,0,0,480,481,5,61,0,0,481,116,1,0,0,0,482,483,5,61,0,0,
		483,484,5,61,0,0,484,118,1,0,0,0,485,486,5,33,0,0,486,487,5,61,0,0,487,
		120,1,0,0,0,488,489,5,43,0,0,489,122,1,0,0,0,490,491,5,43,0,0,491,492,
		5,61,0,0,492,124,1,0,0,0,493,494,5,45,0,0,494,126,1,0,0,0,495,496,5,45,
		0,0,496,497,5,61,0,0,497,128,1,0,0,0,498,499,5,42,0,0,499,130,1,0,0,0,
		500,501,5,42,0,0,501,502,5,61,0,0,502,132,1,0,0,0,503,504,5,42,0,0,504,
		505,5,42,0,0,505,134,1,0,0,0,506,507,5,42,0,0,507,508,5,42,0,0,508,509,
		5,61,0,0,509,136,1,0,0,0,510,511,5,47,0,0,511,138,1,0,0,0,512,513,5,47,
		0,0,513,514,5,61,0,0,514,140,1,0,0,0,515,516,5,37,0,0,516,142,1,0,0,0,
		517,518,5,37,0,0,518,519,5,61,0,0,519,144,1,0,0,0,520,521,5,61,0,0,521,
		146,1,0,0,0,522,523,5,126,0,0,523,148,1,0,0,0,524,525,5,126,0,0,525,526,
		5,61,0,0,526,150,1,0,0,0,527,528,5,60,0,0,528,529,5,60,0,0,529,152,1,0,
		0,0,530,531,5,60,0,0,531,532,5,60,0,0,532,533,5,61,0,0,533,154,1,0,0,0,
		534,535,5,62,0,0,535,536,5,62,0,0,536,156,1,0,0,0,537,538,5,62,0,0,538,
		539,5,62,0,0,539,540,5,61,0,0,540,158,1,0,0,0,541,542,5,45,0,0,542,543,
		5,62,0,0,543,160,1,0,0,0,544,545,5,60,0,0,545,546,5,45,0,0,546,162,1,0,
		0,0,547,548,5,96,0,0,548,549,5,96,0,0,549,164,1,0,0,0,550,552,3,163,81,
		0,551,550,1,0,0,0,551,552,1,0,0,0,552,553,1,0,0,0,553,555,3,197,98,0,554,
		556,3,163,81,0,555,554,1,0,0,0,555,556,1,0,0,0,556,166,1,0,0,0,557,560,
		3,125,62,0,558,560,3,121,60,0,559,557,1,0,0,0,559,558,1,0,0,0,559,560,
		1,0,0,0,560,561,1,0,0,0,561,571,7,2,0,0,562,564,5,39,0,0,563,562,1,0,0,
		0,564,567,1,0,0,0,565,563,1,0,0,0,565,566,1,0,0,0,566,568,1,0,0,0,567,
		565,1,0,0,0,568,570,7,2,0,0,569,565,1,0,0,0,570,573,1,0,0,0,571,569,1,
		0,0,0,571,572,1,0,0,0,572,575,1,0,0,0,573,571,1,0,0,0,574,576,3,181,90,
		0,575,574,1,0,0,0,575,576,1,0,0,0,576,168,1,0,0,0,577,580,3,125,62,0,578,
		580,3,121,60,0,579,577,1,0,0,0,579,578,1,0,0,0,579,580,1,0,0,0,580,581,
		1,0,0,0,581,582,5,48,0,0,582,590,7,3,0,0,583,585,5,39,0,0,584,583,1,0,
		0,0,585,588,1,0,0,0,586,584,1,0,0,0,586,587,1,0,0,0,587,589,1,0,0,0,588,
		586,1,0,0,0,589,591,3,215,107,0,590,586,1,0,0,0,591,592,1,0,0,0,592,590,
		1,0,0,0,592,593,1,0,0,0,593,595,1,0,0,0,594,596,3,181,90,0,595,594,1,0,
		0,0,595,596,1,0,0,0,596,170,1,0,0,0,597,600,3,125,62,0,598,600,3,121,60,
		0,599,597,1,0,0,0,599,598,1,0,0,0,599,600,1,0,0,0,600,601,1,0,0,0,601,
		602,5,48,0,0,602,610,7,4,0,0,603,605,5,39,0,0,604,603,1,0,0,0,605,608,
		1,0,0,0,606,604,1,0,0,0,606,607,1,0,0,0,607,609,1,0,0,0,608,606,1,0,0,
		0,609,611,7,5,0,0,610,606,1,0,0,0,611,612,1,0,0,0,612,610,1,0,0,0,612,
		613,1,0,0,0,613,615,1,0,0,0,614,616,3,181,90,0,615,614,1,0,0,0,615,616,
		1,0,0,0,616,172,1,0,0,0,617,620,3,125,62,0,618,620,3,121,60,0,619,617,
		1,0,0,0,619,618,1,0,0,0,619,620,1,0,0,0,620,634,1,0,0,0,621,631,7,2,0,
		0,622,624,5,39,0,0,623,622,1,0,0,0,624,627,1,0,0,0,625,623,1,0,0,0,625,
		626,1,0,0,0,626,628,1,0,0,0,627,625,1,0,0,0,628,630,7,2,0,0,629,625,1,
		0,0,0,630,633,1,0,0,0,631,629,1,0,0,0,631,632,1,0,0,0,632,635,1,0,0,0,
		633,631,1,0,0,0,634,621,1,0,0,0,634,635,1,0,0,0,635,636,1,0,0,0,636,637,
		5,46,0,0,637,647,7,2,0,0,638,640,5,39,0,0,639,638,1,0,0,0,640,643,1,0,
		0,0,641,639,1,0,0,0,641,642,1,0,0,0,642,644,1,0,0,0,643,641,1,0,0,0,644,
		646,7,2,0,0,645,641,1,0,0,0,646,649,1,0,0,0,647,645,1,0,0,0,647,648,1,
		0,0,0,648,651,1,0,0,0,649,647,1,0,0,0,650,652,3,183,91,0,651,650,1,0,0,
		0,651,652,1,0,0,0,652,654,1,0,0,0,653,655,7,6,0,0,654,653,1,0,0,0,654,
		655,1,0,0,0,655,677,1,0,0,0,656,666,7,2,0,0,657,659,5,39,0,0,658,657,1,
		0,0,0,659,662,1,0,0,0,660,658,1,0,0,0,660,661,1,0,0,0,661,663,1,0,0,0,
		662,660,1,0,0,0,663,665,7,2,0,0,664,660,1,0,0,0,665,668,1,0,0,0,666,664,
		1,0,0,0,666,667,1,0,0,0,667,674,1,0,0,0,668,666,1,0,0,0,669,675,7,6,0,
		0,670,672,3,183,91,0,671,673,7,6,0,0,672,671,1,0,0,0,672,673,1,0,0,0,673,
		675,1,0,0,0,674,669,1,0,0,0,674,670,1,0,0,0,675,677,1,0,0,0,676,619,1,
		0,0,0,676,656,1,0,0,0,677,174,1,0,0,0,678,681,5,39,0,0,679,682,8,7,0,0,
		680,682,3,185,92,0,681,679,1,0,0,0,681,680,1,0,0,0,682,683,1,0,0,0,683,
		684,5,39,0,0,684,176,1,0,0,0,685,690,5,34,0,0,686,689,8,8,0,0,687,689,
		3,185,92,0,688,686,1,0,0,0,688,687,1,0,0,0,689,692,1,0,0,0,690,688,1,0,
		0,0,690,691,1,0,0,0,691,693,1,0,0,0,692,690,1,0,0,0,693,694,5,34,0,0,694,
		178,1,0,0,0,695,696,5,94,0,0,696,697,5,34,0,0,697,703,1,0,0,0,698,702,
		8,9,0,0,699,700,5,34,0,0,700,702,5,34,0,0,701,698,1,0,0,0,701,699,1,0,
		0,0,702,705,1,0,0,0,703,701,1,0,0,0,703,704,1,0,0,0,704,706,1,0,0,0,705,
		703,1,0,0,0,706,707,5,34,0,0,707,180,1,0,0,0,708,709,5,115,0,0,709,720,
		5,98,0,0,710,720,7,10,0,0,711,712,5,117,0,0,712,720,5,115,0,0,713,720,
		7,11,0,0,714,715,5,117,0,0,715,720,5,108,0,0,716,720,5,110,0,0,717,718,
		5,117,0,0,718,720,5,110,0,0,719,708,1,0,0,0,719,710,1,0,0,0,719,711,1,
		0,0,0,719,713,1,0,0,0,719,714,1,0,0,0,719,716,1,0,0,0,719,717,1,0,0,0,
		720,182,1,0,0,0,721,723,7,12,0,0,722,724,7,13,0,0,723,722,1,0,0,0,723,
		724,1,0,0,0,724,725,1,0,0,0,725,735,7,2,0,0,726,728,5,96,0,0,727,726,1,
		0,0,0,728,731,1,0,0,0,729,727,1,0,0,0,729,730,1,0,0,0,730,732,1,0,0,0,
		731,729,1,0,0,0,732,734,7,2,0,0,733,729,1,0,0,0,734,737,1,0,0,0,735,733,
		1,0,0,0,735,736,1,0,0,0,736,184,1,0,0,0,737,735,1,0,0,0,738,742,3,187,
		93,0,739,742,3,189,94,0,740,742,3,213,106,0,741,738,1,0,0,0,741,739,1,
		0,0,0,741,740,1,0,0,0,742,186,1,0,0,0,743,744,5,94,0,0,744,766,5,39,0,
		0,745,746,5,94,0,0,746,766,5,34,0,0,747,748,5,94,0,0,748,766,5,94,0,0,
		749,750,5,94,0,0,750,766,5,48,0,0,751,752,5,94,0,0,752,766,5,97,0,0,753,
		754,5,94,0,0,754,766,5,98,0,0,755,756,5,94,0,0,756,766,5,102,0,0,757,758,
		5,94,0,0,758,766,5,110,0,0,759,760,5,94,0,0,760,766,5,114,0,0,761,762,
		5,94,0,0,762,766,5,116,0,0,763,764,5,94,0,0,764,766,5,118,0,0,765,743,
		1,0,0,0,765,745,1,0,0,0,765,747,1,0,0,0,765,749,1,0,0,0,765,751,1,0,0,
		0,765,753,1,0,0,0,765,755,1,0,0,0,765,757,1,0,0,0,765,759,1,0,0,0,765,
		761,1,0,0,0,765,763,1,0,0,0,766,188,1,0,0,0,767,768,5,94,0,0,768,769,5,
		120,0,0,769,770,1,0,0,0,770,793,3,215,107,0,771,772,5,94,0,0,772,773,5,
		120,0,0,773,774,1,0,0,0,774,775,3,215,107,0,775,776,3,215,107,0,776,793,
		1,0,0,0,777,778,5,94,0,0,778,779,5,120,0,0,779,780,1,0,0,0,780,781,3,215,
		107,0,781,782,3,215,107,0,782,783,3,215,107,0,783,793,1,0,0,0,784,785,
		5,94,0,0,785,786,5,120,0,0,786,787,1,0,0,0,787,788,3,215,107,0,788,789,
		3,215,107,0,789,790,3,215,107,0,790,791,3,215,107,0,791,793,1,0,0,0,792,
		767,1,0,0,0,792,771,1,0,0,0,792,777,1,0,0,0,792,784,1,0,0,0,793,190,1,
		0,0,0,794,797,3,193,96,0,795,797,7,14,0,0,796,794,1,0,0,0,796,795,1,0,
		0,0,797,192,1,0,0,0,798,799,7,15,0,0,799,194,1,0,0,0,800,801,8,16,0,0,
		801,196,1,0,0,0,802,806,3,199,99,0,803,805,3,201,100,0,804,803,1,0,0,0,
		805,808,1,0,0,0,806,804,1,0,0,0,806,807,1,0,0,0,807,198,1,0,0,0,808,806,
		1,0,0,0,809,812,3,203,101,0,810,812,5,95,0,0,811,809,1,0,0,0,811,810,1,
		0,0,0,812,200,1,0,0,0,813,819,3,203,101,0,814,819,3,205,102,0,815,819,
		3,207,103,0,816,819,3,209,104,0,817,819,3,211,105,0,818,813,1,0,0,0,818,
		814,1,0,0,0,818,815,1,0,0,0,818,816,1,0,0,0,818,817,1,0,0,0,819,202,1,
		0,0,0,820,828,3,217,108,0,821,828,3,219,109,0,822,828,3,221,110,0,823,
		828,3,223,111,0,824,828,3,225,112,0,825,828,3,227,113,0,826,828,3,213,
		106,0,827,820,1,0,0,0,827,821,1,0,0,0,827,822,1,0,0,0,827,823,1,0,0,0,
		827,824,1,0,0,0,827,825,1,0,0,0,827,826,1,0,0,0,828,204,1,0,0,0,829,832,
		3,237,118,0,830,832,3,213,106,0,831,829,1,0,0,0,831,830,1,0,0,0,832,206,
		1,0,0,0,833,836,3,235,117,0,834,836,3,213,106,0,835,833,1,0,0,0,835,834,
		1,0,0,0,836,208,1,0,0,0,837,841,3,229,114,0,838,841,3,231,115,0,839,841,
		3,213,106,0,840,837,1,0,0,0,840,838,1,0,0,0,840,839,1,0,0,0,841,210,1,
		0,0,0,842,845,3,233,116,0,843,845,3,213,106,0,844,842,1,0,0,0,844,843,
		1,0,0,0,845,212,1,0,0,0,846,847,5,94,0,0,847,848,5,117,0,0,848,849,1,0,
		0,0,849,850,3,215,107,0,850,851,3,215,107,0,851,852,3,215,107,0,852,853,
		3,215,107,0,853,867,1,0,0,0,854,855,5,94,0,0,855,856,5,85,0,0,856,857,
		1,0,0,0,857,858,3,215,107,0,858,859,3,215,107,0,859,860,3,215,107,0,860,
		861,3,215,107,0,861,862,3,215,107,0,862,863,3,215,107,0,863,864,3,215,
		107,0,864,865,3,215,107,0,865,867,1,0,0,0,866,846,1,0,0,0,866,854,1,0,
		0,0,867,214,1,0,0,0,868,870,7,17,0,0,869,868,1,0,0,0,870,216,1,0,0,0,871,
		872,7,18,0,0,872,218,1,0,0,0,873,874,7,19,0,0,874,220,1,0,0,0,875,876,
		7,20,0,0,876,222,1,0,0,0,877,878,7,21,0,0,878,224,1,0,0,0,879,880,7,22,
		0,0,880,226,1,0,0,0,881,882,7,23,0,0,882,228,1,0,0,0,883,884,2,768,784,
		0,884,230,1,0,0,0,885,886,7,24,0,0,886,232,1,0,0,0,887,888,7,25,0,0,888,
		234,1,0,0,0,889,890,7,26,0,0,890,236,1,0,0,0,891,892,7,27,0,0,892,238,
		1,0,0,0,55,0,242,251,257,268,551,555,559,565,571,575,579,586,592,595,599,
		606,612,615,619,625,631,634,641,647,651,654,660,666,672,674,676,681,688,
		690,701,703,719,723,729,735,741,765,792,796,806,811,818,827,831,835,840,
		844,866,869,3,1,0,0,1,1,1,0,2,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace LoschScript.Parser
