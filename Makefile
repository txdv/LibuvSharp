
http=http-parser/http_parser.o
uv=libuv/uv.a

all: libuv.so

$(http):
	make -C http-parser http_parser.o

$(uv):
	make -C libuv uv.a

libuv.so: $(http) $(uv)
	$(CC) -shared -o libuv.so `find libuv/ -name *.o` $(http)


clean:
	make -C http-parser clean
	make -C libuv clean
	rm -rf libuv.so



