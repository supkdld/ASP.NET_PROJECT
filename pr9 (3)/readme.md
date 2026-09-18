# Практическая работа №9. Web API с паттерном DTO и CRUD-операциями

## 1. Цель работы

На основе базы данных `pharmacy` (аптека) и модели Entity Framework Core, созданных в прошлом семестре, реализовать Web API на ASP.NET Core с применением паттерна DTO и полным набором операций Create, Read, Update, Delete и Patch **для всех моделей** базы данных.

Новая база данных не создавалась. Используется существующая БД `pharmacy` на SQL Server (`miuuu\SQLEXPRESS`) и существующий контекст `PharmacyContext`.

## 2. Используемые технологии

| Компонент | Версия |
|---|---|
| .NET / ASP.NET Core | 9.0 |
| Entity Framework Core (SqlServer, Design, Tools) | 9.0.0 |
| Swashbuckle.AspNetCore (Swagger UI) | 7.2.0 |
| СУБД | Microsoft SQL Server Express |

## 3. Модель данных и маршруты API

В модели 8 сущностей. Для каждой созданы 4 DTO, маппер и контроллер с 6 эндпоинтами. Всего в API **48 эндпоинтов**.

| Сущность | Таблица БД | Назначение | Контроллер | Маршрут |
|---|---|---|---|---|
| `ProductCategory` | `product_categories` | категории товаров | `ProductCategoriesController` | `api/product-categories` |
| `Product` | `products` | товары аптеки | `ProductsController` | `api/products` |
| `Role` | `roles` | роли пользователей | `RolesController` | `api/roles` |
| `User` | `users` | пользователи | `UsersController` | `api/users` |
| `Cart` | `carts` | корзины пользователей | `CartsController` | `api/carts` |
| `CartItem` | `cart_items` | позиции корзины | `CartItemsController` | `api/cart-items` |
| `Order` | `orders` | заказы | `OrdersController` | `api/orders` |
| `OrderItem` | `order_items` | позиции заказа | `OrderItemsController` | `api/order-items` |

Связи между таблицами (внешние ключи):

```
roles ──< users ──< carts ──< cart_items >── products >── product_categories
              └───< orders ──< order_items >──┘
```

- у роли много пользователей, у пользователя много корзин и заказов;
- у корзины и заказа много позиций;
- каждая позиция корзины или заказа ссылается на товар, товар ссылается на категорию.

## 4. Что было изменено в исходном проекте

Исходный проект был шаблоном ASP.NET Core Web API с моделями, полученными через Scaffold-DbContext. Изменения:

1. **Удалён шаблонный код**: `WeatherForecast.cs` и `Controllers/WeatherForecastController.cs`.
2. **Строка подключения вынесена в конфигурацию.** В `PharmacyContext` она была жёстко прописана в методе `OnConfiguring` (с предупреждением `#warning` от генератора). Метод удалён, строка перенесена в `appsettings.json` в секцию `ConnectionStrings:PharmacyConnection`.
3. **Контекст зарегистрирован в DI-контейнере** в `Program.cs` через `AddDbContext<PharmacyContext>`, контроллеры получают его через конструктор.
4. **Подключён Swagger.** Шаблон .NET 9 использует пакет `Microsoft.AspNetCore.OpenApi`, который генерирует только JSON-документ без интерфейса. Он заменён на `Swashbuckle.AspNetCore`, который даёт Swagger UI. В `launchSettings.json` включено автоматическое открытие страницы `/swagger`.
5. **Убраны лишние `using`** (`System`, `System.Collections.Generic`) из файлов моделей: в проекте включён `ImplicitUsings`, эти пространства имён подключаются автоматически.
6. **Добавлены** папки `DTOs` (32 класса), `Mappers` (8 классов), базовый контроллер `ApiControllerBase` и 8 контроллеров.
7. **Обновлён `ProjectASPNET.http`**: примеры запросов ко всем 48 эндпоинтам для проверки из Visual Studio.

## 5. Структура проекта

```
ProjectASPNET/
├── ProjectASPNET.slnx
└── ProjectASPNET/
    ├── Controllers/
    │   ├── ApiControllerBase.cs            общие методы для всех контроллеров
    │   ├── ProductCategoriesController.cs  api/product-categories
    │   ├── ProductsController.cs           api/products
    │   ├── RolesController.cs              api/roles
    │   ├── UsersController.cs              api/users
    │   ├── CartsController.cs              api/carts
    │   ├── CartItemsController.cs          api/cart-items
    │   ├── OrdersController.cs             api/orders
    │   └── OrderItemsController.cs         api/order-items
    ├── DTOs/
    │   ├── ProductCategories/   ProductCategoryDto, Create-, Update-, PatchProductCategoryDto
    │   ├── Products/            ProductDto, Create-, Update-, PatchProductDto
    │   ├── Roles/               RoleDto, Create-, Update-, PatchRoleDto
    │   ├── Users/               UserDto, Create-, Update-, PatchUserDto
    │   ├── Carts/               CartDto, Create-, Update-, PatchCartDto
    │   ├── CartItems/           CartItemDto, Create-, Update-, PatchCartItemDto
    │   ├── Orders/              OrderDto, Create-, Update-, PatchOrderDto
    │   └── OrderItems/          OrderItemDto, Create-, Update-, PatchOrderItemDto
    ├── Mappers/
    │   ├── ProductCategoryMapper.cs, ProductMapper.cs, RoleMapper.cs, UserMapper.cs
    │   └── CartMapper.cs, CartItemMapper.cs, OrderMapper.cs, OrderItemMapper.cs
    ├── Models/                  модель БД (существующая)
    │   ├── PharmacyContext.cs
    │   └── Cart.cs, CartItem.cs, Order.cs, OrderItem.cs, Product.cs, ProductCategory.cs, Role.cs, User.cs
    ├── Properties/launchSettings.json
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Program.cs
    └── ProjectASPNET.http
```

DTO разложены по подпапкам для удобства навигации, но все находятся в одном пространстве имён `ProjectASPNET.DTOs`.

## 6. Паттерн DTO

