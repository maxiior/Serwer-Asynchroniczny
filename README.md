# Serwer-Asynchroniczny

W branchu main jest zawarta wersja konsolowa, a w brachu GUI wersja z interfejsem graficznym.

### Loginy i hasła:  
maksim:maksim  
maksim1:maxiior  
kacper2:kbuczko  
milosz33:milorl  
test22:test44  

### Instrukcja
Po uruchomieniu serwera mogą podłączać się do niego klienci. Po nazwiązaniu połączenia klient może się zalogować na jedno z wybranych powyżej kont, może założyć własne konto lub wyświetlić listę wszystkich użytkowników w bazie danych SQLite.  
Po zalogowaniu użytkownik może rozpocząć grę w "Kamień, papier, nożyce" z innym graczem, którego serwer wybiera na podstawie punktów ELO gracza, faktu, że jest zalogowany i nie jest aktualnie zajęty. Oprócz tego, gracz może podejrzeć swoje statystyki wybierając opcje "Show profile" z MENU, może zmienić hasło, usunąć konto lub wylogować się.  
Dostępna jest także opcja komunikacji między użytkownikami. Każdy z nich może nawiązać połączenie z wybranym z listy użytkownikiem, pisać z nim, a w celu zakońcenia konwersacji, wysłać do serwera komunikat "exit". 
