# Deployment segítése

A deployment segítésére a __Docker__-t választottuk. A Docker egy olyan szoftverplatform, amelyet a konténerek koncepcióján alapuló alkalmazások telepítésének megkönnyítésére fejlesztettek ki.

A platform képes becsomagolni az alkalmazásunkat egy gyengén izolált környezetbe amit konténernek hívunk. 
Az alapelemek a Dockerben az image-ek és a konténerek. Az image-ek olyanok, mint egy objektum orientált nyelvben az osztályok. Leírják, hogy milyen egy konténer, tehát egy image példánya a konténer.
A Dockerfile-ba írjuk le az image felépítését, amiből az image-ek létrehozhatók.
A Docker konténerek elkülönített környezetek, ahol az alkalmazások más folyamatok beavatkozása nélkül futatthatók.

Hasonlóan a virtuális gépek környezetéhez, az egyes konténerekhez specifikus számítási erõforrásokat allokálhatnak. A virtuális gépekkel ellentétben a Docker nem igényel hardveres emulációt, hanem minden gazdagép fizikai hardverét használja.

Az image fájlokat a Docker Hub-on tároljuk, ez a Docker által biztosított hivatalos nyilvántartás.

A GitHub Actions támogatja a Docker image automatikus létrehozását. Ehhez egy Dockerfile és egy actino.yml-re van szükségünk.

A Dockerfile instrukciókat tartalmaz, felépítése:

![](\images\docker_inst.png)

Action lefutása után a Docker Hub-on megjelenik a build:

![](\images\docker_buidl.png)

Végül pedig az image:

![](\images\docker_image.png)


