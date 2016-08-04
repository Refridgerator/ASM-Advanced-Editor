; для вызова этих функций из C/C++ используйте
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
; Этот файл должен быть включен в проект
; В проекте должен быть включен MASM (через "Build Customizations...")
; Свойство "Item Type" этого файла должно быть "Microsoft Macro Assembler"
; название файла должно быть уникальным

.XMM
.MODEL FLAT, C
.STACK 1024

PUBLIC array_sum, convolute, std_dev
PUBLIC radius, fpow, frotate

.DATA
; эти переменные не используются, приведены просто для примера
; выделяются глобально в общей памяти
const_byte byte 255; или db
const_word word 65535; или dw
const_dword dword 0FFFFFFFFh; или dd
const_single real4 1.0; или dd
const_double real8 1.0;или qword или dq
const_extdouble real10 1.0;или tword или dt
const_128bit oword 0

.CODE
; сумма массива
array_sum PROC data:PTR DWORD, data_size:DWORD;[+]
	pushad; сохранение в стеке всех регистров общего назначения

	mov edi, data;=ссылка на текущий элемент входного массива
	mov ecx, data_size;=длина входного массива
	lea edx, [edi+ecx*8];=маркер конца входного массива

	fldz; загрузка нуля в стек FPU - начальное значение для суммы
	test ecx, ecx; проверка на нулевую длину
	jz @exit

	@loop:;[
		fadd QWORD PTR [edi]
		add edi, 8; переход на следующий элемент массива, 8 это sizeof(double)
		cmp edi, edx; сравнить с маркером конца
		jl @loop;]

@exit:
	; возвращаемое значение нужно оставить на вершине стека FPU
	popad; восстановление из стека всех регистров общего назначения
	ret
array_sum ENDP

; свёртка двух массивов (FIR-фильтр)
convolute PROC data:PTR DWORD, data_size:DWORD, fir_filter:PTR DWORD, fir_size:DWORD, output:PTR DWORD;[+]
	pushad; сохранение всех регистров
	;
	mov edi, data;=входной массив
	mov ecx, data_size;=длина входного массива
	lea edx, [edi+ecx*8];=маркер конца входного массива

	mov esi, fir_filter;=fir фильтр
	mov ecx, fir_size;=длина фильтра
	shl ecx, 3; *= 2 в степени 3 (это 8 = размер типа double в байтах)

	mov ebx, output;=выходной массив
	
	; предварительное обнуление выходного массива
	fldz; загрузка нуля
	mov eax, data_size; длина входного массива
	lea ebp, [ebx+eax*8]
	; примечание: 
	; так как значение ebp мы изменили, 
	; обращаться к параметрам функции больше нельзя
	lea ebp, [ebp+ecx];=маркер конца выходного массива
	mov eax, ebx;=выходной массив
	loop_clear:
		fst QWORD PTR [eax]; запись нуля в выходной массив
		add eax, 8; += 8 байт
		cmp eax, ebp; сравнить
		jl loop_clear; переход, если eax < ebp
	
	fstp st(0); освободить регистр с нулём

	; свёртка
	loop_i:
		fld QWORD PTR [edi]; загрузка input_data[i]
		xor eax, eax; j=0; ;=j
		
		loop_j:
			fld QWORD PTR [esi+eax]; загрузка input_filter[j]
			fmul st(0), st(1); input_filter[j] * input_data[i]
			fadd QWORD PTR [ebx+eax]; input_filter[j] * input_data[i] + output_fir[i+j]
			fstp QWORD PTR [ebx+eax]; сохранение output_fir[i+j]
			add eax, 8; j++
			cmp eax, ecx
			jl loop_j
	
		fstp st(0); освободить регистр
		add ebx, 8
		add edi, 8; i++
		cmp edi, edx
		jl loop_i
	;
	popad; восстановление всех регистров
	ret
convolute ENDP

