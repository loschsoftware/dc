parser grammar LoschScriptParser;

options { tokenVocab = LoschScriptLexer; }

compilation_unit
    : (import_directive | macro_call | NewLine)* (export_directive NewLine*)? file_body EOF
    ;

file_body
    : top_level_statements
    | full_program
    ;

top_level_statements
    : (expression | macro_call)+
    ;

full_program
    : (type_definition | macro_call)+
    ;

macro_call
    : At_Sign (.)*? NewLine*
    ;

import_directive
    : Exclamation_Mark? Import full_identifier (Comma full_identifier)* #basic_import
    | Exclamation_Mark? Import Type full_identifier (Comma full_identifier)* #type_import
    | Exclamation_Mark? Import Identifier Equals full_identifier (Comma Identifier Equals full_identifier)* #alias
    ;

export_directive
    : Export full_identifier
    ;

full_identifier
    : Identifier (Dot Identifier)*
    ;

code_block
    : Open_Brace ((NewLine | expression)* | placeholder) Close_Brace
    ;

expression
    : full_identifier arglist? #full_identifier_member_access_expression
    | full_identifier #full_identifier_expression
    | Identifier #identifier_expression
    | Tilde expression #bitwise_complement_expression
    | expression At_Sign expression Equals expression #array_element_assignment
    | expression Double_Asterisk expression #power_expression
    | Exclamation_Mark expression #logical_negation_expression
    | expression Asterisk expression #multiply_expression
    | expression Slash expression #divide_expression
    | expression Percent expression #remainder_expression
    | expression Plus expression #addition_expression
    | expression Minus expression #subtraction_expression
    | expression Double_Less_Than expression #left_shift_expression
    | expression Double_Greater_Than expression #right_shift_expression
    | expression op=(Double_Equals | Exclamation_Equals) expression #equality_expression
    | expression op=(Less_Than | Less_Equals | Greater_Than | Greater_Equals) expression #comparison_expression
    | expression Ampersand expression #and_expression
    | expression Double_Ampersand expression #logical_and_expression
    | expression Bar expression #or_expression
    | expression Double_Bar expression #logical_or_expression
    | expression Caret expression #xor_expression
    | Minus expression #unary_negation_expression
    | Caret Identifier  #typeof_expression
    | Percent_Caret expression #nameof_expression
    | expression Double_Dot_Question_Mark expression #implementation_query_exception
    | (Var | Val)? Identifier (Colon type_name)? Equals expression #local_declaration_or_assignment
    | expression Dot Identifier arglist? #member_access_expression
    | expression Bar_GreaterThan full_identifier #right_pipe_expression
    | full_identifier LessThan_Bar expression #left_pipe_expression
    | expression Dot Identifier #dotted_expression
    | range #range_expression
    | expression At_Sign expression #index_expression
    | attribute+ expression #attributed_expression
    | if_branch elif_branch* else_branch?  #prefix_if_expression
    | expression postfix_if_branch #postfix_if_expression
    | code_block postfix_if_branch #block_postfix_if_expression
    | unless_branch else_unless_branch* else_branch? #prefix_unless_expression
    | expression postfix_unless_branch #postfix_unless_expression
    | code_block postfix_unless_branch #block_postfix_unless_expression
    | At_Sign expression Equals (code_block | expression) #while_loop
    | Exclamation_At expression Equals (code_block | expression) #until_loop
    | At_Sign expression (Arrow_Right expression)* Equals (code_block | expression) #for_loop
    | Open_Bracket (expression (Comma expression)*)? Close_Bracket #array_expression
    | Open_Paren expression (Comma expression)+ Close_Paren #tuple_expression
    | Open_Bracket (Open_Bracket expression Comma expression Close_Bracket (Comma Open_Bracket expression Comma expression Close_Bracket)*)? Close_Bracket #dictionary_expression
    | atom #atom_expression
    | expression NewLine #newlined_expression
    // | Plus expression #unary_plus_expression
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
    : attribute* Identifier
    | attribute* full_identifier
    ;

type_name
    : identifier_atom
    | generic_identifier
    | union_variable_type
    ;

union_variable_type
    : Open_Paren type_name (Bar type_name)+ Close_Paren
    ;

if_branch
    : Question_Mark expression Equals (code_block | expression)
    ;

postfix_if_branch
    : Question_Mark expression
    ;

elif_branch
    : Colon expression Equals (code_block | expression)
    ;

else_branch
    : Colon Equals (code_block | expression)
    ;

unless_branch
    : Exclamation_Question expression Equals (code_block | expression)
    ;

else_unless_branch
    : Exclamation_Colon expression Equals (code_block | expression)
    ;

postfix_unless_branch
    : Exclamation_Question expression
    ;

range
    : index? Double_Dot index?
    ;

index
    : Caret? integer_atom
    ;

arglist
    : ((Identifier Colon)? expression) (Comma ((Identifier Colon)? expression))* Double_Comma?
    ;

attribute
    : Less_Than full_identifier arglist Greater_Than
    ;

type_definition
    : Type
    ;

generic_identifier
    : identifier_atom Open_Bracket identifier_atom Close_Bracket
    ;

field_access_modifier
    : Global Partial?
    | Local Partial?
    | Internal Partial?
    ;

field_declaration
    : field_access_modifier (Var | Val)? Identifier (Colon type_name)? Equals expression
    ;

placeholder
    : Dot
    ;