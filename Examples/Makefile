basesrc=Async.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

files= \
	$(output)FibonacciAsync.exe   \
	$(output)TcpAsync.exe         \
	$(output)UVDirectoryAsync.exe \
	$(output)DirectoryAsync.exe   \
	$(output)UVTimerAsync.exe     \

all: $(files)

$(output)%.exe: %.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

clean:
	rm -rvf $(files)
