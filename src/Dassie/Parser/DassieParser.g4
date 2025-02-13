parser grammar DassieParser;

options { tokenVocab = DassieLexer; }

@header {
    #pragma warning disable CS0108
}

compilation_unit
    : (import_directive | export_directive | NewLine)* file_body EOF
    ;

file_body
    : (expression | local_function | type_member | NewLine | full_program)*
    ;

full_program
    : (type | attribute | NewLine)+
    ;

import_directive
    : Exclamation_Mark? Import full_identifier (Comma NewLine* full_identifier)* #basic_import
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
    : expression Custom_Operator expression #custom_operator_binary_expression
    // | Custom_Operator expression #custom_operator_unary_expression 
    | Tilde expression #bitwise_complement_expression
    | expression Open_Bracket expression Close_Bracket Equals expression #array_element_assignment
    | <assoc=right> expression Double_Asterisk expression #power_expression
    | Exclamation_Mark expression #logical_negation_expression
    | expression Asterisk expression #multiply_expression
    | expression Slash expression #divide_expression
    | expression Percent expression #remainder_expression
    | expression Double_Percent expression #modulus_expression
    | expression Plus expression #addition_expression
    | expression Minus expression #subtraction_expression
    | expression Double_Less_Than expression #left_shift_expression
    | expression Double_Greater_Than expression #right_shift_expression
    | expression op=(Double_Equals | Exclamation_Equals) expression #equality_expression
    | expression op=(Less_Than | Less_Equals | Greater_Than | Greater_Equals) expression #comparison_expression
    | expression Colon_Question_Mark type_name #isinstance_expression
    | expression Ampersand expression #and_expression
    | expression (Double_Ampersand expression)+ #logical_and_expression
    | expression Bar expression #or_expression
    | expression (Double_Bar expression)+ #logical_or_expression
    | expression Caret expression #xor_expression
    | expression Less_Than_Colon type_name #conversion_expression
    | expression Less_Than_Question_Mark_Colon type_name #safe_conversion_expression
    | Ampersand expression #byref_expression
    | expression Double_Colon expression #index_expression
    | parameter_list (Colon type_name)? Equals_Greater expression #anonymous_function_expression
    | op=(Func | Func_Ampersand) function_pointer_parameter_list expression #function_pointer_expression
    | Caret_Backslash type_name  #typeof_expression
    | Dollar_Backslash expression #nameof_expression
    | expression Double_Dot_Question_Mark expression #implementation_query_expression
    | (Var | Val)? Identifier (Colon type_name)? assignment_operator expression #local_declaration_or_assignment
    | expression Arrow_Right expression #right_pipe_expression
    | expression Arrow_Left expression #left_pipe_expression
    /*| expression Dot Identifier #dotted_expression*/
    | expression Double_Dot expression #delimited_range_expression
    | expression Double_Dot #open_ended_range_expression
    | Double_Dot expression #closed_ended_range_expression
    | Double_Dot #full_range_expression
    | Caret integer_atom #range_index_expression
    | attribute+ expression #attributed_expression
    | if_branch NewLine* elif_branch* NewLine* else_branch? #prefix_if_expression
    | expression postfix_if_branch #postfix_if_expression
    | unless_branch NewLine* else_unless_branch* NewLine* else_branch? #prefix_unless_expression
    | expression postfix_unless_branch #postfix_unless_expression
    | At_Sign Open_Paren? ((Var | Val)? Identifier (Comma ((Var | Val)? Identifier))? Close_Paren? Colon_Greater_Than expression) Equals expression #foreach_loop
    | At_Sign expression Equals expression #while_loop
    | Exclamation_At expression Equals expression #until_loop
    | match_expr #match_expression
    | try_branch catch_branch* fault_branch? finally_branch? #try_expression
    | Raise expression #raise_expression
    | Raise #rethrow_exception
    | Lock expression Equals expression #lock_statement
    | At_Open_Bracket (expression (Comma expression)*)? Close_Bracket #array_expression
    | Open_Bracket (expression (Comma expression)*)? Close_Bracket #list_initializer_expression
    | Open_Bracket (Open_Bracket expression Comma expression Close_Bracket (Comma Open_Bracket expression Comma expression Close_Bracket)*)? Close_Bracket #dictionary_expression
    | atom #atom_expression
    // | NewLine expression #prefix_newlined_expression
    | expression NewLine #newlined_expression
    | expression Semicolon #separated_expression
    | code_block #block_expression
    // | Plus expression #unary_plus_expression
    // | Minus expression #unary_negation_expression
    | full_identifier generic_arg_list? arglist? #full_identifier_member_access_expression
    | expression (Dot Identifier)+ generic_arg_list? arglist? #member_access_expression
    | expression assignment_operator expression #assignment
    | Open_Paren expression (Comma expression)* Comma? Close_Paren #tuple_expression
    // | Identifier generic_parameter_list? parameter_list? (Colon type_name)? (Equals expression)? #local_function_expression
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
    | Binary_Integer_Literal
    | Hex_Integer_Literal
    ;

real_atom
    : Real_Literal
    ;

boolean_atom
    : True
    | False
    ;

string_atom
    : (String_Literal | Verbatim_String_Literal) (Colon identifier_atom)?
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
    | Open_Paren union_or_tuple_type_member (Bar union_or_tuple_type_member)+ Close_Paren
    | Open_Paren union_or_tuple_type_member (Comma union_or_tuple_type_member)+ Close_Paren
    | generic_identifier
    | type_name generic_arg_list
    | type_name (Ampersand | Double_Ampersand)
    | Func Asterisk generic_arg_list // Function pointer type
    ;

