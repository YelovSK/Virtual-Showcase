# Virtual-Showcase

## INFO
Stranka - http://www.st.fmph.uniba.sk/~hampel1/bak/index.html

Bakalarka text - https://www.overleaf.com/read/jstzfmshnmnv

### 28.1.2022
1. kalman
- do prace napisat a porovnat
- pohladovo zavisle stereo je ta technika
Uvod do problematiky (1. sekcia):
1. teoreticke vychodiska - konvolucne, kalmanov
2. predchodzie prace
- prehlad problematiky - kalman filter, view dependent stereo, co som pouzil na detekciu tvare
- ako funguje stereoskopicke videnie, co je pasivny 3D televizor, pohladovo zavisle stereo, face detection (starsie, konvolucne neuronove siete), tracking by detection (vznika sum), vyhladzovanie trackovania (priemerovanie, kalmanov filter)
- hardwareova specifikacia - stereoskopicky displej, pasivne okuliare
- v technologiach mozno aj stereoskopicky displej
- potom neskor v specifikacii budu konkretnejsie veci
- potom implementacna, tam screenshoty, tabulky, a pod
2. starsia bakalarka github
3. stereo kamera
- vedla seba staci
4. projekcia
5. bakalarka text

- poslat na mail pdf ked bude done

## TODO tbd
- vyskusat kalman filter
    - funguje, neviem velmi co robia Q a R values. Neviem ci to je lepsie ako averaging. Vcelku podobne I guess.
## TODO done
- ked sa najde prva tvar, nehybat este, ukazat nech ide blizsie/dalej, az ked pristupi tak sa aktivuje (napr. 2 sekundy tam je)
    - vytvoreny issue
- 2 kamery	/ polovica obrazovky 1 oko, polovica druhe oho
    - vytvoreny issue
- spojazdnit kameru
    - nemam kompatibilny hardware, nejde switchnut na usb3 vision mode
- ci velkost tvare ovplyvnuje confidence
    - neovplyvnuje
- mozno otestovat ludi s 3D okuliarmi
    - funguje
- vediet ho umiestnit
    - zatial na klavesnici, rotacia, scale, pozicia, reset
- vybrat model v unity, nejak ho uploadnut v UI	// 3d obj zmeshovany
    - loader package = https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547
    - file explorer package = https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006
- rezim kurator, rezim navstevnik napr.
    - zatial sa v menu da loadnut