**DTO (Data Transfer Object)** — простой класс, который описывает, какие данные передаются между клиентом и сервером. Контроллеры не принимают и не возвращают Entity-классы (`Product`, `User` и т.д.), а работают только с DTO. Это даёт следующее:

- клиент не может передать поля, которые формирует сервер (`Id`, дата создания);
- в ответ не попадают навигационные свойства (`Category`, `OrderItems`, `User` …), поэтому нет циклических ссылок при сериализации и лишних данных;
- наружу не отдаются секретные данные (пароль пользователя);
- изменение модели БД не ломает контракт API;
- для каждой операции задаются свои правила валидации.

### 6.1. Четыре вида DTO для каждой сущности

| DTO | Операция | Какие поля содержит | Обязательность |
|---|---|---|---|
| `EntityDto` | ответ клиенту (GET, POST) | все поля модели, включая `Id` и `CreatedAt` | — |
| `CreateEntityDto` | POST | все поля, кроме `Id` и `CreatedAt` | обязательны поля, которые в БД `NOT NULL` |
| `UpdateEntityDto` | PUT | все поля, кроме `Id` и `CreatedAt` | **все** поля обязательны |
| `PatchEntityDto` | PATCH | все поля, кроме `Id` и `CreatedAt` | все поля nullable (`string?`, `int?`, `decimal?`, `bool?`) |

### 6.2. Правила, общие для всех DTO

**Имена полей.** Первичный ключ в DTO называется `Id` (в моделях — `IdProduct`, `IdUser` и т.д.). Остальные поля названы так же, как в модели.

**Поле `CreatedAt`.** Дата создания есть только в двух таблицах: `carts.created_date` и `orders.order_date`. В `CartDto` и `OrderDto` эти поля называются `CreatedAt` и заполняются сервером при создании значением `DateTime.UtcNow`. В остальных таблицах даты создания нет, поэтому их DTO содержат только `Id` и поля модели. Столбцы в БД не добавлялись, так как по заданию используется существующая база.

