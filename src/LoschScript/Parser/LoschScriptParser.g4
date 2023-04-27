parser grammar LoschScriptParser;

options { tokenVocab = LoschScriptLexer; }

compilation_unit
    : import_directive* export_directive? EOF
    ;

import_directive
    : Import full_identifier (Comma full_identifier)* #basic_import
    | Import Type full_identifier (Comma full_identifier)* #type_import
    | Import Identifier Equals full_identifier (Comma Identifier Equals full_identifier)* #alias
    ;

export_directive
    : Export full_identifier
    ;

full_identifier
    : Identifier (Dot Identifier)*
    ;

expression
    : expression Double_Asterisk expression #power_expression
    | Minus expression #unary_negation_expression
    | Plus expression #unary_plus_expression
    | Exclamation_Mark expression #logical_negation_expression
    | expression Asterisk expression #multiply_expression
    | expression Slash expression #divide_expression
    | expression Percent expression #remainder_expression
    | expression Plus expression #addition_expression
    | expression Minus expression #subtraction_expression
    | expression Double_Less_Than expression #left_shift_expression
    | expression Double_Greater_Than expression #right_shift_expression
    | Tilde expression #bitwise_complement_expression
    | expression op=(Less_Than | Less_Equals | Greater_Than | Greater_Equals) expression #comparison_expression
    | expression op=(Double_Equals | Exclamation_Equals) expression #equality_expression
    | expression Ampersand expression #addition_expression
    | expression Double_Ampersand expression #logical_and_expression
    | expression Bar expression #or_expression
    | expression Double_Bar expression #logical_or_expression
    | expression Caret expression #xor_expression
    | Caret  #typeof_expression
    | Percent_Caret expression #nameof_expression
    | expression Double_Dot_Question_Mark expression #implementation_query_exception
    | expression assignment_operator expression #assignment
    | expression (Dot Identifier)+ #dotted_expression
    | range #range_expression
    | attribute expression #attributed_expression
    | expression (Dot Identifier)+ arglist #member_access_expression
    | atom #atom_expression
    ;

atom
    : expression_atom
    | integer_atom
    | real_atom
    | boolean_atom
    | string_atom
    | character_atom
    | empty_atom
    | wildcard_atom
    | identifier_atom
    ;

expression_atom
    : Open_Paren expression Close_Paren
    ;

integer_atom
    : Integer_Literal
    ;

real_atom
    : Real_Literal
    ;

boolean_atom
    : True
    | False
    ;

string_atom
    : String_Literal
    | Verbatim_String_Literal
    ;

character_atom
    : Character_Literal
    ;

empty_atom
    : Open_Paren Close_Paren;

wildcard_atom
    : Underscore
    ;

identifier_atom
    : Identifier
    | full_identifier
    ;

assignment_operator
    : Plus_Equals
    | Minus_Equals
    | Asterisk_Equals
    | Slash_Equals
    | Percent_Equals
    | Dot_Equals
    | Bar_Equals
    | Double_Bar_Equals
    | Ampersand_Equals
    | Double_Ampersand_Equals
    | Double_Less_Than_Equals
    | Double_Greater_Than_Equals
    | Tilde_Equals
    ;

range
    : Integer_Literal Double_Dot Caret? Integer_Literal
    ;

member_access
    : expression (Dot Identifier)+ arglist
    ;

arglist
    : (((Identifier Colon)? expression) (Comma ((Identifier Colon)? expression))*)? Double_Comma?
    ;

attribute
    : Less_Than full_identifier arglist Greater_Than
    ;