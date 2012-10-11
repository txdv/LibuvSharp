basesrc=Async.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

all: \
	$(output)FibonacciAsync.exe   \
	$(output)TcpAsync.exe         \
	$(output)UVDirectoryAsync.exe \
	$(output)DirectoryAsync.exe   \
	$(output)UVTimerAsync.exe     \

$(output)FibonacciAsync.exe: FibonacciAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

$(output)TcpAsync.exe: TcpAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

$(output)UVDirectoryAsync.exe: UVDirectoryAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

$(output)DirectoryAsync.exe: DirectoryAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

$(output)UVTimerAsync.exe: UVTimerAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@
