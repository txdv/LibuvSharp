basesrc=Async.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics

$(output)FibonacciAsync.exe: FibonacciAsync.cs $(basesrc)
	gmcs -sdk:4.5 $^ -r:$(includes) -out:$@
