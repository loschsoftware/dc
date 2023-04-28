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
		Single_Line_Comment=1, Delimited_Comment=2, Import=3, Export=4, Assembly=5, 
		Type=6, Module=7, Global=8, Local=9, Internal=10, Static=11, Protected=12, 
		Sealed=13, Infix=14, Inline=15, Var=16, Val=17, True=18, False=19, Of=20, 
		Open_Paren=21, Close_Paren=22, Open_Bracket=23, Close_Bracket=24, Open_Brace=25, 
		Close_Brace=26, Comma=27, Double_Comma=28, Dot=29, Double_Dot=30, Double_Dot_Question_Mark=31, 
		Dot_Equals=32, Colon=33, Underscore=34, Single_Quote=35, Double_Quote=36, 
		Question_Mark=37, Exclamation_Mark=38, At_Sign=39, Dollar_Sign=40, Caret=41, 
		Percent_Caret=42, Bar=43, Double_Bar=44, Bar_Equals=45, Double_Bar_Equals=46, 
		Ampersand=47, Double_Ampersand=48, Double_Ampersand_Equals=49, Ampersand_Equals=50, 
		Less_Than=51, Greater_Than=52, Less_Equals=53, Greater_Equals=54, Double_Equals=55, 
		Exclamation_Equals=56, Plus=57, Plus_Equals=58, Minus=59, Minus_Equals=60, 
		Asterisk=61, Asterisk_Equals=62, Double_Asterisk=63, Double_Asterisk_Equals=64, 
		Slash=65, Slash_Equals=66, Percent=67, Percent_Equals=68, Equals=69, Tilde=70, 
		Tilde_Equals=71, Double_Less_Than=72, Double_Less_Than_Equals=73, Double_Greater_Than=74, 
		Double_Greater_Than_Equals=75, Arrow_Right=76, Arrow_Left=77, Double_Backtick=78, 
		Identifier=79, Integer_Literal=80, Hex_Integer_Literal=81, Binary_Integer_Literal=82, 
		Real_Literal=83, Character_Literal=84, String_Literal=85, Verbatim_String_Literal=86;
	public const int
		Comments_Channel=2;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN", "Comments_Channel"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"Single_Line_Comment", "Delimited_Comment", "Import", "Export", "Assembly", 
		"Type", "Module", "Global", "Local", "Internal", "Static", "Protected", 
		"Sealed", "Infix", "Inline", "Var", "Val", "True", "False", "Of", "Open_Paren", 
		"Close_Paren", "Open_Bracket", "Close_Bracket", "Open_Brace", "Close_Brace", 
		"Comma", "Double_Comma", "Dot", "Double_Dot", "Double_Dot_Question_Mark", 
		"Dot_Equals", "Colon", "Underscore", "Single_Quote", "Double_Quote", "Question_Mark", 
		"Exclamation_Mark", "At_Sign", "Dollar_Sign", "Caret", "Percent_Caret", 
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
		"CommonCharacter", "SimpleEscapeSequence", "HexEscapeSequence", "NewLine", 
		"Whitespace", "UnicodeClassZS", "InputCharacter", "IdentifierOrKeyword", 
		"IdentifierStartCharacter", "IdentifierPartCharacter", "LetterCharacter", 
		"DecimalDigitCharacter", "ConnectingCharacter", "CombiningCharacter", 
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
		null, null, null, "'import'", "'export'", "'assembly'", "'type'", "'module'", 
		"'global'", "'local'", "'internal'", "'static'", "'protected'", "'sealed'", 
		"'infix'", "'inline'", "'var'", "'val'", "'true'", "'false'", "'of'", 
		"'('", "')'", "'['", "']'", "'{'", "'}'", "','", "',,'", "'.'", "'..'", 
		"'..?'", "'.='", "':'", "'_'", "'''", "'\"'", "'?'", "'!'", "'@'", "'$'", 
		"'^'", "'%^'", "'|'", "'||'", "'|='", "'||='", "'&'", "'&&'", "'&&='", 
		"'&='", "'<'", "'>'", "'<='", "'>='", "'=='", "'!='", "'+'", "'+='", "'-'", 
		"'-='", "'*'", "'*='", "'**'", "'**='", "'/'", "'/='", "'%'", "'%='", 
		"'='", "'~'", "'~='", "'<<'", "'<<='", "'>>'", "'>>='", "'->'", "'<-'", 
		"'``'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "Single_Line_Comment", "Delimited_Comment", "Import", "Export", 
		"Assembly", "Type", "Module", "Global", "Local", "Internal", "Static", 
		"Protected", "Sealed", "Infix", "Inline", "Var", "Val", "True", "False", 
		"Of", "Open_Paren", "Close_Paren", "Open_Bracket", "Close_Bracket", "Open_Brace", 
		"Close_Brace", "Comma", "Double_Comma", "Dot", "Double_Dot", "Double_Dot_Question_Mark", 
		"Dot_Equals", "Colon", "Underscore", "Single_Quote", "Double_Quote", "Question_Mark", 
		"Exclamation_Mark", "At_Sign", "Dollar_Sign", "Caret", "Percent_Caret", 
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
	private static int[] _serializedATN = {
		4,0,86,856,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
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
		7,110,2,111,7,111,2,112,7,112,2,113,7,113,2,114,7,114,2,115,7,115,1,0,
		1,0,5,0,236,8,0,10,0,12,0,239,9,0,1,0,1,0,1,1,1,1,1,1,1,1,5,1,247,8,1,
		10,1,12,1,250,9,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,1,2,1,2,1,2,1,3,
		1,3,1,3,1,3,1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,
		5,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,8,
		1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,10,1,10,1,10,
		1,10,1,10,1,10,1,10,1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,11,1,11,
		1,12,1,12,1,12,1,12,1,12,1,12,1,12,1,13,1,13,1,13,1,13,1,13,1,13,1,14,
		1,14,1,14,1,14,1,14,1,14,1,14,1,15,1,15,1,15,1,15,1,16,1,16,1,16,1,16,
		1,17,1,17,1,17,1,17,1,17,1,18,1,18,1,18,1,18,1,18,1,18,1,19,1,19,1,19,
		1,20,1,20,1,21,1,21,1,22,1,22,1,23,1,23,1,24,1,24,1,25,1,25,1,26,1,26,
		1,27,1,27,1,27,1,28,1,28,1,29,1,29,1,29,1,30,1,30,1,30,1,30,1,31,1,31,
		1,31,1,32,1,32,1,33,1,33,1,34,1,34,1,35,1,35,1,36,1,36,1,37,1,37,1,38,
		1,38,1,39,1,39,1,40,1,40,1,41,1,41,1,41,1,42,1,42,1,43,1,43,1,43,1,44,
		1,44,1,44,1,45,1,45,1,45,1,45,1,46,1,46,1,47,1,47,1,47,1,48,1,48,1,48,
		1,48,1,49,1,49,1,49,1,50,1,50,1,51,1,51,1,52,1,52,1,52,1,53,1,53,1,53,
		1,54,1,54,1,54,1,55,1,55,1,55,1,56,1,56,1,57,1,57,1,57,1,58,1,58,1,59,
		1,59,1,59,1,60,1,60,1,61,1,61,1,61,1,62,1,62,1,62,1,63,1,63,1,63,1,63,
		1,64,1,64,1,65,1,65,1,65,1,66,1,66,1,67,1,67,1,67,1,68,1,68,1,69,1,69,
		1,70,1,70,1,70,1,71,1,71,1,71,1,72,1,72,1,72,1,72,1,73,1,73,1,73,1,74,
		1,74,1,74,1,74,1,75,1,75,1,75,1,76,1,76,1,76,1,77,1,77,1,77,1,78,3,78,
		526,8,78,1,78,1,78,3,78,530,8,78,1,79,1,79,5,79,534,8,79,10,79,12,79,537,
		9,79,1,79,5,79,540,8,79,10,79,12,79,543,9,79,1,79,3,79,546,8,79,1,80,1,
		80,1,80,5,80,551,8,80,10,80,12,80,554,9,80,1,80,4,80,557,8,80,11,80,12,
		80,558,1,80,3,80,562,8,80,1,81,1,81,1,81,5,81,567,8,81,10,81,12,81,570,
		9,81,1,81,4,81,573,8,81,11,81,12,81,574,1,81,3,81,578,8,81,1,82,1,82,5,
		82,582,8,82,10,82,12,82,585,9,82,1,82,5,82,588,8,82,10,82,12,82,591,9,
		82,3,82,593,8,82,1,82,1,82,1,82,5,82,598,8,82,10,82,12,82,601,9,82,1,82,
		5,82,604,8,82,10,82,12,82,607,9,82,1,82,3,82,610,8,82,1,82,3,82,613,8,
		82,1,82,1,82,5,82,617,8,82,10,82,12,82,620,9,82,1,82,5,82,623,8,82,10,
		82,12,82,626,9,82,1,82,1,82,1,82,3,82,631,8,82,3,82,633,8,82,3,82,635,
		8,82,1,83,1,83,1,83,3,83,640,8,83,1,83,1,83,1,84,1,84,1,84,5,84,647,8,
		84,10,84,12,84,650,9,84,1,84,1,84,1,85,1,85,1,85,1,85,1,85,1,85,5,85,660,
		8,85,10,85,12,85,663,9,85,1,85,1,85,1,86,1,86,1,86,1,86,1,86,1,86,1,86,
		1,86,1,86,1,86,1,86,3,86,678,8,86,1,87,1,87,3,87,682,8,87,1,87,1,87,5,
		87,686,8,87,10,87,12,87,689,9,87,1,87,5,87,692,8,87,10,87,12,87,695,9,
		87,1,88,1,88,1,88,3,88,700,8,88,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,
		1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,1,89,
		3,89,724,8,89,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,
		90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,1,90,3,
		90,751,8,90,1,91,1,91,1,91,3,91,756,8,91,1,92,1,92,3,92,760,8,92,1,93,
		1,93,1,94,1,94,1,95,1,95,5,95,768,8,95,10,95,12,95,771,9,95,1,96,1,96,
		3,96,775,8,96,1,97,1,97,1,97,1,97,1,97,3,97,782,8,97,1,98,1,98,1,98,1,
		98,1,98,1,98,1,98,3,98,791,8,98,1,99,1,99,3,99,795,8,99,1,100,1,100,3,
		100,799,8,100,1,101,1,101,1,101,3,101,804,8,101,1,102,1,102,3,102,808,
		8,102,1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,
		1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,1,103,3,103,830,8,103,
		1,104,3,104,833,8,104,1,105,1,105,1,106,1,106,1,107,1,107,1,108,1,108,
		1,109,1,109,1,110,1,110,1,111,1,111,1,112,1,112,1,113,1,113,1,114,1,114,
		1,115,1,115,1,248,0,116,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,
		21,11,23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,39,20,41,21,43,22,
		45,23,47,24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,63,32,65,33,67,34,
		69,35,71,36,73,37,75,38,77,39,79,40,81,41,83,42,85,43,87,44,89,45,91,46,
		93,47,95,48,97,49,99,50,101,51,103,52,105,53,107,54,109,55,111,56,113,
		57,115,58,117,59,119,60,121,61,123,62,125,63,127,64,129,65,131,66,133,
		67,135,68,137,69,139,70,141,71,143,72,145,73,147,74,149,75,151,76,153,
		77,155,78,157,79,159,80,161,81,163,82,165,83,167,84,169,85,171,86,173,
		0,175,0,177,0,179,0,181,0,183,0,185,0,187,0,189,0,191,0,193,0,195,0,197,
		0,199,0,201,0,203,0,205,0,207,0,209,0,211,0,213,0,215,0,217,0,219,0,221,
		0,223,0,225,0,227,0,229,0,231,0,1,0,26,1,0,48,57,2,0,88,88,120,120,2,0,
		66,66,98,98,1,0,48,49,6,0,68,68,77,77,83,83,100,100,109,109,115,115,6,
		0,10,10,13,13,39,39,92,92,133,133,8232,8233,6,0,10,10,13,13,34,34,92,92,
		133,133,8232,8233,1,0,34,34,2,0,98,98,115,115,2,0,108,108,117,117,2,0,
		69,69,101,101,2,0,43,43,45,45,4,0,10,10,13,13,133,133,8232,8233,2,0,9,
		9,11,12,9,0,32,32,160,160,5760,5760,6158,6158,8192,8198,8200,8202,8239,
		8239,8287,8287,12288,12288,3,0,48,57,65,70,97,102,82,0,65,90,192,214,216,
		222,256,310,313,327,330,381,385,386,388,395,398,401,403,404,406,408,412,
		413,415,416,418,425,428,435,437,444,452,461,463,475,478,494,497,500,502,
		504,506,562,570,571,573,574,577,582,584,590,880,882,886,895,902,906,908,
		929,931,939,975,980,984,1006,1012,1015,1017,1018,1021,1071,1120,1152,1162,
		1229,1232,1326,1329,1366,4256,4293,4295,4301,7680,7828,7838,7934,7944,
		7951,7960,7965,7976,7983,7992,7999,8008,8013,8025,8031,8040,8047,8120,
		8123,8136,8139,8152,8155,8168,8172,8184,8187,8450,8455,8459,8461,8464,
		8466,8469,8477,8484,8493,8496,8499,8510,8511,8517,8579,11264,11310,11360,
		11364,11367,11376,11378,11381,11390,11392,11394,11490,11499,11501,11506,
		42560,42562,42604,42624,42650,42786,42798,42802,42862,42873,42886,42891,
		42893,42896,42898,42902,42925,42928,42929,65313,65338,81,0,97,122,181,
		246,248,255,257,375,378,384,387,389,392,402,405,411,414,417,419,421,424,
		429,432,436,438,447,454,460,462,499,501,505,507,569,572,578,583,659,661,
		687,881,883,887,893,912,974,976,977,981,983,985,1011,1013,1119,1121,1153,
		1163,1215,1218,1327,1377,1415,7424,7467,7531,7543,7545,7578,7681,7837,
		7839,7943,7952,7957,7968,7975,7984,7991,8000,8005,8016,8023,8032,8039,
		8048,8061,8064,8071,8080,8087,8096,8103,8112,8116,8118,8119,8126,8132,
		8134,8135,8144,8147,8150,8151,8160,8167,8178,8180,8182,8183,8458,8467,
		8495,8505,8508,8509,8518,8521,8526,8580,11312,11358,11361,11372,11377,
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
		43472,43481,43504,43513,43600,43609,44016,44025,65296,65305,899,0,1,1,
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
		1,0,0,0,1,233,1,0,0,0,3,242,1,0,0,0,5,256,1,0,0,0,7,263,1,0,0,0,9,270,
		1,0,0,0,11,279,1,0,0,0,13,284,1,0,0,0,15,291,1,0,0,0,17,298,1,0,0,0,19,
		304,1,0,0,0,21,313,1,0,0,0,23,320,1,0,0,0,25,330,1,0,0,0,27,337,1,0,0,
		0,29,343,1,0,0,0,31,350,1,0,0,0,33,354,1,0,0,0,35,358,1,0,0,0,37,363,1,
		0,0,0,39,369,1,0,0,0,41,372,1,0,0,0,43,374,1,0,0,0,45,376,1,0,0,0,47,378,
		1,0,0,0,49,380,1,0,0,0,51,382,1,0,0,0,53,384,1,0,0,0,55,386,1,0,0,0,57,
		389,1,0,0,0,59,391,1,0,0,0,61,394,1,0,0,0,63,398,1,0,0,0,65,401,1,0,0,
		0,67,403,1,0,0,0,69,405,1,0,0,0,71,407,1,0,0,0,73,409,1,0,0,0,75,411,1,
		0,0,0,77,413,1,0,0,0,79,415,1,0,0,0,81,417,1,0,0,0,83,419,1,0,0,0,85,422,
		1,0,0,0,87,424,1,0,0,0,89,427,1,0,0,0,91,430,1,0,0,0,93,434,1,0,0,0,95,
		436,1,0,0,0,97,439,1,0,0,0,99,443,1,0,0,0,101,446,1,0,0,0,103,448,1,0,
		0,0,105,450,1,0,0,0,107,453,1,0,0,0,109,456,1,0,0,0,111,459,1,0,0,0,113,
		462,1,0,0,0,115,464,1,0,0,0,117,467,1,0,0,0,119,469,1,0,0,0,121,472,1,
		0,0,0,123,474,1,0,0,0,125,477,1,0,0,0,127,480,1,0,0,0,129,484,1,0,0,0,
		131,486,1,0,0,0,133,489,1,0,0,0,135,491,1,0,0,0,137,494,1,0,0,0,139,496,
		1,0,0,0,141,498,1,0,0,0,143,501,1,0,0,0,145,504,1,0,0,0,147,508,1,0,0,
		0,149,511,1,0,0,0,151,515,1,0,0,0,153,518,1,0,0,0,155,521,1,0,0,0,157,
		525,1,0,0,0,159,531,1,0,0,0,161,547,1,0,0,0,163,563,1,0,0,0,165,634,1,
		0,0,0,167,636,1,0,0,0,169,643,1,0,0,0,171,653,1,0,0,0,173,677,1,0,0,0,
		175,679,1,0,0,0,177,699,1,0,0,0,179,723,1,0,0,0,181,750,1,0,0,0,183,755,
		1,0,0,0,185,759,1,0,0,0,187,761,1,0,0,0,189,763,1,0,0,0,191,765,1,0,0,
		0,193,774,1,0,0,0,195,781,1,0,0,0,197,790,1,0,0,0,199,794,1,0,0,0,201,
		798,1,0,0,0,203,803,1,0,0,0,205,807,1,0,0,0,207,829,1,0,0,0,209,832,1,
		0,0,0,211,834,1,0,0,0,213,836,1,0,0,0,215,838,1,0,0,0,217,840,1,0,0,0,
		219,842,1,0,0,0,221,844,1,0,0,0,223,846,1,0,0,0,225,848,1,0,0,0,227,850,
		1,0,0,0,229,852,1,0,0,0,231,854,1,0,0,0,233,237,5,35,0,0,234,236,3,189,
		94,0,235,234,1,0,0,0,236,239,1,0,0,0,237,235,1,0,0,0,237,238,1,0,0,0,238,
		240,1,0,0,0,239,237,1,0,0,0,240,241,6,0,0,0,241,2,1,0,0,0,242,243,5,35,
		0,0,243,244,5,91,0,0,244,248,1,0,0,0,245,247,9,0,0,0,246,245,1,0,0,0,247,
		250,1,0,0,0,248,249,1,0,0,0,248,246,1,0,0,0,249,251,1,0,0,0,250,248,1,
		0,0,0,251,252,5,93,0,0,252,253,5,35,0,0,253,254,1,0,0,0,254,255,6,1,0,
		0,255,4,1,0,0,0,256,257,5,105,0,0,257,258,5,109,0,0,258,259,5,112,0,0,
		259,260,5,111,0,0,260,261,5,114,0,0,261,262,5,116,0,0,262,6,1,0,0,0,263,
		264,5,101,0,0,264,265,5,120,0,0,265,266,5,112,0,0,266,267,5,111,0,0,267,
		268,5,114,0,0,268,269,5,116,0,0,269,8,1,0,0,0,270,271,5,97,0,0,271,272,
		5,115,0,0,272,273,5,115,0,0,273,274,5,101,0,0,274,275,5,109,0,0,275,276,
		5,98,0,0,276,277,5,108,0,0,277,278,5,121,0,0,278,10,1,0,0,0,279,280,5,
		116,0,0,280,281,5,121,0,0,281,282,5,112,0,0,282,283,5,101,0,0,283,12,1,
		0,0,0,284,285,5,109,0,0,285,286,5,111,0,0,286,287,5,100,0,0,287,288,5,
		117,0,0,288,289,5,108,0,0,289,290,5,101,0,0,290,14,1,0,0,0,291,292,5,103,
		0,0,292,293,5,108,0,0,293,294,5,111,0,0,294,295,5,98,0,0,295,296,5,97,
		0,0,296,297,5,108,0,0,297,16,1,0,0,0,298,299,5,108,0,0,299,300,5,111,0,
		0,300,301,5,99,0,0,301,302,5,97,0,0,302,303,5,108,0,0,303,18,1,0,0,0,304,
		305,5,105,0,0,305,306,5,110,0,0,306,307,5,116,0,0,307,308,5,101,0,0,308,
		309,5,114,0,0,309,310,5,110,0,0,310,311,5,97,0,0,311,312,5,108,0,0,312,
		20,1,0,0,0,313,314,5,115,0,0,314,315,5,116,0,0,315,316,5,97,0,0,316,317,
		5,116,0,0,317,318,5,105,0,0,318,319,5,99,0,0,319,22,1,0,0,0,320,321,5,
		112,0,0,321,322,5,114,0,0,322,323,5,111,0,0,323,324,5,116,0,0,324,325,
		5,101,0,0,325,326,5,99,0,0,326,327,5,116,0,0,327,328,5,101,0,0,328,329,
		5,100,0,0,329,24,1,0,0,0,330,331,5,115,0,0,331,332,5,101,0,0,332,333,5,
		97,0,0,333,334,5,108,0,0,334,335,5,101,0,0,335,336,5,100,0,0,336,26,1,
		0,0,0,337,338,5,105,0,0,338,339,5,110,0,0,339,340,5,102,0,0,340,341,5,
		105,0,0,341,342,5,120,0,0,342,28,1,0,0,0,343,344,5,105,0,0,344,345,5,110,
		0,0,345,346,5,108,0,0,346,347,5,105,0,0,347,348,5,110,0,0,348,349,5,101,
		0,0,349,30,1,0,0,0,350,351,5,118,0,0,351,352,5,97,0,0,352,353,5,114,0,
		0,353,32,1,0,0,0,354,355,5,118,0,0,355,356,5,97,0,0,356,357,5,108,0,0,
		357,34,1,0,0,0,358,359,5,116,0,0,359,360,5,114,0,0,360,361,5,117,0,0,361,
		362,5,101,0,0,362,36,1,0,0,0,363,364,5,102,0,0,364,365,5,97,0,0,365,366,
		5,108,0,0,366,367,5,115,0,0,367,368,5,101,0,0,368,38,1,0,0,0,369,370,5,
		111,0,0,370,371,5,102,0,0,371,40,1,0,0,0,372,373,5,40,0,0,373,42,1,0,0,
		0,374,375,5,41,0,0,375,44,1,0,0,0,376,377,5,91,0,0,377,46,1,0,0,0,378,
		379,5,93,0,0,379,48,1,0,0,0,380,381,5,123,0,0,381,50,1,0,0,0,382,383,5,
		125,0,0,383,52,1,0,0,0,384,385,5,44,0,0,385,54,1,0,0,0,386,387,5,44,0,
		0,387,388,5,44,0,0,388,56,1,0,0,0,389,390,5,46,0,0,390,58,1,0,0,0,391,
		392,5,46,0,0,392,393,5,46,0,0,393,60,1,0,0,0,394,395,5,46,0,0,395,396,
		5,46,0,0,396,397,5,63,0,0,397,62,1,0,0,0,398,399,5,46,0,0,399,400,5,61,
		0,0,400,64,1,0,0,0,401,402,5,58,0,0,402,66,1,0,0,0,403,404,5,95,0,0,404,
		68,1,0,0,0,405,406,5,39,0,0,406,70,1,0,0,0,407,408,5,34,0,0,408,72,1,0,
		0,0,409,410,5,63,0,0,410,74,1,0,0,0,411,412,5,33,0,0,412,76,1,0,0,0,413,
		414,5,64,0,0,414,78,1,0,0,0,415,416,5,36,0,0,416,80,1,0,0,0,417,418,5,
		94,0,0,418,82,1,0,0,0,419,420,5,37,0,0,420,421,5,94,0,0,421,84,1,0,0,0,
		422,423,5,124,0,0,423,86,1,0,0,0,424,425,5,124,0,0,425,426,5,124,0,0,426,
		88,1,0,0,0,427,428,5,124,0,0,428,429,5,61,0,0,429,90,1,0,0,0,430,431,5,
		124,0,0,431,432,5,124,0,0,432,433,5,61,0,0,433,92,1,0,0,0,434,435,5,38,
		0,0,435,94,1,0,0,0,436,437,5,38,0,0,437,438,5,38,0,0,438,96,1,0,0,0,439,
		440,5,38,0,0,440,441,5,38,0,0,441,442,5,61,0,0,442,98,1,0,0,0,443,444,
		5,38,0,0,444,445,5,61,0,0,445,100,1,0,0,0,446,447,5,60,0,0,447,102,1,0,
		0,0,448,449,5,62,0,0,449,104,1,0,0,0,450,451,5,60,0,0,451,452,5,61,0,0,
		452,106,1,0,0,0,453,454,5,62,0,0,454,455,5,61,0,0,455,108,1,0,0,0,456,
		457,5,61,0,0,457,458,5,61,0,0,458,110,1,0,0,0,459,460,5,33,0,0,460,461,
		5,61,0,0,461,112,1,0,0,0,462,463,5,43,0,0,463,114,1,0,0,0,464,465,5,43,
		0,0,465,466,5,61,0,0,466,116,1,0,0,0,467,468,5,45,0,0,468,118,1,0,0,0,
		469,470,5,45,0,0,470,471,5,61,0,0,471,120,1,0,0,0,472,473,5,42,0,0,473,
		122,1,0,0,0,474,475,5,42,0,0,475,476,5,61,0,0,476,124,1,0,0,0,477,478,
		5,42,0,0,478,479,5,42,0,0,479,126,1,0,0,0,480,481,5,42,0,0,481,482,5,42,
		0,0,482,483,5,61,0,0,483,128,1,0,0,0,484,485,5,47,0,0,485,130,1,0,0,0,
		486,487,5,47,0,0,487,488,5,61,0,0,488,132,1,0,0,0,489,490,5,37,0,0,490,
		134,1,0,0,0,491,492,5,37,0,0,492,493,5,61,0,0,493,136,1,0,0,0,494,495,
		5,61,0,0,495,138,1,0,0,0,496,497,5,126,0,0,497,140,1,0,0,0,498,499,5,126,
		0,0,499,500,5,61,0,0,500,142,1,0,0,0,501,502,5,60,0,0,502,503,5,60,0,0,
		503,144,1,0,0,0,504,505,5,60,0,0,505,506,5,60,0,0,506,507,5,61,0,0,507,
		146,1,0,0,0,508,509,5,62,0,0,509,510,5,62,0,0,510,148,1,0,0,0,511,512,
		5,62,0,0,512,513,5,62,0,0,513,514,5,61,0,0,514,150,1,0,0,0,515,516,5,45,
		0,0,516,517,5,62,0,0,517,152,1,0,0,0,518,519,5,60,0,0,519,520,5,45,0,0,
		520,154,1,0,0,0,521,522,5,96,0,0,522,523,5,96,0,0,523,156,1,0,0,0,524,
		526,3,155,77,0,525,524,1,0,0,0,525,526,1,0,0,0,526,527,1,0,0,0,527,529,
		3,191,95,0,528,530,3,155,77,0,529,528,1,0,0,0,529,530,1,0,0,0,530,158,
		1,0,0,0,531,541,7,0,0,0,532,534,5,96,0,0,533,532,1,0,0,0,534,537,1,0,0,
		0,535,533,1,0,0,0,535,536,1,0,0,0,536,538,1,0,0,0,537,535,1,0,0,0,538,
		540,7,0,0,0,539,535,1,0,0,0,540,543,1,0,0,0,541,539,1,0,0,0,541,542,1,
		0,0,0,542,545,1,0,0,0,543,541,1,0,0,0,544,546,3,173,86,0,545,544,1,0,0,
		0,545,546,1,0,0,0,546,160,1,0,0,0,547,548,5,48,0,0,548,556,7,1,0,0,549,
		551,5,96,0,0,550,549,1,0,0,0,551,554,1,0,0,0,552,550,1,0,0,0,552,553,1,
		0,0,0,553,555,1,0,0,0,554,552,1,0,0,0,555,557,3,209,104,0,556,552,1,0,
		0,0,557,558,1,0,0,0,558,556,1,0,0,0,558,559,1,0,0,0,559,561,1,0,0,0,560,
		562,3,173,86,0,561,560,1,0,0,0,561,562,1,0,0,0,562,162,1,0,0,0,563,564,
		5,48,0,0,564,572,7,2,0,0,565,567,5,96,0,0,566,565,1,0,0,0,567,570,1,0,
		0,0,568,566,1,0,0,0,568,569,1,0,0,0,569,571,1,0,0,0,570,568,1,0,0,0,571,
		573,7,3,0,0,572,568,1,0,0,0,573,574,1,0,0,0,574,572,1,0,0,0,574,575,1,
		0,0,0,575,577,1,0,0,0,576,578,3,173,86,0,577,576,1,0,0,0,577,578,1,0,0,
		0,578,164,1,0,0,0,579,589,7,0,0,0,580,582,5,96,0,0,581,580,1,0,0,0,582,
		585,1,0,0,0,583,581,1,0,0,0,583,584,1,0,0,0,584,586,1,0,0,0,585,583,1,
		0,0,0,586,588,7,0,0,0,587,583,1,0,0,0,588,591,1,0,0,0,589,587,1,0,0,0,
		589,590,1,0,0,0,590,593,1,0,0,0,591,589,1,0,0,0,592,579,1,0,0,0,592,593,
		1,0,0,0,593,594,1,0,0,0,594,595,5,46,0,0,595,605,7,0,0,0,596,598,5,96,
		0,0,597,596,1,0,0,0,598,601,1,0,0,0,599,597,1,0,0,0,599,600,1,0,0,0,600,
		602,1,0,0,0,601,599,1,0,0,0,602,604,7,0,0,0,603,599,1,0,0,0,604,607,1,
		0,0,0,605,603,1,0,0,0,605,606,1,0,0,0,606,609,1,0,0,0,607,605,1,0,0,0,
		608,610,3,175,87,0,609,608,1,0,0,0,609,610,1,0,0,0,610,612,1,0,0,0,611,
		613,7,4,0,0,612,611,1,0,0,0,612,613,1,0,0,0,613,635,1,0,0,0,614,624,7,
		0,0,0,615,617,5,96,0,0,616,615,1,0,0,0,617,620,1,0,0,0,618,616,1,0,0,0,
		618,619,1,0,0,0,619,621,1,0,0,0,620,618,1,0,0,0,621,623,7,0,0,0,622,618,
		1,0,0,0,623,626,1,0,0,0,624,622,1,0,0,0,624,625,1,0,0,0,625,632,1,0,0,
		0,626,624,1,0,0,0,627,633,7,4,0,0,628,630,3,175,87,0,629,631,7,4,0,0,630,
		629,1,0,0,0,630,631,1,0,0,0,631,633,1,0,0,0,632,627,1,0,0,0,632,628,1,
		0,0,0,633,635,1,0,0,0,634,592,1,0,0,0,634,614,1,0,0,0,635,166,1,0,0,0,
		636,639,5,39,0,0,637,640,8,5,0,0,638,640,3,177,88,0,639,637,1,0,0,0,639,
		638,1,0,0,0,640,641,1,0,0,0,641,642,5,39,0,0,642,168,1,0,0,0,643,648,5,
		34,0,0,644,647,8,6,0,0,645,647,3,177,88,0,646,644,1,0,0,0,646,645,1,0,
		0,0,647,650,1,0,0,0,648,646,1,0,0,0,648,649,1,0,0,0,649,651,1,0,0,0,650,
		648,1,0,0,0,651,652,5,34,0,0,652,170,1,0,0,0,653,654,5,94,0,0,654,655,
		5,34,0,0,655,661,1,0,0,0,656,660,8,7,0,0,657,658,5,34,0,0,658,660,5,34,
		0,0,659,656,1,0,0,0,659,657,1,0,0,0,660,663,1,0,0,0,661,659,1,0,0,0,661,
		662,1,0,0,0,662,664,1,0,0,0,663,661,1,0,0,0,664,665,5,34,0,0,665,172,1,
		0,0,0,666,667,5,115,0,0,667,678,5,98,0,0,668,678,7,8,0,0,669,670,5,117,
		0,0,670,678,5,115,0,0,671,678,7,9,0,0,672,673,5,117,0,0,673,678,5,108,
		0,0,674,678,5,110,0,0,675,676,5,117,0,0,676,678,5,110,0,0,677,666,1,0,
		0,0,677,668,1,0,0,0,677,669,1,0,0,0,677,671,1,0,0,0,677,672,1,0,0,0,677,
		674,1,0,0,0,677,675,1,0,0,0,678,174,1,0,0,0,679,681,7,10,0,0,680,682,7,
		11,0,0,681,680,1,0,0,0,681,682,1,0,0,0,682,683,1,0,0,0,683,693,7,0,0,0,
		684,686,5,96,0,0,685,684,1,0,0,0,686,689,1,0,0,0,687,685,1,0,0,0,687,688,
		1,0,0,0,688,690,1,0,0,0,689,687,1,0,0,0,690,692,7,0,0,0,691,687,1,0,0,
		0,692,695,1,0,0,0,693,691,1,0,0,0,693,694,1,0,0,0,694,176,1,0,0,0,695,
		693,1,0,0,0,696,700,3,179,89,0,697,700,3,181,90,0,698,700,3,207,103,0,
		699,696,1,0,0,0,699,697,1,0,0,0,699,698,1,0,0,0,700,178,1,0,0,0,701,702,
		5,94,0,0,702,724,5,39,0,0,703,704,5,94,0,0,704,724,5,34,0,0,705,706,5,
		94,0,0,706,724,5,94,0,0,707,708,5,94,0,0,708,724,5,48,0,0,709,710,5,94,
		0,0,710,724,5,97,0,0,711,712,5,94,0,0,712,724,5,98,0,0,713,714,5,94,0,
		0,714,724,5,102,0,0,715,716,5,94,0,0,716,724,5,110,0,0,717,718,5,94,0,
		0,718,724,5,114,0,0,719,720,5,94,0,0,720,724,5,116,0,0,721,722,5,94,0,
		0,722,724,5,118,0,0,723,701,1,0,0,0,723,703,1,0,0,0,723,705,1,0,0,0,723,
		707,1,0,0,0,723,709,1,0,0,0,723,711,1,0,0,0,723,713,1,0,0,0,723,715,1,
		0,0,0,723,717,1,0,0,0,723,719,1,0,0,0,723,721,1,0,0,0,724,180,1,0,0,0,
		725,726,5,94,0,0,726,727,5,120,0,0,727,728,1,0,0,0,728,751,3,209,104,0,
		729,730,5,94,0,0,730,731,5,120,0,0,731,732,1,0,0,0,732,733,3,209,104,0,
		733,734,3,209,104,0,734,751,1,0,0,0,735,736,5,94,0,0,736,737,5,120,0,0,
		737,738,1,0,0,0,738,739,3,209,104,0,739,740,3,209,104,0,740,741,3,209,
		104,0,741,751,1,0,0,0,742,743,5,94,0,0,743,744,5,120,0,0,744,745,1,0,0,
		0,745,746,3,209,104,0,746,747,3,209,104,0,747,748,3,209,104,0,748,749,
		3,209,104,0,749,751,1,0,0,0,750,725,1,0,0,0,750,729,1,0,0,0,750,735,1,
		0,0,0,750,742,1,0,0,0,751,182,1,0,0,0,752,753,5,13,0,0,753,756,5,10,0,
		0,754,756,7,12,0,0,755,752,1,0,0,0,755,754,1,0,0,0,756,184,1,0,0,0,757,
		760,3,187,93,0,758,760,7,13,0,0,759,757,1,0,0,0,759,758,1,0,0,0,760,186,
		1,0,0,0,761,762,7,14,0,0,762,188,1,0,0,0,763,764,8,12,0,0,764,190,1,0,
		0,0,765,769,3,193,96,0,766,768,3,195,97,0,767,766,1,0,0,0,768,771,1,0,
		0,0,769,767,1,0,0,0,769,770,1,0,0,0,770,192,1,0,0,0,771,769,1,0,0,0,772,
		775,3,197,98,0,773,775,5,95,0,0,774,772,1,0,0,0,774,773,1,0,0,0,775,194,
		1,0,0,0,776,782,3,197,98,0,777,782,3,199,99,0,778,782,3,201,100,0,779,
		782,3,203,101,0,780,782,3,205,102,0,781,776,1,0,0,0,781,777,1,0,0,0,781,
		778,1,0,0,0,781,779,1,0,0,0,781,780,1,0,0,0,782,196,1,0,0,0,783,791,3,
		211,105,0,784,791,3,213,106,0,785,791,3,215,107,0,786,791,3,217,108,0,
		787,791,3,219,109,0,788,791,3,221,110,0,789,791,3,207,103,0,790,783,1,
		0,0,0,790,784,1,0,0,0,790,785,1,0,0,0,790,786,1,0,0,0,790,787,1,0,0,0,
		790,788,1,0,0,0,790,789,1,0,0,0,791,198,1,0,0,0,792,795,3,231,115,0,793,
		795,3,207,103,0,794,792,1,0,0,0,794,793,1,0,0,0,795,200,1,0,0,0,796,799,
		3,229,114,0,797,799,3,207,103,0,798,796,1,0,0,0,798,797,1,0,0,0,799,202,
		1,0,0,0,800,804,3,223,111,0,801,804,3,225,112,0,802,804,3,207,103,0,803,
		800,1,0,0,0,803,801,1,0,0,0,803,802,1,0,0,0,804,204,1,0,0,0,805,808,3,
		227,113,0,806,808,3,207,103,0,807,805,1,0,0,0,807,806,1,0,0,0,808,206,
		1,0,0,0,809,810,5,94,0,0,810,811,5,117,0,0,811,812,1,0,0,0,812,813,3,209,
		104,0,813,814,3,209,104,0,814,815,3,209,104,0,815,816,3,209,104,0,816,
		830,1,0,0,0,817,818,5,94,0,0,818,819,5,85,0,0,819,820,1,0,0,0,820,821,
		3,209,104,0,821,822,3,209,104,0,822,823,3,209,104,0,823,824,3,209,104,
		0,824,825,3,209,104,0,825,826,3,209,104,0,826,827,3,209,104,0,827,828,
		3,209,104,0,828,830,1,0,0,0,829,809,1,0,0,0,829,817,1,0,0,0,830,208,1,
		0,0,0,831,833,7,15,0,0,832,831,1,0,0,0,833,210,1,0,0,0,834,835,7,16,0,
		0,835,212,1,0,0,0,836,837,7,17,0,0,837,214,1,0,0,0,838,839,7,18,0,0,839,
		216,1,0,0,0,840,841,7,19,0,0,841,218,1,0,0,0,842,843,7,20,0,0,843,220,
		1,0,0,0,844,845,7,21,0,0,845,222,1,0,0,0,846,847,2,768,784,0,847,224,1,
		0,0,0,848,849,7,22,0,0,849,226,1,0,0,0,850,851,7,23,0,0,851,228,1,0,0,
		0,852,853,7,24,0,0,853,230,1,0,0,0,854,855,7,25,0,0,855,232,1,0,0,0,50,
		0,237,248,525,529,535,541,545,552,558,561,568,574,577,583,589,592,599,
		605,609,612,618,624,630,632,634,639,646,648,659,661,677,681,687,693,699,
		723,750,755,759,769,774,781,790,794,798,803,807,829,832,1,0,2,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace LoschScript.Parser
