# Unity Giriş + Yükleme Ekranı Kurulumu

## 1) Build Settings sahneleri
`File > Build Settings` içine şu sırayla ekle:
1. `Menu` (giriş ekranı)
2. `Loading` (yükleme ekranı)
3. `Main` (oyun sahnesi)

## 2) Menu sahnesi
1. Yeni sahne oluştur: `Menu.unity`
2. Canvas oluştur (UI Toolkit değil, UGUI)
3. 2 buton ekle:
   - `StartButton`
   - `QuitButton`
4. Boş obje ekle: `MainMenuManager`
5. `MainMenuController` scriptini ekle
   - `Gameplay Scene Name`: `Main`
   - `Loading Scene Name`: `Loading`
6. Buton OnClick bağlantıları:
   - StartButton -> `MainMenuController.StartGame()`
   - QuitButton -> `MainMenuController.QuitGame()`

## 3) Loading sahnesi
1. Yeni sahne oluştur: `Loading.unity`
2. Canvas içine Slider + Text ekle
3. Boş obje ekle: `LoadingManager`
4. `LoadingScreenController` scriptini ekle
5. Inspector alanları:
   - `Progress Slider` -> slider referansı
   - `Progress Text` -> text referansı
   - `Fallback Scene Name` -> `Main`

## 4) Çalıştırma
Play'e `Menu` sahnesinden bas:
- Start -> Loading sahnesi -> Main sahnesi
- Quit -> Editor'de log üretir, build'de oyunu kapatır

## Olası bug notu
Eğer loading ekranı takılı kalırsa çoğunlukla sebep Build Settings'e `Main` sahnesinin eklenmemesidir.
