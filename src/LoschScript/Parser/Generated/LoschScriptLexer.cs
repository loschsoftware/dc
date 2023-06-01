//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.12.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\Jonas\source\repos\lsc\src\LoschScript\Parser\LoschScriptLexer.g4 by ANTLR 4.12.0

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace LoschScript.Parser {
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
		Ws=1, NewLine=2, Single_Line_Comment=3, Delimited_Comment=4, Import=5, 
		Export=6, Assembly=7, Type=8, Module=9, Global=10, Local=11, Internal=12, 
		Static=13, Protected=14, Sealed=15, Partial=16, Infix=17, Inline=18, Var=19, 
		Val=20, True=21, False=22, Of=23, Open_Paren=24, Close_Paren=25, Open_Bracket=26, 
		Close_Bracket=27, Open_Brace=28, Close_Brace=29, Comma=30, Double_Comma=31, 
		Dot=32, Double_Dot=33, Double_Dot_Question_Mark=34, Dot_Equals=35, Colon=36, 
		Underscore=37, Single_Quote=38, Double_Quote=39, Question_Mark=40, Exclamation_Mark=41, 
		Exclamation_Question=42, Exclamation_Colon=43, At_Sign=44, Exclamation_At=45, 
		Dollar_Sign=46, Caret=47, Percent_Caret=48, Bar=49, Bar_GreaterThan=50, 
		LessThan_Bar=51, Double_Bar=52, Bar_Equals=53, Double_Bar_Equals=54, Ampersand=55, 
		Double_Ampersand=56, Double_Ampersand_Equals=57, Ampersand_Equals=58, 
		Less_Than=59, Greater_Than=60, Less_Equals=61, Greater_Equals=62, Double_Equals=63, 
		Exclamation_Equals=64, Plus=65, Plus_Equals=66, Minus=67, Minus_Equals=68, 
		Asterisk=69, Asterisk_Equals=70, Double_Asterisk=71, Double_Asterisk_Equals=72, 
		Slash=73, Slash_Equals=74, Percent=75, Percent_Equals=76, Equals=77, Tilde=78, 
		Double_Tilde=79, Tilde_Equals=80, Double_Less_Than=81, Double_Less_Than_Equals=82, 
		Double_Greater_Than=83, Double_Greater_Than_Equals=84, Arrow_Right=85, 
		Arrow_Left=86, Double_Backtick=87, Identifier=88, Integer_Literal=89, 
		Hex_Integer_Literal=90, Binary_Integer_Literal=91, Real_Literal=92, Character_Literal=93, 
		String_Literal=94, Verbatim_String_Literal=95;
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
		"Static", "Protected", "Sealed", "Partial", "Infix", "Inline", "Var", 
		"Val", "True", "False", "Of", "Open_Paren", "Close_Paren", "Open_Bracket", 
		"Close_Bracket", "Open_Brace", "Close_Brace", "Comma", "Double_Comma", 
		"Dot", "Double_Dot", "Double_Dot_Question_Mark", "Dot_Equals", "Colon", 
		"Underscore", "Single_Quote", "Double_Quote", "Question_Mark", "Exclamation_Mark", 
		"Exclamation_Question", "Exclamation_Colon", "At_Sign", "Exclamation_At", 
		"Dollar_Sign", "Caret", "Percent_Caret", "Bar", "Bar_GreaterThan", "LessThan_Bar", 
		"Double_Bar", "Bar_Equals", "Double_Bar_Equals", "Ampersand", "Double_Ampersand", 
		"Double_Ampersand_Equals", "Ampersand_Equals", "Less_Than", "Greater_Than", 
		"Less_Equals", "Greater_Equals", "Double_Equals", "Exclamation_Equals", 
		"Plus", "Plus_Equals", "Minus", "Minus_Equals", "Asterisk", "Asterisk_Equals", 
		"Double_Asterisk", "Double_Asterisk_Equals", "Slash", "Slash_Equals", 
		"Percent", "Percent_Equals", "Equals", "Tilde", "Double_Tilde", "Tilde_Equals", 
		"Double_Less_Than", "Double_Less_Than_Equals", "Double_Greater_Than", 
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


	public LoschScriptLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public LoschScriptLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, null, "'import'", "'export'", "'assembly'", "'type'", 
		"'module'", "'global'", "'local'", "'internal'", "'static'", "'protected'", 
		"'sealed'", "'partial'", "'infix'", "'inline'", "'var'", "'val'", "'true'", 
		"'false'", "'of'", "'('", "')'", "'['", "']'", "'{'", "'}'", "','", "',,'", 
		"'.'", "'..'", "'..?'", "'.='", "':'", "'_'", "'''", "'\"'", "'?'", "'!'", 
		"'!?'", "'!:'", "'@'", "'!@'", "'$'", "'^'", "'%^'", "'|'", "'|>'", "'<|'", 
		"'||'", "'|='", "'||='", "'&'", "'&&'", "'&&='", "'&='", "'<'", "'>'", 
		"'<='", "'>='", "'=='", "'!='", "'+'", "'+='", "'-'", "'-='", "'*'", "'*='", 
		"'**'", "'**='", "'/'", "'/='", "'%'", "'%='", "'='", "'~'", "'~~'", "'~='", 
		"'<<'", "'<<='", "'>>'", "'>>='", "'->'", "'<-'", "'``'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "Ws", "NewLine", "Single_Line_Comment", "Delimited_Comment", "Import", 
		"Export", "Assembly", "Type", "Module", "Global", "Local", "Internal", 
		"Static", "Protected", "Sealed", "Partial", "Infix", "Inline", "Var", 
		"Val", "True", "False", "Of", "Open_Paren", "Close_Paren", "Open_Bracket", 
		"Close_Bracket", "Open_Brace", "Close_Brace", "Comma", "Double_Comma", 
		"Dot", "Double_Dot", "Double_Dot_Question_Mark", "Dot_Equals", "Colon", 
		"Underscore", "Single_Quote", "Double_Quote", "Question_Mark", "Exclamation_Mark", 
		"Exclamation_Question", "Exclamation_Colon", "At_Sign", "Exclamation_At", 
		"Dollar_Sign", "Caret", "Percent_Caret", "Bar", "Bar_GreaterThan", "LessThan_Bar", 
		"Double_Bar", "Bar_Equals", "Double_Bar_Equals", "Ampersand", "Double_Ampersand", 
		"Double_Ampersand_Equals", "Ampersand_Equals", "Less_Than", "Greater_Than", 
		"Less_Equals", "Greater_Equals", "Double_Equals", "Exclamation_Equals", 
		"Plus", "Plus_Equals", "Minus", "Minus_Equals", "Asterisk", "Asterisk_Equals", 
		"Double_Asterisk", "Double_Asterisk_Equals", "Slash", "Slash_Equals", 
		"Percent", "Percent_Equals", "Equals", "Tilde", "Double_Tilde", "Tilde_Equals", 
		"Double_Less_Than", "Double_Less_Than_Equals", "Double_Greater_Than", 
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
	private static int[] _serializedATN = {
		4,0,95,921,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
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
		7,116,2,117,7,117,2,118,7,118,2,119,7,119,2,120,7,120,2,121,7,121,2,122,
		7,122,2,123,7,123,1,0,4,0,251,8,0,11,0,12,0,252,1,0,1,0,1,1,1,1,1,1,3,
		1,260,8,1,1,2,1,2,5,2,264,8,2,10,2,12,2,267,9,2,1,2,1,2,1,3,1,3,1,3,1,
		3,5,3,275,8,3,10,3,12,3,278,9,3,1,3,1,3,1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,
		4,1,4,1,4,1,5,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,
		1,6,1,7,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,
		9,1,9,1,9,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,1,11,1,11,1,11,1,11,
		1,11,1,11,1,11,1,12,1,12,1,12,1,12,1,12,1,12,1,12,1,13,1,13,1,13,1,13,
		1,13,1,13,1,13,1,13,1,13,1,13,1,14,1,14,1,14,1,14,1,14,1,14,1,14,1,15,
		1,15,1,15,1,15,1,15,1,15,1,15,1,15,1,16,1,16,1,16,1,16,1,16,1,16,1,17,
		1,17,1,17,1,17,1,17,1,17,1,17,1,18,1,18,1,18,1,18,1,19,1,19,1,19,1,19,
		1,20,1,20,1,20,1,20,1,20,1,21,1,21,1,21,1,21,1,21,1,21,1,22,1,22,1,22,
		1,23,1,23,1,24,1,24,1,25,1,25,1,26,1,26,1,27,1,27,1,28,1,28,1,29,1,29,
		1,30,1,30,1,30,1,31,1,31,1,32,1,32,1,32,1,33,1,33,1,33,1,33,1,34,1,34,
		1,34,1,35,1,35,1,36,1,36,1,37,1,37,1,38,1,38,1,39,1,39,1,40,1,40,1,41,
		1,41,1,41,1,42,1,42,1,42,1,43,1,43,1,44,1,44,1,44,1,45,1,45,1,46,1,46,
		1,47,1,47,1,47,1,48,1,48,1,49,1,49,1,49,1,50,1,50,1,50,1,51,1,51,1,51,
		1,52,1,52,1,52,1,53,1,53,1,53,1,53,1,54,1,54,1,55,1,55,1,55,1,56,1,56,
		1,56,1,56,1,57,1,57,1,57,1,58,1,58,1,59,1,59,1,60,1,60,1,60,1,61,1,61,
		1,61,1,62,1,62,1,62,1,63,1,63,1,63,1,64,1,64,1,65,1,65,1,65,1,66,1,66,
		1,67,1,67,1,67,1,68,1,68,1,69,1,69,1,69,1,70,1,70,1,70,1,71,1,71,1,71,
		1,71,1,72,1,72,1,73,1,73,1,73,1,74,1,74,1,75,1,75,1,75,1,76,1,76,1,77,
		1,77,1,78,1,78,1,78,1,79,1,79,1,79,1,80,1,80,1,80,1,81,1,81,1,81,1,81,
		1,82,1,82,1,82,1,83,1,83,1,83,1,83,1,84,1,84,1,84,1,85,1,85,1,85,1,86,
		1,86,1,86,1,87,3,87,580,8,87,1,87,1,87,3,87,584,8,87,1,88,1,88,3,88,588,
		8,88,1,88,1,88,5,88,592,8,88,10,88,12,88,595,9,88,1,88,5,88,598,8,88,10,
		88,12,88,601,9,88,1,88,3,88,604,8,88,1,89,1,89,3,89,608,8,89,1,89,1,89,
		1,89,5,89,613,8,89,10,89,12,89,616,9,89,1,89,4,89,619,8,89,11,89,12,89,
		620,1,89,3,89,624,8,89,1,90,1,90,3,90,628,8,90,1,90,1,90,1,90,5,90,633,
		8,90,10,90,12,90,636,9,90,1,90,4,90,639,8,90,11,90,12,90,640,1,90,3,90,
		644,8,90,1,91,1,91,3,91,648,8,91,1,91,1,91,5,91,652,8,91,10,91,12,91,655,
		9,91,1,91,5,91,658,8,91,10,91,12,91,661,9,91,3,91,663,8,91,1,91,1,91,1,
		91,5,91,668,8,91,10,91,12,91,671,9,91,1,91,5,91,674,8,91,10,91,12,91,677,
		9,91,1,91,3,91,680,8,91,1,91,3,91,683,8,91,1,91,1,91,5,91,687,8,91,10,
		91,12,91,690,9,91,1,91,5,91,693,8,91,10,91,12,91,696,9,91,1,91,1,91,1,
		91,3,91,701,8,91,3,91,703,8,91,3,91,705,8,91,1,92,1,92,1,92,3,92,710,8,
		92,1,92,1,92,1,93,1,93,1,93,5,93,717,8,93,10,93,12,93,720,9,93,1,93,1,
		93,1,94,1,94,1,94,1,94,1,94,1,94,5,94,730,8,94,10,94,12,94,733,9,94,1,
		94,1,94,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,1,95,3,95,748,
		8,95,1,96,1,96,3,96,752,8,96,1,96,1,96,5,96,756,8,96,10,96,12,96,759,9,
		96,1,96,5,96,762,8,96,10,96,12,96,765,9,96,1,97,1,97,1,97,3,97,770,8,97,
		1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,
		1,98,1,98,1,98,1,98,1,98,1,98,1,98,1,98,3,98,794,8,98,1,99,1,99,1,99,1,
		99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,1,
		99,1,99,1,99,1,99,1,99,1,99,1,99,1,99,3,99,821,8,99,1,100,1,100,3,100,
		825,8,100,1,101,1,101,1,102,1,102,1,103,1,103,5,103,833,8,103,10,103,12,
		103,836,9,103,1,104,1,104,3,104,840,8,104,1,105,1,105,1,105,1,105,1,105,
		3,105,847,8,105,1,106,1,106,1,106,1,106,1,106,1,106,1,106,3,106,856,8,
		106,1,107,1,107,3,107,860,8,107,1,108,1,108,3,108,864,8,108,1,109,1,109,
		1,109,3,109,869,8,109,1,110,1,110,3,110,873,8,110,1,111,1,111,1,111,1,
		111,1,111,1,111,1,111,1,111,1,111,1,111,1,111,1,111,1,111,1,111,1,111,
		1,111,1,111,1,111,1,111,1,111,3,111,895,8,111,1,112,3,112,898,8,112,1,
		113,1,113,1,114,1,114,1,115,1,115,1,116,1,116,1,117,1,117,1,118,1,118,
		1,119,1,119,1,120,1,120,1,121,1,121,1,122,1,122,1,123,1,123,1,276,0,124,
		1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,12,25,13,27,14,
		29,15,31,16,33,17,35,18,37,19,39,20,41,21,43,22,45,23,47,24,49,25,51,26,
		53,27,55,28,57,29,59,30,61,31,63,32,65,33,67,34,69,35,71,36,73,37,75,38,
		77,39,79,40,81,41,83,42,85,43,87,44,89,45,91,46,93,47,95,48,97,49,99,50,
		101,51,103,52,105,53,107,54,109,55,111,56,113,57,115,58,117,59,119,60,
		121,61,123,62,125,63,127,64,129,65,131,66,133,67,135,68,137,69,139,70,
		141,71,143,72,145,73,147,74,149,75,151,76,153,77,155,78,157,79,159,80,
		161,81,163,82,165,83,167,84,169,85,171,86,173,87,175,88,177,89,179,90,
		181,91,183,92,185,93,187,94,189,95,191,0,193,0,195,0,197,0,199,0,201,0,
		203,0,205,0,207,0,209,0,211,0,213,0,215,0,217,0,219,0,221,0,223,0,225,
		0,227,0,229,0,231,0,233,0,235,0,237,0,239,0,241,0,243,0,245,0,247,0,1,
		0,31,2,0,9,9,32,32,4,0,10,10,13,13,133,133,8232,8233,1,0,48,57,2,0,88,
		88,120,120,2,0,66,66,98,98,1,0,48,49,3,0,100,100,109,109,115,115,5,0,10,
		10,13,13,39,39,133,133,8232,8233,5,0,10,10,13,13,34,34,133,133,8232,8233,
		1,0,34,34,2,0,83,83,115,115,4,0,66,66,83,83,98,98,115,115,2,0,85,85,117,
		117,4,0,76,76,85,85,108,108,117,117,2,0,76,76,108,108,2,0,78,78,110,110,
		2,0,69,69,101,101,2,0,43,43,45,45,2,0,9,9,11,12,9,0,32,32,160,160,5760,
		5760,6158,6158,8192,8198,8200,8202,8239,8239,8287,8287,12288,12288,3,0,
		48,57,65,70,97,102,82,0,65,90,192,214,216,222,256,310,313,327,330,381,
		385,386,388,395,398,401,403,404,406,408,412,413,415,416,418,425,428,435,
		437,444,452,461,463,475,478,494,497,500,502,504,506,562,570,571,573,574,
		577,582,584,590,880,882,886,895,902,906,908,929,931,939,975,980,984,1006,
		1012,1015,1017,1018,1021,1071,1120,1152,1162,1229,1232,1326,1329,1366,
		4256,4293,4295,4301,7680,7828,7838,7934,7944,7951,7960,7965,7976,7983,
		7992,7999,8008,8013,8025,8031,8040,8047,8120,8123,8136,8139,8152,8155,
		8168,8172,8184,8187,8450,8455,8459,8461,8464,8466,8469,8477,8484,8493,
		8496,8499,8510,8511,8517,8579,11264,11310,11360,11364,11367,11376,11378,
		11381,11390,11392,11394,11490,11499,11501,11506,42560,42562,42604,42624,
		42650,42786,42798,42802,42862,42873,42886,42891,42893,42896,42898,42902,
		42925,42928,42929,65313,65338,81,0,97,122,181,246,248,255,257,375,378,
		384,387,389,392,402,405,411,414,417,419,421,424,429,432,436,438,447,454,
		460,462,499,501,505,507,569,572,578,583,659,661,687,881,883,887,893,912,
		974,976,977,981,983,985,1011,1013,1119,1121,1153,1163,1215,1218,1327,1377,
		1415,7424,7467,7531,7543,7545,7578,7681,7837,7839,7943,7952,7957,7968,
		7975,7984,7991,8000,8005,8016,8023,8032,8039,8048,8061,8064,8071,8080,
		8087,8096,8103,8112,8116,8118,8119,8126,8132,8134,8135,8144,8147,8150,
		8151,8160,8167,8178,8180,8182,8183,8458,8467,8495,8505,8508,8509,8518,
		8521,8526,8580,11312,11358,11361,11372,11377,11387,11393,11500,11502,11507,
		11520,11557,11559,11565,42561,42605,42625,42651,42787,42801,42803,42872,
		42874,42876,42879,42887,42892,42894,42897,42901,42903,42921,43002,43866,
		43876,43877,64256,64262,64275,64279,65345,65370,6,0,453,459,498,8079,8088,
		8095,8104,8111,8124,8140,8188,8188,33,0,688,705,710,721,736,740,748,750,
		884,890,1369,1600,1765,1766,2036,2037,2042,2074,2084,2088,2417,3654,3782,
		4348,6103,6211,6823,7293,7468,7530,7544,7615,8305,8319,8336,8348,11388,
		11389,11631,11823,12293,12341,12347,12542,40981,42237,42508,42623,42652,
		42653,42775,42783,42864,42888,43000,43001,43471,43494,43632,43741,43763,
		43764,43868,43871,65392,65439,234,0,170,186,443,451,660,1514,1520,1522,
		1568,1599,1601,1610,1646,1647,1649,1747,1749,1788,1791,1808,1810,1839,
		1869,1957,1969,2026,2048,2069,2112,2136,2208,2226,2308,2361,2365,2384,
		2392,2401,2418,2432,2437,2444,2447,2448,2451,2472,2474,2480,2482,2489,
		2493,2510,2524,2525,2527,2529,2544,2545,2565,2570,2575,2576,2579,2600,
		2602,2608,2610,2611,2613,2614,2616,2617,2649,2652,2654,2676,2693,2701,
		2703,2705,2707,2728,2730,2736,2738,2739,2741,2745,2749,2768,2784,2785,
		2821,2828,2831,2832,2835,2856,2858,2864,2866,2867,2869,2873,2877,2913,
		2929,2947,2949,2954,2958,2960,2962,2965,2969,2970,2972,2986,2990,3001,
		3024,3084,3086,3088,3090,3112,3114,3129,3133,3212,3214,3216,3218,3240,
		3242,3251,3253,3257,3261,3294,3296,3297,3313,3314,3333,3340,3342,3344,
		3346,3386,3389,3406,3424,3425,3450,3455,3461,3478,3482,3505,3507,3515,
		3517,3526,3585,3632,3634,3635,3648,3653,3713,3714,3716,3722,3725,3735,
		3737,3743,3745,3747,3749,3751,3754,3755,3757,3760,3762,3763,3773,3780,
		3804,3807,3840,3911,3913,3948,3976,3980,4096,4138,4159,4181,4186,4189,
		4193,4208,4213,4225,4238,4346,4349,4680,4682,4685,4688,4694,4696,4701,
		4704,4744,4746,4749,4752,4784,4786,4789,4792,4798,4800,4805,4808,4822,
		4824,4880,4882,4885,4888,4954,4992,5007,5024,5108,5121,5740,5743,5759,
		5761,5786,5792,5866,5873,5880,5888,5900,5902,5905,5920,5937,5952,5969,
		5984,5996,5998,6000,6016,6067,6108,6210,6212,6263,6272,6312,6314,6389,
		6400,6430,6480,6509,6512,6516,6528,6571,6593,6599,6656,6678,6688,6740,
		6917,6963,6981,6987,7043,7072,7086,7087,7098,7141,7168,7203,7245,7247,
		7258,7287,7401,7404,7406,7409,7413,7414,8501,8504,11568,11623,11648,11670,
		11680,11686,11688,11694,11696,11702,11704,11710,11712,11718,11720,11726,
		11728,11734,11736,11742,12294,12348,12353,12438,12447,12538,12543,12589,
		12593,12686,12704,12730,12784,12799,13312,19893,19968,40908,40960,40980,
		40982,42124,42192,42231,42240,42507,42512,42527,42538,42539,42606,42725,
		42999,43009,43011,43013,43015,43018,43020,43042,43072,43123,43138,43187,
		43250,43255,43259,43301,43312,43334,43360,43388,43396,43442,43488,43492,
		43495,43503,43514,43518,43520,43560,43584,43586,43588,43595,43616,43631,
		43633,43638,43642,43695,43697,43709,43712,43714,43739,43740,43744,43754,
		43762,43782,43785,43790,43793,43798,43808,43814,43816,43822,43968,44002,
		44032,55203,55216,55238,55243,55291,63744,64109,64112,64217,64285,64296,
		64298,64310,64312,64316,64318,64433,64467,64829,64848,64911,64914,64967,
		65008,65019,65136,65140,65142,65276,65382,65391,65393,65437,65440,65470,
		65474,65479,65482,65487,65490,65495,65498,65500,2,0,5870,5872,8544,8559,
		3,0,2307,2307,2366,2368,2377,2380,3,0,173,173,1536,1539,1757,1757,6,0,
		95,95,8255,8256,8276,8276,65075,65076,65101,65103,65343,65343,37,0,48,
		57,1632,1641,1776,1785,1984,1993,2406,2415,2534,2543,2662,2671,2790,2799,
		2918,2927,3046,3055,3174,3183,3302,3311,3430,3439,3558,3567,3664,3673,
		3792,3801,3872,3881,4160,4169,4240,4249,6112,6121,6160,6169,6470,6479,
		6608,6617,6784,6793,6800,6809,6992,7001,7088,7097,7232,7241,7248,7257,
		42528,42537,43216,43225,43264,43273,43472,43481,43504,43513,43600,43609,
		44016,44025,65296,65305,974,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,
		0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,
		0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,29,
		1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,0,0,37,1,0,0,0,0,39,1,0,0,
		0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,1,0,0,0,0,49,1,0,0,0,0,51,
		1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,
		0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,1,0,0,0,0,71,1,0,0,0,0,73,
		1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,0,0,81,1,0,0,0,0,83,1,0,0,
		0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,1,0,0,0,0,91,1,0,0,0,0,93,1,0,0,0,0,95,
		1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,0,0,101,1,0,0,0,0,103,1,0,0,0,0,105,1,
		0,0,0,0,107,1,0,0,0,0,109,1,0,0,0,0,111,1,0,0,0,0,113,1,0,0,0,0,115,1,
		0,0,0,0,117,1,0,0,0,0,119,1,0,0,0,0,121,1,0,0,0,0,123,1,0,0,0,0,125,1,
		0,0,0,0,127,1,0,0,0,0,129,1,0,0,0,0,131,1,0,0,0,0,133,1,0,0,0,0,135,1,
		0,0,0,0,137,1,0,0,0,0,139,1,0,0,0,0,141,1,0,0,0,0,143,1,0,0,0,0,145,1,
		0,0,0,0,147,1,0,0,0,0,149,1,0,0,0,0,151,1,0,0,0,0,153,1,0,0,0,0,155,1,
		0,0,0,0,157,1,0,0,0,0,159,1,0,0,0,0,161,1,0,0,0,0,163,1,0,0,0,0,165,1,
		0,0,0,0,167,1,0,0,0,0,169,1,0,0,0,0,171,1,0,0,0,0,173,1,0,0,0,0,175,1,
		0,0,0,0,177,1,0,0,0,0,179,1,0,0,0,0,181,1,0,0,0,0,183,1,0,0,0,0,185,1,
		0,0,0,0,187,1,0,0,0,0,189,1,0,0,0,1,250,1,0,0,0,3,259,1,0,0,0,5,261,1,
		0,0,0,7,270,1,0,0,0,9,284,1,0,0,0,11,291,1,0,0,0,13,298,1,0,0,0,15,307,
		1,0,0,0,17,312,1,0,0,0,19,319,1,0,0,0,21,326,1,0,0,0,23,332,1,0,0,0,25,
		341,1,0,0,0,27,348,1,0,0,0,29,358,1,0,0,0,31,365,1,0,0,0,33,373,1,0,0,
		0,35,379,1,0,0,0,37,386,1,0,0,0,39,390,1,0,0,0,41,394,1,0,0,0,43,399,1,
		0,0,0,45,405,1,0,0,0,47,408,1,0,0,0,49,410,1,0,0,0,51,412,1,0,0,0,53,414,
		1,0,0,0,55,416,1,0,0,0,57,418,1,0,0,0,59,420,1,0,0,0,61,422,1,0,0,0,63,
		425,1,0,0,0,65,427,1,0,0,0,67,430,1,0,0,0,69,434,1,0,0,0,71,437,1,0,0,
		0,73,439,1,0,0,0,75,441,1,0,0,0,77,443,1,0,0,0,79,445,1,0,0,0,81,447,1,
		0,0,0,83,449,1,0,0,0,85,452,1,0,0,0,87,455,1,0,0,0,89,457,1,0,0,0,91,460,
		1,0,0,0,93,462,1,0,0,0,95,464,1,0,0,0,97,467,1,0,0,0,99,469,1,0,0,0,101,
		472,1,0,0,0,103,475,1,0,0,0,105,478,1,0,0,0,107,481,1,0,0,0,109,485,1,
		0,0,0,111,487,1,0,0,0,113,490,1,0,0,0,115,494,1,0,0,0,117,497,1,0,0,0,
		119,499,1,0,0,0,121,501,1,0,0,0,123,504,1,0,0,0,125,507,1,0,0,0,127,510,
		1,0,0,0,129,513,1,0,0,0,131,515,1,0,0,0,133,518,1,0,0,0,135,520,1,0,0,
		0,137,523,1,0,0,0,139,525,1,0,0,0,141,528,1,0,0,0,143,531,1,0,0,0,145,
		535,1,0,0,0,147,537,1,0,0,0,149,540,1,0,0,0,151,542,1,0,0,0,153,545,1,
		0,0,0,155,547,1,0,0,0,157,549,1,0,0,0,159,552,1,0,0,0,161,555,1,0,0,0,
		163,558,1,0,0,0,165,562,1,0,0,0,167,565,1,0,0,0,169,569,1,0,0,0,171,572,
		1,0,0,0,173,575,1,0,0,0,175,579,1,0,0,0,177,587,1,0,0,0,179,607,1,0,0,
		0,181,627,1,0,0,0,183,704,1,0,0,0,185,706,1,0,0,0,187,713,1,0,0,0,189,
		723,1,0,0,0,191,747,1,0,0,0,193,749,1,0,0,0,195,769,1,0,0,0,197,793,1,
		0,0,0,199,820,1,0,0,0,201,824,1,0,0,0,203,826,1,0,0,0,205,828,1,0,0,0,
		207,830,1,0,0,0,209,839,1,0,0,0,211,846,1,0,0,0,213,855,1,0,0,0,215,859,
		1,0,0,0,217,863,1,0,0,0,219,868,1,0,0,0,221,872,1,0,0,0,223,894,1,0,0,
		0,225,897,1,0,0,0,227,899,1,0,0,0,229,901,1,0,0,0,231,903,1,0,0,0,233,
		905,1,0,0,0,235,907,1,0,0,0,237,909,1,0,0,0,239,911,1,0,0,0,241,913,1,
		0,0,0,243,915,1,0,0,0,245,917,1,0,0,0,247,919,1,0,0,0,249,251,7,0,0,0,
		250,249,1,0,0,0,251,252,1,0,0,0,252,250,1,0,0,0,252,253,1,0,0,0,253,254,
		1,0,0,0,254,255,6,0,0,0,255,2,1,0,0,0,256,257,5,13,0,0,257,260,5,10,0,
		0,258,260,7,1,0,0,259,256,1,0,0,0,259,258,1,0,0,0,260,4,1,0,0,0,261,265,
		5,35,0,0,262,264,3,205,102,0,263,262,1,0,0,0,264,267,1,0,0,0,265,263,1,
		0,0,0,265,266,1,0,0,0,266,268,1,0,0,0,267,265,1,0,0,0,268,269,6,2,1,0,
		269,6,1,0,0,0,270,271,5,35,0,0,271,272,5,91,0,0,272,276,1,0,0,0,273,275,
		9,0,0,0,274,273,1,0,0,0,275,278,1,0,0,0,276,277,1,0,0,0,276,274,1,0,0,
		0,277,279,1,0,0,0,278,276,1,0,0,0,279,280,5,93,0,0,280,281,5,35,0,0,281,
		282,1,0,0,0,282,283,6,3,1,0,283,8,1,0,0,0,284,285,5,105,0,0,285,286,5,
		109,0,0,286,287,5,112,0,0,287,288,5,111,0,0,288,289,5,114,0,0,289,290,
		5,116,0,0,290,10,1,0,0,0,291,292,5,101,0,0,292,293,5,120,0,0,293,294,5,
		112,0,0,294,295,5,111,0,0,295,296,5,114,0,0,296,297,5,116,0,0,297,12,1,
		0,0,0,298,299,5,97,0,0,299,300,5,115,0,0,300,301,5,115,0,0,301,302,5,101,
		0,0,302,303,5,109,0,0,303,304,5,98,0,0,304,305,5,108,0,0,305,306,5,121,
		0,0,306,14,1,0,0,0,307,308,5,116,0,0,308,309,5,121,0,0,309,310,5,112,0,
		0,310,311,5,101,0,0,311,16,1,0,0,0,312,313,5,109,0,0,313,314,5,111,0,0,
		314,315,5,100,0,0,315,316,5,117,0,0,316,317,5,108,0,0,317,318,5,101,0,
		0,318,18,1,0,0,0,319,320,5,103,0,0,320,321,5,108,0,0,321,322,5,111,0,0,
		322,323,5,98,0,0,323,324,5,97,0,0,324,325,5,108,0,0,325,20,1,0,0,0,326,
		327,5,108,0,0,327,328,5,111,0,0,328,329,5,99,0,0,329,330,5,97,0,0,330,
		331,5,108,0,0,331,22,1,0,0,0,332,333,5,105,0,0,333,334,5,110,0,0,334,335,
		5,116,0,0,335,336,5,101,0,0,336,337,5,114,0,0,337,338,5,110,0,0,338,339,
		5,97,0,0,339,340,5,108,0,0,340,24,1,0,0,0,341,342,5,115,0,0,342,343,5,
		116,0,0,343,344,5,97,0,0,344,345,5,116,0,0,345,346,5,105,0,0,346,347,5,
		99,0,0,347,26,1,0,0,0,348,349,5,112,0,0,349,350,5,114,0,0,350,351,5,111,
		0,0,351,352,5,116,0,0,352,353,5,101,0,0,353,354,5,99,0,0,354,355,5,116,
		0,0,355,356,5,101,0,0,356,357,5,100,0,0,357,28,1,0,0,0,358,359,5,115,0,
		0,359,360,5,101,0,0,360,361,5,97,0,0,361,362,5,108,0,0,362,363,5,101,0,
		0,363,364,5,100,0,0,364,30,1,0,0,0,365,366,5,112,0,0,366,367,5,97,0,0,
		367,368,5,114,0,0,368,369,5,116,0,0,369,370,5,105,0,0,370,371,5,97,0,0,
		371,372,5,108,0,0,372,32,1,0,0,0,373,374,5,105,0,0,374,375,5,110,0,0,375,
		376,5,102,0,0,376,377,5,105,0,0,377,378,5,120,0,0,378,34,1,0,0,0,379,380,
		5,105,0,0,380,381,5,110,0,0,381,382,5,108,0,0,382,383,5,105,0,0,383,384,
		5,110,0,0,384,385,5,101,0,0,385,36,1,0,0,0,386,387,5,118,0,0,387,388,5,
		97,0,0,388,389,5,114,0,0,389,38,1,0,0,0,390,391,5,118,0,0,391,392,5,97,
		0,0,392,393,5,108,0,0,393,40,1,0,0,0,394,395,5,116,0,0,395,396,5,114,0,
		0,396,397,5,117,0,0,397,398,5,101,0,0,398,42,1,0,0,0,399,400,5,102,0,0,
		400,401,5,97,0,0,401,402,5,108,0,0,402,403,5,115,0,0,403,404,5,101,0,0,
		404,44,1,0,0,0,405,406,5,111,0,0,406,407,5,102,0,0,407,46,1,0,0,0,408,
		409,5,40,0,0,409,48,1,0,0,0,410,411,5,41,0,0,411,50,1,0,0,0,412,413,5,
		91,0,0,413,52,1,0,0,0,414,415,5,93,0,0,415,54,1,0,0,0,416,417,5,123,0,
		0,417,56,1,0,0,0,418,419,5,125,0,0,419,58,1,0,0,0,420,421,5,44,0,0,421,
		60,1,0,0,0,422,423,5,44,0,0,423,424,5,44,0,0,424,62,1,0,0,0,425,426,5,
		46,0,0,426,64,1,0,0,0,427,428,5,46,0,0,428,429,5,46,0,0,429,66,1,0,0,0,
		430,431,5,46,0,0,431,432,5,46,0,0,432,433,5,63,0,0,433,68,1,0,0,0,434,
		435,5,46,0,0,435,436,5,61,0,0,436,70,1,0,0,0,437,438,5,58,0,0,438,72,1,
		0,0,0,439,440,5,95,0,0,440,74,1,0,0,0,441,442,5,39,0,0,442,76,1,0,0,0,
		443,444,5,34,0,0,444,78,1,0,0,0,445,446,5,63,0,0,446,80,1,0,0,0,447,448,
		5,33,0,0,448,82,1,0,0,0,449,450,5,33,0,0,450,451,5,63,0,0,451,84,1,0,0,
		0,452,453,5,33,0,0,453,454,5,58,0,0,454,86,1,0,0,0,455,456,5,64,0,0,456,
		88,1,0,0,0,457,458,5,33,0,0,458,459,5,64,0,0,459,90,1,0,0,0,460,461,5,
		36,0,0,461,92,1,0,0,0,462,463,5,94,0,0,463,94,1,0,0,0,464,465,5,37,0,0,
		465,466,5,94,0,0,466,96,1,0,0,0,467,468,5,124,0,0,468,98,1,0,0,0,469,470,
		5,124,0,0,470,471,5,62,0,0,471,100,1,0,0,0,472,473,5,60,0,0,473,474,5,
		124,0,0,474,102,1,0,0,0,475,476,5,124,0,0,476,477,5,124,0,0,477,104,1,
		0,0,0,478,479,5,124,0,0,479,480,5,61,0,0,480,106,1,0,0,0,481,482,5,124,
		0,0,482,483,5,124,0,0,483,484,5,61,0,0,484,108,1,0,0,0,485,486,5,38,0,
		0,486,110,1,0,0,0,487,488,5,38,0,0,488,489,5,38,0,0,489,112,1,0,0,0,490,
		491,5,38,0,0,491,492,5,38,0,0,492,493,5,61,0,0,493,114,1,0,0,0,494,495,
		5,38,0,0,495,496,5,61,0,0,496,116,1,0,0,0,497,498,5,60,0,0,498,118,1,0,
		0,0,499,500,5,62,0,0,500,120,1,0,0,0,501,502,5,60,0,0,502,503,5,61,0,0,
		503,122,1,0,0,0,504,505,5,62,0,0,505,506,5,61,0,0,506,124,1,0,0,0,507,
		508,5,61,0,0,508,509,5,61,0,0,509,126,1,0,0,0,510,511,5,33,0,0,511,512,
		5,61,0,0,512,128,1,0,0,0,513,514,5,43,0,0,514,130,1,0,0,0,515,516,5,43,
		0,0,516,517,5,61,0,0,517,132,1,0,0,0,518,519,5,45,0,0,519,134,1,0,0,0,
		520,521,5,45,0,0,521,522,5,61,0,0,522,136,1,0,0,0,523,524,5,42,0,0,524,
		138,1,0,0,0,525,526,5,42,0,0,526,527,5,61,0,0,527,140,1,0,0,0,528,529,
		5,42,0,0,529,530,5,42,0,0,530,142,1,0,0,0,531,532,5,42,0,0,532,533,5,42,
		0,0,533,534,5,61,0,0,534,144,1,0,0,0,535,536,5,47,0,0,536,146,1,0,0,0,
		537,538,5,47,0,0,538,539,5,61,0,0,539,148,1,0,0,0,540,541,5,37,0,0,541,
		150,1,0,0,0,542,543,5,37,0,0,543,544,5,61,0,0,544,152,1,0,0,0,545,546,
		5,61,0,0,546,154,1,0,0,0,547,548,5,126,0,0,548,156,1,0,0,0,549,550,5,126,
		0,0,550,551,5,126,0,0,551,158,1,0,0,0,552,553,5,126,0,0,553,554,5,61,0,
		0,554,160,1,0,0,0,555,556,5,60,0,0,556,557,5,60,0,0,557,162,1,0,0,0,558,
		559,5,60,0,0,559,560,5,60,0,0,560,561,5,61,0,0,561,164,1,0,0,0,562,563,
		5,62,0,0,563,564,5,62,0,0,564,166,1,0,0,0,565,566,5,62,0,0,566,567,5,62,
		0,0,567,568,5,61,0,0,568,168,1,0,0,0,569,570,5,45,0,0,570,571,5,62,0,0,
		571,170,1,0,0,0,572,573,5,60,0,0,573,574,5,45,0,0,574,172,1,0,0,0,575,
		576,5,96,0,0,576,577,5,96,0,0,577,174,1,0,0,0,578,580,3,173,86,0,579,578,
		1,0,0,0,579,580,1,0,0,0,580,581,1,0,0,0,581,583,3,207,103,0,582,584,3,
		173,86,0,583,582,1,0,0,0,583,584,1,0,0,0,584,176,1,0,0,0,585,588,3,133,
		66,0,586,588,3,129,64,0,587,585,1,0,0,0,587,586,1,0,0,0,587,588,1,0,0,
		0,588,589,1,0,0,0,589,599,7,2,0,0,590,592,5,39,0,0,591,590,1,0,0,0,592,
		595,1,0,0,0,593,591,1,0,0,0,593,594,1,0,0,0,594,596,1,0,0,0,595,593,1,
		0,0,0,596,598,7,2,0,0,597,593,1,0,0,0,598,601,1,0,0,0,599,597,1,0,0,0,
		599,600,1,0,0,0,600,603,1,0,0,0,601,599,1,0,0,0,602,604,3,191,95,0,603,
		602,1,0,0,0,603,604,1,0,0,0,604,178,1,0,0,0,605,608,3,133,66,0,606,608,
		3,129,64,0,607,605,1,0,0,0,607,606,1,0,0,0,607,608,1,0,0,0,608,609,1,0,
		0,0,609,610,5,48,0,0,610,618,7,3,0,0,611,613,5,39,0,0,612,611,1,0,0,0,
		613,616,1,0,0,0,614,612,1,0,0,0,614,615,1,0,0,0,615,617,1,0,0,0,616,614,
		1,0,0,0,617,619,3,225,112,0,618,614,1,0,0,0,619,620,1,0,0,0,620,618,1,
		0,0,0,620,621,1,0,0,0,621,623,1,0,0,0,622,624,3,191,95,0,623,622,1,0,0,
		0,623,624,1,0,0,0,624,180,1,0,0,0,625,628,3,133,66,0,626,628,3,129,64,
		0,627,625,1,0,0,0,627,626,1,0,0,0,627,628,1,0,0,0,628,629,1,0,0,0,629,
		630,5,48,0,0,630,638,7,4,0,0,631,633,5,39,0,0,632,631,1,0,0,0,633,636,
		1,0,0,0,634,632,1,0,0,0,634,635,1,0,0,0,635,637,1,0,0,0,636,634,1,0,0,
		0,637,639,7,5,0,0,638,634,1,0,0,0,639,640,1,0,0,0,640,638,1,0,0,0,640,
		641,1,0,0,0,641,643,1,0,0,0,642,644,3,191,95,0,643,642,1,0,0,0,643,644,
		1,0,0,0,644,182,1,0,0,0,645,648,3,133,66,0,646,648,3,129,64,0,647,645,
		1,0,0,0,647,646,1,0,0,0,647,648,1,0,0,0,648,662,1,0,0,0,649,659,7,2,0,
		0,650,652,5,39,0,0,651,650,1,0,0,0,652,655,1,0,0,0,653,651,1,0,0,0,653,
		654,1,0,0,0,654,656,1,0,0,0,655,653,1,0,0,0,656,658,7,2,0,0,657,653,1,
		0,0,0,658,661,1,0,0,0,659,657,1,0,0,0,659,660,1,0,0,0,660,663,1,0,0,0,
		661,659,1,0,0,0,662,649,1,0,0,0,662,663,1,0,0,0,663,664,1,0,0,0,664,665,
		5,46,0,0,665,675,7,2,0,0,666,668,5,39,0,0,667,666,1,0,0,0,668,671,1,0,
		0,0,669,667,1,0,0,0,669,670,1,0,0,0,670,672,1,0,0,0,671,669,1,0,0,0,672,
		674,7,2,0,0,673,669,1,0,0,0,674,677,1,0,0,0,675,673,1,0,0,0,675,676,1,
		0,0,0,676,679,1,0,0,0,677,675,1,0,0,0,678,680,3,193,96,0,679,678,1,0,0,
		0,679,680,1,0,0,0,680,682,1,0,0,0,681,683,7,6,0,0,682,681,1,0,0,0,682,
		683,1,0,0,0,683,705,1,0,0,0,684,694,7,2,0,0,685,687,5,39,0,0,686,685,1,
		0,0,0,687,690,1,0,0,0,688,686,1,0,0,0,688,689,1,0,0,0,689,691,1,0,0,0,
		690,688,1,0,0,0,691,693,7,2,0,0,692,688,1,0,0,0,693,696,1,0,0,0,694,692,
		1,0,0,0,694,695,1,0,0,0,695,702,1,0,0,0,696,694,1,0,0,0,697,703,7,6,0,
		0,698,700,3,193,96,0,699,701,7,6,0,0,700,699,1,0,0,0,700,701,1,0,0,0,701,
		703,1,0,0,0,702,697,1,0,0,0,702,698,1,0,0,0,703,705,1,0,0,0,704,647,1,
		0,0,0,704,684,1,0,0,0,705,184,1,0,0,0,706,709,5,39,0,0,707,710,8,7,0,0,
		708,710,3,195,97,0,709,707,1,0,0,0,709,708,1,0,0,0,710,711,1,0,0,0,711,
		712,5,39,0,0,712,186,1,0,0,0,713,718,5,34,0,0,714,717,8,8,0,0,715,717,
		3,195,97,0,716,714,1,0,0,0,716,715,1,0,0,0,717,720,1,0,0,0,718,716,1,0,
		0,0,718,719,1,0,0,0,719,721,1,0,0,0,720,718,1,0,0,0,721,722,5,34,0,0,722,
		188,1,0,0,0,723,724,5,94,0,0,724,725,5,34,0,0,725,731,1,0,0,0,726,730,
		8,9,0,0,727,728,5,34,0,0,728,730,5,34,0,0,729,726,1,0,0,0,729,727,1,0,
		0,0,730,733,1,0,0,0,731,729,1,0,0,0,731,732,1,0,0,0,732,734,1,0,0,0,733,
		731,1,0,0,0,734,735,5,34,0,0,735,190,1,0,0,0,736,737,7,10,0,0,737,748,
		7,4,0,0,738,748,7,11,0,0,739,740,7,12,0,0,740,748,7,10,0,0,741,748,7,13,
		0,0,742,743,7,12,0,0,743,748,7,14,0,0,744,748,7,15,0,0,745,746,7,12,0,
		0,746,748,7,15,0,0,747,736,1,0,0,0,747,738,1,0,0,0,747,739,1,0,0,0,747,
		741,1,0,0,0,747,742,1,0,0,0,747,744,1,0,0,0,747,745,1,0,0,0,748,192,1,
		0,0,0,749,751,7,16,0,0,750,752,7,17,0,0,751,750,1,0,0,0,751,752,1,0,0,
		0,752,753,1,0,0,0,753,763,7,2,0,0,754,756,5,96,0,0,755,754,1,0,0,0,756,
		759,1,0,0,0,757,755,1,0,0,0,757,758,1,0,0,0,758,760,1,0,0,0,759,757,1,
		0,0,0,760,762,7,2,0,0,761,757,1,0,0,0,762,765,1,0,0,0,763,761,1,0,0,0,
		763,764,1,0,0,0,764,194,1,0,0,0,765,763,1,0,0,0,766,770,3,197,98,0,767,
		770,3,199,99,0,768,770,3,223,111,0,769,766,1,0,0,0,769,767,1,0,0,0,769,
		768,1,0,0,0,770,196,1,0,0,0,771,772,5,94,0,0,772,794,5,39,0,0,773,774,
		5,94,0,0,774,794,5,34,0,0,775,776,5,94,0,0,776,794,5,94,0,0,777,778,5,
		94,0,0,778,794,5,48,0,0,779,780,5,94,0,0,780,794,5,97,0,0,781,782,5,94,
		0,0,782,794,5,98,0,0,783,784,5,94,0,0,784,794,5,102,0,0,785,786,5,94,0,
		0,786,794,5,110,0,0,787,788,5,94,0,0,788,794,5,114,0,0,789,790,5,94,0,
		0,790,794,5,116,0,0,791,792,5,94,0,0,792,794,5,118,0,0,793,771,1,0,0,0,
		793,773,1,0,0,0,793,775,1,0,0,0,793,777,1,0,0,0,793,779,1,0,0,0,793,781,
		1,0,0,0,793,783,1,0,0,0,793,785,1,0,0,0,793,787,1,0,0,0,793,789,1,0,0,
		0,793,791,1,0,0,0,794,198,1,0,0,0,795,796,5,94,0,0,796,797,5,120,0,0,797,
		798,1,0,0,0,798,821,3,225,112,0,799,800,5,94,0,0,800,801,5,120,0,0,801,
		802,1,0,0,0,802,803,3,225,112,0,803,804,3,225,112,0,804,821,1,0,0,0,805,
		806,5,94,0,0,806,807,5,120,0,0,807,808,1,0,0,0,808,809,3,225,112,0,809,
		810,3,225,112,0,810,811,3,225,112,0,811,821,1,0,0,0,812,813,5,94,0,0,813,
		814,5,120,0,0,814,815,1,0,0,0,815,816,3,225,112,0,816,817,3,225,112,0,
		817,818,3,225,112,0,818,819,3,225,112,0,819,821,1,0,0,0,820,795,1,0,0,
		0,820,799,1,0,0,0,820,805,1,0,0,0,820,812,1,0,0,0,821,200,1,0,0,0,822,
		825,3,203,101,0,823,825,7,18,0,0,824,822,1,0,0,0,824,823,1,0,0,0,825,202,
		1,0,0,0,826,827,7,19,0,0,827,204,1,0,0,0,828,829,8,1,0,0,829,206,1,0,0,
		0,830,834,3,209,104,0,831,833,3,211,105,0,832,831,1,0,0,0,833,836,1,0,
		0,0,834,832,1,0,0,0,834,835,1,0,0,0,835,208,1,0,0,0,836,834,1,0,0,0,837,
		840,3,213,106,0,838,840,5,95,0,0,839,837,1,0,0,0,839,838,1,0,0,0,840,210,
		1,0,0,0,841,847,3,213,106,0,842,847,3,215,107,0,843,847,3,217,108,0,844,
		847,3,219,109,0,845,847,3,221,110,0,846,841,1,0,0,0,846,842,1,0,0,0,846,
		843,1,0,0,0,846,844,1,0,0,0,846,845,1,0,0,0,847,212,1,0,0,0,848,856,3,
		227,113,0,849,856,3,229,114,0,850,856,3,231,115,0,851,856,3,233,116,0,
		852,856,3,235,117,0,853,856,3,237,118,0,854,856,3,223,111,0,855,848,1,
		0,0,0,855,849,1,0,0,0,855,850,1,0,0,0,855,851,1,0,0,0,855,852,1,0,0,0,
		855,853,1,0,0,0,855,854,1,0,0,0,856,214,1,0,0,0,857,860,3,247,123,0,858,
		860,3,223,111,0,859,857,1,0,0,0,859,858,1,0,0,0,860,216,1,0,0,0,861,864,
		3,245,122,0,862,864,3,223,111,0,863,861,1,0,0,0,863,862,1,0,0,0,864,218,
		1,0,0,0,865,869,3,239,119,0,866,869,3,241,120,0,867,869,3,223,111,0,868,
		865,1,0,0,0,868,866,1,0,0,0,868,867,1,0,0,0,869,220,1,0,0,0,870,873,3,
		243,121,0,871,873,3,223,111,0,872,870,1,0,0,0,872,871,1,0,0,0,873,222,
		1,0,0,0,874,875,5,94,0,0,875,876,5,117,0,0,876,877,1,0,0,0,877,878,3,225,
		112,0,878,879,3,225,112,0,879,880,3,225,112,0,880,881,3,225,112,0,881,
		895,1,0,0,0,882,883,5,94,0,0,883,884,5,85,0,0,884,885,1,0,0,0,885,886,
		3,225,112,0,886,887,3,225,112,0,887,888,3,225,112,0,888,889,3,225,112,
		0,889,890,3,225,112,0,890,891,3,225,112,0,891,892,3,225,112,0,892,893,
		3,225,112,0,893,895,1,0,0,0,894,874,1,0,0,0,894,882,1,0,0,0,895,224,1,
		0,0,0,896,898,7,20,0,0,897,896,1,0,0,0,898,226,1,0,0,0,899,900,7,21,0,
		0,900,228,1,0,0,0,901,902,7,22,0,0,902,230,1,0,0,0,903,904,7,23,0,0,904,
		232,1,0,0,0,905,906,7,24,0,0,906,234,1,0,0,0,907,908,7,25,0,0,908,236,
		1,0,0,0,909,910,7,26,0,0,910,238,1,0,0,0,911,912,2,768,784,0,912,240,1,
		0,0,0,913,914,7,27,0,0,914,242,1,0,0,0,915,916,7,28,0,0,916,244,1,0,0,
		0,917,918,7,29,0,0,918,246,1,0,0,0,919,920,7,30,0,0,920,248,1,0,0,0,55,
		0,252,259,265,276,579,583,587,593,599,603,607,614,620,623,627,634,640,
		643,647,653,659,662,669,675,679,682,688,694,700,702,704,709,716,718,729,
		731,747,751,757,763,769,793,820,824,834,839,846,855,859,863,868,872,894,
		897,2,6,0,0,0,2,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace LoschScript.Parser
