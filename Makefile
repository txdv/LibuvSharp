
uv=libuv/uv.a
gensrc=LibuvSharp/Internal/uv_err_code.cs LibuvSharp/Internal/UvHandleType.cs LibuvSharp/Internal/UvRequestType.cs

all: libuv.so $(gensrc)

libuv/Makefile:
	git submodule sync
	git submodule update

$(uv): libuv/Makefile
	make -C libuv uv.a

libuv.so: $(uv)
	$(CC) -shared -o libuv.so `find libuv/ -name *.o`

generate: generate.c libuv.so
	$(CC) -Ilibuv/include/ -lm -lpthread -ldl -lrt libuv.so generate.c -o generate

LibuvSharp/Internal/uv_err_code.cs: libuv/include/uv.h generate
	./generate err > $@

LibuvSharp/Internal/UvHandleType.cs: libuv/include/uv.h generate
	./generate handle > $@

LibuvSharp/Internal/UvRequestType.cs: libuv/include/uv.h generate
	./generate req > $@
clean:
	make -C libuv clean
	rm -rf libuv.so generate $(gensrc)



