# Assembler Advanced Editor
Assembler extension for Visual Studio 2012+ with highlighting, outlining and dynamic hints.    
Applicable to .asm and .inc files.    
![sample screenshot](screenshot.png "sample screenshot")    
Sample file: [sample.asm](sample.asm)    
For comment/uncomment feature use [this](https://github.com/Refridgerator/-Un-Comment) extension.    

## Highlighting
Configure colors from TOOLS->Options...->Fonts and Colors.

## Outlining
Outlining provides through comments.   
* Open region:  
`;[ description`  
* Collapsed by default open region:  
`;[+ description`  
* close region:  
`;]`  
* Collapsed by default PROC or MACRO block:  
`;[+]`

## Dynamic hints
Provide assignment hints to registers through comments.
* Assign hint to first register in the command:   
`;=description`  
or  
`; any comment ;=description`  
* explicit assign  
`;eax=description`
