#!/bin/sh

set -eu

ScriptDirectory="$(dirname "$(readlink -f "$0")")"
cd "$ScriptDirectory"

#- Globals
CommonCompilerFlags="-DOS_LINUX=1 -fsanitize-trap -nostdinc++"
CommonWarningFlags="-Wall -Wextra -Wconversion -Wdouble-promotion -Wno-sign-conversion -Wno-sign-compare -Wno-double-promotion -Wno-unused-but-set-variable -Wno-unused-variable -Wno-write-strings -Wno-pointer-arith -Wno-unused-parameter -Wno-unused-function -Wno-format"
LinkerFlags="-lm -ldl -lpthread -lX11 -lxcb -lGL -lGLX -lglfw -lXext -lGLdispatch -lXau -lXdmcp"

DebugFlags="-g -ggdb -g3"
ReleaseFlags="-O3"

ClangFlags="-fdiagnostics-absolute-paths -ftime-trace
-Wno-null-dereference -Wno-missing-braces -Wno-vla-extension -Wno-writable-strings -Wno-missing-field-initializers -Wno-address-of-temporary -Wno-int-to-void-pointer-cast"

GCCFlags="-Wno-cast-function-type -Wno-missing-field-initializers -Wno-int-to-pointer-cast"

#- Main

# Defaults
clang=1
gcc=0
debug=1
release=0

for Arg in "$@"; do eval "$Arg=1"; done
# Exclusive flags
[ "$release" = 1 ] && debug=0
[ "$gcc"     = 1 ] && clang=0

[ "$gcc"   = 1 ] && Compiler="g++"
[ "$clang" = 1 ] && Compiler="clang"

Flags="$CommonCompilerFlags"
[ "$debug"   = 1 ] && Flags="$Flags $DebugFlags"
[ "$release" = 1 ] && Flags="$Flags $ReleaseFlags"
Flags="$Flags $CommonCompilerFlags"
Flags="$Flags $CommonWarningFlags"
[ "$clang" = 1 ] && Flags="$Flags $ClangFlags"
[ "$gcc"   = 1 ] && Flags="$Flags $GCCFlags"
Flags="$Flags $LinkerFlags"

[ "$debug"   = 1 ] && Mode="debug"
[ "$release" = 1 ] && Mode="release"
printf '[%s mode]\n' "$Mode"
printf '[%s compile]\n' "$Compiler"

Build="../build"
mkdir -p "$Build"
cd "$Build"

Temp="temp_$Mode"

RaylibFiles="raudio rcore rmodels rshapes rtext rtextures utils"
RaylibSourceFiles=
for File in $RaylibFiles; do RaylibSourceFiles="$RaylibSourceFiles ../../code/raylib/${File}.c "; done
ObjFiles=
for File in $RaylibFiles; do ObjFiles="$ObjFiles $Temp/${File}.o "; done

# Based on raylib/projects/scripts/build-linux.sh
if ! [ -d "$Temp" ]
then
	printf 'Building raylib.\n'
	mkdir -p "$Temp"
	cd "$Temp"
	$Compiler $Flags -w -c -D_DEFAULT_SOURCE -DPLATFORM_DESKTOP -DGRAPHICS_API_OPENGL_33 $RaylibSourceFiles
	cd ..
fi

printf 'codootoor.cpp\n'
$Compiler -I./raylib $Flags -o codootoor.elf $ObjFiles ../code/codootoor.cpp $LinkerFlags
