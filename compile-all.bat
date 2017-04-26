@echo off
find . -iname "*.tidl" | grep good | xargs "%~dp0\tidl2java.bat"
find . -iname "*.tidl" | grep good | xargs "%~dp0\tidl2cs.bat"
