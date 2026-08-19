# Events

Full-stack приложение для управления мероприятиями (events).

Пользователи могут регистрироваться и авторизовываться, создавать и редактировать мероприятия, присоединяться к ним, подписываться на других пользователей, загружать фотографии, оставлять комментарии и общаться в реальном времени через чат.

Проект полностью контейнеризирован с помощью Docker. Для CI используется GitHub Actions, Docker-образы публикуются в GitHub Container Registry.

## Стек проекта

### Backend

- C#
- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT
- SignalR
- AutoMapper
- FluentValidation

### Frontend

- React
- TypeScript
- Axios
- MobX
- React Router
- Formik
- React Query
- Material UI

### DevOps

- Docker
- Docker Compose
- Nginx
- GitHub Actions
- GitHub Container Registry (GHCR)

## Запуск проекта

### Требования

Для запуска проекта необходимы:

- Docker
- Docker Compose

.NET, Node.js и SQL Server устанавливать локально не требуется — всё необходимое запускается в Docker.

### 1. Создать `.env`

В корне проекта создать файл `.env` на основе `.env.example`:

```env
DB_PASSWORD=your_database_password
TOKEN_KEY=your_token_key
CLOUDINARY_CLOUD_NAME=your_cloud_name
CLOUDINARY_API_KEY=your_api_key
CLOUDINARY_API_SECRET=your_api_secret
```

2. Запустить проект

Из корня проекта выполнить:

```
docker compose -f compose.yml -f compose.dev.yml up -d --build

```
После запуска приложение будет доступно по адресу:

```
http://localhost
```
Production

Для production подготовлен отдельный compose.prod.yml.

Production-версия использует готовые Docker-образы из GitHub Container Registry вместо локальной сборки:

ghcr.io/<owner>/events-backend
ghcr.io/<owner>/events-frontend

На сервере запуск выполняется через:

```
docker compose -f compose.yml -f compose.prod.yml pull

docker compose -f compose.yml -f compose.prod.yml up -d
```
Полноценный автоматический deployment на сервер будет настроен после добавления production-сервера.