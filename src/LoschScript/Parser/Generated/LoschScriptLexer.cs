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
		Exclamation_Mark=42, At_Sign=43, Dollar_Sign=44, Caret=45, Percent_Caret=46, 
		Bar=47, Double_Bar=48, Bar_Equals=49, Double_Bar_Equals=50, Ampersand=51, 
		Double_Ampersand=52, Double_Ampersand_Equals=53, Ampersand_Equals=54, 
		Less_Than=55, Greater_Than=56, Less_Equals=57, Greater_Equals=58, Double_Equals=59, 
		Exclamation_Equals=60, Plus=61, Plus_Equals=62, Minus=63, Minus_Equals=64, 
		Asterisk=65, Asterisk_Equals=66, Double_Asterisk=67, Double_Asterisk_Equals=68, 
		Slash=69, Slash_Equals=70, Percent=71, Percent_Equals=72, Equals=73, Tilde=74, 
		Tilde_Equals=75, Double_Less_Than=76, Double_Less_Than_Equals=77, Double_Greater_Than=78, 
		Double_Greater_Than_Equals=79, Arrow_Right=80, Arrow_Left=81, Double_Backtick=82, 
		Identifier=83, Integer_Literal=84, Hex_Integer_Literal=85, Binary_Integer_Literal=86, 
		Real_Literal=87, Character_Literal=88, String_Literal=89, Verbatim_String_Literal=90;
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
		"Double_Quote", "Question_Mark", "Exclamation_Mark", "At_Sign", "Dollar_Sign", 
		"Caret", "Percent_Caret", "Bar", "Double_Bar", "Bar_Equals", "Double_Bar_Equals", 
		"Ampersand", "Double_Ampersand", "Double_Ampersand_Equals", "Ampersand_Equals", 
		"Less_Than", "Greater_Than", "Less_Equals", "Greater_Equals", "Double_Equals", 
		"Exclamation_Equals", "Plus", "Plus_Equals", "Minus", "Minus_Equals", 
		"Asterisk", "Asterisk_Equals", "Double_Asterisk", "Double_Asterisk_Equals", 
		"Slash", "Slash_Equals", "Percent", "Percent_Equals", "Equals", "Tilde", 
		"Tilde_Equals", "Double_Less_Than", "Double_Less_Than_Equals", "Double_Greater_Than", 
		"Double_Greater_Than_Equals", "Arrow_Right", "Arrow_Left", "Double_Backtick", 
		"Identifier", "Integer_Literal", "Hex_Integer_Literal", "Binary_Integer_Literal", 
		"Real_Literal", "Character_Literal", "String_Literal", "Verbatim_String_Literal", 
		"Integer_Suffix", "ExponentPart", "CommonCharacter", "SimpleEscapeSequence", 
		"HexEscapeSequence", "Whitespace", "UnicodeClassZS", "InputCharacter", 
		"IdentifierOrKeyword", "IdentifierStartCharacter", "IdentifierPartCharacter", 
		"LetterCharacter", "DecimalDigitCharacter", "ConnectingCharacter", "CombiningCharacter", 
		"FormattingCharacter", "UnicodeEscapeSequence", "HexDigit", "UnicodeClassLU", 
		"UnicodeClassLL", "UnicodeClassLT", "UnicodeClassLM", "UnicodeClassLO", 
		"UnicodeClassNL", "UnicodeClassMN", "UnicodeClassMC", "UnicodeClassCF", 
		"UnicodeClassPC", "UnicodeClassND"
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
		"'@'", "'$'", "'^'", "'%^'", "'|'", "'||'", "'|='", "'||='", "'&'", "'&&'", 
		"'&&='", "'&='", "'<'", "'>'", "'<='", "'>='", "'=='", "'!='", "'+'", 
		"'+='", "'-'", "'-='", "'*'", "'*='", "'**'", "'**='", "'/'", "'/='", 
		"'%'", "'%='", "'='", "'~'", "'~='", "'<<'", "'<<='", "'>>'", "'>>='", 
		"'->'", "'<-'", "'``'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "INDENT", "DEDENT", "Ws", "NewLine", "Single_Line_Comment", "Delimited_Comment", 
		"Import", "Export", "Assembly", "Type", "Module", "Global", "Local", "Internal", 
		"Static", "Protected", "Sealed", "Infix", "Inline", "Var", "Val", "True", 
		"False", "Of", "Open_Paren", "Close_Paren", "Open_Bracket", "Close_Bracket", 
		"Open_Brace", "Close_Brace", "Comma", "Double_Comma", "Dot", "Double_Dot", 
		"Double_Dot_Question_Mark", "Dot_Equals", "Colon", "Underscore", "Single_Quote", 
		"Double_Quote", "Question_Mark", "Exclamation_Mark", "At_Sign", "Dollar_Sign", 
		"Caret", "Percent_Caret", "Bar", "Double_Bar", "Bar_Equals", "Double_Bar_Equals", 
		"Ampersand", "Double_Ampersand", "Double_Ampersand_Equals", "Ampersand_Equals", 
		"Less_Than", "Greater_Than", "Less_Equals", "Greater_Equals", "Double_Equals", 
		"Exclamation_Equals", "Plus", "Plus_Equals", "Minus", "Minus_Equals", 
		"Asterisk", "Asterisk_Equals", "Double_Asterisk", "Double_Asterisk_Equals", 
		"Slash", "Slash_Equals", "Percent", "Percent_Equals", "Equals", "Tilde", 
		"Tilde_Equals", "Double_Less_Than", "Double_Less_Than_Equals", "Double_Greater_Than", 
		"Double_Greater_Than_Equals", "Arrow_Right", "Arrow_Left", "Double_Backtick", 
		"Identifier", "Integer_Literal", "Hex_Integer_Literal", "Binary_Integer_Literal", 
		"Real_Literal", "Character_Literal", "String_Literal", "Verbatim_String_Literal"
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
		4,0,90,883,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
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
		7,116,1,0,4,0,237,8,0,11,0,12,0,238,1,0,1,0,1,1,1,1,1,1,1,1,1,1,3,1,248,
		8,1,1,2,1,2,5,2,252,8,2,10,2,12,2,255,9,2,1,2,1,2,1,3,1,3,1,3,1,3,5,3,
		263,8,3,10,3,12,3,266,9,3,1,3,1,3,1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,4,1,4,
		1,4,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,
		7,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,9,1,9,
		1,9,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,
		11,1,11,1,12,1,12,1,12,1,12,1,12,1,12,1,12,1,13,1,13,1,13,1,13,1,13,1,
		13,1,13,1,13,1,13,1,13,1,14,1,14,1,14,1,14,1,14,1,14,1,14,1,15,1,15,1,
		15,1,15,1,15,1,15,1,16,1,16,1,16,1,16,1,16,1,16,1,16,1,17,1,17,1,17,1,
		17,1,18,1,18,1,18,1,18,1,19,1,19,1,19,1,19,1,19,1,20,1,20,1,20,1,20,1,
		20,1,20,1,21,1,21,1,21,1,22,1,22,1,23,1,23,1,24,1,24,1,25,1,25,1,26,1,
		26,1,27,1,27,1,28,1,28,1,29,1,29,1,29,1,30,1,30,1,31,1,31,1,31,1,32,1,
		32,1,32,1,32,1,33,1,33,1,33,1,34,1,34,1,35,1,35,1,36,1,36,1,37,1,37,1,
		38,1,38,1,39,1,39,1,40,1,40,1,41,1,41,1,42,1,42,1,43,1,43,1,43,1,44,1,
		44,1,45,1,45,1,45,1,46,1,46,1,46,1,47,1,47,1,47,1,47,1,48,1,48,1,49,1,
		49,1,49,1,50,1,50,1,50,1,50,1,51,1,51,1,51,1,52,1,52,1,53,1,53,1,54,1,
		54,1,54,1,55,1,55,1,55,1,56,1,56,1,56,1,57,1,57,1,57,1,58,1,58,1,59,1,
		59,1,59,1,60,1,60,1,61,1,61,1,61,1,62,1,62,1,63,1,63,1,63,1,64,1,64,1,
		64,1,65,1,65,1,65,1,65,1,66,1,66,1,67,1,67,1,67,1,68,1,68,1,69,1,69,1,
		69,1,70,1,70,1,71,1,71,1,72,1,72,1,72,1,73,1,73,1,73,1,74,1,74,1,74,1,
		74,1,75,1,75,1,75,1,76,1,76,1,76,1,76,1,77,1,77,1,77,1,78,1,78,1,78,1,
		79,1,79,1,79,1,80,3,80,542,8,80,1,80,1,80,3,80,546,8,80,1,81,1,81,3,81,
		550,8,81,1,81,1,81,5,81,554,8,81,10,81,12,81,557,9,81,1,81,5,81,560,8,
		81,10,81,12,81,563,9,81,1,81,3,81,566,8,81,1,82,1,82,3,82,570,8,82,1,82,
		1,82,1,82,5,82,575,8,82,10,82,12,82,578,9,82,1,82,4,82,581,8,82,11,82,
		12,82,582,1,82,3,82,586,8,82,1,83,1,83,3,83,590,8,83,1,83,1,83,1,83,5,
		83,595,8,83,10,83,12,83,598,9,83,1,83,4,83,601,8,83,11,83,12,83,602,1,
		83,3,83,606,8,83,1,84,1,84,3,84,610,8,84,1,84,1,84,5,84,614,8,84,10,84,
		12,84,617,9,84,1,84,5,84,620,8,84,10,84,12,84,623,9,84,3,84,625,8,84,1,
		84,1,84,1,84,5,84,630,8,84,10,84,12,84,633,9,84,1,84,5,84,636,8,84,10,
		84,12,84,639,9,84,1,84,3,84,642,8,84,1,84,3,84,645,8,84,1,84,1,84,5,84,
		649,8,84,10,84,12,84,652,9,84,1,84,5,84,655,8,84,10,84,12,84,658,9,84,
		1,84,1,84,1,84,3,84,663,8,84,3,84,665,8,84,3,84,667,8,84,1,85,1,85,1,85,
		3,85,672,8,85,1,85,1,85,1,86,1,86,1,86,5,86,679,8,86,10,86,12,86,682,9,
		86,1,86,1,86,1,87,1,87,1,87,1,87,1,87,1,87,5,87,692,8,87,10,87,12,87,695,
		9,87,1,87,1,87,1,88,1,88,1,88,1,88,1,88,1,88,1,88,1,88,1,88,1,88,1,88,
		3,88,710,8,88,1,89,1,89,3,89,714,8,89,1,89,1,89,5,89,718,8,89,10,89,12,
		89,721,9,89,1,89,5,89,724,8,89,10,89,12,89,727,9,89,1,90,1,90,1,90,3,90,
		732,8,90,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,
		91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,1,91,3,91,756,8,91,1,92,1,92,
		1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,
		1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,1,92,3,92,783,8,92,1,93,1,93,3,
		93,787,8,93,1,94,1,94,1,95,1,95,1,96,1,96,5,96,795,8,96,10,96,12,96,798,
		9,96,1,97,1,97,3,97,802,8,97,1,98,1,98,1,98,1,98,1,98,3,98,809,8,98,1,
		99,1,99,1,99,1,99,1,99,1,99,1,99,3,99,818,8,99,1,100,1,100,3,100,822,8,
		100,1,101,1,101,3,101,826,8,101,1,102,1,102,1,102,3,102,831,8,102,1,103,
		1,103,3,103,835,8,103,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,
		1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,1,104,
		3,104,857,8,104,1,105,3,105,860,8,105,1,106,1,106,1,107,1,107,1,108,1,
		108,1,109,1,109,1,110,1,110,1,111,1,111,1,112,1,112,1,113,1,113,1,114,
		1,114,1,115,1,115,1,116,1,116,1,264,0,117,1,3,3,4,5,5,7,6,9,7,11,8,13,
		9,15,10,17,11,19,12,21,13,23,14,25,15,27,16,29,17,31,18,33,19,35,20,37,
		21,39,22,41,23,43,24,45,25,47,26,49,27,51,28,53,29,55,30,57,31,59,32,61,
		33,63,34,65,35,67,36,69,37,71,38,73,39,75,40,77,41,79,42,81,43,83,44,85,
		45,87,46,89,47,91,48,93,49,95,50,97,51,99,52,101,53,103,54,105,55,107,
		56,109,57,111,58,113,59,115,60,117,61,119,62,121,63,123,64,125,65,127,
		66,129,67,131,68,133,69,135,70,137,71,139,72,141,73,143,74,145,75,147,
		76,149,77,151,78,153,79,155,80,157,81,159,82,161,83,163,84,165,85,167,
		86,169,87,171,88,173,89,175,90,177,0,179,0,181,0,183,0,185,0,187,0,189,
		0,191,0,193,0,195,0,197,0,199,0,201,0,203,0,205,0,207,0,209,0,211,0,213,
		0,215,0,217,0,219,0,221,0,223,0,225,0,227,0,229,0,231,0,233,0,1,0,28,2,
		0,9,9,32,32,4,0,10,10,13,13,133,133,8232,8232,1,0,48,57,2,0,88,88,120,
		120,2,0,66,66,98,98,1,0,48,49,3,0,100,100,109,109,115,115,6,0,10,10,13,
		13,39,39,92,92,133,133,8232,8233,6,0,10,10,13,13,34,34,92,92,133,133,8232,
		8233,1,0,34,34,2,0,98,98,115,115,2,0,108,108,117,117,2,0,69,69,101,101,
		2,0,43,43,45,45,2,0,9,9,11,12,9,0,32,32,160,160,5760,5760,6158,6158,8192,
		8198,8200,8202,8239,8239,8287,8287,12288,12288,4,0,10,10,13,13,133,133,
		8232,8233,3,0,48,57,65,70,97,102,82,0,65,90,192,214,216,222,256,310,313,
		327,330,381,385,386,388,395,398,401,403,404,406,408,412,413,415,416,418,
		425,428,435,437,444,452,461,463,475,478,494,497,500,502,504,506,562,570,
		571,573,574,577,582,584,590,880,882,886,895,902,906,908,929,931,939,975,
		980,984,1006,1012,1015,1017,1018,1021,1071,1120,1152,1162,1229,1232,1326,
		1329,1366,4256,4293,4295,4301,7680,7828,7838,7934,7944,7951,7960,7965,
		7976,7983,7992,7999,8008,8013,8025,8031,8040,8047,8120,8123,8136,8139,
		8152,8155,8168,8172,8184,8187,8450,8455,8459,8461,8464,8466,8469,8477,
		8484,8493,8496,8499,8510,8511,8517,8579,11264,11310,11360,11364,11367,
		11376,11378,11381,11390,11392,11394,11490,11499,11501,11506,42560,42562,
		42604,42624,42650,42786,42798,42802,42862,42873,42886,42891,42893,42896,
		42898,42902,42925,42928,42929,65313,65338,81,0,97,122,181,246,248,255,
		257,375,378,384,387,389,392,402,405,411,414,417,419,421,424,429,432,436,
		438,447,454,460,462,499,501,505,507,569,572,578,583,659,661,687,881,883,
		887,893,912,974,976,977,981,983,985,1011,1013,1119,1121,1153,1163,1215,
		1218,1327,1377,1415,7424,7467,7531,7543,7545,7578,7681,7837,7839,7943,
		7952,7957,7968,7975,7984,7991,8000,8005,8016,8023,8032,8039,8048,8061,
		8064,8071,8080,8087,8096,8103,8112,8116,8118,8119,8126,8132,8134,8135,
		8144,8147,8150,8151,8160,8167,8178,8180,8182,8183,8458,8467,8495,8505,
		8508,8509,8518,8521,8526,8580,11312,11358,11361,11372,11377,11387,11393,
		11500,11502,11507,11520,11557,11559,11565,42561,42605,42625,42651,42787,
		42801,42803,42872,42874,42876,42879,42887,42892,42894,42897,42901,42903,
		42921,43002,43866,43876,43877,64256,64262,64275,64279,65345,65370,6,0,
		453,459,498,8079,8088,8095,8104,8111,8124,8140,8188,8188,33,0,688,705,
		710,721,736,740,748,750,884,890,1369,1600,1765,1766,2036,2037,2042,2074,
		2084,2088,2417,3654,3782,4348,6103,6211,6823,7293,7468,7530,7544,7615,
		8305,8319,8336,8348,11388,11389,11631,11823,12293,12341,12347,12542,40981,
		42237,42508,42623,42652,42653,42775,42783,42864,42888,43000,43001,43471,
		43494,43632,43741,43763,43764,43868,43871,65392,65439,234,0,170,186,443,
		451,660,1514,1520,1522,1568,1599,1601,1610,1646,1647,1649,1747,1749,1788,
		1791,1808,1810,1839,1869,1957,1969,2026,2048,2069,2112,2136,2208,2226,
		2308,2361,2365,2384,2392,2401,2418,2432,2437,2444,2447,2448,2451,2472,
		2474,2480,2482,2489,2493,2510,2524,2525,2527,2529,2544,2545,2565,2570,
		2575,2576,2579,2600,2602,2608,2610,2611,2613,2614,2616,2617,2649,2652,
		2654,2676,2693,2701,2703,2705,2707,2728,2730,2736,2738,2739,2741,2745,
		2749,2768,2784,2785,2821,2828,2831,2832,2835,2856,2858,2864,2866,2867,
		2869,2873,2877,2913,2929,2947,2949,2954,2958,2960,2962,2965,2969,2970,
		2972,2986,2990,3001,3024,3084,3086,3088,3090,3112,3114,3129,3133,3212,
		3214,3216,3218,3240,3242,3251,3253,3257,3261,3294,3296,3297,3313,3314,
		3333,3340,3342,3344,3346,3386,3389,3406,3424,3425,3450,3455,3461,3478,
		3482,3505,3507,3515,3517,3526,3585,3632,3634,3635,3648,3653,3713,3714,
		3716,3722,3725,3735,3737,3743,3745,3747,3749,3751,3754,3755,3757,3760,
		3762,3763,3773,3780,3804,3807,3840,3911,3913,3948,3976,3980,4096,4138,
		4159,4181,4186,4189,4193,4208,4213,4225,4238,4346,4349,4680,4682,4685,
		4688,4694,4696,4701,4704,4744,4746,4749,4752,4784,4786,4789,4792,4798,
		4800,4805,4808,4822,4824,4880,4882,4885,4888,4954,4992,5007,5024,5108,
		5121,5740,5743,5759,5761,5786,5792,5866,5873,5880,5888,5900,5902,5905,
		5920,5937,5952,5969,5984,5996,5998,6000,6016,6067,6108,6210,6212,6263,
		6272,6312,6314,6389,6400,6430,6480,6509,6512,6516,6528,6571,6593,6599,
		6656,6678,6688,6740,6917,6963,6981,6987,7043,7072,7086,7087,7098,7141,
		7168,7203,7245,7247,7258,7287,7401,7404,7406,7409,7413,7414,8501,8504,
		11568,11623,11648,11670,11680,11686,11688,11694,11696,11702,11704,11710,
		11712,11718,11720,11726,11728,11734,11736,11742,12294,12348,12353,12438,
		12447,12538,12543,12589,12593,12686,12704,12730,12784,12799,13312,19893,
		19968,40908,40960,40980,40982,42124,42192,42231,42240,42507,42512,42527,
		42538,42539,42606,42725,42999,43009,43011,43013,43015,43018,43020,43042,
		43072,43123,43138,43187,43250,43255,43259,43301,43312,43334,43360,43388,
		43396,43442,43488,43492,43495,43503,43514,43518,43520,43560,43584,43586,
		43588,43595,43616,43631,43633,43638,43642,43695,43697,43709,43712,43714,
		43739,43740,43744,43754,43762,43782,43785,43790,43793,43798,43808,43814,
		43816,43822,43968,44002,44032,55203,55216,55238,55243,55291,63744,64109,
		64112,64217,64285,64296,64298,64310,64312,64316,64318,64433,64467,64829,
		64848,64911,64914,64967,65008,65019,65136,65140,65142,65276,65382,65391,
		65393,65437,65440,65470,65474,65479,65482,65487,65490,65495,65498,65500,
		2,0,5870,5872,8544,8559,3,0,2307,2307,2366,2368,2377,2380,3,0,173,173,
		1536,1539,1757,1757,6,0,95,95,8255,8256,8276,8276,65075,65076,65101,65103,
		65343,65343,37,0,48,57,1632,1641,1776,1785,1984,1993,2406,2415,2534,2543,
		2662,2671,2790,2799,2918,2927,3046,3055,3174,3183,3302,3311,3430,3439,
		3558,3567,3664,3673,3792,3801,3872,3881,4160,4169,4240,4249,6112,6121,
		6160,6169,6470,6479,6608,6617,6784,6793,6800,6809,6992,7001,7088,7097,
		7232,7241,7248,7257,42528,42537,43216,43225,43264,43273,43472,43481,43504,
		43513,43600,43609,44016,44025,65296,65305,937,0,1,1,0,0,0,0,3,1,0,0,0,
		0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,
		0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,
		27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,
		0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,
		0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,
		1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,
		0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,
		1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,1,0,0,0,0,91,1,0,0,
		0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,0,0,101,1,0,0,0,0,
		103,1,0,0,0,0,105,1,0,0,0,0,107,1,0,0,0,0,109,1,0,0,0,0,111,1,0,0,0,0,
		113,1,0,0,0,0,115,1,0,0,0,0,117,1,0,0,0,0,119,1,0,0,0,0,121,1,0,0,0,0,
		123,1,0,0,0,0,125,1,0,0,0,0,127,1,0,0,0,0,129,1,0,0,0,0,131,1,0,0,0,0,
		133,1,0,0,0,0,135,1,0,0,0,0,137,1,0,0,0,0,139,1,0,0,0,0,141,1,0,0,0,0,
		143,1,0,0,0,0,145,1,0,0,0,0,147,1,0,0,0,0,149,1,0,0,0,0,151,1,0,0,0,0,
		153,1,0,0,0,0,155,1,0,0,0,0,157,1,0,0,0,0,159,1,0,0,0,0,161,1,0,0,0,0,
		163,1,0,0,0,0,165,1,0,0,0,0,167,1,0,0,0,0,169,1,0,0,0,0,171,1,0,0,0,0,
		173,1,0,0,0,0,175,1,0,0,0,1,236,1,0,0,0,3,247,1,0,0,0,5,249,1,0,0,0,7,
		258,1,0,0,0,9,272,1,0,0,0,11,279,1,0,0,0,13,286,1,0,0,0,15,295,1,0,0,0,
		17,300,1,0,0,0,19,307,1,0,0,0,21,314,1,0,0,0,23,320,1,0,0,0,25,329,1,0,
		0,0,27,336,1,0,0,0,29,346,1,0,0,0,31,353,1,0,0,0,33,359,1,0,0,0,35,366,
		1,0,0,0,37,370,1,0,0,0,39,374,1,0,0,0,41,379,1,0,0,0,43,385,1,0,0,0,45,
		388,1,0,0,0,47,390,1,0,0,0,49,392,1,0,0,0,51,394,1,0,0,0,53,396,1,0,0,
		0,55,398,1,0,0,0,57,400,1,0,0,0,59,402,1,0,0,0,61,405,1,0,0,0,63,407,1,
		0,0,0,65,410,1,0,0,0,67,414,1,0,0,0,69,417,1,0,0,0,71,419,1,0,0,0,73,421,
		1,0,0,0,75,423,1,0,0,0,77,425,1,0,0,0,79,427,1,0,0,0,81,429,1,0,0,0,83,
		431,1,0,0,0,85,433,1,0,0,0,87,435,1,0,0,0,89,438,1,0,0,0,91,440,1,0,0,
		0,93,443,1,0,0,0,95,446,1,0,0,0,97,450,1,0,0,0,99,452,1,0,0,0,101,455,
		1,0,0,0,103,459,1,0,0,0,105,462,1,0,0,0,107,464,1,0,0,0,109,466,1,0,0,
		0,111,469,1,0,0,0,113,472,1,0,0,0,115,475,1,0,0,0,117,478,1,0,0,0,119,
		480,1,0,0,0,121,483,1,0,0,0,123,485,1,0,0,0,125,488,1,0,0,0,127,490,1,
		0,0,0,129,493,1,0,0,0,131,496,1,0,0,0,133,500,1,0,0,0,135,502,1,0,0,0,
		137,505,1,0,0,0,139,507,1,0,0,0,141,510,1,0,0,0,143,512,1,0,0,0,145,514,
		1,0,0,0,147,517,1,0,0,0,149,520,1,0,0,0,151,524,1,0,0,0,153,527,1,0,0,
		0,155,531,1,0,0,0,157,534,1,0,0,0,159,537,1,0,0,0,161,541,1,0,0,0,163,
		549,1,0,0,0,165,569,1,0,0,0,167,589,1,0,0,0,169,666,1,0,0,0,171,668,1,
		0,0,0,173,675,1,0,0,0,175,685,1,0,0,0,177,709,1,0,0,0,179,711,1,0,0,0,
		181,731,1,0,0,0,183,755,1,0,0,0,185,782,1,0,0,0,187,786,1,0,0,0,189,788,
		1,0,0,0,191,790,1,0,0,0,193,792,1,0,0,0,195,801,1,0,0,0,197,808,1,0,0,
		0,199,817,1,0,0,0,201,821,1,0,0,0,203,825,1,0,0,0,205,830,1,0,0,0,207,
		834,1,0,0,0,209,856,1,0,0,0,211,859,1,0,0,0,213,861,1,0,0,0,215,863,1,
		0,0,0,217,865,1,0,0,0,219,867,1,0,0,0,221,869,1,0,0,0,223,871,1,0,0,0,
		225,873,1,0,0,0,227,875,1,0,0,0,229,877,1,0,0,0,231,879,1,0,0,0,233,881,
		1,0,0,0,235,237,7,0,0,0,236,235,1,0,0,0,237,238,1,0,0,0,238,236,1,0,0,
		0,238,239,1,0,0,0,239,240,1,0,0,0,240,241,6,0,0,0,241,2,1,0,0,0,242,243,
		5,13,0,0,243,248,5,10,0,0,244,248,7,1,0,0,245,246,5,8233,0,0,246,248,6,
		1,1,0,247,242,1,0,0,0,247,244,1,0,0,0,247,245,1,0,0,0,248,4,1,0,0,0,249,
		253,5,35,0,0,250,252,3,191,95,0,251,250,1,0,0,0,252,255,1,0,0,0,253,251,
		1,0,0,0,253,254,1,0,0,0,254,256,1,0,0,0,255,253,1,0,0,0,256,257,6,2,2,
		0,257,6,1,0,0,0,258,259,5,35,0,0,259,260,5,91,0,0,260,264,1,0,0,0,261,
		263,9,0,0,0,262,261,1,0,0,0,263,266,1,0,0,0,264,265,1,0,0,0,264,262,1,
		0,0,0,265,267,1,0,0,0,266,264,1,0,0,0,267,268,5,93,0,0,268,269,5,35,0,
		0,269,270,1,0,0,0,270,271,6,3,2,0,271,8,1,0,0,0,272,273,5,105,0,0,273,
		274,5,109,0,0,274,275,5,112,0,0,275,276,5,111,0,0,276,277,5,114,0,0,277,
		278,5,116,0,0,278,10,1,0,0,0,279,280,5,101,0,0,280,281,5,120,0,0,281,282,
		5,112,0,0,282,283,5,111,0,0,283,284,5,114,0,0,284,285,5,116,0,0,285,12,
		1,0,0,0,286,287,5,97,0,0,287,288,5,115,0,0,288,289,5,115,0,0,289,290,5,
		101,0,0,290,291,5,109,0,0,291,292,5,98,0,0,292,293,5,108,0,0,293,294,5,
		121,0,0,294,14,1,0,0,0,295,296,5,116,0,0,296,297,5,121,0,0,297,298,5,112,
		0,0,298,299,5,101,0,0,299,16,1,0,0,0,300,301,5,109,0,0,301,302,5,111,0,
		0,302,303,5,100,0,0,303,304,5,117,0,0,304,305,5,108,0,0,305,306,5,101,
		0,0,306,18,1,0,0,0,307,308,5,103,0,0,308,309,5,108,0,0,309,310,5,111,0,
		0,310,311,5,98,0,0,311,312,5,97,0,0,312,313,5,108,0,0,313,20,1,0,0,0,314,
		315,5,108,0,0,315,316,5,111,0,0,316,317,5,99,0,0,317,318,5,97,0,0,318,
		319,5,108,0,0,319,22,1,0,0,0,320,321,5,105,0,0,321,322,5,110,0,0,322,323,
		5,116,0,0,323,324,5,101,0,0,324,325,5,114,0,0,325,326,5,110,0,0,326,327,
		5,97,0,0,327,328,5,108,0,0,328,24,1,0,0,0,329,330,5,115,0,0,330,331,5,
		116,0,0,331,332,5,97,0,0,332,333,5,116,0,0,333,334,5,105,0,0,334,335,5,
		99,0,0,335,26,1,0,0,0,336,337,5,112,0,0,337,338,5,114,0,0,338,339,5,111,
		0,0,339,340,5,116,0,0,340,341,5,101,0,0,341,342,5,99,0,0,342,343,5,116,
		0,0,343,344,5,101,0,0,344,345,5,100,0,0,345,28,1,0,0,0,346,347,5,115,0,
		0,347,348,5,101,0,0,348,349,5,97,0,0,349,350,5,108,0,0,350,351,5,101,0,
		0,351,352,5,100,0,0,352,30,1,0,0,0,353,354,5,105,0,0,354,355,5,110,0,0,
		355,356,5,102,0,0,356,357,5,105,0,0,357,358,5,120,0,0,358,32,1,0,0,0,359,
		360,5,105,0,0,360,361,5,110,0,0,361,362,5,108,0,0,362,363,5,105,0,0,363,
		364,5,110,0,0,364,365,5,101,0,0,365,34,1,0,0,0,366,367,5,118,0,0,367,368,
		5,97,0,0,368,369,5,114,0,0,369,36,1,0,0,0,370,371,5,118,0,0,371,372,5,
		97,0,0,372,373,5,108,0,0,373,38,1,0,0,0,374,375,5,116,0,0,375,376,5,114,
		0,0,376,377,5,117,0,0,377,378,5,101,0,0,378,40,1,0,0,0,379,380,5,102,0,
		0,380,381,5,97,0,0,381,382,5,108,0,0,382,383,5,115,0,0,383,384,5,101,0,
		0,384,42,1,0,0,0,385,386,5,111,0,0,386,387,5,102,0,0,387,44,1,0,0,0,388,
		389,5,40,0,0,389,46,1,0,0,0,390,391,5,41,0,0,391,48,1,0,0,0,392,393,5,
		91,0,0,393,50,1,0,0,0,394,395,5,93,0,0,395,52,1,0,0,0,396,397,5,123,0,
		0,397,54,1,0,0,0,398,399,5,125,0,0,399,56,1,0,0,0,400,401,5,44,0,0,401,
		58,1,0,0,0,402,403,5,44,0,0,403,404,5,44,0,0,404,60,1,0,0,0,405,406,5,
		46,0,0,406,62,1,0,0,0,407,408,5,46,0,0,408,409,5,46,0,0,409,64,1,0,0,0,
		410,411,5,46,0,0,411,412,5,46,0,0,412,413,5,63,0,0,413,66,1,0,0,0,414,
		415,5,46,0,0,415,416,5,61,0,0,416,68,1,0,0,0,417,418,5,58,0,0,418,70,1,
		0,0,0,419,420,5,95,0,0,420,72,1,0,0,0,421,422,5,39,0,0,422,74,1,0,0,0,
		423,424,5,34,0,0,424,76,1,0,0,0,425,426,5,63,0,0,426,78,1,0,0,0,427,428,
		5,33,0,0,428,80,1,0,0,0,429,430,5,64,0,0,430,82,1,0,0,0,431,432,5,36,0,
		0,432,84,1,0,0,0,433,434,5,94,0,0,434,86,1,0,0,0,435,436,5,37,0,0,436,
		437,5,94,0,0,437,88,1,0,0,0,438,439,5,124,0,0,439,90,1,0,0,0,440,441,5,
		124,0,0,441,442,5,124,0,0,442,92,1,0,0,0,443,444,5,124,0,0,444,445,5,61,
		0,0,445,94,1,0,0,0,446,447,5,124,0,0,447,448,5,124,0,0,448,449,5,61,0,
		0,449,96,1,0,0,0,450,451,5,38,0,0,451,98,1,0,0,0,452,453,5,38,0,0,453,
		454,5,38,0,0,454,100,1,0,0,0,455,456,5,38,0,0,456,457,5,38,0,0,457,458,
		5,61,0,0,458,102,1,0,0,0,459,460,5,38,0,0,460,461,5,61,0,0,461,104,1,0,
		0,0,462,463,5,60,0,0,463,106,1,0,0,0,464,465,5,62,0,0,465,108,1,0,0,0,
		466,467,5,60,0,0,467,468,5,61,0,0,468,110,1,0,0,0,469,470,5,62,0,0,470,
		471,5,61,0,0,471,112,1,0,0,0,472,473,5,61,0,0,473,474,5,61,0,0,474,114,
		1,0,0,0,475,476,5,33,0,0,476,477,5,61,0,0,477,116,1,0,0,0,478,479,5,43,
		0,0,479,118,1,0,0,0,480,481,5,43,0,0,481,482,5,61,0,0,482,120,1,0,0,0,
		483,484,5,45,0,0,484,122,1,0,0,0,485,486,5,45,0,0,486,487,5,61,0,0,487,
		124,1,0,0,0,488,489,5,42,0,0,489,126,1,0,0,0,490,491,5,42,0,0,491,492,
		5,61,0,0,492,128,1,0,0,0,493,494,5,42,0,0,494,495,5,42,0,0,495,130,1,0,
		0,0,496,497,5,42,0,0,497,498,5,42,0,0,498,499,5,61,0,0,499,132,1,0,0,0,
		500,501,5,47,0,0,501,134,1,0,0,0,502,503,5,47,0,0,503,504,5,61,0,0,504,
		136,1,0,0,0,505,506,5,37,0,0,506,138,1,0,0,0,507,508,5,37,0,0,508,509,
		5,61,0,0,509,140,1,0,0,0,510,511,5,61,0,0,511,142,1,0,0,0,512,513,5,126,
		0,0,513,144,1,0,0,0,514,515,5,126,0,0,515,516,5,61,0,0,516,146,1,0,0,0,
		517,518,5,60,0,0,518,519,5,60,0,0,519,148,1,0,0,0,520,521,5,60,0,0,521,
		522,5,60,0,0,522,523,5,61,0,0,523,150,1,0,0,0,524,525,5,62,0,0,525,526,
		5,62,0,0,526,152,1,0,0,0,527,528,5,62,0,0,528,529,5,62,0,0,529,530,5,61,
		0,0,530,154,1,0,0,0,531,532,5,45,0,0,532,533,5,62,0,0,533,156,1,0,0,0,
		534,535,5,60,0,0,535,536,5,45,0,0,536,158,1,0,0,0,537,538,5,96,0,0,538,
		539,5,96,0,0,539,160,1,0,0,0,540,542,3,159,79,0,541,540,1,0,0,0,541,542,
		1,0,0,0,542,543,1,0,0,0,543,545,3,193,96,0,544,546,3,159,79,0,545,544,
		1,0,0,0,545,546,1,0,0,0,546,162,1,0,0,0,547,550,3,121,60,0,548,550,3,117,
		58,0,549,547,1,0,0,0,549,548,1,0,0,0,549,550,1,0,0,0,550,551,1,0,0,0,551,
		561,7,2,0,0,552,554,5,39,0,0,553,552,1,0,0,0,554,557,1,0,0,0,555,553,1,
		0,0,0,555,556,1,0,0,0,556,558,1,0,0,0,557,555,1,0,0,0,558,560,7,2,0,0,
		559,555,1,0,0,0,560,563,1,0,0,0,561,559,1,0,0,0,561,562,1,0,0,0,562,565,
		1,0,0,0,563,561,1,0,0,0,564,566,3,177,88,0,565,564,1,0,0,0,565,566,1,0,
		0,0,566,164,1,0,0,0,567,570,3,121,60,0,568,570,3,117,58,0,569,567,1,0,
		0,0,569,568,1,0,0,0,569,570,1,0,0,0,570,571,1,0,0,0,571,572,5,48,0,0,572,
		580,7,3,0,0,573,575,5,39,0,0,574,573,1,0,0,0,575,578,1,0,0,0,576,574,1,
		0,0,0,576,577,1,0,0,0,577,579,1,0,0,0,578,576,1,0,0,0,579,581,3,211,105,
		0,580,576,1,0,0,0,581,582,1,0,0,0,582,580,1,0,0,0,582,583,1,0,0,0,583,
		585,1,0,0,0,584,586,3,177,88,0,585,584,1,0,0,0,585,586,1,0,0,0,586,166,
		1,0,0,0,587,590,3,121,60,0,588,590,3,117,58,0,589,587,1,0,0,0,589,588,
		1,0,0,0,589,590,1,0,0,0,590,591,1,0,0,0,591,592,5,48,0,0,592,600,7,4,0,
		0,593,595,5,39,0,0,594,593,1,0,0,0,595,598,1,0,0,0,596,594,1,0,0,0,596,
		597,1,0,0,0,597,599,1,0,0,0,598,596,1,0,0,0,599,601,7,5,0,0,600,596,1,
		0,0,0,601,602,1,0,0,0,602,600,1,0,0,0,602,603,1,0,0,0,603,605,1,0,0,0,
		604,606,3,177,88,0,605,604,1,0,0,0,605,606,1,0,0,0,606,168,1,0,0,0,607,
		610,3,121,60,0,608,610,3,117,58,0,609,607,1,0,0,0,609,608,1,0,0,0,609,
		610,1,0,0,0,610,624,1,0,0,0,611,621,7,2,0,0,612,614,5,39,0,0,613,612,1,
		0,0,0,614,617,1,0,0,0,615,613,1,0,0,0,615,616,1,0,0,0,616,618,1,0,0,0,
		617,615,1,0,0,0,618,620,7,2,0,0,619,615,1,0,0,0,620,623,1,0,0,0,621,619,
		1,0,0,0,621,622,1,0,0,0,622,625,1,0,0,0,623,621,1,0,0,0,624,611,1,0,0,
		0,624,625,1,0,0,0,625,626,1,0,0,0,626,627,5,46,0,0,627,637,7,2,0,0,628,
		630,5,39,0,0,629,628,1,0,0,0,630,633,1,0,0,0,631,629,1,0,0,0,631,632,1,
		0,0,0,632,634,1,0,0,0,633,631,1,0,0,0,634,636,7,2,0,0,635,631,1,0,0,0,
		636,639,1,0,0,0,637,635,1,0,0,0,637,638,1,0,0,0,638,641,1,0,0,0,639,637,
		1,0,0,0,640,642,3,179,89,0,641,640,1,0,0,0,641,642,1,0,0,0,642,644,1,0,
		0,0,643,645,7,6,0,0,644,643,1,0,0,0,644,645,1,0,0,0,645,667,1,0,0,0,646,
		656,7,2,0,0,647,649,5,39,0,0,648,647,1,0,0,0,649,652,1,0,0,0,650,648,1,
		0,0,0,650,651,1,0,0,0,651,653,1,0,0,0,652,650,1,0,0,0,653,655,7,2,0,0,
		654,650,1,0,0,0,655,658,1,0,0,0,656,654,1,0,0,0,656,657,1,0,0,0,657,664,
		1,0,0,0,658,656,1,0,0,0,659,665,7,6,0,0,660,662,3,179,89,0,661,663,7,6,
		0,0,662,661,1,0,0,0,662,663,1,0,0,0,663,665,1,0,0,0,664,659,1,0,0,0,664,
		660,1,0,0,0,665,667,1,0,0,0,666,609,1,0,0,0,666,646,1,0,0,0,667,170,1,
		0,0,0,668,671,5,39,0,0,669,672,8,7,0,0,670,672,3,181,90,0,671,669,1,0,
		0,0,671,670,1,0,0,0,672,673,1,0,0,0,673,674,5,39,0,0,674,172,1,0,0,0,675,
		680,5,34,0,0,676,679,8,8,0,0,677,679,3,181,90,0,678,676,1,0,0,0,678,677,
		1,0,0,0,679,682,1,0,0,0,680,678,1,0,0,0,680,681,1,0,0,0,681,683,1,0,0,
		0,682,680,1,0,0,0,683,684,5,34,0,0,684,174,1,0,0,0,685,686,5,94,0,0,686,
		687,5,34,0,0,687,693,1,0,0,0,688,692,8,9,0,0,689,690,5,34,0,0,690,692,
		5,34,0,0,691,688,1,0,0,0,691,689,1,0,0,0,692,695,1,0,0,0,693,691,1,0,0,
		0,693,694,1,0,0,0,694,696,1,0,0,0,695,693,1,0,0,0,696,697,5,34,0,0,697,
		176,1,0,0,0,698,699,5,115,0,0,699,710,5,98,0,0,700,710,7,10,0,0,701,702,
		5,117,0,0,702,710,5,115,0,0,703,710,7,11,0,0,704,705,5,117,0,0,705,710,
		5,108,0,0,706,710,5,110,0,0,707,708,5,117,0,0,708,710,5,110,0,0,709,698,
		1,0,0,0,709,700,1,0,0,0,709,701,1,0,0,0,709,703,1,0,0,0,709,704,1,0,0,
		0,709,706,1,0,0,0,709,707,1,0,0,0,710,178,1,0,0,0,711,713,7,12,0,0,712,
		714,7,13,0,0,713,712,1,0,0,0,713,714,1,0,0,0,714,715,1,0,0,0,715,725,7,
		2,0,0,716,718,5,96,0,0,717,716,1,0,0,0,718,721,1,0,0,0,719,717,1,0,0,0,
		719,720,1,0,0,0,720,722,1,0,0,0,721,719,1,0,0,0,722,724,7,2,0,0,723,719,
		1,0,0,0,724,727,1,0,0,0,725,723,1,0,0,0,725,726,1,0,0,0,726,180,1,0,0,
		0,727,725,1,0,0,0,728,732,3,183,91,0,729,732,3,185,92,0,730,732,3,209,
		104,0,731,728,1,0,0,0,731,729,1,0,0,0,731,730,1,0,0,0,732,182,1,0,0,0,
		733,734,5,94,0,0,734,756,5,39,0,0,735,736,5,94,0,0,736,756,5,34,0,0,737,
		738,5,94,0,0,738,756,5,94,0,0,739,740,5,94,0,0,740,756,5,48,0,0,741,742,
		5,94,0,0,742,756,5,97,0,0,743,744,5,94,0,0,744,756,5,98,0,0,745,746,5,
		94,0,0,746,756,5,102,0,0,747,748,5,94,0,0,748,756,5,110,0,0,749,750,5,
		94,0,0,750,756,5,114,0,0,751,752,5,94,0,0,752,756,5,116,0,0,753,754,5,
		94,0,0,754,756,5,118,0,0,755,733,1,0,0,0,755,735,1,0,0,0,755,737,1,0,0,
		0,755,739,1,0,0,0,755,741,1,0,0,0,755,743,1,0,0,0,755,745,1,0,0,0,755,
		747,1,0,0,0,755,749,1,0,0,0,755,751,1,0,0,0,755,753,1,0,0,0,756,184,1,
		0,0,0,757,758,5,94,0,0,758,759,5,120,0,0,759,760,1,0,0,0,760,783,3,211,
		105,0,761,762,5,94,0,0,762,763,5,120,0,0,763,764,1,0,0,0,764,765,3,211,
		105,0,765,766,3,211,105,0,766,783,1,0,0,0,767,768,5,94,0,0,768,769,5,120,
		0,0,769,770,1,0,0,0,770,771,3,211,105,0,771,772,3,211,105,0,772,773,3,
		211,105,0,773,783,1,0,0,0,774,775,5,94,0,0,775,776,5,120,0,0,776,777,1,
		0,0,0,777,778,3,211,105,0,778,779,3,211,105,0,779,780,3,211,105,0,780,
		781,3,211,105,0,781,783,1,0,0,0,782,757,1,0,0,0,782,761,1,0,0,0,782,767,
		1,0,0,0,782,774,1,0,0,0,783,186,1,0,0,0,784,787,3,189,94,0,785,787,7,14,
		0,0,786,784,1,0,0,0,786,785,1,0,0,0,787,188,1,0,0,0,788,789,7,15,0,0,789,
		190,1,0,0,0,790,791,8,16,0,0,791,192,1,0,0,0,792,796,3,195,97,0,793,795,
		3,197,98,0,794,793,1,0,0,0,795,798,1,0,0,0,796,794,1,0,0,0,796,797,1,0,
		0,0,797,194,1,0,0,0,798,796,1,0,0,0,799,802,3,199,99,0,800,802,5,95,0,
		0,801,799,1,0,0,0,801,800,1,0,0,0,802,196,1,0,0,0,803,809,3,199,99,0,804,
		809,3,201,100,0,805,809,3,203,101,0,806,809,3,205,102,0,807,809,3,207,
		103,0,808,803,1,0,0,0,808,804,1,0,0,0,808,805,1,0,0,0,808,806,1,0,0,0,
		808,807,1,0,0,0,809,198,1,0,0,0,810,818,3,213,106,0,811,818,3,215,107,
		0,812,818,3,217,108,0,813,818,3,219,109,0,814,818,3,221,110,0,815,818,
		3,223,111,0,816,818,3,209,104,0,817,810,1,0,0,0,817,811,1,0,0,0,817,812,
		1,0,0,0,817,813,1,0,0,0,817,814,1,0,0,0,817,815,1,0,0,0,817,816,1,0,0,
		0,818,200,1,0,0,0,819,822,3,233,116,0,820,822,3,209,104,0,821,819,1,0,
		0,0,821,820,1,0,0,0,822,202,1,0,0,0,823,826,3,231,115,0,824,826,3,209,
		104,0,825,823,1,0,0,0,825,824,1,0,0,0,826,204,1,0,0,0,827,831,3,225,112,
		0,828,831,3,227,113,0,829,831,3,209,104,0,830,827,1,0,0,0,830,828,1,0,
		0,0,830,829,1,0,0,0,831,206,1,0,0,0,832,835,3,229,114,0,833,835,3,209,
		104,0,834,832,1,0,0,0,834,833,1,0,0,0,835,208,1,0,0,0,836,837,5,94,0,0,
		837,838,5,117,0,0,838,839,1,0,0,0,839,840,3,211,105,0,840,841,3,211,105,
		0,841,842,3,211,105,0,842,843,3,211,105,0,843,857,1,0,0,0,844,845,5,94,
		0,0,845,846,5,85,0,0,846,847,1,0,0,0,847,848,3,211,105,0,848,849,3,211,
		105,0,849,850,3,211,105,0,850,851,3,211,105,0,851,852,3,211,105,0,852,
		853,3,211,105,0,853,854,3,211,105,0,854,855,3,211,105,0,855,857,1,0,0,
		0,856,836,1,0,0,0,856,844,1,0,0,0,857,210,1,0,0,0,858,860,7,17,0,0,859,
		858,1,0,0,0,860,212,1,0,0,0,861,862,7,18,0,0,862,214,1,0,0,0,863,864,7,
		19,0,0,864,216,1,0,0,0,865,866,7,20,0,0,866,218,1,0,0,0,867,868,7,21,0,
		0,868,220,1,0,0,0,869,870,7,22,0,0,870,222,1,0,0,0,871,872,7,23,0,0,872,
		224,1,0,0,0,873,874,2,768,784,0,874,226,1,0,0,0,875,876,7,24,0,0,876,228,
		1,0,0,0,877,878,7,25,0,0,878,230,1,0,0,0,879,880,7,26,0,0,880,232,1,0,
		0,0,881,882,7,27,0,0,882,234,1,0,0,0,55,0,238,247,253,264,541,545,549,
		555,561,565,569,576,582,585,589,596,602,605,609,615,621,624,631,637,641,
		644,650,656,662,664,666,671,678,680,691,693,709,713,719,725,731,755,782,
		786,796,801,808,817,821,825,830,834,856,859,3,1,0,0,1,1,1,0,2,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace LoschScript.Parser
