basesrc=Common.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

files= \
	CrossAsync.exe       \
	DirectoryAsync.exe   \
	Fibonacci.exe        \
	FibonacciAsync.exe   \
	HashStream.exe       \
	HashStreamAsync.exe  \
	Stat.exe             \
	TcpAsync.exe         \
	UVDirectoryAsync.exe \
	UVTimerAsync.exe     \

binaries=$(addprefix $(output), $(files))

all: $(binaries)

$(output)%.exe: %.cs $(basesrc)
	gmcs -unsafe -sdk:4.5 $^ -r:$(includes) -out:$@

clean:
	rm -rvf $(binaries)
