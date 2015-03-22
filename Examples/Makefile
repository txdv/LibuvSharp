basesrc=Common.cs
output=../bin/Debug/
includes=../bin/Debug/LibuvSharp.dll,System.Numerics,System.Net.Http.dll
src=$(shell ls *.cs | grep -v $(basesrc))

binaries=$(addprefix $(output), $(src:%.cs=%.exe))

all: $(binaries)

$(output)%.exe: %.cs $(basesrc)
	gmcs -unsafe -sdk:4.5 $^ -r:$(includes) -out:$@

clean:
	rm -rvf $(binaries)
