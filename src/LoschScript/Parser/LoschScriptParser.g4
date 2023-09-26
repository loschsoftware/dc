parser grammar LoschScriptParser;

options { tokenVocab = LoschScriptLexer; }

compilation_unit
    : (import_directive | NewLine)* (export_directive NewLine*)? file_body EOF
    ;

file_body
    : top_level_statements
    | full_program
    ;

top_level_statements
    : (expression | NewLine)*
    ;

full_program
    : type NewLine* (type | NewLine)*
    ;

import_directive
    : Exclamation_Mark? Import full_identifier (Comma full_identifier)* #basic_import
    | Exclamation_Mark? Import Module full_identifier (Comma full_identifier)* #type_import
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
    : Tilde expression #bitwise_complement_expression
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
    | Ampersand expression #byref_expression
    | Caret_Backslash Identifier  #typeof_expression
    | Dollar_Backslash expression #nameof_expression
    | expression Double_Dot_Question_Mark expression #implementation_query_expression
    | (Var | Val)? Identifier (Colon type_name)? Equals expression #local_declaration_or_assignment
    | expression Arrow_Right expression #right_pipe_expression
    | expression Arrow_Left expression #left_pipe_expression
    | expression Dot Identifier #dotted_expression
    | range #range_expression
    | expression At_Sign expression #index_expression
    | attribute+ expression #attributed_expression
    | if_branch NewLine* elif_branch* NewLine* else_branch? #prefix_if_expression
    | expression postfix_if_branch #postfix_if_expression
    | code_block NewLine? postfix_if_branch #block_postfix_if_expression
    | unless_branch NewLine* else_unless_branch* NewLine* else_branch? #prefix_unless_expression
    | expression postfix_unless_branch #postfix_unless_expression
    | code_block NewLine? postfix_unless_branch #block_postfix_unless_expression
    | Identifier Arrow_Right ((Identifier Arrow_Right)* Identifier)? expression #loop_expression
    | At_Sign expression Equals (code_block | expression) #while_loop
    | Exclamation_At expression Equals (code_block | expression) #until_loop
    | try_branch catch_branch* fault_branch? finally_branch? #try_expression
    | Raise expression #raise_expression
    | Raise #rethrow_exception
    | Open_Bracket (expression (Comma expression)*)? Close_Bracket #array_expression
    | Open_Paren expression (Comma expression)+ Close_Paren #tuple_expression
    | Open_Bracket (Open_Bracket expression Comma expression Close_Bracket (Comma Open_Bracket expression Comma expression Close_Bracket)*)? Close_Bracket #dictionary_expression
    | atom #atom_expression
    | expression NewLine #newlined_expression
    | code_block #block_expression
    // | Plus expression #unary_plus_expression
    // Minus expression #unary_negation_expression
    | full_identifier arglist? #full_identifier_member_access_expression
    | expression (Dot Identifier)+ arglist? #member_access_expression
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
    | this_atom
    ;

this_atom
    : This
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
    : builtin_type_alias Ampersand?
    | Open_Paren type_name (Bar type_name)+ Close_Paren
    | generic_identifier Ampersand?
    | identifier_atom Ampersand?
    | param_list_type
    ;

builtin_type_alias
    : Int8 | UInt8 | Int16 | UInt16 | Int32 | UInt32 | Int64 | UInt64 | Native | UNative | Float32 | Float64 | Decimal | Char | String | Bool | Object
    ;

param_list_type
    : Open_Paren_Bar type_name (Comma type_name)* Bar_Close_Paren
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
    : Less_Than type_name arglist? Greater_Than NewLine*
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

type_access_modifier
    : Global
    | Internal
    ;

nested_type_access_modifier
    : type_access_modifier
    | Local
    | Protected Internal?
    ;

type_special_modifier
    : Open
    ;

type
    : (type_access_modifier | nested_type_access_modifier)? type_special_modifier? type_kind Identifier type_parameter_list? inheritance_list? Equals type_block
    ;

type_parameter_list
    : Open_Bracket type_parameter (Comma type_parameter)* Close_Bracket
    ;

type_parameter
    : Identifier (Colon type_parameter_constraint)?
    ;

type_parameter_constraint
    : type_kind
    | type_name
    ;

inheritance_list
    : Colon type_name (Comma type_name)*
    ;

type_kind
    : Ref? Type
    | Val Type
    | Template
    | Module
    ;

member_access_modifier
    : Global
    | Local
    | Internal
    | Protected
    ;

member_oop_modifier
    : Virtual
    ;

member_special_modifier
    : Extern
    | Infix
    | Inline
    | Static
    ;

type_member
    : attribute? member_access_modifier? member_oop_modifier? member_special_modifier* Identifier type_parameter_list? parameter_list? (Colon type_name)? Equals expression
    | attribute? member_access_modifier? member_oop_modifier? member_special_modifier* Identifier type_parameter_list? Colon type_name
    ;

parameter_list
    : Open_Paren (parameter (Comma parameter)*)? Close_Paren
    | parameter (Comma parameter)*
    ;

parameter_modifier
    : Ampersand // ref
    | Ampersand_Greater // in
    | Less_Ampersand // out
    ;

parameter
    : attribute? parameter_modifier? Identifier Double_Dot? (Colon type_name)? parameter_constraint? (Equals expression)?
    ;

parameter_constraint
    : Question_Mark? Open_Brace expression Close_Brace
    ;

type_block
    : Open_Brace (type_member | type | NewLine)* Close_Brace
    ;

try_branch
    : Try Equals expression
    ;

catch_branch
    : Catch ((Identifier Colon)? type_name)? Equals expression
    ;

finally_branch
    : Finally Equals expression
    ;

fault_branch
    : Fault Equals expression
    ;