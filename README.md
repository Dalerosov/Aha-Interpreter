I wanted to try writing my own esoteric interpreted programming language

It is very inefficient, but efficiency is not the point

there is no indentation or anything

the program is only really split into functions, which start with keyword FUNCTION and end with keyword END

each program must contain main function:

FUNCTION main
//progarm
END

if you want other header files, just write INCLUDE nameoffile.aha preferably outside any function, but technically you can write it anywhere

Any included file must contain function setup, which can be empty, but is used for when you want to add dependencies.
All header files you add will automatically be added only once, even if they are included multiple times

you can use VOMIT CODE in any function to get all the code that can be executed, this needs to be executed, so preferably into the main function

each header file should start with SECSTART *name* and SECEND *name*, which signals start and end of a sector, this is, so there is no colision of function names between different headers

if you want to see what sectors you have, use VOMIT SECTORS

all variables are stored as bits so there is only really one type

there are two kinds of variables, which are

VAR *name*\[*size*\]
and
GLOBAL *name* \[*size*\]

VAR is only accesible in current function,
GLOBAL is well Global

you can use ASSIGN to set value of any existing var

ASSIGN *name* *value*

when creating a variable, you can add = *binary value* to assign a value

this language only supports two basic operations AND and NOT

OPERATION *result var* IS *var a* AND *var b*
OPERATION *result var* IS NOT *var a*

these two operations take input and output of size 1 bit

if you want to use anything bigger, you need to use SET keyword

SET *target_name* *starting_index||var_with_value_of_starting_index* IS *set_from where_name* *starting_index||var_with_value_of_starting_index* *length*

to call functions you can use USE if you want a return value or INVOKE if not

INVOKE *function_name*\(*parameters,...*\)

USE *return_where IS *function_name*\(*parameters,...*\)

!!!parameters must be stored in variable, not raw numbers
!!!dont make any spaces between parameters
!!!parameters are for some reason passed by reference and i didnt get to fixing it yet, i recommend writing a fucntion to coppy values

to skip lines you can use 

SKIP or NSIP

SKIP *literal number of lines* IF*|*NOT *var_name* *index_of_bit_compared||var_containing_index*
IF checks for 1, NOT for 0
NSKIP negates this, you can add multiple IF... statements after SKIP or NSKIP to make a logical AND
if the bit you are checking for is out of bounds, both IF and NOT return false

you can print with PRINT
if you want to convert to decimal PRINT DECIMAL
or usnigned decimal PRINT UDECIMAL

you can watch the program going through lines by calling ASSIGN setting_dps 100 after the start of main function
