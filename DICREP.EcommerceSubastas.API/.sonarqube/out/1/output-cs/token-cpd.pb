É
äC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Validation\RutValidAttribute.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /

Validation		/ 9
{

 
public 

class 
RutValidAttribute "
:# $
ValidationAttribute% 8
{ 
public 
override 
bool 
IsValid $
($ %
object% +
value, 1
)1 2
{ 	
if 
( 
value 
== 
null 
) 
return 
false 
; 
var 
rut 
= 
value 
. 
ToString $
($ %
)% &
.& '
ToUpper' .
(. /
)/ 0
. 
Replace 
( 
$str 
, 
$str  
)  !
. 
Replace 
( 
$str 
, 
$str  
)  !
;! "
if 
( 
! 
Regex 
. 
IsMatch 
( 
rut "
," #
$str$ 6
)6 7
)7 8
return 
false 
; 
var 
cuerpo 
= 
rut 
. 
	Substring &
(& '
$num' (
,( )
rut* -
.- .
Length. 4
-5 6
$num7 8
)8 9
;9 :
var 
dv 
= 
rut 
[ 
^ 
$num 
] 
; 
return 

CalcularDV 
( 
cuerpo $
)$ %
==& (
dv) +
;+ ,
} 	
private   
char   

CalcularDV   
(    
string    &
cuerpo  ' -
)  - .
{!! 	
int"" 
suma"" 
="" 
$num"" 
;"" 
int## 
multiplicador## 
=## 
$num##  !
;##! "
for%% 
(%% 
int%% 
i%% 
=%% 
cuerpo%% 
.%%  
Length%%  &
-%%' (
$num%%) *
;%%* +
i%%, -
>=%%. 0
$num%%1 2
;%%2 3
i%%4 5
--%%5 7
)%%7 8
{&& 
suma'' 
+='' 
int'' 
.'' 
Parse'' !
(''! "
cuerpo''" (
[''( )
i'') *
]''* +
.''+ ,
ToString'', 4
(''4 5
)''5 6
)''6 7
*''8 9
multiplicador'': G
;''G H
multiplicador(( 
=(( 
multiplicador((  -
==((. 0
$num((1 2
?((3 4
$num((5 6
:((7 8
multiplicador((9 F
+((G H
$num((I J
;((J K
})) 
int++ 
resto++ 
=++ 
$num++ 
-++ 
(++ 
suma++ "
%++# $
$num++% '
)++' (
;++( )
return-- 
resto-- 
switch-- 
{.. 
$num// 
=>// 
$char// 
,// 
$num00 
=>00 
$char00 
,00 
_11 
=>11 
resto11 
.11 
ToString11 #
(11# $
)11$ %
[11% &
$num11& '
]11' (
}22 
;22 
}33 	
}44 
}55 ¸
òC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\UseCases\FichaProducto\ReceiveFichaUseCase.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
UseCases/ 7
.7 8
FichaProducto8 E
{ 
public 

class 
ReceiveFichaUseCase $
{ 
private 
readonly 
IFotoRepository (
_iFotoRepository) 9
;9 :
private 
readonly 
NormalizacionHelper , 
_normalizacionHelper- A
;A B
private 
readonly $
IFichaProductoRepository 1%
_ifichaProductoRepository2 K
;K L
public 
ReceiveFichaUseCase "
(" #
NormalizacionHelper$ 7
normalizacionHelper8 K
,K L$
IFichaProductoRepository$ <$
ifichaProductoRepository= U
)U V
{ 	 
_normalizacionHelper  
=! "
normalizacionHelper# 6
;6 7%
_ifichaProductoRepository %
=& '$
ifichaProductoRepository( @
;@ A
} 	
public!! 
async!! 
Task!! 
<!! 
ResponseDTO!! %
<!!% &
int!!& )
>!!) *
>!!* +
ExecuteAsync!!, 8
(!!8 9
ReceiveFichaDto!!9 H

requestDto!!I S
)!!S T
{"" 	
var## 
sbResultado## 
=## 
new## !
StringBuilder##" /
(##/ 0
)##0 1
;##1 2
if$$ 
($$ 

requestDto$$ 
==$$ 
null$$ "
)$$" #
{%% 
throw&& 
new&& #
DatosFaltantesException&& 1
(&&1 2
nameof&&2 8
(&&8 9

requestDto&&9 C
)&&C D
)&&D E
;&&E F
}'' 
return22 
await22 %
_ifichaProductoRepository22 6
.226 7
SendFichaAsync227 E
(22E F

requestDto22F P
)22P Q
;22Q R
}44 	
}66 
}77 ﬂ
ìC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\UseCases\Acceso\GetAllPermisosUseCase.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
UseCases/ 7
.7 8
Acceso8 >
{ 
public		 

class		 !
GetAllPermisosUseCase		 &
{

 
} 
} ë
öC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\UseCases\Acceso\GetAllFuncionalidadesUseCase.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
UseCases/ 7
.7 8
Acceso8 >
{ 
public 

class (
GetAllFuncionalidadesUseCase -
{ 
private 
readonly $
IFuncionalidadRepository 1%
_ifuncionalidadRepository2 K
;K L
private 
readonly  
IFuncionalidadMapper -!
_ifuncionalidadMapper. C
;C D
private 
readonly 
ILogger  
<  !(
GetAllFuncionalidadesUseCase! =
>= >
_logger? F
;F G
public (
GetAllFuncionalidadesUseCase +
(+ ,
ILogger, 3
<3 4(
GetAllFuncionalidadesUseCase4 P
>P Q
loggerR X
,X Y$
IFuncionalidadRepositoryZ r%
ifuncionalidadRepository	s ã
,
ã å"
IFuncionalidadMapper
ç °"
ifuncionalidadMapper
¢ ∂
)
∂ ∑
{ 	%
_ifuncionalidadRepository %
=& '$
ifuncionalidadRepository( @
;@ A!
_ifuncionalidadMapper !
=" # 
ifuncionalidadMapper$ 8
;8 9
_logger 
= 
logger 
; 
} 	
public 
async 
Task 
< 
IEnumerable %
<% &
FuncionalidadDTO& 6
>6 7
>7 8
ExecuteAsync9 E
(E F
)F G
{ 	
try 
{ 
var!! 
funcionalidades!! #
=!!$ %
await!!& +%
_ifuncionalidadRepository!!, E
.!!E F
GetAllAsync!!F Q
(!!Q R
)!!R S
;!!S T
if"" 
("" 
funcionalidades"" #
==""$ &
null""' +
)""+ ,
return## 

Enumerable## %
.##% &
Empty##& +
<##+ ,
FuncionalidadDTO##, <
>##< =
(##= >
)##> ?
;##? @
return%% 
funcionalidades%% &
.%%& '
Select%%' -
(%%- .
r%%. /
=>%%0 2!
_ifuncionalidadMapper%%3 H
.%%H I
ToDTO%%I N
(%%N O
r%%O P
)%%P Q
)%%Q R
;%%R S
}&& 
catch'' 
('' 
	Exception'' 
ex'' 
)''  
{(( 
_logger)) 
.)) 
LogError))  
())  !
ex))! #
,))# $
$str))% Q
)))Q R
;))R S
throw** 
new** 
	Exception** #
(**# $
$str**$ K
)**K L
;**L M
}++ 
},, 	
}.. 
}// Ò!
äC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Mappers\IFuncionalidadMapper.cs
	namespace

 	
DICREP


 
.

 
EcommerceSubastas

 "
.

" #
Application

# .
.

. /
Mappers

/ 6
{ 
public 

	interface  
IFuncionalidadMapper )
{ 
Funcionalidade 
ToEntity 
(  
FuncionalidadDTO  0
dto1 4
)4 5
;5 6
FuncionalidadDTO 
ToDTO 
( 
Funcionalidade -
entity. 4
)4 5
;5 6
void 
UpdateEntity 
( 
Funcionalidade (
entity) /
,/ 0
FuncionalidadDTO1 A
dtoB E
)E F
;F G
} 
public 

class 
FuncionalidadMapper $
:% & 
IFuncionalidadMapper' ;
{ 
public 
Funcionalidade 
ToEntity &
(& '
FuncionalidadDTO' 7
dto8 ;
); <
{ 	
return 
new 
Funcionalidade %
{ 
Nombre 
= 
dto 
. 
Nombre #
,# $
EndpointApi 
= 
dto !
.! "
EndpointApi" -
,- .

MetodoHttp 
= 
dto  
.  !

MetodoHttp! +
,+ ,
Grupo 
= 
dto 
. 
Grupo !
,! "
EsMenu 
= 
dto 
. 
EsMenu #
,# $
Activo 
= 
dto 
. 
Activo #
,# $
Codigo   
=   
dto   
.   
Codigo   #
,  # $
}!! 
;!! 
}"" 	
public%% 
FuncionalidadDTO%% 
ToDTO%%  %
(%%% &
Funcionalidade%%& 4
entity%%5 ;
)%%; <
{&& 	
return'' 
new'' 
FuncionalidadDTO'' '
{(( 
Nombre)) 
=)) 
entity)) 
.))  
Nombre))  &
,))& '
EndpointApi** 
=** 
entity** $
.**$ %
EndpointApi**% 0
,**0 1

MetodoHttp++ 
=++ 
entity++ #
.++# $

MetodoHttp++$ .
,++. /
Grupo,, 
=,, 
entity,, 
.,, 
Grupo,, $
,,,$ %
EsMenu-- 
=-- 
entity-- 
.--  
EsMenu--  &
,--& '
Activo.. 
=.. 
entity.. 
...  
Activo..  &
,..& '
Codigo// 
=// 
entity// 
.//  
Codigo//  &
,//& '
}00 
;00 
}11 	
public33 
void33 
UpdateEntity33  
(33  !
Funcionalidade33! /
entity330 6
,336 7
FuncionalidadDTO338 H
dto33I L
)33L M
{44 	
entity55 
.55 
Nombre55 
=55 
dto55 
.55  
Nombre55  &
;55& '
entity66 
.66 
EndpointApi66 
=66  
dto66! $
.66$ %
EndpointApi66% 0
;660 1
entity77 
.77 

MetodoHttp77 
=77 
dto77  #
.77# $

MetodoHttp77$ .
;77. /
entity88 
.88 
Grupo88 
=88 
dto88 
.88 
Grupo88 $
;88$ %
entity99 
.99 
EsMenu99 
=99 
dto99 
.99  
EsMenu99  &
;99& '
entity:: 
.:: 
Activo:: 
=:: 
dto:: 
.::  
Activo::  &
;::& '
entity;; 
.;; 
Codigo;; 
=;; 
dto;; 
.;;  
Codigo;;  &
;;;& '
}<< 	
}== 
}>> ¥
àC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IPermisoService.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Interfaces/ 9
{ 
public		 

	interface		 
IPermisoService		 $
{

 
Task 
< 
bool 
> %
EmpleadoTienePermisoAsync ,
(, -
int- 0

empleadoId1 ;
,; <
string= C
codigoFuncionalidadD W
)W X
;X Y
} 
} ¯
äC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IPerfilRepository.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /

