# GitHub Actions CI/CD Pipeline

Цей проект використовує GitHub Actions для автоматизованого тестування, білдування та deployment.

## 📋 Workflows

### 1. **build.yml** - Білд проекту
- **Тригер:** push на `dev`, pull request до `main`
- **Завдання:** Перевіряє білдування .NET проекту
- **Артефакти:** Зберігає білд-результати на 1 день

### 2. **ci.yml** - Unit & Integration Тести
- **Тригер:** push на `dev`, pull request до `main`
- **Завдання:** 
  - Білдує проект
  - Запускає unit тести
  - Запускає integration тести з WebApplicationFactory
  - Збирає покриття коду
- **Сервіси:** PostgreSQL для тестування

### 3. **database.yml** - Database Тести
- **Тригер:** push на `dev`, pull request до `main`
- **Завдання:**
  - Запускає тести на базі даних (Testcontainers)
  - Перевіряє PostgreSQL constraints
  - Тестує міграції
- **Сервіси:** PostgreSQL

### 4. **k6.yml** - Performance Тести
- **Тригер:** push на `dev`, pull request до `main`
- **Завдання:**
  - Запускає API з seeded базою даних (10,000 записів)
  - Запускає k6 smoke тести
  - Запускає k6 load тести (23 VUs)
  - Запускає k6 stress тести (до 50 VUs)
- **Сервіси:** PostgreSQL

### 5. **code-quality.yml** - Перевірка якості коду
- **Тригер:** push на `dev`, pull request до `main`
- **Завдання:**
  - Аналізує код на помилки
  - Перевіряє форматування (dotnet format)
- **Примітка:** Failures не блокують merge

### 6. **pr-checks.yml** - Перевірки Pull Request
- **Тригер:** Коли відкривається або оновлюється PR до `main`
- **Завдання:**
  - Перевіряє формат заголовка PR (conventional commits)
  - Перевіряє наявність змін
  - Запускає всі тести
  - Генерує звіт про результати

### 7. **release.yml** - Release і Publish
- **Тригер:** push до `main` або створення тегу `v*`
- **Завдання:**
  - Білдує проект у режимі Release
  - Запускає всі тести
  - Публікує артефакти
  - Створює GitHub Release (для тегів)

## 🔄 Робочий процес

### Для розробника:

1. **Робота на гілці `dev`:**
   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/my-feature
   ```

2. **На кожен push до `dev`:**
   - ✅ Білдується проект
   - ✅ Запускаються всі unit тести
   - ✅ Запускаються integration тести
   - ✅ Запускаються database тести
   - ✅ Запускаються k6 performance тести

3. **Перед Pull Request:**
   - Переконайтеся, що всі тести проходять локально
   - Переконайтеся, що код форматований правильно

4. **Створення PR до `main`:**
   ```bash
   git push origin feature/my-feature
   # Відкрити PR на GitHub
   ```

5. **На Pull Request до `main`:**
   - ✅ Перевіряється заголовок (conventional commits)
   - ✅ Запускаються всі тести ще раз
   - ✅ GitHub Actions дає статус успіху/невдачі

6. **После merge до `main`:**
   - 📦 Проект публікується як Release
   - 🏷️ Артефакти зберігаються

## 📝 Conventional Commits

Заголовки PR повинні дотримуватися формату:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Типи:**
- `feat`: Нова функція
- `fix`: Виправлення помилки
- `docs`: Документація
- `style`: Форматування коду
- `refactor`: Рефакторинг
- `perf`: Поліпшення продуктивності
- `test`: Додання тестів
- `chore`: Оновлення залежностей

**Приклади:**
- ✅ `feat(reservations): add cancellation feature`
- ✅ `fix(auth): resolve token validation issue`
- ✅ `docs: update readme`
- ❌ `Update code`

## 🔍 Статус перевірок

На сторінці PR ви побачите:

```
✅ build — Build passed
✅ Unit & Integration Tests — Tests passed
✅ Database Tests — Tests passed
✅ K6 Performance Tests — Tests passed
✅ Code Quality — Analysis completed
✅ PR Checks — All checks passed
```

## 📊 Artifacts

Після кожного запуску workflow можна завантажити артефакти:

- `build-output` — Результати білдування
- `test-results` — Unit & Integration тест звіти
- `database-test-results` — Database тест звіти
- `k6-results` — Performance тест звіти
- `release-build` — Release артефакти

## 🚀 Запуск workflows вручну

GitHub Actions дозволяє запускати workflows вручну з вкладки Actions на GitHub.

## 💡 Поради

1. **Локальне тестування перед push:**
   ```bash
   dotnet test HotelBooking.Tests/HotelBooking.Tests.csproj
   ```

2. **Перевірка форматування:**
   ```bash
   dotnet format --verify-no-changes
   ```

3. **Білдування як у CI:**
   ```bash
   dotnet build --configuration Release
   ```

4. **Запуск k6 локально:**
   ```bash
   k6 run HotelBooking.Tests/Performance/smoke.js --env BASE_URL=http://localhost:5000
   ```

## 📞 Налаштування

### Необхідні secrets у GitHub:

Зазвичай не потрібні для цього проекту, але якщо потрібні:

1. Перейти до **Settings** → **Secrets and variables** → **Actions**
2. Додати потрібні secrets

## 🐛 Troubleshooting

**Якщо workflow не запускається:**
- Перевірте, чи файли `.yml` в `.github/workflows/`
- Перевірте синтаксис YAML
- Перевірте, чи коректні гілки (dev, main)

**Якщо тести падають:**
- Перевірте логи в GitHub Actions
- Переконайтеся, що PostgreSQL доступна
- Перевірте connection strings

**Якщо performance тести помилюються:**
- API може не встигнути стартувати
- Перевірте, чи достатньо ресурсів на runner
- Збільшите timeout в k6.yml

