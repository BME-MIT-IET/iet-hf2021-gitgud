# BDD tesztek készítése Specflow segítségével

A BDD tesztek elkészítéséhez a Specflow (https://specflow.org/) Frameworkot válaszottuk, hogy megismerkedhessünk egy iparban széleskörben használt technológiával, és később akár saját projektjeinkben is alkalmazhassuk. A telepítés a Visual Studio saját extensionrendszerén keresztül történt (egy specflow specifikus külön branchen), ami után létrehoztuk a Specflow saját projektjét, és hozzáadtuk a tesztekhez szükséges .feature fájlt. Ezután mindenki megkapta a saját feladatát egy github Issue-n keresztül, majd mindenki létrehozta a saját .cs fájlját a tesztkód leírására, de csak miután a tesztet (Scenariot) definiálta a .feature fájlban.

![](/doc/images/specflow_1.png)

Ezek külön-külön el lettek készítve, majd a mergelést követően, amikor megbizonyosodtunk róla hogy a tesztek mind lefutnak, létrehoztuk az Issue megoldására szánt pull requestet. Alább a külön-külön tesztesetek dokumentációja látható, bár a Specflow sajátosságából kifolyólag nagyon olvasmányos kód van a .feature fájlban.

![](/doc/images/specflow_2.PNG)
Anna: A gráfok tesztelésével foglalkoztam, ezen belül is a gráfok létrehozásával és TripleStoreban való tárolásával. Az első tesztesetben beépített forrásból töltöttem be a gráfot, majd az Add metódus segítségével hozzáadtam a TripleStorehoz, ezután pedig teszteltem, hogy a gráf belekerült-e a Storeba. A második teszt során fájlból töltöttem be a gráfot, majd beállítottam a baseURI-t és megnéztem, hogy belekerült-e a gráf a TripleStoreba.

Abigél: Az egyik tesztesetben két gráf összeolvasztásával foglalkoztam, létrehozva a gráfokat adott számú triple-ökkel, és a merge után ellenőriztem, hogy az eredményként kapott gráfban a megfelelő számú triple-ök legyenek. A másik tesztben az URI-k feloldását vizsgáltam, megadtam egy gráfnak egy base URI-t, majd a gráfhoz hozzáadtam egy új node-ot egy adott útvonallal. Ellenőriztem, hogy az így kapott node URI-ja megegyezik-e azzal az URI-val, amit a gráf base URI és a megadott útvonal konkatenálásával kapunk meg.

Dávid: Én a gráfok összehasonlítására készített függvényeket teszteltem, az ahol a pozitív és a negatív eredmény is tesztelésre került. 
