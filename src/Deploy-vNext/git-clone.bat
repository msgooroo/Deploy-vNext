set GIT_PATH=C:\Program Files (x86)\Git\bin
set SRC_PATH=D:\web\msg-dev\src
set SSH_PATH=D:\web\msg-dev\msgooroo-bitbucket
set REPOSITORY=git@bitbucket.org:msgooroo/msgooroo.git

call "%GIT_PATH%\ssh-agent.exe" > __ssh-agent.out

FOR /F "eol=; tokens=1* delims=;" %%i in ('findstr /v echo __ssh-agent.out') do @set %%i
del __ssh-agent.out

call "%GIT_PATH%\ssh-add.exe" "%SSH_PATH%"


call "%GIT_PATH%\git.exe" clone %REPOSITORY% "%SRC_PATH%"

call "%GIT_PATH%\ssh-agent.exe" -k