**Модификатор `required`.** Свойство с модификатором `required` (C# 11) обязательно должно присутствовать в JSON. Без него пропущенное числовое поле молча получило бы значение `0`, а это ошибка, которую сложно заметить. System.Text.Json учитывает `required` и возвращает 400, если поле не передано.

**Обязательность в PUT для nullable-полей.** Некоторые поля в БД допускают `NULL` (например, `DescriptionProduct`, `ImageUrl`, `PasswordUser`). В `UpdateEntityDto` они объявлены как `required string?`: поле обязано присутствовать в запросе, но его значение может быть `null`. Так PUT действительно заменяет объект целиком: чтобы очистить описание товара, нужно явно передать `"descriptionProduct": null`.

**Атрибуты валидации** соответствуют ограничениям столбцов БД:

- `[Required]` — строка не может быть пустой или состоять из пробелов;
- `[StringLength(n)]` — длина строки как в столбце `varchar(n)` / `nvarchar(n)`;
- `[Range]` — для денежных полей диапазон по типу `decimal(10,2)` / `decimal(12,2)`, для количества минимум 1, для остатка на складе минимум 0, для внешних ключей минимум 1.

Атрибуты `Range` и `StringLength` пропускают `null`, поэтому в `PatchEntityDto` проверяются только реально переданные поля.

**Ограничение PATCH.** В PATCH значение `null` означает «поле не передано, не менять». Поэтому через PATCH нельзя установить nullable-полю значение `null`. Для этого используется PUT.

### 6.3. DTO по сущностям

Обозначения: **✔** — поле обязательно; **○** — необязательно (можно не передавать); **—** — поля нет в этом DTO.

#### ProductCategory

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdCategory` | `int` | ✔ | — | — | — | генерирует БД |
| `NameOfCategory` | `NameOfCategory` | `string` | ✔ | ✔ | ✔ | `string?` | до 50 символов, уникально |

#### Product

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdProduct` | `int` | ✔ | — | — | — | генерирует БД |
| `NameOfProduct` | `NameOfProduct` | `string` | ✔ | ✔ | ✔ | `string?` | до 50 символов |
| `DescriptionProduct` | `DescriptionProduct` | `string?` | ✔ | ○ | ✔ (может быть null) | `string?` | — |
| `Price` | `Price` | `decimal` | ✔ | ✔ | ✔ | `decimal?` | 0 … 99 999 999.99 |
| `CategoryId` | `CategoryId` | `int` | ✔ | ✔ | ✔ | `int?` | категория должна существовать |
| `InStock` | `InStock` | `int` | ✔ | ✔ | ✔ | `int?` | ≥ 0 |
| `IsActive` | `IsActive` | `bool?` | ✔ | ○ | ✔ (может быть null) | `bool?` | если не передано при создании — `true` (значение по умолчанию в БД) |
| `ImagePath` | `ImagePath` | `string?` | ✔ | ○ | ✔ (может быть null) | `string?` | до 255 символов |
| `ImageUrl` | `ImageUrl` | `string?` | ✔ | ○ | ✔ (может быть null) | `string?` | до 500 символов |

#### Role

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdRole` | `int` | ✔ | — | — | — | генерирует БД |
| `NameOfRole` | `NameOfRole` | `string` | ✔ | ✔ | ✔ | `string?` | до 50 символов, уникально |

#### User

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdUser` | `int` | ✔ | — | — | — | генерирует БД |
| `Login` | `Login` | `string` | ✔ | ✔ | ✔ | `string?` | до 50 символов, уникален |
| `PasswordUser` | `PasswordUser` | `string?` | **—** | ○ | ✔ (может быть null) | `string?` | до 255 символов |
| `RoleId` | `RoleId` | `int` | ✔ | ✔ | ✔ | `int?` | роль должна существовать |

**Важно:** `UserDto` намеренно **не содержит пароль**. Пароль можно передать при создании и изменении пользователя, но API никогда не возвращает его клиенту. Это единственное сознательное отступление от правила «DTO для чтения содержит все поля модели». Защита конфиденциальных данных — одна из главных причин применения паттерна DTO.

#### Cart

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdCart` | `int` | ✔ | — | — | — | генерирует БД |
| `UserId` | `UserId` | `int` | ✔ | ✔ | ✔ | `int?` | пользователь должен существовать |
| `CreatedAt` | `CreatedDate` | `DateTime?` | ✔ | — | — | — | сервер, `DateTime.UtcNow` |

#### CartItem

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdCartItem` | `int` | ✔ | — | — | — | генерирует БД |
| `CartId` | `CartId` | `int` | ✔ | ✔ | ✔ | `int?` | корзина должна существовать |
| `ProductId` | `ProductId` | `int` | ✔ | ✔ | ✔ | `int?` | товар должен существовать |
| `Quantity` | `Quantity` | `int` | ✔ | ✔ | ✔ | `int?` | ≥ 1 |

Пара `CartId` + `ProductId` уникальна: один товар может быть в корзине только одной позицией.

#### Order

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdOrder` | `int` | ✔ | — | — | — | генерирует БД |
| `UserId` | `UserId` | `int` | ✔ | ✔ | ✔ | `int?` | пользователь должен существовать |
| `Status` | `Status` | `string` | ✔ | ✔ | ✔ | `string?` | до 50 символов |
| `TotalAmount` | `TotalAmount` | `decimal` | ✔ | ✔ | ✔ | `decimal?` | 0 … 9 999 999 999.99 |
| `CreatedAt` | `OrderDate` | `DateTime?` | ✔ | — | — | — | сервер, `DateTime.UtcNow` |

#### OrderItem

| Поле DTO | Поле модели | Тип | Dto | Create | Update | Patch | Ограничения |
|---|---|---|---|---|---|---|---|
| `Id` | `IdOrderItem` | `int` | ✔ | — | — | — | генерирует БД |
| `OrderId` | `OrderId` | `int` | ✔ | ✔ | ✔ | `int?` | заказ должен существовать |
| `ProductId` | `ProductId` | `int` | ✔ | ✔ | ✔ | `int?` | товар должен существовать |
| `Quantity` | `Quantity` | `int` | ✔ | ✔ | ✔ | `int?` | ≥ 1 |
| `PriceAtOrder` | `PriceAtOrder` | `decimal` | ✔ | ✔ | ✔ | `decimal?` | 0 … 99 999 999.99 |

Пара `OrderId` + `ProductId` уникальна.

### 6.4. Пример: DTO товара

`DTOs/Products/CreateProductDto.cs`:

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "NameOfProduct обязательно.")]
    [StringLength(50, ErrorMessage = "NameOfProduct не может быть длиннее 50 символов.")]
    public required string NameOfProduct { get; set; }

    public string? DescriptionProduct { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "Price должна быть в диапазоне от 0 до 99999999.99.")]
    public required decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть положительным числом.")]
    public required int CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "InStock не может быть отрицательным.")]
    public required int InStock { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(255, ErrorMessage = "ImagePath не может быть длиннее 255 символов.")]
    public string? ImagePath { get; set; }

    [StringLength(500, ErrorMessage = "ImageUrl не может быть длиннее 500 символов.")]
    public string? ImageUrl { get; set; }
}
```

`DTOs/Products/PatchProductDto.cs` — те же поля, но все nullable и без `required`:

```csharp
public class PatchProductDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "NameOfProduct должно содержать от 1 до 50 символов.")]
    public string? NameOfProduct { get; set; }

    public string? DescriptionProduct { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "Price должна быть в диапазоне от 0 до 99999999.99.")]
    public decimal? Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть положительным числом.")]
    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "InStock не может быть отрицательным.")]
    public int? InStock { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(255, ErrorMessage = "ImagePath не может быть длиннее 255 символов.")]
    public string? ImagePath { get; set; }

    [StringLength(500, ErrorMessage = "ImageUrl не может быть длиннее 500 символов.")]
    public string? ImageUrl { get; set; }
}
```

## 7. Мапперы

Для каждой сущности в папке `Mappers` есть статический класс с методом `ToDto`, который преобразует Entity в DTO. Преобразование вынесено в одно место, чтобы не повторять его в каждом методе контроллера.

| Класс | Метод |
|---|---|
| `ProductCategoryMapper` | `ProductCategoryDto ToDto(ProductCategory category)` |
| `ProductMapper` | `ProductDto ToDto(Product product)` |
| `RoleMapper` | `RoleDto ToDto(Role role)` |
| `UserMapper` | `UserDto ToDto(User user)` |
| `CartMapper` | `CartDto ToDto(Cart cart)` |
| `CartItemMapper` | `CartItemDto ToDto(CartItem cartItem)` |
| `OrderMapper` | `OrderDto ToDto(Order order)` |
| `OrderItemMapper` | `OrderItemDto ToDto(OrderItem orderItem)` |

Пример — `Mappers/CartMapper.cs`:

```csharp
public static class CartMapper
{
    public static CartDto ToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.IdCart,
            UserId = cart.UserId,
            CreatedAt = cart.CreatedDate
        };
    }
}
```

`UserMapper` копирует только `Id`, `Login` и `RoleId`; пароль в DTO не попадает.

Обратное преобразование (DTO → Entity) выполняется в контроллерах, потому что для POST, PUT и PATCH оно разное: при создании формируется новый объект, при PUT перезаписываются все поля, при PATCH — только ненулевые.

## 8. Внедрение зависимостей и настройка приложения

`Program.cs`:

```csharp
builder.Services.AddDbContext<PharmacyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PharmacyConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

`AddDbContext` регистрирует `PharmacyContext` со временем жизни **Scoped**: на каждый HTTP-запрос создаётся свой экземпляр контекста, который освобождается после завершения запроса.

`appsettings.json`:

```json
"ConnectionStrings": {
  "PharmacyConnection": "Data Source=miuuu\\SQLEXPRESS;Initial Catalog=pharmacy;Integrated Security=True;Trust Server Certificate=True"
}
```

Каждый контроллер получает контекст через конструктор:

```csharp
private readonly PharmacyContext _context;

public ProductsController(PharmacyContext context)
{
    _context = context;
}
```

## 9. Базовый контроллер ApiControllerBase

Код пагинации и формирования ответов об ошибках одинаков во всех 8 контроллерах. Чтобы не дублировать его, создан абстрактный класс `ApiControllerBase`, от которого наследуются все контроллеры.

```csharp
public abstract class ApiControllerBase : ControllerBase
{
    protected const int MaxPageSize = 100;

    protected static List<T> GetPage<T>(IQueryable<T> orderedQuery, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        return orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    protected ActionResult FieldError(string field, string message)
    {
        ModelState.AddModelError(field, message);
        return ValidationProblem(ModelState);
    }

    protected ActionResult ConflictError(string message)
    {
        return Problem(detail: message, statusCode: StatusCodes.Status409Conflict, title: "Conflict");
    }
}
```

- `GetPage` — применяет пагинацию к уже отсортированному запросу. Некорректные параметры исправляются: номер страницы не меньше 1, размер страницы от 1 до 100. Ограничение сверху не даёт одним запросом выгрузить всю таблицу. `Skip` и `Take` EF Core преобразует в SQL `OFFSET … FETCH NEXT …`, то есть из БД читается только нужная страница.
- `FieldError` — возвращает **400 Bad Request** с ошибкой по конкретному полю, в том же формате, что и автоматическая валидация DTO.
- `ConflictError` — возвращает **409 Conflict** в формате ProblemDetails.

Методы объявлены `protected`, поэтому ASP.NET Core не считает их эндпоинтами.

## 10. Контроллеры

Все 8 контроллеров построены по одной схеме. Каждый помечен атрибутами:

- `[ApiController]` — автоматическая проверка модели (при ошибках валидации DTO сразу возвращается 400), автоматическое чтение DTO из тела запроса, ответы 404 в формате ProblemDetails;
- `[Route("api/…")]` — базовый маршрут;
- `[Produces("application/json")]` — формат ответа.

Атрибуты `[ProducesResponseType]` над методами описывают возможные коды ответа; благодаря им Swagger показывает все варианты ответов. Ограничение `{id:int}` в маршрутах гарантирует, что в метод попадёт только целое число.

### 10.1. Эндпоинты каждого контроллера

| № | Метод | Маршрут | Имя метода | Принимает | Возвращает | Коды |
|---|---|---|---|---|---|---|
| 1 | GET | `api/{ресурс}?page=1&pageSize=10` | `GetAll` | `page`, `pageSize` | `List<EntityDto>` | 200 |
| 2 | GET | `api/{ресурс}/{id}` | `GetById` | `id` | `EntityDto` | 200, 404 |
| 3 | POST | `api/{ресурс}` | `Create` | `CreateEntityDto` | `EntityDto` | 201, 400, 409 |
| 4 | PUT | `api/{ресурс}/{id}` | `Update` | `id`, `UpdateEntityDto` | — | 204, 400, 404, 409 |
| 5 | PATCH | `api/{ресурс}/{id}` | `Patch` | `id`, `PatchEntityDto` | — | 204, 400, 404, 409 |
| 6 | DELETE | `api/{ресурс}/{id}` | `Delete` | `id` | — | 204, 404, 409 |

Код 409 возвращают только те контроллеры, у сущностей которых есть уникальные поля или зависимые записи (см. раздел 11).

### 10.2. Разбор методов на примере ProductsController

#### GET api/products — список с пагинацией

```csharp
[HttpGet]
public ActionResult<IEnumerable<ProductDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    var query = _context.Products
        .AsNoTracking()
        .OrderBy(p => p.IdProduct);

    return Ok(GetPage(query, page, pageSize).Select(ProductMapper.ToDto).ToList());
}
```

- Параметры `page` и `pageSize` по умолчанию равны 1 и 10.
- Сортировка по `Id` обязательна: без `OrderBy` порядок строк в SQL не гарантирован, и страницы могли бы пересекаться.
- `AsNoTracking()` отключает отслеживание изменений: данные только читаются, это быстрее и экономит память.
- Если на странице нет записей, возвращается пустой массив `[]` с кодом 200.

#### GET api/products/{id} — получение по идентификатору

```csharp
[HttpGet("{id:int}")]
public ActionResult<ProductDto> GetById(int id)
{
    var product = _context.Products
        .AsNoTracking()
        .FirstOrDefault(p => p.IdProduct == id);

    if (product == null)
    {
        return NotFound();
    }

    return Ok(ProductMapper.ToDto(product));
}
```

Возвращается `ProductDto`, а не `Product`. Если запись не найдена — **404 Not Found**.

#### POST api/products — создание

```csharp
[HttpPost]
public ActionResult<ProductDto> Create(CreateProductDto dto)
{
    if (!CategoryExists(dto.CategoryId))
    {
        return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId} не найдена.");
    }

    var product = new Product
    {
        NameOfProduct = dto.NameOfProduct,
        DescriptionProduct = dto.DescriptionProduct,
        Price = dto.Price,
        CategoryId = dto.CategoryId,
        InStock = dto.InStock,
        IsActive = dto.IsActive,
        ImagePath = dto.ImagePath,
        ImageUrl = dto.ImageUrl
    };

    _context.Products.Add(product);
    _context.SaveChanges();

    var result = ProductMapper.ToDto(product);

    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

- Принимается `CreateProductDto`, поэтому клиент не может задать `Id`.
- Перед сохранением проверяется, что категория существует.
- После `_context.SaveChanges()` EF Core записывает в объект `Id`, сгенерированный базой, и значения по умолчанию из БД (например, `IsActive = true`, если поле не передано).
- `CreatedAtAction` возвращает **201 Created**, созданный объект в теле и заголовок `Location` со ссылкой на `GetById`.

Для `Cart` и `Order` при создании дополнительно заполняется дата:

```csharp
var cart = new Cart
{
    UserId = dto.UserId,
    CreatedDate = DateTime.UtcNow
};
```

#### PUT api/products/{id} — полное обновление

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, UpdateProductDto dto)
{
    var product = _context.Products.Find(id);

    if (product == null)
    {
        return NotFound();
    }

    if (!CategoryExists(dto.CategoryId))
    {
        return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId} не найдена.");
    }

    product.NameOfProduct = dto.NameOfProduct;
    product.DescriptionProduct = dto.DescriptionProduct;
    product.Price = dto.Price;
    product.CategoryId = dto.CategoryId;
    product.InStock = dto.InStock;
    product.IsActive = dto.IsActive;
    product.ImagePath = dto.ImagePath;
    product.ImageUrl = dto.ImageUrl;

    _context.SaveChanges();

    return NoContent();
}
```

Порядок проверок: сначала существование объекта (404), затем корректность данных (400). Перезаписываются все поля. Объект загружен через `Find` с отслеживанием, поэтому EF Core сам определяет изменённые столбцы и формирует `UPDATE`. Ответ — **204 No Content**.

#### PATCH api/products/{id} — частичное обновление

```csharp
[HttpPatch("{id:int}")]
public IActionResult Patch(int id, PatchProductDto dto)
{
    var product = _context.Products.Find(id);

    if (product == null)
    {
        return NotFound();
    }

    if (dto.CategoryId.HasValue)
    {
        if (!CategoryExists(dto.CategoryId.Value))
        {
            return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId.Value} не найдена.");
        }

        product.CategoryId = dto.CategoryId.Value;
    }

    if (dto.NameOfProduct != null)
    {
        product.NameOfProduct = dto.NameOfProduct;
    }

    if (dto.Price.HasValue)
    {
        product.Price = dto.Price.Value;
    }

    // ... аналогично для остальных полей

    _context.SaveChanges();

    return NoContent();
}
```

Обновляются только поля, которые в DTO не равны `null`. Для ссылок и числовых полей используется `HasValue`, для строк — сравнение с `null`. Ответ — **204 No Content**.

Разница между PUT и PATCH: при PUT клиент обязан передать весь объект (пропущенное поле → 400), при PATCH — только изменяемые поля (пропущенное поле остаётся прежним).

#### DELETE api/products/{id} — удаление

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    var product = _context.Products.Find(id);

    if (product == null)
    {
        return NotFound();
    }

    if (_context.CartItems.Any(ci => ci.ProductId == id) || _context.OrderItems.Any(oi => oi.ProductId == id))
    {
        return ConflictError("Нельзя удалить товар, который есть в корзинах или заказах.");
    }

    _context.Products.Remove(product);
    _context.SaveChanges();

    return NoContent();
}
```

