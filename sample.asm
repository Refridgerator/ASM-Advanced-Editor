; ��� ������ ���� ������� �� C/C++ �����������
;
;extern "C"
;{
;	double array_sum(double* data, int size);
;	double convolute(double* data, int data_size, double* fir_filter, int fir_size, double* output);
;	double std_dev(double* data, int size);
;	double radius(double x, double y);
;	double fpow(double x, double y);
;	double frotate(double* x, double* y, double angle);
;}
;
; ���� ���� ������ ���� ������� � ������
; � ������� ������ ���� ������� MASM (����� "Build Customizations...")
; �������� "Item Type" ����� ����� ������ ���� "Microsoft Macro Assembler"
; �������� ����� ������ ���� ����������

.XMM
.MODEL FLAT, C
.STACK 1024

PUBLIC array_sum, convolute, std_dev
PUBLIC radius, fpow, frotate

.DATA
; ��� ���������� �� ������������, ��������� ������ ��� �������
; ���������� ��������� � ����� ������
const_byte byte 255; ��� db
const_word word 65535; ��� dw
const_dword dword 0FFFFFFFFh; ��� dd
const_single real4 1.0; ��� dd
const_double real8 1.0;��� qword ��� dq
const_extdouble real10 1.0;��� tword ��� dt
const_128bit oword 0

.CODE
; ����� �������
array_sum PROC data:PTR DWORD, data_size:DWORD;[+]
	pushad; ���������� � ����� ���� ��������� ������ ����������

	mov edi, data;=������ �� ������� ������� �������� �������
	mov ecx, data_size;=����� �������� �������
	lea edx, [edi+ecx*8];=������ ����� �������� �������

	fldz; �������� ���� � ���� FPU - ��������� �������� ��� �����
	test ecx, ecx; �������� �� ������� �����
	jz @exit

	@loop:;[
		fadd QWORD PTR [edi]
		add edi, 8; ������� �� ��������� ������� �������, 8 ��� sizeof(double)
		cmp edi, edx; �������� � �������� �����
		jl @loop;]

@exit:
	; ������������ �������� ����� �������� �� ������� ����� FPU
	popad; �������������� �� ����� ���� ��������� ������ ����������
	ret
array_sum ENDP

; ������ ���� �������� (FIR-������)
convolute PROC data:PTR DWORD, data_size:DWORD, fir_filter:PTR DWORD, fir_size:DWORD, output:PTR DWORD;[+]
	pushad; ���������� ���� ���������
	;
	mov edi, data;=������� ������
	mov ecx, data_size;=����� �������� �������
	lea edx, [edi+ecx*8];=������ ����� �������� �������

	mov esi, fir_filter;=fir ������
	mov ecx, fir_size;=����� �������
	shl ecx, 3; *= 2 � ������� 3 (��� 8 = ������ ���� double � ������)

	mov ebx, output;=�������� ������
	
	; ��������������� ��������� ��������� �������
	fldz; �������� ����
	mov eax, data_size; ����� �������� �������
	lea ebp, [ebx+eax*8]
	; ����������: 
	; ��� ��� �������� ebp �� ��������, 
	; ���������� � ���������� ������� ������ ������
	lea ebp, [ebp+ecx];=������ ����� ��������� �������
	mov eax, ebx;=�������� ������
	loop_clear:
		fst QWORD PTR [eax]; ������ ���� � �������� ������
		add eax, 8; += 8 ����
		cmp eax, ebp; ��������
		jl loop_clear; �������, ���� eax < ebp
	
	fstp st(0); ���������� ������� � ����

	; ������
	loop_i:
		fld QWORD PTR [edi]; �������� input_data[i]
		xor eax, eax; j=0; ;=j
		
		loop_j:
			fld QWORD PTR [esi+eax]; �������� input_filter[j]
			fmul st(0), st(1); input_filter[j] * input_data[i]
			fadd QWORD PTR [ebx+eax]; input_filter[j] * input_data[i] + output_fir[i+j]
			fstp QWORD PTR [ebx+eax]; ���������� output_fir[i+j]
			add eax, 8; j++
			cmp eax, ecx
			jl loop_j
	
		fstp st(0); ���������� �������
		add ebx, 8
		add edi, 8; i++
		cmp edi, edx
		jl loop_i
	;
	popad; �������������� ���� ���������
	ret
convolute ENDP

; �������������������� ����������
std_dev PROC uses ecx edx edi data:PTR DWORD, data_size:DWORD;[+]
	mov edi, data;=������� ������
	mov ecx, data_size;=����� �������� �������
	lea edx, [edi+ecx*8];=������ ����� �������� �������

	fild data_size; �������� ����� �������
	fldz; �������� ���� - ��������� �������� ��� �����
	test ecx, ecx; �������� �� ������ ������
	jz @exit
	; ���������� ��������
	@loop1:;[
		fadd QWORD PTR [edi]
		add edi, 8
		cmp edi, edx
		jl @loop1;]
	fdiv st, st(1)
	;
	mov edi, data
	fldz; �������� ���� - ��������� �������� ��� ����� ���������
	@loop2:;[
		fld QWORD PTR [edi];	| size | avg | 0 | x |
		fsub st, st(2);			| size | avg | 0 | x-avg |
		fmul st, st;			| size | avg | 0 | (x-avg)*(x-avg) |
		faddp st(1), st;		| size | avg | E |

		add edi, 8
		cmp edi, edx
		jl @loop2;]
	fdivrp st(2), st;			|  E / size |avg |
	fstp st;						|  E / size |
	fsqrt