Interfaces		/ 9
{

 
public 

	interface 
IPerfilRepository &
{ 
Task 
< 
Perfile 
> 
GetByIdAsync "
(" #
int# &
id' )
)) *
;* +
Task 
< 
Perfile 
> 
GetByNombreAsync &
(& '
string' -
nombre. 4
)4 5
;5 6
Task 
< 
IEnumerable 
< 
Perfile  
>  !
>! "
GetAllAsync# .
(. /
)/ 0
;0 1
Task 
< 
Perfile 
> 
CreateAsync !
(! "
Perfile" )
perfil* 0
)0 1
;1 2
Task 
< 
Perfile 
> 
UpdateAsync !
(! "
Perfile" )
perfil* 0
)0 1
;1 2
Task 
< 
Perfile 
> 
DeleteAsync !
(! "
Perfile" )
perfil* 0
)0 1
;1 2
} 
} Ê
àC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IPasswordHasher.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Interfaces/ 9
{ 
public		 

	interface		 
IPasswordHasher		 $
{

 
string 
Hash 
( 
string 
password #
)# $
;$ %
bool 
Verify 
( 
string 
password #
,# $
string% +

storedHash, 6
)6 7
;7 8
} 
} «
ãC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IJwtTokenGenerator.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /

Interfaces		/ 9
{

 
public 

	interface 
IJwtTokenGenerator '
{ 
JwtTokenResult 
GenerateToken $
($ %
Empleado% -
empleado. 6
)6 7
;7 8
} 
} å
ëC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IFuncionalidadRepository.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Interfaces/ 9
{		 
public

 

	interface

 $
IFuncionalidadRepository

 -
{ 
Task 
< 
IEnumerable 
< 
Funcionalidade '
>' (
>( )
GetAllAsync* 5
(5 6
)6 7
;7 8
} 
} Î
àC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IFotoRepository.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Interfaces/ 9
{		 
public

 

	interface

 
IFotoRepository

 $
{ 
Task 
< 
Foto 
> 
GetByIdAsync 
(  
int  #
id$ &
)& '
;' (
Task 
< 
Foto 
> 
GetByUrlAsync  
(  !
string! '
fotoUrl( /
)/ 0
;0 1
Task 
< 
IEnumerable 
< 
Foto 
> 
> 
GetAllAsync  +
(+ ,
), -
;- .
Task 
< 
Foto 
> 
CreateAsync 
( 
Foto #
foto$ (
)( )
;) *
Task 
< 
Foto 
> 
UpdateAsync 
( 
Foto #
foto$ (
)( )
;) *
Task 
< 
Foto 
> 
DeleteAsync 
( 
Foto #
foto$ (
)( )
;) *
Task 
< 
IEnumerable 
< 
Foto 
> 
> 
GetByPrendaIdAsync  2
(2 3
long3 7
prendaId8 @
)@ A
;A B
Task 
< 
IEnumerable 
< 
Foto 
> 
>  
GetByPrendaCLIdAsync  4
(4 5
long5 9

clprendaId: D
)D E
;E F
Task 
< 
Foto 
> $
GetByUrlNormalizadoAsync +
(+ ,
string, 2
url3 6
)6 7
;7 8
} 
} ≤
ëC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IFichaProductoRepository.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /

Interfaces		/ 9
{

 
public 

	interface $
IFichaProductoRepository -
{ 
Task 
< 
ResponseDTO 
< 
int 
> 
> 
SendFichaAsync -
(- .
ReceiveFichaDto. =
dto> A
)A B
;B C
} 
} ¸
åC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IFeriadosRepository.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /

Interfaces		/ 9
{

 
public 

	interface 
IFeriadosRepository (
{ 
Task 
< 
Feriado 
> 
GetByIdAsync "
(" #
int# &
id' )
)) *
;* +
Task 
< 
IEnumerable 
< 
Feriado  
>  !
>! "
GetAllAsync# .
(. /
)/ 0
;0 1
Task 
< 
bool 
> 
ExistsByFechaAsync %
(% &
DateOnly& .
fecha/ 4
)4 5
;5 6
Task 
< 
Feriado 
> 
CreateAsync !
(! "
Feriado" )
entity* 0
)0 1
;1 2
Task 
< 
Feriado 
> 
UpdateAsync !
(! "
Feriado" )
entity* 0
)0 1
;1 2
Task 
< 
Feriado 
> 
DeleteAsync !
(! "
Feriado" )
entity* 0
)0 1
;1 2
} 
} ç
åC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Interfaces\IEmpleadoRepository.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Interfaces/ 9
{		 
public

 

	interface

 
IEmpleadoRepository

 (
{ 
Task 
< 
Empleado 
> 
GetByIdAsync #
(# $
int$ '
id( *
)* +
;+ ,
Task 
< 
Empleado 
> 
GetByUsuarioAsync (
(( )
string) /
usuario0 7
)7 8
;8 9
Task 
< 
IEnumerable 
< 
Empleado !
>! "
>" #
GetAllAsync$ /
(/ 0
)0 1
;1 2
Task 
< 
Empleado 
> 
CreateAsync "
(" #
Empleado# +
empleado, 4
)4 5
;5 6
Task 
< 
Empleado 
> 
UpdateAsync "
(" #
Empleado# +
empleado, 4
)4 5
;5 6
Task 
< 
Empleado 
> 
DeleteAsync "
(" #
Empleado# +
empleado, 4
)4 5
;5 6
} 
} ¡
âC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Helpers\NormalizacionHelper.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /
Helpers		/ 6
{

 
public 

class 
NormalizacionHelper $
{ 
public 
static 
string 
NormalizarNombre -
(- .
string. 4
texto5 :
): ;
{ 	
if 
( 
string 
. 
IsNullOrWhiteSpace )
() *
texto* /
)/ 0
)0 1
return 
$str 
; 
var 
normalizado 
= 
texto #
.# $
ToLowerInvariant$ 4
(4 5
)5 6
.6 7
Trim7 ;
(; <
)< =
;= >
var 
normalizedForm 
=  
normalizado! ,
., -
	Normalize- 6
(6 7
NormalizationForm7 H
.H I
FormDI N
)N O
;O P
var 
sb 
= 
new 
StringBuilder &
(& '
)' (
;( )
foreach 
( 
var 
c 
in 
normalizedForm ,
), -
{ 
var   
unicodeCategory   #
=  $ %
CharUnicodeInfo  & 5
.  5 6
GetUnicodeCategory  6 H
(  H I
c  I J
)  J K
;  K L
if!! 
(!! 
unicodeCategory!! #
!=!!$ &
UnicodeCategory!!' 6
.!!6 7
NonSpacingMark!!7 E
)!!E F
sb"" 
."" 
Append"" 
("" 
c"" 
)""  
;""  !
}## 
var&& 
	resultado&& 
=&& 
sb&& 
.&& 
ToString&& '
(&&' (
)&&( )
.'' 
Replace'' 
('' 
$str'' 
,'' 
$str'' !
)''! "
.(( 
Replace(( 
((( 
$str(( 
,(( 
$str((  
)((  !
.)) 
Replace)) 
()) 
$str)) 
,)) 
$str))  
)))  !
.** 
Replace** 
(** 
$str** 
,** 
$str**  
)**  !
.++ 
Replace++ 
(++ 
$str++ 
,++ 
$str++  
)++  !
.,, 
Replace,, 
(,, 
$str,, 
,,, 
$str,, !
),,! "
;,," #
	resultado// 
=// 
Regex// 
.// 
Replace// %
(//% &
	resultado//& /
,/// 0
$str//1 7
,//7 8
$str//9 <
)//< =
;//= >
return11 
	resultado11 
.11 
Trim11 !
(11! "
)11" #
;11# $
}22 	
}99 
}:: É
îC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Exceptions\InvalidCredentialsException.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Exceptions/ 9
{ 
public		 

class		 '
InvalidCredentialsException		 ,
:		, -
	Exception		. 7
{

 
public '
InvalidCredentialsException *
(* +
)+ ,
{- .
}/ 0
public '
InvalidCredentialsException *
(* +
string+ 1
message2 9
)9 :
:; <
base= A
(A B
messageB I
)I J
{K L
}M N
public '
InvalidCredentialsException *
(* +
string+ 1
message2 9
,9 :
	Exception; D
innerExceptionE S
)S T
:U V
baseW [
([ \
message\ c
,c d
innerExceptione s
)s t
{u v
}w x
public '
InvalidCredentialsException *
(* +
string+ 1
name2 6
,6 7
object8 >
key? B
)B C
: 
base 
( 
$" 
$str 
{ 
name #
}# $
$str$ (
{( )
key) ,
}, -
$str- =
"= >
)> ?
{ 	
}
 
} 
} Î
êC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Exceptions\EntityNotFoundException.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Exceptions/ 9
{ 
public		 

class		 #
EntityNotFoundException		 (
:		) *
	Exception		+ 4
{

 
public #
EntityNotFoundException &
(& '
)' (
{) *
}+ ,
public #
EntityNotFoundException &
(& '
string' -
message. 5
)5 6
:7 8
base9 =
(= >
message> E
)E F
{G H
}I J
public #
EntityNotFoundException &
(& '
string' -
message. 5
,5 6
	Exception7 @
innerExceptionA O
)O P
:Q R
baseS W
(W X
messageX _
,_ `
innerExceptiona o
)o p
{q r
}s t
public #
EntityNotFoundException &
(& '
string' -
name. 2
,2 3
object4 :
key; >
)> ?
: 
base 
( 
$" 
$str 
{ 
name #
}# $
$str$ (
{( )
key) ,
}, -
$str- =
"= >
)> ?
{ 	
}
 
} 
} â
ïC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Exceptions\EntityAlreadyExistsException.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Exceptions/ 9
{ 
public		 

class		 (
EntityAlreadyExistsException		 -
:		. /
	Exception		0 9
{

 
public	 (
EntityAlreadyExistsException ,
(, -
)- .
{/ 0
}1 2
public	 (
EntityAlreadyExistsException ,
(, -
string- 3
message4 ;
); <
:= >
base? C
(C D
messageD K
)K L
{M N
}O P
public	 (
EntityAlreadyExistsException ,
(, -
string- 3
message4 ;
,; <
	Exception= F
innerExceptionG U
)U V
:V W
baseX \
(\ ]
message] d
,d e
innerExceptionf t
)t u
{v w
}x y
public	 (
EntityAlreadyExistsException ,
(, -
string- 3
name4 8
,8 9
object: @
keyA D
)D E
: 
base 
( 
$" 
$str 
{  
name  $
}$ %
$str% 2
{2 3
key3 6
}6 7
$str7 H
"H I
)I J
{	 

} 
} 
} Î
êC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\Exceptions\DatosFaltantesException.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /

Exceptions/ 9
{ 
public		 

class		 #
DatosFaltantesException		 (
:		) *
	Exception		+ 4
{

 
public #
DatosFaltantesException &
(& '
)' (
{) *
}+ ,
public #
DatosFaltantesException &
(& '
string' -
message. 5
)5 6
:7 8
base9 =
(= >
message> E
)E F
{G H
}I J
public #
DatosFaltantesException &
(& '
string' -
message. 5
,5 6
	Exception7 @
innerExceptionA O
)O P
:Q R
baseS W
(W X
messageX _
,_ `
innerExceptiona o
)o p
{q r
}s t
public #
DatosFaltantesException &
(& '
string' -
name. 2
,2 3
object4 :
key; >
)> ?
: 
base 
( 
$" 
$str 
{ 
name #
}# $
$str$ 1
{1 2
key2 5
}5 6
$str6 G
"G H
)H I
{ 	
}
 
} 
} ≈
àC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Responses\ResponseDTO.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
	Responses4 =
{ 
public		 

class		 
ResponseDTO		 
<		 
T		 
>		 
{

 
public 
bool 
Success 
{ 
get !
;! "
set# &
;& '
}( )
public 
T 
Data 
{ 
get 
; 
set  
;  !
}" #
public 
ErrorResponseDto 
Error  %
{& '
get( +
;+ ,
set- 0
;0 1
}2 3
} 
} à
çC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Responses\ErrorResponseDto.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
	Responses4 =
{ 
public		 

class		 
ErrorResponseDto		 !
{

 
public 
int 
	ErrorCode 
{ 
get  #
;# $
set% (
;( )
}* +
public 
string 
Message 
{ 
get  #
;# $
set% (
;( )
}* +
} 
} ù
êC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\ReceiveOrgData\ReceiveOrgData.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
ReceiveOrgData4 B
{ 
public 

class 
ReceiveOrgData 
{ 
public 
string 
ContOrgNombreP $
{% &
get( +
;+ ,
set- 0
;0 1
}2 3
public 
string 
ContOrgNombreS $
{% &
get' *
;* +
set, /
;/ 0
}1 2
} 
} Ôh
êC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\FichaProducto\ReceiveFichaDto.cs
	namespace		 	
DICREP		
 
.		 
EcommerceSubastas		 "
.		" #
Application		# .
.		. /
DTOs		/ 3
.		3 4
FichaProducto		4 A
{

 
public 

class 
FotografiaDto 
{ 
[ 	
Required	 
( 
ErrorMessage 
=  
$str! G
)G H
]H I
[ 	
Url	 
( 
ErrorMessage 
= 
$str =
)= >
]> ?
public 
string 
Url 
{ 
get 
;  
set! $
;$ %
}& '
[ 	
Required	 
( 
ErrorMessage 
=  
$str! S
)S T
]T U
[ 	
RegularExpression	 
( 
$str 1
,1 2
ErrorMessage3 ?
=@ A
$strB z
)z {
]{ |
public 
string 
Formato 
{ 
get  #
;# $
set% (
;( )
}* +
} 
public 

class 
InformeTecnicoDto "
{ 
[ 	
Required	 
( 
ErrorMessage 
=  
$str! J
)J K
]K L
[ 	
Url	 
( 
ErrorMessage 
= 
$str ?
)? @
]@ A
public 
string 
Url 
{ 
get 
;  
set! $
;$ %
}& '
[ 	
Required	 
( 
ErrorMessage 
=  
$str! V
)V W
]W X
[ 	
RegularExpression	 
( 
$str -
,- .
ErrorMessage/ ;
=< =
$str> r
)r s
]s t
public 
string 
Tipo 
{ 
get  
;  !
set" %
;% &
}' (
}   
public"" 

class""  
ContactoOrganismoDto"" %
{## 
[$$ 	
Required$$	 
($$ 
ErrorMessage$$ 
=$$  
$str$$! C
)$$C D
]$$D E
public%% 
int%% 
IdOrganismo%% 
{%%  
get%%! $
;%%$ %
set%%& )
;%%) *
}%%+ ,
['' 	
Required''	 
('' 
ErrorMessage'' 
=''  
$str''! D
)''D E
]''E F
[(( 	
RutValid((	 
((( 
ErrorMessage(( 
=((  
$str((! @
)((@ A
]((A B
public)) 
string)) 
Rut)) 
{)) 
get)) 
;))  
set))! $
;))$ %
}))& '
[++ 	
Required++	 
(++ 
ErrorMessage++ 
=++  
$str++! G
)++G H
]++H I
[,, 	
StringLength,,	 
(,, 
$num,, 
,,, 
MinimumLength,, (
=,,) *
$num,,+ ,
,,,, -
ErrorMessage,,. :
=,,; <
$str,,= {
),,{ |
],,| }
public.. 
string.. 
Nombre.. 
{.. 
get.. "
;.." #
set..$ '
;..' (
}..) *
[00 	
Required00	 
(00 
ErrorMessage00 
=00  
$str00! S
)00S T
]00T U
[11 	
EmailAddress11	 
(11 
ErrorMessage11 "
=11" #
$str11# U
)11U V
]11V W
public22 
string22 
Correo22 
{22 
get22 "
;22" #
set22$ '
;22' (
}22) *
[55 	
Required55	 
(55 
ErrorMessage55 
=55  
$str55! I
)55I J
]55J K
[66 	
RegularExpression66	 
(66 
$str66 @
,66@ A
ErrorMessage66B N
=66O P
$str66Q }
)66} ~
]66~ 
public77 
string77 
Telefono77 
{77  
get77! $
;77$ %
set77& )
;77) *
}77+ ,
}88 
public:: 

class:: !
DireccionOrganismoDto:: &
{;; 
[<< 	
Required<<	 
(<< 
ErrorMessage<< 
=<<  
$str<<! B
)<<B C
]<<C D
public== 
int== 
IdComuna== 
{== 
get== !
;==! "
set==# &
;==& '
}==( )
[?? 	
Required??	 
(?? 
ErrorMessage?? 
=??  
$str??! F
)??F G
]??G H
[@@ 	
StringLength@@	 
(@@ 
$num@@ 
,@@ 
ErrorMessage@@ '
=@@( )
$str@@* c
)@@c d
]@@d e
publicAA 
stringAA 
ComunaAA 
{AA 
getAA "
;AA" #
setAA$ '
;AA' (
}AA) *
[CC 	
RequiredCC	 
(CC 
ErrorMessageCC 
=CC  
$strCC! J
)CCJ K
]CCK L
[DD 	
StringLengthDD	 
(DD 
$numDD 
,DD 
ErrorMessageDD '
=DD( )
$strDD* g
)DDg h
]DDh i
publicEE 
stringEE 
	DireccionEE 
{EE  !
getEE" %
;EE% &
setEE' *
;EE* +
}EE, -
}FF 
publicHH 

