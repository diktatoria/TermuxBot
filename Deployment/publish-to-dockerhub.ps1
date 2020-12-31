Set-Location -Path ../
docker build --file Deployment/Dockerfile --tag schnitzel9999/termuxbot .
docker push schnitzel9999/termuxbot

Set-Location -Path ./Deployment