Если товар не найден — 404. Если на товар ссылаются позиции корзин или заказов — 409 (см. раздел 11.3). Иначе запись удаляется и возвращается **204 No Content**.

## 11. Проверки целостности данных

В модели для всех внешних ключей настроено `DeleteBehavior.ClientSetNull`, то есть каскадного удаления в БД нет. Кроме того, в таблицах есть уникальные индексы. Если отправить в БД данные, нарушающие эти ограничения, SQL Server выбросит исключение, и клиент получит непонятную ошибку **500 Internal Server Error**. Поэтому все такие ситуации проверяются в контроллерах заранее, и клиент получает понятный ответ.

### 11.1. Существование связанных записей (внешние ключи) → 400

Проверяется в POST, PUT и PATCH (в PATCH — только если поле передано).

| Контроллер | Поле | Проверка | Сообщение |
|---|---|---|---|
| Products | `CategoryId` | категория существует | Категория с Id = … не найдена. |
| Users | `RoleId` | роль существует | Роль с Id = … не найдена. |
| Carts | `UserId` | пользователь существует | Пользователь с Id = … не найден. |
| CartItems | `CartId`, `ProductId` | корзина и товар существуют | Корзина / Товар с Id = … не найдены. |
| Orders | `UserId` | пользователь существует | Пользователь с Id = … не найден. |
| OrderItems | `OrderId`, `ProductId` | заказ и товар существуют | Заказ / Товар с Id = … не найдены. |

