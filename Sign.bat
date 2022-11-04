@echo off

set sha=35f4e53a88751bbe67eaa097513e3db1ad2f49e3
echo /// Signing all binaries ...
for /R "bin\Release" %%I in (*.exe) do "C:\Program Files (x86)\Windows Kits\10\tools\bin\i386\signtool.exe" sign /sha1 %sha% /t http://time.certum.pl /fd sha256 /v %%I
for /R "bin\Release" %%I in (*.dll) do "C:\Program Files (x86)\Windows Kits\10\tools\bin\i386\signtool.exe" sign /sha1 %sha% /t http://time.certum.pl /fd sha256 /v %%I
echo Operation completed.
timeout /t 5 >NUL
exit /b %ERRORLEVEL%