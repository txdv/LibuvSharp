uv=libuv/libuv.a
gensrc=LibuvSharp/Internal/uv_err_code.cs LibuvSharp/HandleType.cs LibuvSharp/Internal/RequestType.cs

all: libuv.so $(gensrc)

libuv/Makefile:
	git submodule sync
	git submodule update

$(uv): libuv/Makefile
	make -C libuv libuv.a

libuv.so: libuv/Makefile
	make -C libuv libuv.so
	cp libuv/libuv.so ./libuv.so

generate: generate.c libuv.so
	$(CC) -Ilibuv/include/ -lpthread -ldl -lrt libuv.so -lm generate.c -o generate

LibuvSharp/Internal/uv_err_code.cs: libuv/include/uv.h generate
	./generate err > $@

LibuvSharp/HandleType.cs: libuv/include/uv.h generate
	./generate handle > $@

LibuvSharp/Internal/RequestType.cs: libuv/include/uv.h generate
	./generate req > $@

clean:
	rm -rf libuv.so
	make -C libuv clean
	rm -rf $(gensrc)
