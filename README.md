# Shop

Небольшой сервис оформления и отслеживания заказов. Пользователи создают и смотрят свои заказы, администраторы меняют статусы и выдают права админа другим пользователям.

## Архитектура

Проект разбит на слои, каждый отвечает за своё и зависит только от нижних:

- **Domain** — сущности (пользователь, сессия, заказ), перечисления статусов, интерфейсы репозиториев и сервисов. Никакой логики инфраструктуры, только модель и контракты.
- **Application** — бизнес-логика: регистрация/вход, работа с заказами, управление ролями. Работает только через интерфейсы из Domain.
- **Infrastructure** — конкретные реализации: EF Core + PostgreSQL, BCrypt для паролей, JWT для токенов, in-memory кэш.
- **Presentation** — ASP.NET Core, REST-контроллеры (`/api/auth`, `/api/orders`, `/api/users`), глобальная обработка ошибок (RFC 7807).
- **Frontend** — React-приложение на Vite в папке `Frontend`. Простой MVC: страницы собирают контроллеры, контроллеры дёргают сервисы (`fetch`), сервисы ходят в API. Все строки — через лёгкий i18n-хелпер.

Зависимости идут строго вниз: Presentation → Application → Domain и Infrastructure → Domain. Application не знает про Infrastructure. Код в `Tests` делится на unit-тесты (on in-memory fake'ов) и интеграционные (на PostgreSQL).

## Требования

- .NET 9 SDK
- Node.js (для фронтенда)
- Docker (для локального PostgreSQL)

## Запуск

1. Поднять базу:
   ```bash
   docker compose up -d
   ```

2. Применить миграции:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project Presentation
   ```

3. Запустить backend (по умолчанию `http://localhost:5025`):
   ```bash
   dotnet run --project Presentation
   ```

4. Запустить frontend в отдельном терминале:
   ```bash
   cd Frontend
   npm install
   npm run dev
   ```

   Vite-сервер проксирует запросы к `/api` на backend.

## Администратор

При первом запуске автоматически создаётся администратор (если пользователь с таким логином ещё не существует — существующего не трогаем):

```
логин: admin
пароль: admin1234
```

Имя и пароль берутся из секции `Seed:Admin` в `Presentation/appsettings.json`, пароль при необходимости меняется через переменную окружения `Seed__Admin__Password`.

## Тесты

```bash
dotnet test Tests
```

## Структура репозитория

```
Application/   бизнес-логика
Domain/        модель и контракты
Infrastructure/ реализация хранилищ и сервисов
Presentation/  REST API
Frontend/      React-приложение
Tests/         тесты
docker-compose.yml  локальный PostgreSQL
```
