obj=src/unix/async.o src/unix/core.o src/unix/dl.o src/unix/error.o src/unix/fs.o src/unix/getaddrinfo.o src/unix/loop.o src/unix/loop-watcher.o src/unix/pipe.o src/unix/poll.o src/unix/process.o src/unix/signal.o src/unix/stream.o src/unix/tcp.o src/unix/thread.o src/unix/threadpool.o src/unix/timer.o src/unix/tty.o src/unix/udp.o src/unix/linux/linux-core.o src/unix/linux/inotify.o src/unix/linux/syscalls.o src/fs-poll.o src/inet.o src/uv-common.o src/unix/ev/ev.o
OBJ=$(addprefix libuv/, $(obj))

uv=libuv/libuv.a
ev=libuv/src/unix/ev/ev.o
gensrc=LibuvSharp/Internal/uv_err_code.cs LibuvSharp/HandleType.cs LibuvSharp/Internal/RequestType.cs

all: libuv.so $(gensrc)

libuv/Makefile:
	git submodule sync
	git submodule update

$(uv): libuv/Makefile
	make -C libuv libuv.a

libuv.so: $(uv)
	$(CC) -shared $(uv) -o libuv.so $(OBJ)

generate: generate.c libuv.so
	$(CC) -Ilibuv/include/ -lpthread -ldl -lrt libuv.so $(ev) -lm generate.c -o generate

LibuvSharp/Internal/uv_err_code.cs: libuv/include/uv.h generate
	./generate err > $@

LibuvSharp/HandleType.cs: libuv/include/uv.h generate
	./generate handle > $@

LibuvSharp/Internal/RequestType.cs: libuv/include/uv.h generate
	./generate req > $@
clean:
	make -C libuv clean
	rm -rf libuv.so generate $(gensrc)



