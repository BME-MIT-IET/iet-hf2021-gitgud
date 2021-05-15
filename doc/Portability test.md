Portability:
Protability tesztre a Visual Studio .NET Portability Analyzer nevű extension használtuk. Hasznos funkció, ha szeretnénk az alakalmazásainkat multi-platformos támogatottságát bővíteni. Ha szeretnénk megtudni, mennyi munka szükséges ahhoz, hogy a .NET Framework alkalmazás futtatható legyen a .NET Core rendszeren, akkor a .NET Portability Analyzer elemzi a szerelvényeket, és részletes jelentést nyújt azokról a .NET API-ról, amelyek nem felelnek meg, hogy az alkalmazások vagy könyvtárak hordozhatóak legyenek a megadott célon. 

A Portability Summary fülön százalékosan megmutatja nekünk, hogy az egyes tartalmazott szerelvények mekkora része ültethető át az egyes változatokra.


The Details section of the report lists the APIs missing from any of the selected Targeted Platforms.
Target type: the type has missing API from a Target Platform
Target member: the method is missing from a Target Platform
Assembly name: the .NET Framework assembly that the missing API lives in.
Each of the selected Target Platforms is one column, such as ".NET Core": "Not supported" value means the API is not supported on this Target Platform.
Recommended Changes: the recommended API or technology to change to. Currently, this field is empty or out of date for many APIs. Due to the large number of APIs, we have a significant challenge to keep it up to date. We're looking at alternative solutions to provide helpful information to customers.



Unresolved assembly
This section contains a list of assemblies that are referenced by your analyzed assemblies and were not analyzed. If it's an assembly that you own, include it in the API portability analyzer run so that you can get a detailed, API-level portability report for it.
Eventually, the list should include all the third-party assemblies that your app depends on that have a version supporting your target platform.
