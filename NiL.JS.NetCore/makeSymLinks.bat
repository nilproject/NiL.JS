for /f %%i in ('dir /a:d /b "../nil.js/"') do rmdir "./%%i"
for /f %%i in ('dir /a:d /b "../nil.js/"') do mklink /j %%i "./../nil.js/%%i"