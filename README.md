# Parko

An ASP.NET Core MVC application for managing a parking lot, vehicles, and VIP clients.

## Features

- ASP.NET Core Identity registration and sign-in using a username or email address.
- Customers can view all parking spaces, see which are available or occupied, and park their vehicle in an available space.
- Administrators can view the parking lot and vehicle records, register arrivals, release vehicles, and handle charges.
- Only administrators can view and manage VIP clients, vehicles, and monthly points.
- Administrators can remove registered customer accounts; customers can permanently close their own account after confirming their password.
- A Bootstrap-based homepage inspired by the supplied design.

Authentication is required to access the parking lot. Server-side authorization restricts administrative routes to the `Admin` role; hiding links in the UI is not the only protection. Identity accounts are separate from VIP records in `Clients`, and registering an account does not create a VIP client. Customer parking records store the Identity user ID, and customers cannot release vehicles or change rates.

The parking lot starts with 100 spaces. Standard vehicles receive the first 15 minutes free; up to 6 hours are charged at €1.20/hour, from 6 to 12 hours at €1/hour, and over 12 hours at €12/day. VIP vehicles receive the same free first 15 minutes and are then charged €1.20/hour; an available VIP point covers one parking session. VIP points reset to 40 on the first day of each month.

## Technology and structure

- .NET 8, ASP.NET Core MVC, Entity Framework Core 8, and SQL Server.
- `Controllers`: MVC routes and HTTP/ViewModel binding.
- `Services`: `ParkingService` for parking business logic and `IAccountService`/`AccountService` for Identity operations.
- `Data/ParkoDbContext`: parking/VIP models and Identity tables in the same SQL Server database.
- `Models`: EF Core models and input models for MVC forms.
- `Views`: Razor pages; UI styles and components use the locally supplied Bootstrap assets.

Database and Identity operations are asynchronous. Validation, price calculations, and license plate normalization are synchronous in-memory logic.

## Getting started

Requires the .NET 8 SDK and SQL Server LocalDB (or another SQL Server instance).

1. Check `Parko/appsettings.json` and configure `ConnectionStrings:DefaultConnection` for your SQL Server instance.
2. Restore packages and start the application:

   ```powershell
   dotnet restore Parko.slnx
   dotnet run --project Parko/Parko.csproj
   ```

3. Open the local URL shown in the console. On startup, the application automatically applies pending EF Core migrations.

Registration requires a unique email address and a password with at least 8 characters, an uppercase and lowercase letter, a digit, and a special character. Identity locks accounts after five failed sign-in attempts.

In the `Development` environment, the application automatically creates an `admin` administrator account with the password `admin` if it does not already exist. These weak demo credentials are intended for local testing only. In `Production`, set `AdminAccount__Password` as a protected environment variable/secret (and optionally `AdminAccount__UserName`) before startup; the application refuses to start if the administrator password is missing. Use a unique strong password and never expose the development account to the internet.

---

# Parko – паркинг система с VIP клиенти

Parko е уеб приложение на ASP.NET Core MVC за управление на паркинг, автомобили и VIP клиенти. Клиентите си избират свободно място и паркират, а администраторите следят паркинга, регистрират пристигания и излизания и се грижат за таксуването.

Приложението е на .NET 8. За достъп до SQL Server се използва Entity Framework Core 8, а регистрацията и входът са изградени върху ASP.NET Core Identity.

## Какво може приложението

- Регистрация и вход през ASP.NET Core Identity. За вход става и с потребителско име, и с имейл.
- Клиентите виждат всички места в паркинга, знаят кои са свободни и кои заети и могат да паркират на свободно място.
- Администраторите виждат паркинга и записите за автомобилите, регистрират пристигане, освобождават автомобили и се занимават с таксите.
- Само администраторите могат да разглеждат и управляват VIP клиентите, техните автомобили и месечните точки.
- Администраторите могат да изтриват регистрирани клиентски акаунти. Клиентите могат сами да закрият акаунта си окончателно, като потвърдят паролата си.
- Началната страница е изградена с Bootstrap и следва предоставения дизайн.

Достъпът до паркинга изисква вход в системата. Административните маршрути са защитени на сървъра и са достъпни само за ролята `Admin`, така че скриването на връзки в интерфейса не е единствената защита. Акаунтите в Identity са отделени от VIP записите в `Clients`, а обикновената регистрация не прави никого VIP клиент. Записите за паркиране пазят идентификатора на потребителя от Identity. Клиентите не могат да освобождават автомобили, нито да променят тарифите.

## Тарифи

Паркингът започва със 100 места.

- **Обикновени автомобили:** първите 15 минути са безплатни. До 6 часа се плаща 1,20 €/час, от 6 до 12 часа – 1 €/час, а над 12 часа – 12 € на ден.
- **VIP автомобили:** първите 15 минути също са безплатни, след което се плаща 1,20 €/час. Една налична VIP точка покрива едно паркиране.
- **VIP точки:** на първо число от всеки месец се нулират до 40.

## Технологии и структура

- .NET 8, ASP.NET Core MVC, Entity Framework Core 8 и SQL Server.
- `Controllers` – MVC маршрутите и свързването на HTTP заявките с моделите.
- `Services` – `ParkingService` съдържа бизнес логиката на паркинга, а `IAccountService`/`AccountService` обслужва операциите с Identity.
- `Data/ParkoDbContext` – моделите за паркинга и VIP клиентите и таблиците на Identity са в една и съща SQL Server база.
- `Models` – моделите за EF Core и входните модели за MVC формите.
- `Views` – Razor страниците. Стиловете и компонентите ползват локално добавените Bootstrap файлове.

Работата с базата данни и с Identity е асинхронна. Валидирането, изчисляването на цените и нормализирането на регистрационните номера са синхронна логика в паметта.

## Стартиране

Нужни са .NET 8 SDK и SQL Server LocalDB (или друг SQL Server).

1. Отворете `Parko/appsettings.json` и задайте `ConnectionStrings:DefaultConnection` според вашия SQL Server.
2. Възстановете пакетите и стартирайте приложението:

   ```powershell
   dotnet restore Parko.slnx
   dotnet run --project Parko/Parko.csproj
   ```

3. Отворете локалния адрес, който се показва в конзолата. При стартиране приложението само прилага чакащите EF Core миграции.

## Регистрация и акаунти

За регистрация е нужен уникален имейл и парола поне от 8 символа, съдържаща главна и малка буква, цифра и специален знак. След пет неуспешни опита за вход Identity заключва акаунта.

## Администраторски акаунт

В среда `Development` приложението само създава администратор с потребителско име `admin` и парола `admin`, ако такъв още не съществува. Тези слаби данни са само за локално тестване.

В `Production` преди стартиране трябва да зададете `AdminAccount__Password` като защитена променлива на средата или тайна (по желание и `AdminAccount__UserName`). Ако паролата липсва, приложението отказва да тръгне. Ползвайте уникална и сигурна парола и никога не оставяйте тестовия акаунт достъпен от интернет.