Код **400**, а не 404, так как ресурс по URL существует (или создаётся), а ошибка в данных, переданных клиентом.

### 11.2. Уникальность → 409 Conflict

| Контроллер | Уникальное значение | Индекс в БД |
|---|---|---|
| ProductCategories | `NameOfCategory` | `UQ__product___07EF0C9CB5A15769` |
| Roles | `NameOfRole` | `UQ__roles__35968661533367D7` |
| Users | `Login` | `UQ__users__7838F272544343D5` |
| CartItems | пара `CartId` + `ProductId` | `UQ__cart_ite__EA8CD981CBFB9072` |
| OrderItems | пара `OrderId` + `ProductId` | `UQ__order_it__823672BFE6AC024A` |

При PUT и PATCH из проверки исключается сама изменяемая запись, чтобы можно было сохранить объект с его текущим значением:

```csharp
private bool LoginExists(string login, int? excludeId = null)
{
    return _context.Users
        .Any(u => u.Login == login && (excludeId == null || u.IdUser != excludeId.Value));
}
```

В PATCH для позиций корзины и заказа уникальность пары проверяется по итоговым значениям: если передан только `ProductId`, то `CartId` берётся из текущей записи.

### 11.3. Удаление записей, на которые есть ссылки

Используются два подхода, в зависимости от смысла связи.

**Составная часть удаляется вместе с владельцем.** Позиции корзины не существуют без корзины, а позиции заказа — без заказа. Поэтому при удалении корзины или заказа их позиции загружаются через `Include` и удаляются в той же транзакции:

```csharp
var order = _context.Orders
    .Include(o => o.OrderItems)
    .FirstOrDefault(o => o.IdOrder == id);

if (order == null)
{
    return NotFound();
}

_context.OrderItems.RemoveRange(order.OrderItems);
_context.Orders.Remove(order);
_context.SaveChanges();
```

`SaveChanges()` выполняет все команды в одной транзакции: удаляется либо всё, либо ничего.

**Самостоятельные записи удалять запрещено → 409 Conflict.** Удалять вместе с категорией все её товары или вместе с пользователем всю историю заказов недопустимо, поэтому API отказывает в удалении.

| Удаляемая запись | Поведение |
|---|---|
| `ProductCategory` | 409, если в категории есть товары |
| `Product` | 409, если товар есть в корзинах или заказах |
| `Role` | 409, если роль назначена пользователям |
| `User` | 409, если у пользователя есть корзины или заказы |
| `Cart` | удаляется вместе со своими позициями (`CartItems`) |
| `Order` | удаляется вместе со своими позициями (`OrderItems`) |
| `CartItem` | удаляется (зависимых записей нет) |
| `OrderItem` | удаляется (зависимых записей нет) |

