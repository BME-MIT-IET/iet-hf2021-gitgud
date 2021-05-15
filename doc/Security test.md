Security test:

Security Code Scan
Az átvizsgálás után a Visual Studio Error List ablakában jelennek meg a biztonsági figyelmeztetések:

A Security Code Scan átvizsglásával a következő típusú hibákat kaptuk:
SCS0007 - XML eXternal Entity Injection (XXE)
The XML parser is configured incorrectly. The operation could be vulnerable to XML eXternal Entity (XXE) processing.
Háttér:
Néhány alkalmazás az XML formátumot használja a kliens és a szerver közötti kommunikációhoz. Ezek az alkalmazások valamilyen könyvtárat vagy API-t használnak, hogy feldolgozzák az XML adatokat. 
Egy támadó XML fájlokat tölthet fel, vagy kártékony tartalmakat illeszthet be XML fájlokba.
Kockázat
Egy támadó XXE segítségével fájlokat tud olvasni a szerveren (pl.: /etc/passwd), de akár DOS támadást (Billion laughs) is indíthat az alkalmazás ellen. Extrém esetekben akár server-side request forgery támadást is indíthat.
Egy ilyen támadás adatvesztéshez, adat szivárgáshoz vezet, más támadásokkal együtt akár teljes hoszt átvétel is lehet a következménye, ami súlyosan károsítja a vállalat imidzsét és komoly pénzügyi veszteségekhez vezethet.

Forrás: https://axen-cyber.com/hu/owasp-top-10-serulekenyseg-xxe/
SCS0006 - Weak hashing function
MD5 or SHA1 have known collision weaknesses and are no longer considered strong hashing algorithms.
SCS0005 - Weak Random Number Generator
The random numbers generated could be predicted.
Risk
The use of a predictable random value can lead to vulnerabilities when used in certain security critical contexts.
SCS0015 - Hardcoded Password
The password configuration to this API appears to be hardcoded.
Risk
If hard-coded passwords are used, it is almost certain that malicious users will gain access through the account in question.
Az adott hibára duplán kattintva elnavigál minket a kódban a megfelelő sorhoz. A hiba kódra kattintva pedig böngészőben megnyitja a hozzá tartozó leírást. Itt megtalálhatjuk miért kockázatos a hiba, illetve megoldást is kínál nekünk.


Fortify on Demand Extension for Visual Studio
A Foritfy on Demand egy szolgáltatást nyújt nekünk amivel biztonsági hibákat gyorsan és egyszerűen felderíthetünk.

Az átvizsgálás után egy külön ablakban, Analysis Results néven jelenik meg az eredmény a Visual Studioban. A Fortify először súlyosság (Critical, High) alapján besorolja a biztonsági hibákat, majd azon belül a típus szerint csoportosítja.

A hibára kattintva megnyitja a kérdéses kód részletet, illetve egy Issue Summary ablakban Details fül alatt leírja a hiba okát illetve a Recommendations fül alatt pedig ajánlást ad arra, hogyan lehet feloldani a hibát.
A Fortify által felderített biztonsági hibák a következők voltak:
Serializable Delegate (Critical)
A delegált típus a metódushívásra való hivatkozás tárolására szolgál, amely később meghívható a felhasználói kódban. A Delegate objektum sorosított folyamata nem alkalmas tartós tárolásra vagy távoli alkalmazásba továbbítására, mert a támadó a metódus információkat helyettesítheti olyanokkal, amelyek a rosszindulatú objektum-grafikonokra mutatnak, ami tetszőleges kódfuttatási sebezhetőséget okozhat, annak hivatkozása vagy visszasorosítás után.
Null Dereferences (High)
    A legtöbb null-pointer probléma általános szoftver-megbízhatósági problémákat eredményez, de ha a támadó szándékosan kiválthatja a null-pointer dereferenciát, a támadó képes lehet a kapott kivétel felhasználásával megkerülni a biztonsági logikát, vagy arra késztetni az alkalmazást, hogy felfedje a hibakeresési információkat, amelyek értékesek lehetnek a későbbi támadások tervezésében.
Hardcoded Password (High)
Egy támadó hozzáférhet jogosulatlanul az alkalmazáshoz, felhasználói adatokat lophat, bizalmas információkat szivárogtathat ki. Egy adminisztrátori fiók kompromittálása esetén, teljesen átveheti az uralmat az applikáció felett.
Privacy Violation (High)
A privát információk, például az ügyfél jelszavak vagy a társadalombiztosítási számok nem megfelelő kezelése veszélyeztetheti a felhasználók adatait ami adatvédelmi jogsértést is eredményezhet.
Unreleased Resource: Streams (High)
A program potenciálisan nem tudja felszabadítani a rendszererőforrást. A legtöbb kiadatlan erőforrás probléma általános szoftver-megbízhatósági problémákat eredményez, de ha a támadó szándékosan kiválthatja az erőforrás szivárgást, előfordulhat, hogy a támadó az erőforráskészlet kimerítésével szolgáltatásmegtagadással járó támadást (Denial of Service vagy DoS), más néven túlterheléses támadást intézhet



SonarQube




    A SonarQube 29 security problémát talált a projekt átvizsgálása során.
A SonarQube Security Hotspots fül alatt 4 féle fő problémát különböztet meg.
Ezek a problémakörök a Weak Priority, Insecure Configuration, Object Injection és others.
A Weak Cryptography MEDIUM review priority besorolást kapott, míg az Insecure Configuration, Object Injection és Others pedig Low review priority.
Object Injection és Others pedig Low prioritást.

