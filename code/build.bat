@echo off

REM Based on raylib\projects\scripts\build-windows.bat

REM Go to parent dir
cd %~dp0

set GAME_NAME=codootoor.exe
set SOURCES=..\code\codootoor.cpp
set RAYLIB_SRC=..\code\raylib

IF NOT EXIST ..\build mkdir ..\build
pushd ..\build

call C:\msvc\setup_x64.bat

REM For the ! variable notation
setlocal EnableDelayedExpansion

REM Directories
set "ROOT_DIR=%CD%"
set "SOURCES=!ROOT_DIR!\!SOURCES!"
set "RAYLIB_SRC=!ROOT_DIR!\!RAYLIB_SRC!"

REM Flags
set OUTPUT_FLAG=/Fe: "!GAME_NAME!"
set COMPILATION_FLAGS=/std:c11 /Od /Zi /utf-8 /validate-charset /EHsc
set WARNING_FLAGS=/W3 /sdl
set SUBSYSTEM_FLAGS=/DEBUG
set LINK_FLAGS=/link kernel32.lib user32.lib shell32.lib winmm.lib gdi32.lib opengl32.lib
set VERBOSITY_FLAG=/nologo

echo [Debug build]

set "TEMP_DIR=temp\debug"

REM Build raylib if it hasn't been cached in TEMP_DIR
IF NOT EXIST !TEMP_DIR!\ (
  mkdir !TEMP_DIR!
  cd !TEMP_DIR!
  REM raylib source folder
  set "RAYLIB_DEFINES=/D_DEFAULT_SOURCE /DPLATFORM_DESKTOP /DGRAPHICS_API_OPENGL_33"
  set RAYLIB_C_FILES="!RAYLIB_SRC!\rcore.c" "!RAYLIB_SRC!\rshapes.c" "!RAYLIB_SRC!\rtextures.c" "!RAYLIB_SRC!\rtext.c" "!RAYLIB_SRC!\rmodels.c" "!RAYLIB_SRC!\utils.c" "!RAYLIB_SRC!\raudio.c" "!RAYLIB_SRC!\rglfw.c"
  set RAYLIB_INCLUDE_FLAGS=/I"!RAYLIB_SRC!" /I"!RAYLIB_SRC!\external\glfw\include"

  cl.exe /w /c !VERBOSITY_FLAG! !RAYLIB_DEFINES! !RAYLIB_INCLUDE_FLAGS! !COMPILATION_FLAGS! !RAYLIB_C_FILES! || exit /B

  REM Out of the temp directory
  cd !ROOT_DIR!
)

REM Build the actual game
cl.exe !VERBOSITY_FLAG! !COMPILATION_FLAGS! !WARNING_FLAGS! /c /I"!RAYLIB_SRC!" !SOURCES! || exit /B
cl.exe !VERBOSITY_FLAG! !OUTPUT_FLAG! "!ROOT_DIR!\!TEMP_DIR!\*.obj" *.obj !LINK_FLAGS! !SUBSYSTEM_FLAGS! || exit /B

popd