@exit:
	ret
std_dev ENDP

; ����� �������, ���������, ����������
radius PROC x:QWORD, y:QWORD;[+]
	fld x;              | x |
	fmul st(0), st(0);  | x*x |
	fld y;              | x*x | y |
	fmul st(0), st(0);  | x*x | y*y |
	faddp st(1), st(0); | x*x+y*y |
	fsqrt;              | sqrt(x*x+y*y) |
    ret
radius ENDP

; ���������� � ������� x^y, ����� ������� �������
; x ������ ���� ������ 0, ����� NaN
; ���� ������������ � �������� �������, ������ ���� �������� 1 ������� FPU
fpow PROC x:QWORD, y:QWORD;[+]
	;[ ���������� ��������� ���������� (� �����)
	local oldCtrlWord:WORD 
	local newCtrlWord:WORD;]

	;[ ������������� ����� ���������� � ����
	; ���� ������� ����������� � �����, � ����� �����������
	; ��� ����� ����� ������� �� ������� �����
	fstcw oldCtrlWord
    mov ax, oldCtrlWord
    and ah, 11110011b
    or  ah, 00001100b
    mov newCtrlWord, ax
    fldcw newCtrlWord;]
	
	; ��������� z=y*log2(x)
	fld y ;��������� ��������� � ���������� �������
	fld x
	fyl2x ;| z=y*log2(x) |
	; ��������� 2^z
	fld st(0) ;������� ��� ���� ����� z
	frndint   ;���������
	fsubr st(0),st(1);	| z | z-trunc(z) |
	f2xm1;				| z | 2^(z-trunc(z))-1 |
	fld1;				| z | 2^(z-trunc(z))-1 | 1 |
	faddp st(1),st;		| z | 2^(z-trunc(z)) |
	fscale;				| z | (2^trunc(z))*(2^(z-trunc(z)))=2^t |
	fxch st(1);			| (2^trunc(z))*(2^(z-trunc(z)))=2^t | z |
	fstp st;			| x^y |
	; ��������������� ���������� ����� ����������
	fldcw oldCtrlWord
	ret
fpow ENDP

; ������� ������� �� ���� angle
frotate PROC px: PTR DWORD, py:PTR DWORD, angle:QWORD;[+]
	; ��������� ���� ���������� �� ������
	mov eax, px
	fld QWORD PTR [eax];| x |
	mov eax, py
	fld QWORD PTR [eax];| x | y |
	fld QWORD PTR angle;| x | y | sin |
	fsincos;			| x | y | sin | cos |
	fld st(2);          | x | y | sin | cos | y |
	fmul st(0),st(1);   | x | y | sin | cos | y*cos |
	fld st(4);          | x | y | sin | cos | y*cos | x |
	fmul st(0), st(3);  | x | y | sin | cos | y*cos | x*sin |
	faddp st(1),st(0);  | x | y | sin | cos | x*sin+y*cos | 
	; y
	fstp QWORD PTR [eax];| x | y | sin | cos |
	fmulp st(3),st(0);  | x*cos | y | sin |
	fmulp st(1),st(0);  | x*cos | y*sin |
	fsubp st(1),st(0);  | x*cos+y*sin |
	; x
	mov eax, px
	fstp QWORD PTR [eax];||
	;
    ret                
frotate ENDP


;[+ ��������� ������������ �������

; �������� �� ��������� 2
log2 PROC x:QWORD;[+]
	fld1	
	fld x
	fyl2x
    ret                
log2 ENDP

; �������� �� ��������� e
loge PROC x:QWORD;[+]
	fldln2
	fld x
	fyl2x
    ret                
loge ENDP

; �������� �� ��������� 10
log10 PROC x:QWORD;[+]
	fldlg2	
	fld x
	fyl2x
    ret                
log10 ENDP

; ���������� ������
sqrt PROC x:QWORD;[+]
	fld x
	fsqrt
    ret                
sqrt ENDP

; �����
sin PROC x:QWORD;[+]
	fld x
	fsin
    ret                
sin ENDP

; �������
cos PROC x:QWORD;[+]
	fld x
	fcos
    ret                
cos ENDP

; �������
ftan PROC x:QWORD;[+]
	fld x
	fptan
    ret                
ftan ENDP

; ���������� 
fatan PROC x:QWORD;[+]
	fld x
	fld1
	fpatan
    ret                
fatan ENDP

; ���������� �� ���� ����������
fatan2 PROC x:QWORD, y:QWORD;[+]
	fld x
	fld y
	fpatan
    ret                
fatan2 ENDP
;]

END