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

*******************************************************************************************************************************************************************************
C#/ASP.NET Core MVC паркинг система с VIP клиенти (README.md)
===========================================================

Системата дава възможност на потребителите да заредят автомобилите си на свободен позиции в паркинга и администратора да следи тях. Администраторите могат да променят цените за тарифиране на автомобили, а VIP клиентите имат специална схема за паркиране със създало месечно такса.

Системата е разработена с .NET 8 и ASP.NET Core MVC. Тя използва Entity Framework Core 8 за взаимодействието с SQL Server базата данни, а за управление на потребителските записи (логин, регистрация) и валидацията на формите използва ASP.NET Core Identity.

Основното приложение е `Controllers`, които съдържат HTTP маршрути и ViewModel свързване. Службата `Services` съдържа бизнес логиката за паркинга, включително паркирането на автомобилите, пристигането им, излизането от паркинг и таксуването. Моделите `Models`, които се взаимодействат с базата данни и входните модели за формите, също са част от службата.

Началната страница е изградена с Bootstrap и визуално вдъхновена от предоставения макет. Всички маршрути са защитени от достъп без вход или с неправилна роля, а логинят се задължително прави за целта.

## Функционалност

* Регистрация и вход с ASP.NET Core Identity; входът приема потребителско име или имейл.
* Клиентите виждат само свободните места и могат да заредят автомобил на избрано място.
* Администраторът вижда заетите места и автомобилите, регистрира пристигане и извършва изход/таксуване.
* Само администраторът може да преглежда и променя VIP клиенти, автомобили и месечни точки.
* Начална страница, изградена с Bootstrap и визуално вдъхновена от предоставения макет.

## Стартиране

Необходимо е инсталиран .NET 8 SDK и SQL Server LocalDB (или SQL Server инстанция).

1. Проверете `Parko/appsettings.json` и настройте `ConnectionStrings:DefaultConnection` за желаната SQL Server инстанция.
2. Възстановете пакетите и стартирайте приложението:

   ```powershell
   dotnet restore Parko.slnx
   dotnet run --project Parko/Parko.csproj
