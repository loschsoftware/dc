lexer grammar MacroLexer;

@header {
    #pragma warning disable CS0108
}

Macro_Start
    : '$(' -> pushMode(Macro)
    ;

Escape_Sequence
    : '^' . -> type(Text)
    ;

Text
    : ~[$^]+
    ;

mode Macro;

Macro_Start_M
    : '$(' -> type(Macro_Start), pushMode(Macro)
    ;

Close_Paren
    : ')' -> popMode
    ;

Colon
    : ':'
    ;

Comma
    : ','
    ;

At
    : '@'
    ;

fragment Identifier_Part
    : [a-zA-Z_] [a-zA-Z0-9_]*
    ;

Identifier
    : Identifier_Part ('.' Identifier_Part)*
    ;

Whitespace
    : [ \t\r\n]+ -> skip
    ;

Escape_Sequence_M
    : '^' . -> type(Text)
    ;

Text_M
    : ~[),:^$@]+ -> type(Text)
    ;