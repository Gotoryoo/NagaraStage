program = gel.exe
cxx = g++
libdir = /cygdrive/c/OpenCV/OpenCV-2.4.2-x86/build/lib/Debug
lib = opencv_core242d opencv_highgui242d opencv_imgproc242d
include = /cygdrive/c/OpenCV/OpenCV-2.4.2-x86/build/include

edit: main.o
	$(cxx) -o $(program) main.o
	rm main.o
	echo "Build Complete\n"

main.o:main.cpp
	$(cxx) -c main.cpp -L$(libdir) -l$(lib) -I$(include) -DTest -D_Win32=0