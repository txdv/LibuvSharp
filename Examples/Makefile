basesrc=Async.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

files= \
	DirectoryAsync.exe   \
	FibonacciAsync.exe   \
	Fibonacci.exe        \
	HashStream.exe       \
	TcpAsync.exe         \
	UVDirectoryAsync.exe \
	UVTimerAsync.exe     \

binaries=$(addprefix $(output), $(files))

all: $(binaries)

$(output)%.exe: %.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

clean:
	rm -rvf $(binaries)
