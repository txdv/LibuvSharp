BASESRC=Common.cs
OUTPUT=../bin/Debug/
INCLUDES=../bin/Debug/LibuvSharp.dll,System.Numerics,System.Net.Http.dll
SRC=$(shell ls *.cs | grep -v $(BASESRC))

BIN=$(addprefix $(OUTPUT), $(SRC:%.cs=%.exe))

all: $(BIN)

$(OUTPUT)%.exe: %.cs $(BASESRC)
	gmcs -unsafe -sdk:4.5 $^ -r:$(INCLUDES) -out:$@

clean:
	rm -rvf $(BIN)
