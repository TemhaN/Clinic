# 🩺 ClinicManager

**ClinicManager** — десктопное приложение на WPF (C#) для управления санаторием или медицинским учреждением.  
Позволяет вести учёт **пациентов**, **сотрудников**, **расписания**, **статистики** и **пользователей**.  
Простой в использовании интерфейс, адаптивный дизайн, SQLite и экспорт в Excel — всё по делу.

## ✨ Возможности

- 🔐 **Аутентификация**: вход, выход, роли (`admin`, `rabotnik`, `otchetnik`).
- 👨‍⚕️ **Персонал**: добавление, редактирование, удаление сотрудников, просмотр расписания.
- 🧑‍⚕️ **Пациенты**: управление данными, валидация телефона, email и паспорта.
- 📅 **Расписание**: фильтрация по врачам, отображение текущих приёмов.
- 📊 **Статистика**: графики посещаемости, работы врачей, распространённость диагнозов (LiveCharts).
- 📝 **Отчёты**: экспорт в Excel (EPPlus).
- 🎨 **Интерфейс**: современный дизайн, боковое меню, адаптивная верстка, роли пользователей.

## 📋 Требования

- .NET Framework **4.8+**
- Visual Studio **2019+**
- **SQLite** (база `ClinicDB.sqlite`)
- Windows **7 SP1** или новее
- NuGet-пакеты:
  - `EntityFrameworkCore.Sqlite`
  - `LiveCharts.Wpf`
  - `OfficeOpenXml` (EPPlus)

## 🧩 Зависимости

| Библиотека / Технология     | Назначение                         |
|-----------------------------|------------------------------------|
| WPF                         | Интерфейс                          |
| EntityFrameworkCore.Sqlite  | Работа с SQLite                    |
| LiveCharts.Wpf              | Построение графиков                |
| OfficeOpenXml (EPPlus)      | Экспорт данных в Excel             |
| System.Windows.Controls     | Элементы управления                |

👉 Полный список смотри в `.csproj`.

## 🚀 Установка и запуск

### 1. Клонируй репозиторий
```bash
git clone https://github.com/TemhaN/ClinicManager.git
cd ClinicManager
````

### 2. Установи зависимости

```bash
dotnet restore
```

### 3. Настрой базу данных

Убедись, что `ClinicDB.sqlite` лежит в корне проекта **или** пропиши путь в коде:

```csharp
string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClinicDB.sqlite");
```

### 4. Сборка и запуск

```bash
dotnet build
dotnet run
```

Или открой `.sln` в Visual Studio и запусти через F5.

## 📦 Сборка релиза

```bash
dotnet publish -c Release -r win-x64
```

📁 Сборка появится в:

```
bin/Release/net48/win-x64/publish
```

> Убедись, что файл `ClinicDB.sqlite` лежит рядом с `.exe`.

---

## 🖱️ Использование

### 🔐 Аутентификация

* **LoginWindow** — окно входа (логин/пароль).
* Доступные роли:

  * `admin`: полный доступ
  * `rabotnik`: врачи и сотрудники
  * `otchetnik`: доступ к статистике

### 🩻 Модули приложения

| Окно      | Назначение                                                  |
| --------- | ----------------------------------------------------------- |
| `Window3` | Персонал (ФИО, должность, телефон)                          |
| `Window4` | Пациенты (ФИО, дата рождения, пол, телефон, email, паспорт) |
| `Window6` | Расписание врачей (по дням, врачам, фильтрация)             |
| `Window7` | Статистика: графики, экспорт в Excel                        |
| `Window8` | Пользователи (только `admin`): логины, роли, управление     |

## 📸 Скриншоты

<div style="display: flex; flex-wrap: wrap; gap: 10px; justify-content: center;">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/1.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/2.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/3.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/4.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/5.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/6.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/7.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/8.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/9.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/10.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/11.png?raw=true" alt="Clinic" width="30%">
  <img src="https://github.com/TemhaN/Clinic/blob/master/Clinic/Screenshots/12.png?raw=true" alt="Clinic" width="30%">
</div>    

## 🧠 Автор

**TemhaN**  
[GitHub профиль](https://github.com/TemhaN)

## 🧾 Лицензия

Проект распространяется под лицензией [MIT License].

## 📬 Обратная связь

Нашли баг или хотите предложить улучшение?
Создайте **issue** или присылайте **pull request** в репозиторий!


## ⚙️ Технологии

* **C# / .NET Framework 4.8**
* **WPF + XAML** — UI
* **SQLite** — база данных
* **Entity Framework Core** — ORM
* **LiveCharts** — графики
* **EPPlus** — экспорт Excel
