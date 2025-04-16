for /f "tokens=2*" %%a in ('reg query "HKLM\SOFTWARE\TIM\TIM's Devices" /v "Path" /reg:32') do set Pfad=%%b
if NOT %errorlevel%==0 goto BatchEnd
xcopy *.* "%Pfad%" /y
if NOT %errorlevel%==0 goto BatchEnd
reg add "hklm\software\TIM\TIM's Devices" /v "Version" /f /reg:32 /d %1%
:BatchEnd

 

