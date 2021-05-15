<h1> Statikus tesztelés: </h1>

A Statikus teszteléshez SonarQube-ot használtunk. ( https://www.sonarqube.org/ )
Letöltöttük a szükséges fájlokat, majd a Sonarcube local server indítása után hozzáfértünk a még üres Project Analyzer felülethez.

![1](static_1)

Ezután a megfelelő (C#/.Net Framework) scanner letöltése után (felül látható a pontos scanner verzió és állomány), egy sikeres buildelést, és token generálást követően már láttuk is a projektben lévő problémákat.

![2](static_2)

Ezután már kategóriák szerint böngészve tudtunk külön-külön foglalkozni az egyes problémákkal, főleg bugokkal, és valamennyi code smell-el. Ezekhez külön brancheket csináltunk, ahol hasznos névvel és leírással commitoltuk a fixeket, majd a branchekhez tartozó pull requesteket belinkeltük egy Static Testing Issue-ba. A pull requesteket egy másik feladaton dolgozó kollega ellenőrizte, majd mergelte, és így le tudtuk zárni a Static Testing Issue-t.

A különálló fixekkel kapcsolatban meg lehet tekinteni az Issue-ban a pull requestekhez fűzött megjegyzéseket!
