PAGES=output/

.PHONY: monodocs html edit copy

monodocs:
	monodocer -assembly:../bin/Debug/LibuvSharp.dll -path:monodocs/en -pretty

edit:
	monodoc --edit monodocs/en


html:
	monodocs2html --debug monodocs/en -o html

copy: html
	cp html/index.html $(PAGES)/documentation.html
	cp -rvf html/Sundown/ $(PAGES)

clean:
	rm -rf html