classHH 
DetalleBienDtoHH 
{II 
[JJ 	
RequiredJJ	 
(JJ 
ErrorMessageJJ 
=JJ  
$strJJ! M
)JJM N
]JJN O
publicKK 
stringKK 
IdPublicacionBienKK '
{KK( )
getKK* -
;KK- .
setKK/ 2
;KK2 3
}KK4 5
[MM 	
RequiredMM	 
(MM 
ErrorMessageMM 
=MM  
$strMM! N
)MMN O
]MMO P
publicNN 
intNN 
IdCategoriaNN 
{NN  
getNN! $
;NN$ %
setNN& )
;NN) *
}NN+ ,
[PP 	
RequiredPP	 
(PP 
ErrorMessagePP 
=PP  
$strPP! R
)PPR S
]PPS T
[QQ 	
StringLengthQQ	 
(QQ 
$numQQ 
,QQ 
MinimumLengthQQ (
=QQ) *
$numQQ+ ,
,QQ, -
ErrorMessageQQ. :
=QQ; <
$strQQ= p
)QQp q
]QQq r
publicSS 
stringSS 
	CategoriaSS 
{SS  !
getSS" %
;SS% &
setSS' *
;SS* +
}SS, -
[VV 	
RequiredVV	 
(VV 
ErrorMessageVV 
=VV  
$strVV! B
)VVB C
]VVC D
[WW 	
StringLengthWW	 
(WW 
$numWW 
,WW 
MinimumLengthWW (
=WW) *
$numWW+ ,
,WW, -
ErrorMessageWW. :
=WW; <
$strWW= y
)WWy z
]WWz {
publicXX 
stringXX 
NombreXX 
{XX 
getXX "
;XX" #
setXX$ '
;XX' (
}XX) *
[[[ 	
Required[[	 
([[ 
ErrorMessage[[ 
=[[  
$str[[! G
)[[G H
][[H I
[\\ 	
StringLength\\	 
(\\ 
$num\\ 
,\\ 
ErrorMessage\\ (
=\\) *
$str\\+ `
)\\` a
]\\a b
public]] 
string]] 
Descripcion]] !
{]]" #
get]]$ '
;]]' (
set]]) ,
;]], -
}]]. /
public__ 
string__ 
Tamano__ 
{__ 
get__ "
;__" #
set__$ '
;__' (
}__) *
public`` 
string`` 
Color`` 
{`` 
get`` !
;``! "
set``# &
;``& '
}``( )
[cc 	
Requiredcc	 
(cc 
ErrorMessagecc 
=cc  
$strcc! D
)ccD E
]ccE F
[dd 	
Rangedd	 
(dd 
$numdd 
,dd 
intdd 
.dd 
MaxValuedd 
,dd 
ErrorMessagedd  ,
=dd- .
$strdd/ Q
)ddQ R
]ddR S
publicee 
intee 
Cantidadee 
{ee 
getee !
;ee! "
setee# &
;ee& '
}ee( )
publicgg 
stringgg 
TipoCantidadgg "
{gg# $
getgg% (
;gg( )
setgg* -
;gg- .
}gg/ 0
[ii 	
Requiredii	 
(ii 
ErrorMessageii 
=ii  
$strii! J
)iiJ K
]iiK L
[jj 	
Rangejj	 
(jj 
$numjj 
,jj 
doublejj 
.jj 
MaxValuejj $
,jj$ %
ErrorMessagejj& 2
=jj3 4
$strjj5 g
)jjg h
]jjh i
publickk 
decimalkk 
ValorEstimadokk $
{kk% &
getkk' *
;kk* +
setkk, /
;kk/ 0
}kk1 2
[mm 	
Requiredmm	 
(mm 
ErrorMessagemm 
=mm  
$strmm! I
)mmI J
]mmJ K
publicnn 
intnn 
IdEstadonn 
{nn 
getnn !
;nn! "
setnn# &
;nn& '
}nn( )
[pp 	
Requiredpp	 
(pp 
ErrorMessagepp 
=pp  
$strpp! M
)ppM N
]ppN O
[qq 	
StringLengthqq	 
(qq 
$numqq 
,qq 
ErrorMessageqq &
=qq' (
$strqq) V
)qqV W
]qqW X
publicrr 
stringrr 
Estadorr 
{rr 
getrr "
;rr" #
setrr$ '
;rr' (
}rr) *
publicss 
boolss 
Desmontabless 
{ss  !
getss" %
;ss% &
setss' *
;ss* +
}ss, -
publictt 
booltt 
BajaToxicidadtt !
{tt" #
gettt$ '
;tt' (
settt) ,
;tt, -
}tt. /
[ww 	
Requiredww	 
(ww 
ErrorMessageww 
=ww  
$strww! B
)wwB C
]wwC D
[xx 	
	MinLengthxx	 
(xx 
$numxx 
,xx 
ErrorMessagexx "
=xx# $
$strxx% M
)xxM N
]xxN O
publicyy 
Listyy 
<yy 
InformeTecnicoDtoyy %
>yy% &
InformesTecnicosyy' 7
{yy8 9
getyy: =
;yy= >
setyy? B
;yyB C
}yyD E
[{{ 	
Required{{	 
({{ 
ErrorMessage{{ 
={{  
$str{{! I
){{I J
]{{J K
public||  
ContactoOrganismoDto|| #
ContactoOrganismo||$ 5
{||6 7
get||8 ;
;||; <
set||= @
;||@ A
}||B C
[~~ 	
Required~~	 
(~~ 
ErrorMessage~~ 
=~~  
$str~~! J
)~~J K
]~~K L
public !
DireccionOrganismoDto $
Direcci√≥nOrganismo% 7
{8 9
get: =
;= >
set? B
;B C
}D E
}
ÄÄ 
public
ÇÇ 

class
ÇÇ 
FichaProductoDto
ÇÇ !
{
ÉÉ 
[
ÑÑ 	
Required
ÑÑ	 
(
ÑÑ 
ErrorMessage
ÑÑ 
=
ÑÑ  
$str
ÑÑ! H
)
ÑÑH I
]
ÑÑI J
public
ÖÖ 
List
ÖÖ 
<
ÖÖ 
FotografiaDto
ÖÖ !
>
ÖÖ! "
Fotografias
ÖÖ# .
{
ÖÖ/ 0
get
ÖÖ1 4
;
ÖÖ4 5
set
ÖÖ6 9
;
ÖÖ9 :
}
ÖÖ; <
[
áá 	
Required
áá	 
(
áá 
ErrorMessage
áá 
=
áá  
$str
áá! C
)
ááC D
]
ááD E
public
àà 
DetalleBienDto
àà 
DetalleBien
àà )
{
àà* +
get
àà, /
;
àà/ 0
set
àà1 4
;
àà4 5
}
àà6 7
}
ââ 
public
ãã 

class
ãã 
ReceiveFichaDto
ãã  
{
åå 
[
çç 	
Required
çç	 
(
çç 
ErrorMessage
çç 
=
çç  
$str
çç! G
)
ççG H
]
ççH I
public
éé 
FichaProductoDto
éé 
FichaProducto
éé  -
{
éé. /
get
éé0 3
;
éé3 4
set
éé5 8
;
éé8 9
}
éé: ;
}
èè 
}ëë ˚

åC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Empleado\LoginResponseDTO.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
Empleado4 <
{ 
public		 

class		 
LoginResponseDTO		 !
{

 
public 
string 
Token 
{ 
get !
;! "
set# &
;& '
}( )
public 
string 
Usuario 
{ 
get  #
;# $
set% (
;( )
}* +
public 
string 
NombreCompleto $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
int 
PerfilId 
{ 
get !
;! "
set# &
;& '
}( )
public 
int 

SucursalId 
{ 
get  #
;# $
set% (
;( )
}* +
public 
DateTime 

Expiration "
{# $
get% (
;( )
set* -
;- .
}/ 0
} 
} °
ãC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Empleado\LoginRequestDTO.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
Empleado4 <
{ 
public		 

class		 
LoginRequestDTO		  
{

 
public 
string 
Usuario 
{ 
get  #
;# $
set% (
;( )
}* +
public 
string 
Password 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
int 

AuthMethod 
{ 
get  #
;# $
set% (
;( )
}* +
} 
} œ
áC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Empleado\EmpleadoDTO.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
Empleado4 <
{ 
public 

class 
EmpleadoDTO 
{ 
public 
int 
EmpId 
{ 
get 
; 
set  #
;# $
}% &
public 
string 

EmpUsuario  
{! "
get# &
;& '
set( +
;+ ,
}- .
=/ 0
null1 5
!5 6
;6 7
public 
int 
EmpRut 
{ 
get 
;  
set! $
;$ %
}& '
public 
string 
	EmpRutDig 
{  !
get" %
;% &
set' *
;* +
}, -
=. /
null0 4
!4 5
;5 6
public 
string 
	EmpNombre 
{  !
get" %
;% &
set' *
;* +
}, -
=. /
null0 4
!4 5
;5 6
public 
string 
? 
EmpSegundoNombre '
{( )
get* -
;- .
set/ 2
;2 3
}4 5
public 
string 
EmpApellido !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
null2 6
!6 7
;7 8
public 
string 
? 
EmpSegundoApellido )
{* +
get, /
;/ 0
set1 4
;4 5
}6 7
public 
string 
? 
EmpAnexo 
{  !
get" %
;% &
set' *
;* +
}, -
public 
string 
	EmpCorreo 
{  !
get" %
;% &
set' *
;* +
}, -
=. /
null0 4
!4 5
;5 6
public 
bool 
	EmpActivo 
{ 
get  #
;# $
set% (
;( )
}* +
public 
DateTime 
? 
EmpFechaLog $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
DateTime 
? 
EmpFechaExp $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public"" 
int"" 
PerfilId"" 
{"" 
get"" !
;""! "
set""# &
;""& '
}""( )
public## 
int## 

SucursalId## 
{## 
get##  #
;### $
set##% (
;##( )
}##* +
public$$ 
int$$ 

AuthMethod$$ 
{$$ 
get$$  #
;$$# $
set$$% (
;$$( )
}$$* +
[&& 	
StringLength&&	 
(&& 
$num&& 
)&& 
]&& 
public'' 
string'' 
?'' 
PasswordHash'' #
{''$ %
get''& )
;'') *
set''+ .
;''. /
}''0 1
[)) 	
StringLength))	 
()) 
$num)) 
))) 
])) 
public** 
string** 
?** 
ClaveUnicaSub** $
{**% &
get**' *
;*** +
set**, /
;**/ 0
}**1 2
}.. 
}// ∆

îC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Empleado\Historial_MovimientosDTO.cs
	namespace

 	
DICREP


 
.

 
EcommerceSubastas

 "
.

" #
Application

# .
.

. /
DTOs

/ 3
.

3 4
Empleado

4 <
{ 
public 

class $
Historial_MovimientosDTO )
{ 
public 
DateTime 
? 
MovFecha !
{" #
get$ '
;' (
set) ,
;, -
}. /
public 
string 
? 
MovTipoCambio $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
string 
? 
MovValorAnterior '
{( )
get* -
;- .
set/ 2
;2 3
}4 5
public 
string 
? 
MovValorNuevo $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
int 
? 
EmpId 
{ 
get 
;  
set! $
;$ %
}& '
} 
} ˛
ÜC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Auth\JwtTokenResult.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
Auth4 8
{ 
public		 

class		 
JwtTokenResult		 
{

 
public 
string 
Token 
{ 
get !
;! "
set# &
;& '
}( )
public 
DateTime 

Expiration "
{# $
get% (
;( )
set* -
;- .
}/ 0
} 
} ≥
àC:\Users\sawad\Downloads\repositorios_gitlab_dicrep\api_remates_fase1\DICREP.EcommerceSubastas.Application\DTOs\Auth\FuncionalidadDTO.cs
	namespace 	
DICREP
 
. 
EcommerceSubastas "
." #
Application# .
.. /
DTOs/ 3
.3 4
Auth4 8
{ 
public 

class 
FuncionalidadDTO !
{ 
public 
int 
FuncionalidadId "
{# $
get% (
;( )
set* -
;- .
}/ 0
public 
string 
Nombre 
{ 
get "
;" #
set$ '
;' (
}) *
=+ ,
null- 1
!1 2
;2 3
public 
string 
EndpointApi !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
null2 6
!6 7
;7 8
public 
string 

MetodoHttp  
{! "
get# &
;& '
set( +
;+ ,
}- .
=/ 0
null1 5
!5 6
;6 7
public 
string 
? 
Grupo 
{ 
get "
;" #
set$ '
;' (
}) *
public 
bool 
EsMenu 
{ 
get  
;  !
set" %
;% &
}' (
=) *
false+ 0
;0 1
public 
bool 
Activo 
{ 
get  
;  !
set" %
;% &
}' (
=) *
true+ /
;/ 0
public 
string 
Codigo 
{ 
get "
;" #
set$ '
;' (
}) *
=+ ,
string- 3
.3 4
Empty4 9
!9 :
;: ;
} 
} 