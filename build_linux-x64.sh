dotnet publish -c release -r linux-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --self-contained true /p:DefineConstants="STANDALONE" -o ./build
