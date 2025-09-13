dotnet build -c Release
rsync -a ./src/Dassie/bin/Release/net9.0/ ./build/
