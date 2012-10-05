
uv=libuv/uv.a
ev=libuv/src/unix/ev/ev.o
gensrc=LibuvSharp/Internal/uv_err_code.cs LibuvSharp/HandleType.cs LibuvSharp/RequestType.cs

all: libuv.so $(gensrc)

libuv/Makefile:
	git submodule sync
	git submodule update

$(uv): libuv/Makefile
	make -C libuv uv.a

libuv.so: $(uv)
	$(CC) -shared libuv/uv.a $(ev) -o libuv.so

generate: generate.c libuv.so
	$(CC) -Ilibuv/include/ -lpthread -ldl -lrt libuv.so $(ev) -lm generate.c -o generate

LibuvSharp/Internal/uv_err_code.cs: libuv/include/uv.h generate
	./generate err > $@

LibuvSharp/HandleType.cs: libuv/include/uv.h generate
	./generate handle > $@

LibuvSharp/RequestType.cs: libuv/include/uv.h generate
	./generate req > $@
clean:
	make -C libuv clean
	rm -rf libuv.so generate $(gensrc)



