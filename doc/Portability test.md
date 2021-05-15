# Portability teszt
Protability tesztre a Visual Studio .NET Portability Analyzer nevű extension használtuk. Hasznos funkció, ha szeretnénk az alakalmazásainkat multi-platformos támogatottságát bővíteni. Ha kíváncsiak vagyunk arra, hogy mennyi munka szükséges ahhoz, hogy a .NET Framework alkalmazás futtatható legyen a .NET Core rendszeren is, akkor a .NET Portability Analyzer elemzi a szerelvényeket és részletes jelentést nyújt azokról a .NET API-ról, amelyek nem felelnek meg, hogy az alkalmazások vagy könyvtárak hordozhatóak legyenek a megadott célon. 

A __Portability Summary__ fülön százalékosan megmutatja nekünk, hogy az egyes tartalmazott szerelvények mekkora része ültethető át az egyes változatokra.

![1](/doc/images/port_summary.png)


A __Details__  felsorolja a kiválaszottt cél platformok bármelyikből hiányzó API-kat.

![2](/doc/images/port_details.png)

Az itt található információk:
- __Target type:__ Célplatformról hiányzó API típus
- __Target member:__ A metódus hiányzik a célplatformról
- __Assembly name:__ A .NET Framework szerelvény amelyben a hiányzó API él
- Minden kiválaszottt célplatformhoz tartozik egy oszlop, mint páldául ".NET Core": "Not supported" érték azt jelenti, hogy az API nem támogatott a célplatformon
- __Recommended Changes:__ az ajánlott API vagy technológia, amelyre váltani kell (Megjegyzés: Előfordulhat, hogy ez a rész üres, mivel a tool támogatója szerint az API-k nagy száma miatt jelentős kihívás számukra annak naprakészen tartása)


Végül pedig az __Unresolved assembly__ fülön annak a szerelvények listáját tartalmazza, amelyekre az elemzett összeállítások hivatkoznak de még nem elemeztek.
A listának tartalmaznia kell az összes third-party szerelvényeket, amelytől függ az alkalmazás, és amelynek verziója támogatja a célplatformot.

![3](/doc/images/port_unresolved.PNG)
