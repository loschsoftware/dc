parser grammar MacroParser;
options { tokenVocab = MacroLexer; }

@header {
    #pragma warning disable CS0108
}

document
    : part* EOF
    ;

part
    : literal
    | macro_call
    | param_ref
    ;

literal
    : (Text | Identifier | Colon)+
    ;

macro_call
    : Macro_Start Identifier (Colon arglist)? Close_Paren
    ;

param_ref
    : Macro_Start At Identifier Close_Paren
    ;

arglist
    : argument (Comma argument)*
    ;

argument
    : part+
    ;