; среднеквадратическое отклонение
std_dev PROC uses ecx edx edi data:PTR DWORD, data_size:DWORD;[+]
	mov edi, data;=входной массив
	mov ecx, data_size;=длина входного массива
	lea edx, [edi+ecx*8];=маркер конца входного массива

	fild data_size; загрузка длины массива
	fldz; загрузка нуля - начальное значение для суммы
	test ecx, ecx; проверка на пустой массив
	jz @exit
	; вычисление среднего
	@loop1:;[
		fadd QWORD PTR [edi]
		add edi, 8
		cmp edi, edx
		jl @loop1;]
	fdiv st, st(1)
	;
	mov edi, data
	fldz; загрузка нуля - начальное значение для суммы квадратов
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

; длина вектора, амплитуда, гипотенуза
radius PROC x:QWORD, y:QWORD;[+]
	fld x;              | x |
	fmul st(0), st(0);  | x*x |
	fld y;              | x*x | y |
	fmul st(0), st(0);  | x*x | y*y |
	faddp st(1), st(0); | x*x+y*y |
	fsqrt;              | sqrt(x*x+y*y) |
    ret
radius ENDP

; возведение в степень x^y, самый быстрый вариант
; x должно быть больше 0, иначе NaN
; если использовать в качестве макроса, должен быть свободен 1 регистр FPU
fpow PROC x:QWORD, y:QWORD;[+]
	;[ объявление локальных переменных (в стеке)
	local oldCtrlWord:WORD 
	local newCtrlWord:WORD;]

	;[ устанавливаем режим округления к нулю
	; если функция выполняется в цикле, в целях оптимизации
	; эту часть можно вынести за пределы цикла
	fstcw oldCtrlWord
    mov ax, oldCtrlWord
    and ah, 11110011b
    or  ah, 00001100b
    mov newCtrlWord, ax
    fldcw newCtrlWord;]
	
	; вычисляем z=y*log2(x)
	fld y ;Загружаем основание и показатель степени
	fld x
	fyl2x ;| z=y*log2(x) |
	; вычисляем 2^z
	fld st(0) ;Создаем еще одну копию z
	frndint   ;Округляем
	fsubr st(0),st(1);	| z | z-trunc(z) |
	f2xm1;				| z | 2^(z-trunc(z))-1 |
	fld1;				| z | 2^(z-trunc(z))-1 | 1 |
	faddp st(1),st;		| z | 2^(z-trunc(z)) |
	fscale;				| z | (2^trunc(z))*(2^(z-trunc(z)))=2^t |
	fxch st(1);			| (2^trunc(z))*(2^(z-trunc(z)))=2^t | z |
	fstp st;			| x^y |
	; восстанавливаем предыдущий режим округления
	fldcw oldCtrlWord
	ret
fpow ENDP

; поворот вектора на угол angle
frotate PROC px: PTR DWORD, py:PTR DWORD, angle:QWORD;[+]
	; загружаем наши переменные по ссылке
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


;[+ некоторые элементарные функции

; логарифм по основанию 2
log2 PROC x:QWORD;[+]
	fld1	
	fld x
	fyl2x
    ret                
log2 ENDP

; логарифм по основанию e
loge PROC x:QWORD;[+]
	fldln2
	fld x
	fyl2x
    ret                
loge ENDP

; логарифм по основанию 10
log10 PROC x:QWORD;[+]
	fldlg2	
	fld x
	fyl2x
    ret                
log10 ENDP

; квадратный корень
sqrt PROC x:QWORD;[+]
	fld x
	fsqrt
    ret                
sqrt ENDP

; синус
sin PROC x:QWORD;[+]
	fld x
	fsin
    ret                
sin ENDP

; косинус
cos PROC x:QWORD;[+]
	fld x
	fcos
    ret                
cos ENDP

; тангенс
ftan PROC x:QWORD;[+]
	fld x
	fptan
    ret                
ftan ENDP

; арктангенс 
fatan PROC x:QWORD;[+]
	fld x
	fld1
	fpatan
    ret                
fatan ENDP

; арктангенс от двух аргументов
fatan2 PROC x:QWORD, y:QWORD;[+]
	fld x
	fld y
	fpatan
    ret                
fatan2 ENDP
;]

END