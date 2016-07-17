for /f %%i in ('dir /a:d /b "../nil.js/"') do mklink /j %%i "../nil.js/%%i"
for %%i in (../nil.js/*.cs) do mklink %%i "../nil.js/%%i"