:run.bat
@echo off
cd .\SMA Project 2 Version 1\bin\Debug
@echo Demonstrating Project  
@echo off
@echo -------------------------------------------------------------------------
@echo                    TYPE ANALYZER
@echo -------------------------------------------------------------------------

@echo ***********************************************************************
@echo                  Demonstrating Requirement 1
@echo ***********************************************************************

@echo Project is implemented in C# using the facilities of the .Net Framework Class Library (FCL), version 4.5, as provided in the EECS clusters.
Executive.exe "TestFiles" *.txt /s

@echo ***********************************************************************
@echo                  Demonstrating Requirement 2
@echo ***********************************************************************
@echo A. The Analyzer shall provide a command line option /S indicating that all subdirectories rooted at the path are to be searched for files matching all supplied patterns. If there is no /S on the  command line only the cited path will be searched.
@echo B. Shall display function sizes, and complexities for each function in each file, and a summary for all the files in the specified set.

Executive.exe "TestFiles" *.cs /s

@echo ***********************************************************************
@echo                  Demonstrating Requirement 3
@echo ***********************************************************************
@echo -----------------------------------------------------------------------
@echo  Shall provide a command line option, /R, which, when present causes the analyzer to display the relationships between all types defined in the file set, e.g., inheritance, composition, aggregation, and using relationships instead of the function sizes and complexitie
@echo -----------------------------------------------------------------------

Executive.exe "TestFiles" *.cs /r

@echo ***********************************************************************
@echo                  Demonstrating Requirement 4
@echo ***********************************************************************
@echo -----------------------------------------------------------------------
@echo  Shall display all output on standard output3. The analyzer shall provide an option, /X, which when present causes the output to also be written to a file in XML format
@echo -----------------------------------------------------------------------

Executive.exe "TestFiles" *.cs /x
cd..
cd..
cd..