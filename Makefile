uv=libuv/libuv.a
gensrc=LibuvSharp/UVErrorCode.cs LibuvSharp/HandleType.cs LibuvSharp/Internal/RequestType.cs

all: libuv.so $(gensrc)

libuv/Makefile:
	git submodule sync
	git submodule update
	(cd libuv && ./autogen.sh && ./configure)

$(uv): libuv/Makefile
	make -C libuv

libuv.so: libuv/Makefile
	make -C libuv
	cp libuv/.libs/libuv.so ./libuv.so

generate: generate.c libuv.so
	$(CC) -Ilibuv/include/ -lpthread -ldl -lrt libuv.so -lm generate.c -o generate

LibuvSharp/UVErrorCode.cs: libuv/include/uv.h generate
	./generate err > $@

LibuvSharp/HandleType.cs: libuv/include/uv.h generate
	./generate handle > $@

LibuvSharp/Internal/RequestType.cs: libuv/include/uv.h generate
	./generate req > $@

clean:
	rm -rf libuv.so
	make -C libuv clean
	rm -rf $(gensrc)
