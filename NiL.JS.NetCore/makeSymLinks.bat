for /f %%i in ('dir /a:d /b "../nil.js/"') do rd %%i
for /f %%i in ('dir /a:d /b "../nil.js/"') do mklink /j %%i "./../nil.js/%%i"