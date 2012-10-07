basesrc=Async.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

all: $(output)FibonacciAsync.exe $(output)TcpAsync.exe

$(output)FibonacciAsync.exe: FibonacciAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@

$(output)TcpAsync.exe: TcpAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@
