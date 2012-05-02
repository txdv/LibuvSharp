
http=http-parser/http_parser.o
uv=libuv/uv.a

all: libuv.so LibuvSharp/Internal/uv_err_code.cs

$(http):
	make -C http-parser http_parser.o

$(uv):
	make -C libuv uv.a

libuv.so: $(http) $(uv)
	$(CC) -shared -o libuv.so `find libuv/ -name *.o` $(http)

generate: generate.c
	$(CC) -Ilibuv/include/ `find libuv/ -name ev.o` libuv/uv.a -lm generate.c -o generate

LibuvSharp/Internal/uv_err_code.cs: libuv/include/uv.h generate
	./generate > $@

clean:
	make -C http-parser clean
	make -C libuv clean
	rm -rf libuv.so