## 12. HTTP-коды ответов

| Код | Когда возвращается |
|---|---|
| 200 OK | успешный GET списка или одной записи |
| 201 Created | успешное создание (POST), с заголовком `Location` |
| 204 No Content | успешные PUT, PATCH, DELETE (тело ответа пустое) |
| 400 Bad Request | не прошла валидация DTO, отсутствует обязательное поле, некорректный JSON, ссылка на несуществующую связанную запись |
| 404 Not Found | запись с указанным `id` не найдена (GET по id, PUT, PATCH, DELETE) |
| 409 Conflict | нарушение уникальности или попытка удалить запись, на которую ссылаются другие |

## 13. Запуск проекта

1. Убедиться, что SQL Server Express запущен и база `pharmacy` существует. Если имя сервера отличается от `miuuu\SQLEXPRESS`, исправить строку подключения в `appsettings.json`.
2. Открыть `ProjectASPNET.slnx` в Visual Studio 2022 (при первой сборке NuGet-пакеты скачаются автоматически).
3. Запустить проект (F5, профиль `https`). Браузер откроется на странице Swagger: `https://localhost:7021/swagger`.

Из командной строки:

```
cd ProjectASPNET
dotnet run --launch-profile https
```

В Swagger UI эндпоинты сгруппированы по контроллерам: `CartItems`, `Carts`, `OrderItems`, `Orders`, `ProductCategories`, `Products`, `Roles`, `Users`.

## 14. Проверка работы через Swagger

Все методы проверялись в Swagger UI: раскрыть эндпоинт → **Try it out** → заполнить параметры или тело → **Execute**. Для каждого запроса ниже приведены данные запроса и ответ так, как они отображаются в блоке *Server response*.

Проверка выполнялась как сквозной сценарий в порядке зависимостей: категория → товар → роль → пользователь → корзина → позиция корзины → заказ → позиция заказа. Затем проверялось удаление в обратном порядке. Значения `Id` новых записей зависят от данных, уже имеющихся в базе.

Формат ответа **404 Not Found** одинаков для всех контроллеров:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-6f1c0b8e4d2a...-00"
}
```

### 14.1. ProductCategories — `api/product-categories`

**GET** `/api/product-categories?page=1&pageSize=3` → **200 OK**

```json
[
  { "id": 1, "nameOfCategory": "Обезболивающие" },
  { "id": 2, "nameOfCategory": "Антибиотики" },
  { "id": 3, "nameOfCategory": "Средства от простуды" }
]
```

**POST** `/api/product-categories`

```json
{ "nameOfCategory": "Витамины" }
```

→ **201 Created**, заголовок `location: https://localhost:7021/api/product-categories/6`

```json
{ "id": 6, "nameOfCategory": "Витамины" }
```

**POST** повторно с тем же названием → **409 Conflict**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Категория 'Витамины' уже существует.",
  "traceId": "00-2b9d...-00"
}
```

**GET** `/api/product-categories/6` → **200 OK**: `{ "id": 6, "nameOfCategory": "Витамины" }`

**PUT** `/api/product-categories/6` с телом `{ "nameOfCategory": "Витамины и БАДы" }` → **204 No Content**. Проверка GET → `"nameOfCategory": "Витамины и БАДы"`.

**PATCH** `/api/product-categories/6` с телом `{ "nameOfCategory": "Витамины" }` → **204 No Content**. Проверка GET → `"nameOfCategory": "Витамины"`.

**GET / PUT / PATCH / DELETE** `/api/product-categories/9999` → **404 Not Found**.

### 14.2. Products — `api/products`

**POST** `/api/products`

```json
{
  "nameOfProduct": "Витамин C 500 мг",
  "descriptionProduct": "Аскорбиновая кислота, 30 таблеток",
  "price": 189.90,
  "categoryId": 6,
  "inStock": 120
}
```

Поля `isActive`, `imagePath`, `imageUrl` не переданы. → **201 Created**, `location: https://localhost:7021/api/products/21`

```json
{
  "id": 21,
  "nameOfProduct": "Витамин C 500 мг",
  "descriptionProduct": "Аскорбиновая кислота, 30 таблеток",
  "price": 189.90,
  "categoryId": 6,
  "inStock": 120,
  "isActive": true,
  "imagePath": null,
  "imageUrl": null
}
```

`isActive` получил значение `true` по умолчанию из БД.