union_or_tuple_type_member
    : (Identifier Colon)? type_name
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

arglist
    : ((Identifier Colon)? expression) (Comma ((Identifier Colon)? expression))* Double_Comma?
    ;

attribute
    : Less_Than ((Identifier | Module) Colon)? type_name arglist? Greater_Than NewLine*
    ;

generic_identifier
    : identifier_atom Open_Bracket generic_parameter_list Close_Bracket
    ;

field_access_modifier
    : Global
    | Local
    | Internal
    ;

field_declaration
    : field_access_modifier (Var | Val)? Identifier (Colon type_name)? Equals expression
    ;

placeholder
    : Dot
    ;

type_access_modifier
    : Local
    | Global
    | Internal
    ;

nested_type_access_modifier
    : type_access_modifier
    | Protected Internal?
    ;

type_special_modifier
    : Open
    ;

type
    : attribute* (type_access_modifier | nested_type_access_modifier)? type_special_modifier? type_kind Identifier generic_parameter_list? parameter_list? inheritance_list? (Equals type_block)?
    ;

generic_parameter_list
    : Open_Bracket generic_parameter (Comma generic_parameter)* Close_Bracket
    ;

generic_parameter
    : generic_parameter_attribute* generic_parameter_variance? Identifier (Colon type_name (Comma type_name)*)?
    | (Single_Quote | Double_Quote) Identifier (Colon type_name)? parameter_constraint?
    ;

generic_parameter_attribute
    : Ref
    | Val
    | Default
    ;

generic_parameter_variance
    : Plus                  // covariant
    | Minus                 // contravariant
    | Equals                // invariant
    ;

inheritance_list
    : Colon type_name (Comma type_name)*
    ;

type_kind
    : Ref? Type
    | Val Exclamation_Mark? Ampersand? Type
    | Val Ampersand? Exclamation_Mark? Type
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
    : Closed
    ;

member_special_modifier
    : Extern
    | Inline
    | Static
    | Abstract
    | Literal
    ;

type_member
    // Method or field with initializer
    : attribute* member_access_modifier? member_oop_modifier? member_special_modifier* (Var | Val)? Identifier generic_parameter_list? parameter_list? (Colon type_name)? (Equals NewLine* expression)?
    // Field without initializer
    | attribute* member_access_modifier? member_oop_modifier? member_special_modifier* (Var | Val)? Identifier generic_parameter_list? Colon type_name
    // Custom operator
    | attribute* member_access_modifier? member_oop_modifier? member_special_modifier* (Custom_Operator | Open_Paren Custom_Operator Close_Paren) parameter_list (Colon type_name)? Equals NewLine* expression
    // Property or event
    | attribute* member_access_modifier? member_oop_modifier? member_special_modifier* Identifier generic_parameter_list? (Colon type_name)? Equals NewLine* property_or_event_block
    ;

access_modifier_member_group
    : (member_access_modifier | member_oop_modifier | member_special_modifier) member_special_modifier* Equals Open_Brace (NewLine | type_member)* Close_Brace
    ;

parameter_list
    : Open_Paren (parameter (Comma parameter)*)? Close_Paren
    | parameter (Comma parameter)*
    ;

parameter_modifier
    // : Ampersand // ref
    : Ampersand_Greater // in
    | Less_Ampersand // out
    ;

parameter
    : attribute? (Val | Var)? parameter_modifier? Identifier Double_Dot? (Colon type_name)? parameter_constraint? (Equals expression)?
    ;

parameter_constraint
    : Question_Mark? Open_Brace expression Close_Brace
    ;

type_block
    : Open_Brace (type_member | type | NewLine | access_modifier_member_group)* Close_Brace
    | type_name
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

generic_arg_list
    : Open_Bracket generic_argument (Comma generic_argument)* Close_Bracket
    ;

generic_argument
    : type_name
    | expression
    ;

assignment_operator
	: Equals
	| Plus_Equals
	| Minus_Equals
	| Asterisk_Equals
	| Slash_Equals
	| Double_Asterisk_Equals
	| Percent_Equals
    | Double_Percent_Equals
	| Tilde_Equals
	| Double_Less_Than_Equals
	| Double_Greater_Than_Equals
	| Bar_Equals
	| Ampersand_Equals
	| Double_Bar_Equals
	| Double_Ampersand_Equals
	| Caret_Equals
	| Dot_Equals
	| Double_Question_Mark_Equals
	;

function_pointer_parameter_list
    : (Open_Paren type_name (Comma type_name)* Close_Paren)? (Colon type_name)?
    ;

match_expr
    : Dollar_Sign expression Equals match_block
    ;

match_block
    : Open_Brace NewLine* (match_first_case NewLine*)? (match_alternative_case NewLine*)* match_default_case? NewLine* Close_Brace
    ;

match_first_case
    : Question_Mark match_case_expression Equals expression
    ;

match_alternative_case
    : Colon match_case_expression Equals expression
    ;

match_default_case
    : Colon Equals expression
    ;

match_case_expression
    : expression
    ;

local_function
    : (Var | Val)? Identifier generic_parameter_list? parameter_list? (Colon type_name)? Equals NewLine* expression
    ;

add_handler
    : Add_Handler Equals expression
    ;

remove_handler
    : Remove_Handler Equals expression
    ;

property_getter
    : Get Equals expression
    ;

property_setter
    : Set Equals expression
    ;

property_or_event_block
    : Open_Brace NewLine* ((add_handler | remove_handler | NewLine)+ | (property_getter | property_setter | NewLine)+) NewLine* Close_Brace
    ;