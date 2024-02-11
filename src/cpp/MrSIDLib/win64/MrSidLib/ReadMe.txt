========================================================================
    DYNAMIC LINK LIBRARY: MrSidLib-Projektübersicht 
========================================================================

Diese MrSidLib-DLL wurde vom Anwendungs-Assistenten für
Sie erstellt.  

Diese Datei bietet eine Übersicht über den Inhalt der einzelnen Dateien,
aus denen Ihre MrSidLib–Anwendung besteht.


MrSidLib.vcproj
    Dies ist die Hauptprojektdatei für VC++-Projekte, die mit dem Anwendungs-
    Assistenten generiert werden. 
    Sie enthält Informationen über die Version von Visual C++, mit der die
    Datei generiert wurde, sowie über die Plattformen, Konfigurationen und 
    Projektfeatures, die im Anwendungs-Assistenten ausgewählt wurden.

MrSidLib.cpp
    Dies ist die Hauptquelldatei der DLL.

	Da diese DLL keine Symbole exportiert, wird beim Erstellen keine
	LIB-Datei generiert. Wenn dieses Projekt eine Projektabhängigkeit
	eines anderen Projekts sein soll, müssen Sie Code hinzufügen, um  
	Symbole aus der DLL zu exportieren, damit eine Exportbibliothek  
	erstellt wird, oder Sie müssen in den Projekteigenschaften auf der
	Eigenschaftenseite "Allgemein" des Linkerordners die Eigenschaft 
	"Eingabebibliothek ignorieren" auf "Ja" festlegen.

/////////////////////////////////////////////////////////////////////////////
Weitere Standarddateien:

StdAfx.h, StdAfx.cpp
    Mit diesen Dateien werden eine vorkompilierte Header (PCH)-Datei
    mit dem Namen "MrSidLib.pch" und eine 
    vorkompilierte Typendatei mit dem Namen "StdAfx.obj" erstellt.

/////////////////////////////////////////////////////////////////////////////
Weitere Hinweise:

Der Anwendungs-Assistent weist Sie mit "TODO:"-Kommentaren auf Teile des
Quellcodes hin, die Sie ergänzen oder anpassen sollten.

/////////////////////////////////////////////////////////////////////////////