**POST** с несуществующей категорией (`"categoryId": 9999`) → **400 Bad Request**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "CategoryId": [ "Категория с Id = 9999 не найдена." ]
  }
}
```

**POST** с отрицательной ценой (`"price": -10`) → **400 Bad Request**, `"Price": [ "Price должна быть в диапазоне от 0 до 99999999.99." ]`.

**POST** без поля `price` → **400 Bad Request**

```json
{
  "status": 400,
  "errors": {
    "$": [ "JSON deserialization for type 'ProjectASPNET.DTOs.CreateProductDto' was missing required properties including: 'price'." ],
    "dto": [ "The dto field is required." ]
  }
}
```

**GET** `/api/products?page=3&pageSize=10` → **200 OK**, третья страница списка товаров (записи с 21-й по 30-ю в порядке `id`), среди них созданный товар `id = 21`.

**PUT** `/api/products/21`

```json
{
  "nameOfProduct": "Витамин C 1000 мг",
  "descriptionProduct": null,
  "price": 329.00,
  "categoryId": 6,
  "inStock": 80,
  "isActive": true,
  "imagePath": null,
  "imageUrl": "https://example.com/vitamin-c.png"
}
```

→ **204 No Content**. Проверка `GET /api/products/21` → **200 OK**:

```json
{
  "id": 21,
  "nameOfProduct": "Витамин C 1000 мг",
  "descriptionProduct": null,
  "price": 329.00,
  "categoryId": 6,
  "inStock": 80,
  "isActive": true,
  "imagePath": null,
  "imageUrl": "https://example.com/vitamin-c.png"
}
```

Описание очищено, так как в PUT явно передано `null`.

**PUT** без поля `descriptionProduct` → **400 Bad Request** (в PUT обязательны все поля).

**PATCH** `/api/products/21` с телом `{ "inStock": 75 }` → **204 No Content**. Проверка GET: изменился только `inStock` (`75`), остальные поля прежние.

### 14.3. Roles — `api/roles`

**GET** `/api/roles` → **200 OK**

```json
[
  { "id": 1, "nameOfRole": "Администратор" },
  { "id": 2, "nameOfRole": "Клиент" }
]
```

**POST** `/api/roles` с телом `{ "nameOfRole": "Фармацевт" }` → **201 Created**, `location: …/api/roles/3`

```json
{ "id": 3, "nameOfRole": "Фармацевт" }
```

**POST** `{ "nameOfRole": "Клиент" }` → **409 Conflict**, `"detail": "Роль 'Клиент' уже существует."`

**POST** `{ "nameOfRole": "" }` → **400 Bad Request**, `"NameOfRole": [ "NameOfRole обязательно." ]`

**PUT** `/api/roles/3` с телом `{ "nameOfRole": "Старший фармацевт" }` → **204 No Content**.

**PATCH** `/api/roles/3` с телом `{ "nameOfRole": "Фармацевт" }` → **204 No Content**. Проверка GET → `{ "id": 3, "nameOfRole": "Фармацевт" }`.

**PATCH** `/api/roles/3` с телом `{ "nameOfRole": "Администратор" }` → **409 Conflict** (имя занято другой ролью).

### 14.4. Users — `api/users`

**POST** `/api/users`

```json
{
  "login": "ivanov",
  "passwordUser": "Qwerty123",
  "roleId": 3
}
```

→ **201 Created**, `location: …/api/users/11`

```json
{ "id": 11, "login": "ivanov", "roleId": 3 }
```

Пароль сохранён в БД, но в ответе отсутствует.

**POST** с тем же логином → **409 Conflict**, `"detail": "Пользователь с логином 'ivanov' уже существует."`

**POST** с `"roleId": 9999` → **400 Bad Request**, `"RoleId": [ "Роль с Id = 9999 не найдена." ]`

**GET** `/api/users/11` → **200 OK**: `{ "id": 11, "login": "ivanov", "roleId": 3 }`

**PUT** `/api/users/11`

```json
{ "login": "ivanov_a", "passwordUser": "Qwerty123", "roleId": 2 }
```

→ **204 No Content**. Проверка GET → `{ "id": 11, "login": "ivanov_a", "roleId": 2 }`.

**PATCH** `/api/users/11` с телом `{ "passwordUser": "NewPass456" }` → **204 No Content**. Проверка GET: логин и роль не изменились; в БД обновился только столбец `password_user`.

### 14.5. Carts — `api/carts`

**POST** `/api/carts` с телом `{ "userId": 11 }` → **201 Created**, `location: …/api/carts/8`

```json
{ "id": 8, "userId": 11, "createdAt": "2026-09-17T08:21:05.1347702Z" }
```

`createdAt` заполнен сервером (`DateTime.UtcNow`).

**POST** `{ "userId": 9999 }` → **400 Bad Request**, `"UserId": [ "Пользователь с Id = 9999 не найден." ]`

**GET** `/api/carts?page=1&pageSize=10` → **200 OK**, массив `CartDto`, отсортированный по `id`.

**PUT** `/api/carts/8` с телом `{ "userId": 1 }` → **204 No Content**. Проверка GET → `"userId": 1`, `createdAt` не изменился.

**PATCH** `/api/carts/8` с телом `{ "userId": 11 }` → **204 No Content**. Проверка GET → `"userId": 11`.

### 14.6. CartItems — `api/cart-items`

**POST** `/api/cart-items`

```json
{ "cartId": 8, "productId": 21, "quantity": 2 }
```

→ **201 Created**, `location: …/api/cart-items/15`

```json
{ "id": 15, "cartId": 8, "productId": 21, "quantity": 2 }
```

**POST** тот же товар в ту же корзину → **409 Conflict**, `"detail": "Этот товар уже есть в корзине."`

**POST** `{ "cartId": 8, "productId": 21, "quantity": 0 }` → **400 Bad Request**, `"Quantity": [ "Quantity должно быть не меньше 1." ]`

**PUT** `/api/cart-items/15` с телом `{ "cartId": 8, "productId": 21, "quantity": 3 }` → **204 No Content**.

**PATCH** `/api/cart-items/15` с телом `{ "quantity": 5 }` → **204 No Content**. Проверка GET → `{ "id": 15, "cartId": 8, "productId": 21, "quantity": 5 }`.

### 14.7. Orders — `api/orders`

**POST** `/api/orders`

```json
{ "userId": 11, "status": "Новый", "totalAmount": 1250.50 }
```

→ **201 Created**, `location: …/api/orders/12`

```json
{
  "id": 12,
  "userId": 11,
  "status": "Новый",
  "totalAmount": 1250.50,
  "createdAt": "2026-09-17T08:24:47.5539012Z"
}
```

**POST** без `status` → **400 Bad Request** (обязательное поле).

**PUT** `/api/orders/12`

```json
{ "userId": 11, "status": "В обработке", "totalAmount": 1400.00 }
```

→ **204 No Content**.

**PATCH** `/api/orders/12` с телом `{ "status": "Выполнен" }` → **204 No Content**. Проверка GET:

```json
{
  "id": 12,
  "userId": 11,
  "status": "Выполнен",
  "totalAmount": 1400.00,
  "createdAt": "2026-09-17T08:24:47.553"
}
```

Изменился только `status`; дата создания не меняется ни при PUT, ни при PATCH.

### 14.8. OrderItems — `api/order-items`

**POST** `/api/order-items`

```json
{ "orderId": 12, "productId": 21, "quantity": 2, "priceAtOrder": 329.00 }
```

→ **201 Created**, `location: …/api/order-items/30`

```json
{ "id": 30, "orderId": 12, "productId": 21, "quantity": 2, "priceAtOrder": 329.00 }
```

**POST** тот же товар в тот же заказ → **409 Conflict**, `"detail": "Этот товар уже есть в заказе."`

**POST** `"orderId": 9999` → **400 Bad Request**, `"OrderId": [ "Заказ с Id = 9999 не найден." ]`

**PUT** `/api/order-items/30` с телом `{ "orderId": 12, "productId": 21, "quantity": 3, "priceAtOrder": 319.00 }` → **204 No Content**.

**PATCH** `/api/order-items/30` с телом `{ "quantity": 1 }` → **204 No Content**. Проверка GET → `{ "id": 30, "orderId": 12, "productId": 21, "quantity": 1, "priceAtOrder": 319.00 }`.

### 14.9. Удаление и защита целостности

Созданные записи удалялись так, чтобы проверить оба варианта поведения DELETE.

| № | Запрос | Ответ | Пояснение |
|---|---|---|---|
| 1 | DELETE `/api/product-categories/6` | **409 Conflict** | «Нельзя удалить категорию, в которой есть товары.» |
| 2 | DELETE `/api/products/21` | **409 Conflict** | «Нельзя удалить товар, который есть в корзинах или заказах.» |
| 3 | DELETE `/api/roles/2` | **409 Conflict** | «Нельзя удалить роль, которая назначена пользователям.» |
| 4 | DELETE `/api/users/11` | **409 Conflict** | «Нельзя удалить пользователя, у которого есть корзины или заказы.» |
| 5 | DELETE `/api/order-items/30` | **204 No Content** | позиция заказа удалена |
| 6 | GET `/api/order-items/30` | **404 Not Found** | запись действительно удалена |
| 7 | DELETE `/api/order-items/30` | **404 Not Found** | повторное удаление |
| 8 | POST `/api/order-items` (снова позиция в заказ 12) | **201 Created** | подготовка к проверке удаления заказа |
| 9 | DELETE `/api/orders/12` | **204 No Content** | заказ удалён вместе со своими позициями |
| 10 | GET `/api/order-items?page=1&pageSize=100` | **200 OK** | позиций с `orderId = 12` нет |
| 11 | DELETE `/api/carts/8` | **204 No Content** | корзина удалена вместе с позицией `cart-items/15` |
| 12 | GET `/api/cart-items/15` | **404 Not Found** | позиция удалена вместе с корзиной |
| 13 | DELETE `/api/users/11` | **204 No Content** | корзин и заказов больше нет, удаление разрешено |
| 14 | DELETE `/api/roles/3` | **204 No Content** | роль «Фармацевт» больше никому не назначена (после PUT пользователь был переведён на роль 2 и затем удалён) |
| 15 | DELETE `/api/products/21` | **204 No Content** | товар больше не используется |
| 16 | DELETE `/api/product-categories/6` | **204 No Content** | категория пуста |
| 17 | GET `/api/product-categories/6` | **404 Not Found** | категория удалена |

### 14.10. Итог тестирования

| Контроллер | GET список | GET по id (200/404) | POST (201/400/409) | PUT (204/400/404) | PATCH (204/404) | DELETE (204/404/409) |
|---|---|---|---|---|---|---|
| ProductCategories | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Products | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Roles | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Users | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Carts | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| CartItems | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| Orders | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |
| OrderItems | ✔ | ✔ | ✔ | ✔ | ✔ | ✔ |

## 15. Соответствие требованиям задания

| Требование | Реализация |
|---|---|
| Используется существующая БД и модель | `PharmacyContext`, база `pharmacy`, все 8 сущностей |
| Entity не используется в параметрах и возвращаемых значениях контроллеров | контроллеры работают только с DTO |
| DTO для чтения со всеми полями, включая Id и CreatedAt | `…Dto` для каждой сущности; `CreatedAt` у `CartDto` и `OrderDto` (у остальных таблиц нет даты создания); пароль исключён из `UserDto` |
| DTO для создания без Id и CreatedAt | `Create…Dto` × 8 |
| DTO для PUT, все поля обязательные | `Update…Dto` × 8 (`required` + атрибуты валидации) |
| DTO для PATCH, все поля nullable | `Patch…Dto` × 8 |
| Статический метод-маппер | `…Mapper.ToDto(entity)` × 8 |
| Маршрут `api/entity` | `api/product-categories`, `api/products`, `api/roles`, `api/users`, `api/carts`, `api/cart-items`, `api/orders`, `api/order-items` |
| Контекст внедряется через конструктор (DI) | `AddDbContext` + конструктор каждого контроллера |
| GET с пагинацией, значения по умолчанию, сортировка по Id | `GetAll(page = 1, pageSize = 10)`, `OrderBy(Id)`, `GetPage` |
| GET по id возвращает DTO / 404 | `GetById` |
| POST: CreatedAt = DateTime.UtcNow, SaveChanges, 201 + CreatedAtAction | `Create` |
| PUT: 404 / обновление всех полей / 204 | `Update` |
| PATCH: 404 / обновление только не-null полей / 204 | `Patch` |
| DELETE: 404 / удаление + SaveChanges / 204 | `Delete` |
| Проверка наличия объекта перед изменением | `Find` / `FirstOrDefault` + `NotFound()` в PUT, PATCH, DELETE |
| Корректные HTTP-коды | 200, 201, 204, 404; дополнительно 400 и 409 для ошибок данных |
| Проверка через Swagger | раздел 14 |
| Стандарты оформления C# | PascalCase для классов и методов, `_camelCase` для приватных полей, отступы 4 пробела, без лишних `using`, file-scoped namespace |

## 16. Выводы

В ходе работы на базе существующей модели EF Core реализован Web API для всех восьми сущностей базы данных аптеки: категорий, товаров, ролей, пользователей, корзин, позиций корзины, заказов и позиций заказа. Для каждой сущности созданы четыре DTO, статический маппер и контроллер с полным набором CRUD-операций.

Паттерн DTO разделил модель базы данных и контракт API: клиент получает только нужные поля, не может изменить серверные поля (`Id`, дату создания), не видит пароли пользователей, а для каждой операции действуют свои правила валидации. Общий код пагинации и формирования ошибок вынесен в базовый контроллер.

Ограничения базы данных (внешние ключи, уникальные индексы) проверяются до сохранения, поэтому клиент вместо ошибки сервера получает понятные ответы 400 или 409. Контекст базы данных внедряется через DI, строка подключения вынесена в конфигурацию. Все эндпоинты проверены через Swagger UI.
