#!/bin/bash
platform="$(uname)"
arch="$(uname -p)"

if [ "$arch" == "x86_64" ]; then
    arch="x64"
fi

rm -rf ./tccbin/$platform/
mkdir -p ./tccbin/$platform/
mkdir -p ./tccbin/$platform/$arch/lib
pushd tinycc
make clean
make
cp libtcc1.a bcheck.o bt-exe.o bt-log.o ../tccbin/$platform/$arch/lib/
make clean
make libtcc.so
cp libtcc.so ../tccbin/$platform/$arch/
popd

