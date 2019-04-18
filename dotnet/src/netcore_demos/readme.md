The netcore demo is share the same source code of .NET framework version 
located under ..\demos directory with the same project name.

Only the project file of the netcore demo is located under each project.

# Build and run the demo
* Enter one of the demo directory, such as checksyntax
* run `dotnet restore` to restore all of the required files if necessary.
* run `dotnet msbuild` to build the project.
* run `dotnet run` to the program.


pass argument `/t mysql` to application:
`dotnet run -- /t mysql`