Az object injection probléma sok helyen megjelenik. A deszerializációs folyamat során 
kivonjuk az adatokat a sorosított objektumból és közvetlenül rekonstruálja azokat 
konstruktorok hívása nélkül. Így a konstruktorokban megvalósított adatellenőrzés megkerülhető, 
ha a sorosított objektumokat egy támadó vezérli. A probléma súlyossága attól függ, hogy a konstruktorban
van-e megvalósítva adat validáció vagy, hogy a deszerializált objektumokon keresztül nem történik 
biztonsági ellenőrzés. Számos esetben a Core library-ben mindkét eset fennáll. A probléma megoldható ha a 
deszerializációs művelet végén kikényszerítjük ugyanazokat a validációs ellenőrzéseket, mint alap esetben 
a konstruktorban, különösen akkor ha a sorosított objektumokat egy támadó vezérli. Megfelelő megoldás lehet például,
ha az ISerializable típust használja a deszerializáció ellenőrzésére, és ugyan azokat az ellenőrzéséket végre hajtja 
a konstruktoron belül, mint a deszerializálás során alap esetben.

Az Others problémáknak két fő típusát érzékeli a rendszer. Az OS parancsok végrehajtása során és főleg abban az esetben 
ha például nem adjuk meg a teljes elérési utat, abban az esetben rendkívül nagy felületet biztosítunk a támadóknak, pédául 
a környezeti változók módosításán keresztül vagy akár egy birtokolt mappán keresztül. Alapvetően saját gépen nem okozhat problémát,
de csak abban az esetben, ha a könyvtár ahol elhelyezkedik a program a mi irányításunk alatt van és senki nem fér hozzá. A probléma megoldható 
például ha teljesen minősített, abszolút elérési utat használunk a végrehajtandó OS parancs alkalmazásához.

Az Others problémák közül kiemelt másik biztonsági hiba forrás a protokoll használaton alapszik. Számos helyen az alkalmazás http-t használ kommunikációra a 
https helyett, amiből hiányzik az adat elkódolása. Ezzel elveszti azt a lehetőségét is, hogy egy authentikált kapcsolatot építsen a két végpont között a kommunikáció során.
Ez azt jelenti, hogy bárki aki le tudja hallgatni a forgalmat a hálózatról, az el tudja olvasni, megváltoztatni, vagy esetleg megfertőzni a szállított tartalmat. 
Emellett a http protokoll használata számos böngésőben már elavult technológiának számít. Mivel a könyvtár használata során akár fontos adatokat is cserélhetünk a kommunikáció 
során így mindenképpen problémát  okozhat ez a biztonsági hiba.

A konfigurációs hibáknál megjelenik egy hiba alacsony prioritási szinttel megjelölve. A Cross-Origin Resource Sharing policy használata érzékeny pont lehet biztonsági szempontból.
A böngészők azonos származási házirendje alapértelmezés szerint és biztonsági okokból megakadályozza, hogy a javascript kezelőfelülete cross-origin HTTP-kérelmet hajtson végre egy 
erőforrás számára, amelynek eredete (tartománya, protokollja vagy portja) eltér a sajátjától. A kért cél válaszként további HTTP fejléceket csatolhat CORS néven, amelyek irányelvekként 
működnek a böngészőhöz, és megváltoztatják a hozzáférés-vezérlési házirendet / ellazítják ugyanazt az eredetirendet. 
A Access-Control-Allow-Origin fejlécet csak megbízható originra és meghatározott erőforrásokra szabad beállítani. Csak kijelölt, megbízható tartományokat engedélyezheti az Access-Control-Allow-Origin fejlécben.
A tiltást vagy bármely tartomány engedélyezését részesítse előnyben az engedélyezőlistán szereplő domainek listáján (ne használjon * helyettesítő karaktert, és vakon visszaadott origin fejléc tartalmát ellenőrzés nélkül).

A Weak Cryptography szekcióba 7 darab problémát talált a SonarQube. Számos helyen a szoftverben SHA-1 vagy MD5 hashelést használ, amik már nem számítanak biztonságosnak, mert lehetséges, hogy ütközés lép fel. (Egy kis számítás elég ahhoz, hogy kettő vagy több esetben ugyan azt a hash-t állítsa elő).
Ez abban az esetben okozhat problémát a szoftverben ha például egyediség biztosításához generálunk értékeket, mint például a HandlerHelper.cs osztályban. A probléma megoldására szolgálhat ha például a biztonságosabb verzióját használjuk, 
ami például az SHA-512. De ha password hasheléshez kell akkor javasolt olyan algoritmust választani ami lassabb, hogy védjen a brute force támadások ellen.

A pszeudorandom szám generátorok (PRNGs) biztonság érzékenyek. Amikor a szoftver kiszámítható értékeket generál egy kiszámíthatatlanságot igénylő kontextusban, előfordulhat, hogy a támadó kitalálja a következő generálandó értéket, és ezt a találgatást felhasználhatja egy másik felhasználó megszemélyesítéséhez vagy az érzékeny információkhoz való hozzáféréshez.
Mivel a System.Random osztály álvéletlenszám-generátorra támaszkodik, nem használható biztonsági szempontból kritikus alkalmazásokhoz vagy bizalmas adatok védelméhez. Ebben az összefüggésben a kriptográfiailag erős véletlenszám-generátorra (RNG) támaszkodó System.Cryptography.RandomNumberGenerator osztályt kell használni. Számos osztályban felmerül a probléma mint például a RandomFunction.cs és RandFunction.cs osztályokban

Alapvetően a biztonsági hibák nem okoznak nagy problémát a szoftver szempontjánól mivel nem kriptográfiai alkalmazásként használjuk, de számos esetben sok adat és lekérdezés esetén a gyenge kódolók okozhatnak problémát bizonyos esetekben, amiket a fent említett módon el lehet kerülni.