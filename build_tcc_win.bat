set vcvarsall="C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat"

if '%1' == '' (

    rm -rf ./tccbin/win32
    cmd /c %0 x86
    cmd /c %0 x64
    
) else (

    mkdir .\tccbin\win32\%1
    call %vcvarsall% %1
    pushd tinycc\win32
    if '%1' == 'x86' (
        call build-tcc.bat -t 32 -c cl
    ) else (
        call build-tcc.bat -t 64 -c cl
    )
    cp libtcc.dll ../../tccbin/win32/%1
    cp -r lib ../../tccbin/win32/%1
    cp -r include ../../tccbin/win32/%1
    popd

)
