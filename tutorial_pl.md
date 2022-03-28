ala ma kota i nie ma psa
# Tutorial Docker

## Troche teorii
Docker - oprogramowanie do konteneryzacji, czyli oddzielenia środowiska uruchomieniowego aplikacji od systemu operacyjnego.  
Pozwala na pracę lokalną oraz na prace jako system rozproszony - Docker Swarm.  
System pozwala również na wirtualizację sieci oraz dysków.   
Głównym przeznaczeniem jest oddzielenie środowiska uruchomieniowego aplikacji co pozwala na jednolitą pracę developerów niezależną od systemu operacyjnego który posiadają.

## 0) Wymagania
Do tego tutorialu będzie potrzebny [Docker](http://docker.com) 
którego instrukcje instalacji na swój system można można znaleźć [tutaj](https://docs.docker.com/install/).

## 1) Tworzenie pliku Dockerfile 
[Oficjalna dokumentacja](https://docs.docker.com/engine/reference/builder/)  
Plik Dockerfile instrukcją zbudowania obrazu na podstawie któego tworzone są kontenery.  

### Najważniejsze elementy:
> FROM base_image\[:tag\] \[AS stage\] - Instrukcja wskazujący obraz bazowy do wykorzystania np. `python` wtedy zostanie użyta najnowsza wersja lub `python:3.6.9` gdy potrzebujemy konkretnej wersji. Dodanie bloku `AS stage` pozwala na wielostopniową budowę i ostateczne zmniejszenie rozmiaru obrazu (co zostanie opisane szczegółowo później). Stroną z repozytorium obrazów bazowych jest [hub.docker.com](https://hub.docker.com) i znajdują się tam obrazy np. baz danych (postgres, mysql), czystych systemów (ubuntu, debian) oraz gotowych środowisk (python, node, dotnet).

> COPY \[--from=stage\] source destination - Instrukcja kopiująca dane z folderu budowania lub w przypadku użycia opcji `--from` z wybranego stopnia budowania. Instrukcja pozwala na kopiowanie katalogów jak również wielu plikow np. `packages*.json`.

> RUN - Instrukcja pozwala na wykonanie komendy w środowisku na etapie budowania obrazu. Częstym wykorzystaniem jest pobieranie paczke, budowanie i kompilacja np. `npm install`, `pip install -r requirements.txt`, `dotnet restore`, `dotnet publish` itd. 

> WORKDIR dir - Instrukcja zmieniająca katalog.

> CMD cmd - Instrukcja określająca komendę wykonywaną przez kontener. Może to być komendy pozwalające na ciągłe działanie kontenera np. `tail -f /dev/null` lub `dotnet appname.dll` ale można też wykonywac komendy kończące działanie jak np. `echo 1234` która wyświetli `1234` i zakończy działanie kontenera.

### Przykładowy plik Dockerfile:
```
FROM python:latest                  # Podstawowym obrazem jest ostatni obraz python 
COPY . .                            # Kopiowanie całego katalogu budowania do kontenera
RUN pip install -r requirements.txt # Wykonaj komendę pip instaującą zależności aplikacji
CMD python app.py                   # wykonanie aplikacji
```

Aby zbudować taki plik należy wykonać: `docker build $path` gdzie $path to będize ścieżka w której znajduje się plik.   
W celu łatwiejszego identyfikowania zbudowanych obrazów warto wykorzystać opcję `-t $name:$tag` gdzie $name to nazwa naszego obrazu np. backend/app/... a $tag oznacza tag obrazu np. latest/1.0.0/... .  

Uruchomienie kontenera korzystającego z takiego obrazu można wykonać za pomocą: `docker run --rm -it $name:$tag [cmd]` gdzie `--rm` oznacza że po zakończeniu działania kontenera czy to przez zakończenie komendy czy też w skutek przerwania działania (Ctrl - C) kontener zostanie automatycznie usunięty, `-it` pozwalają na interację z kontenerem w trakcie działania, `cmd` gdy określone nadpisuje `CMD` wewnątrz Dockerfile, co jest bardzo przydatne przy debugowaniu Dockerfile (np. wywołanie shella zamiast zadeklarowanej komendy).

Ważnym parametrem przy uruchamianiu kontenera jest również mapowanie portów za pomocą opcji `-p`. Np. `-p 80:80` <=> `-p 0.0.0.0:80:80` spowoduje mapowanie portu `80` hosta na dowolnym interfejsie na port `80` w kontenerze. Takie mapowanie moze stanowić zagrożenie dla bezpieczeństwa aplikacji więc częśto stosuje się mapowanie `127.0.0.1:80:80` pozwalające na dostęp tylko z komputera hosta.

### Następnym krokiem będize stworzenie wielostopniowego pliku Dockerfile
Docker idealnie nadaje się jako stabilne i deterministyczne środowisko do budowania aplikacji. Natomiast wiele plików jest niepotrzebnych w ostatecznym obrazie. Tu na scenę wkraczają wielostopniowe pliki Dockerfile. Naszym przykładem niech będzie Dockerfile dla aplikacji frontendowej. W pierwszym stopniu zbudujemy nasza aplikację a w następnym dodamy ztranspilowane i zminifikowane pliki statyczne do obrazu serwera HTTP nginx.

Stopień pierwszy
```
FROM node:latest AS build       # Wykorzystamy gotowy obraz środowiska nodejs
WORKDIR /app                    # Zmiana katalogu
COPY package*.json .            # Kopiowanie plików z zależnościami (wyjaśnienie: Ważne Informacje)
RUN npm install                 # Pobieranie zależności

COPY . .                        # Kopiowanie reszty plików aplikacji
RUN npm run serve               # Budowanie plików statycznych dla aplikacji frontendowej
```

Po wykonaniu tych kroków powinniśmy zbudować aplikację oraz otrzymać pliki statyczne wewnątrz katalogu `/app/dist/`. W drugim stopniu wykorzystamy plki ze stopnia pierwszego oraz pliki konfiguracyjne z hosta i zbudujemy gotowy w pełni skonfigurowany obraz serwera HTTP wraz z naszą aplikacją.

Stopień drugi
```
FROM nginx:latest AS release                      # Wykorzystamy obarz nginx
COPY --from=build /app/dist /var/www              # Skopiowanie statycznych plików do katalogu na aplikację
COPY ./nginx.conf /etc/nginx/conf.d/default.conf  # Podmiana domyslnego pliku konfiguracyjnego na własny
```

Obraz buduje się oraz uruchamia dokładnie tak samo jak przy jednostopniowym. Dzięki takiej konfiguracji nasze środowisko budowania jest stałe pomiędzy wszystkimi developerami a obraz otrzymany na wyjściu jest zdecydowanie mniejszy jako że nie posiada plików potrzebnych tylko do budowania.

### Ważne informacje:
- Docker cachuje poszczególne wartstwy dlatego jeśli nie będziemy zmieniać często zależnośći warto skopiować listy zależnośći (package.json/requirements.txt/app.csproj) i zainstalować zależnośći przed skopiowaniem reszty plików. Dzięki temu jeśli zmienią się pliki w projekcie ale nie zależnośći Docker może zaoszczędzić czas i skorzystać z cache'a budowania zamiast pobierać je jeszcze raz
- Przy budowaniu pliku Dockerfile Docker kopiuje katalog w którym się znajduje do kontekstu budowania. W przypadku dużych plików i katalogów (np. node_modules) które znajdują się w katalogu budowania warto dodać je do pliku .dockerignore który ma identyczną składnie jak pliku .gitignore.
- Każda instrukcja w Dockerfile jest pojedyńczą warstwą, która sama w sobie ma narzut. wykonując wiele komend RUN warto połączyć je w jedno:
```
...
RUN apt update
RUN apt install abc
RUN pip install efg 
...
```
-> 
```
...
RUN apt update && \
    apt install abc && \
    pip install efg 
...
```

### Ćwiczenie 1.
Utwórz plik Dockerfile dla aplikacji producer oraz consumer wykorzystując jako wzorzec aplikację backend.  
Opisz w sprawozdaniu co wykonuje każda linijka oraz jaki obraz bazowy zostanie wykorzystany w końcowych obrazach.  
UWAGA!: Ze względu na czas uruchamiania się i migracji backendu oraz RabbitMQ dodaj opóźnienie 30 sekund: `echo 30 && cmd` 

## 2) docker-compose
ToDo

## 3) Docker Swarm
ToDo

## [Feedback](https://forms.gle/UhTDD9n6Ys6ucHsh7)
