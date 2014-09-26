set GIT_PATH=<{GitPath}>
set SRC_PATH=<{Path}>\src
set SSH_PATH=<{SshKeyPath}>
set REPOSITORY=<{Repository}>
set BRANCH=<{Branch}>
set REMOTE=<{Remote}>

call "%GIT_PATH%\ssh-agent.exe" > __ssh-agent.out

FOR /F "eol=; tokens=1* delims=;" %%i in ('findstr /v echo __ssh-agent.out') do @set %%i
del __ssh-agent.out

call "%GIT_PATH%\ssh-add.exe" "%SSH_PATH%"

cd "%SRC_PATH%"
call "%GIT_PATH%\git.exe" pull %REMOTE% %BRANCH%
call "%GIT_PATH%\git.exe" log -n1

call "%GIT_PATH%\ssh-agent.exe" -k
