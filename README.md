# Mini Helpdesk

ASP.NET Core MVC app for managing support tickets.

## How to run the application

1. Make sure SQL Server is running and reachable with the connection string
   in `HelpdeskMvc/appsettings.json` (defaults to `localhost,1433`, user `sa`).
2. From the `HelpdeskMvc` folder:
   ```
   dotnet run
   ```
3. The app creates the `HelpdeskMvcDb` database and schema automatically on
   startup (`Database.EnsureCreated()` in `Program.cs` — no migrations
   needed for this assignment).
4. Open the URL printed in the console, e.g. `http://localhost:5000/Tickets`.

## How to run the tests

From the repository root:
```
dotnet test HelpdeskMvc.Tests
```
The tests don't touch a real database — they use `FakeTicketRepository`,
an in-memory stand-in for `ITicketRepository`, so they only exercise the
Service layer's logic.

## Database

SQL Server, accessed through EF Core (`Microsoft.EntityFrameworkCore.SqlServer`).
Schema is created via `EnsureCreated()` rather than migrations.

## Where things are

- **Middleware**: `HelpdeskMvc/Middleware/RequestTimingMiddleware.cs` (logs
  method, path, status code, elapsed ms) and
  `HelpdeskMvc/Middleware/ExceptionHandlingMiddleware.cs` (global exception
  handling). Both are registered in `Program.cs`, near the top of the
  pipeline.
- **Transaction**: `HelpdeskMvc/Repositories/TicketRepository.cs`, method
  `AddTicketWithCommentAsync`. It wraps the ticket insert and the comment
  insert in one `Database.BeginTransactionAsync()` / `CommitAsync()` /
  `RollbackAsync()` block.
- **Tests**: `HelpdeskMvc.Tests/TicketServiceTests.cs` (5 tests, more than
  the required 3), using `HelpdeskMvc.Tests/Fakes/FakeTicketRepository.cs`.
- **Layers**: `Controllers/` (HTTP only) → `Services/` (business rules,
  validation) → `Repositories/` (EF Core data access) → `Data/AppDbContext.cs`.

## Answers

**Dlaczego kolejność middleware w Program.cs ma znaczenie?**
Każde wywołanie `app.Use...` owija wszystko, co zostało zarejestrowane
później — żądanie przechodzi przez middleware w kolejności rejestracji "w
dół", a odpowiedź wraca w odwrotnej kolejności "w górę". Dlatego np.
`ExceptionHandlingMiddleware` musi być zarejestrowany jako jeden z
pierwszych: żeby mógł złapać wyjątek rzucony przez cokolwiek dalej w
pipeline (w tym przez `RequestTimingMiddleware` czy przez MVC). Gdyby był
zarejestrowany na końcu, nie złapałby błędów z wcześniejszych middleware.

**Czym różni się app.Use od app.Run?**
`app.Use` (i `app.UseMiddleware<T>`) dodaje middleware, które może
wykonać coś przed i po wywołaniu kolejnego elementu w pipeline (przez
`await next(context)`) — pipeline jest kontynuowany. `app.Run` dodaje
middleware terminalne — nie przyjmuje `next` i zawsze kończy pipeline w
tym miejscu, więc nic zarejestrowane po nim się nie wykona.

**Dlaczego kontroler nie powinien zawierać całej logiki aplikacji?**
Kontroler ma odpowiadać tylko za warstwę HTTP: odebranie żądania,
wybranie odpowiedniego widoku/statusu. Jeśli reguły biznesowe (walidacja,
zmiana statusu, zapis w transakcji) trafią do kontrolera, trudno je
przetestować bez uruchamiania całego frameworka MVC, trudno je ponownie
wykorzystać (np. w innym kontrolerze albo w tle jako job), a kontroler
robi się długi i trudny w utrzymaniu. Wydzielenie tego do Service
pozwala testować logikę w izolacji, tak jak w `TicketServiceTests`.

**Co daje test jednostkowy warstwy Service?**
Pozwala sprawdzić logikę biznesową (walidację, zmianę statusu) szybko, bez
prawdziwej bazy danych i bez uruchamiania serwera HTTP — przez podstawienie
fake'owego repozytorium. Dzięki temu testy są szybkie, powtarzalne i łatwo
pokazują, która reguła biznesowa się zepsuła, jeśli coś przestanie działać.

**Co powinno się stać, jeśli zapis zgłoszenia się uda, ale zapis komentarza
zakończy się błędem?**
Cała operacja powinna zostać wycofana — w bazie nie powinno zostać ani
zgłoszenie, ani komentarz. Dlatego oba zapisy są wykonane w jednej
transakcji (`AddTicketWithCommentAsync`): jeśli drugi `SaveChangesAsync`
rzuci wyjątek, łapiemy go, wywołujemy `RollbackAsync()` i przekazujemy
wyjątek dalej (gdzie zostanie obsłużony globalnie przez
`ExceptionHandlingMiddleware`).
