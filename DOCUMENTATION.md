# Документация проекта DailyPlanerV3

Полное описание всех классов, методов и переменных проекта «Ежедневник» (курсовой проект). Проект — настольное WPF-приложение (.NET Framework 4.8, C#) для планирования задач, событий и заметок с системой напоминаний и хранением данных в SQL Server (ADO.NET, без Entity Framework).

## Содержание

1. [Модели (Models)](#модели-models)
2. [Сервисы (Services)](#сервисы-services)
3. [Конвертеры (Converters)](#конвертеры-converters)
4. [ViewModel (ViewModels)](#viewmodel-viewmodels)
5. [Файл приложения (App)](#файл-приложения-app)
6. [Представления (Views)](#представления-views)
7. [Диалоговые окна (Dialogs)](#диалоговые-окна-dialogs)
8. [Конфигурационные файлы](#конфигурационные-файлы)

---

## Модели (Models)

### Класс `User` — DailyPlaner/Models/User.cs

Модель пользователя. Соответствует таблице `Users` базы данных.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ пользователя |
| `Username` | `string` | Логин (уникальный) |
| `PasswordHash` | `string` | SHA256-хэш пароля |
| `Email` | `string` | Электронная почта |
| `GenderId` | `int` | Внешний ключ на `Genders.Id` |
| `Gender` | `Gender` | Навигационное свойство пола |
| `RoleId` | `int` | Внешний ключ на `Roles.Id` |
| `CreatedAt` | `DateTime` | Дата регистрации |

### Класс `Gender` — DailyPlaner/Models/Gender.cs

Справочник полов. Таблица `Genders`.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ |
| `Name` | `string` | Название («Мужской»/«Женский») |

### Класс `Task` — DailyPlaner/Models/Task.cs

Модель задачи. Реализует `INotifyPropertyChanged` для обновления интерфейса при смене статуса. Таблица `Tasks`.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ |
| `UserId` | `int` | Владелец задачи |
| `Title` | `string` | Название |
| `Description` | `string` | Описание |
| `DueDate` | `DateTime` | Срок выполнения |
| `Priority` | `string` | Приоритет: «Низкий», «Средний», «Высокий» |
| `Status` | `string` | Статус: «Ожидает»/«Выполнена» (уведомляет UI через `OnPropertyChanged`) |
| `IsCompleted` | `bool` | Вычисляемое: `true`, если `Status` равен «Выполнена» или «Completed»; при записи переводит в «Выполнена»/«Ожидает» |

Скрытое поле: `_status` (`string`) — хранит текущий статус.

Метод: `OnPropertyChanged(string propertyName)` — `protected virtual void` — вызывает событие `PropertyChanged` для обновления привязок.

Событие: `PropertyChanged` — уведомляет интерфейс об изменении свойств.

### Класс `Event` — DailyPlaner/Models/Event.cs

Модель события. Реализует `INotifyPropertyChanged`. Таблица `Events`.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ |
| `UserId` | `int` | Владелец события |
| `Title` | `string` | Название |
| `Description` | `string` | Описание |
| `StartDate` | `DateTime` | Дата и время начала |
| `EndDate` | `DateTime` | Дата и время окончания |
| `Location` | `string` | Место проведения |
| `Status` | `string` | Статус: «Запланировано»/«Завершено» (уведомляет UI) |
| `IsCompleted` | `bool` | Вычисляемое: `true`, если `Status` равен «Завершено» или «Completed»; при записи переводит в «Завершено»/«Запланировано» |

Скрытое поле: `_status` (`string`) — хранит текущий статус.

Метод: `OnPropertyChanged(string propertyName)` — `protected virtual void` — вызывает событие `PropertyChanged`.

Событие: `PropertyChanged`.

### Класс `Note` — DailyPlaner/Models/Note.cs

Модель заметки. Таблица `Notes`.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ |
| `UserId` | `int` | Владелец заметки |
| `Title` | `string` | Заголовок |
| `Content` | `string` | Текст заметки |
| `CreatedDate` | `DateTime` | Дата создания (колонка `CreatedAt`) |

### Класс `Reminder` — DailyPlaner/Models/Reminder.cs

Модель напоминания. Таблица `Reminders`.

| Свойство | Тип | Описание |
|---|---|---|
| `Id` | `int` | Первичный ключ |
| `UserId` | `int` | Владелец напоминания |
| `TaskId` | `int` | Связанная задача (0, если не привязано) |
| `ReminderDate` | `DateTime` | Время срабатывания (колонка `ReminderTime`) |
| `Message` | `string` | Текст напоминания |
| `IsShown` | `bool` | Активно ли напоминание (колонка `IsActive`) |

---

## Сервисы (Services)

### Класс `DatabaseService` — DailyPlaner/Services/DatabaseService.cs

Слой доступа к данным через ADO.NET (`SqlConnection`, `SqlCommand`). Все CRUD-операции проекта.

Поле: `connectionString` (`readonly string`) — строка подключения из `App.config` (`DailyPlannerConnection`) или резервная (`Server=PCGl1tch;Database=DailyPlannerDB;Trusted_Connection=True`).

Конструкторы:

- `DatabaseService()` — читает строку подключения из конфигурации.
- `DatabaseService(string customConnectionString)` — явная строка подключения.

Публичные методы:

- `TestConnection() : bool` — открывает подключение, выполняет миграцию БД (`EnsureDatabaseIsReady`); при ошибке SQL выбрасывает `InvalidOperationException` с понятным текстом.
- `GetFriendlyDatabaseError(Exception) : string` — `static` — переводит коды ошибок SQL Server (-1/2/53 сервер недоступен, 4060 нет базы, 18456/18452 авторизация) в понятные сообщения.
- `HashPassword(string password) : string` — `static` — SHA256-хэш пароля в hex-строке.
- `GetUserByUsername(string username) : User` — поиск пользователя по логину (`null`, если не найден).
- `UsernameExists(string username) : bool` — проверка занятости логина (`SELECT COUNT(1)`).
- `CreateUser(User) : bool` — регистрация нового пользователя (INSERT в `Users`).
- `GetAllGenders() : List<Gender>` — список полов для комбобокса регистрации.
- `GetTasksByUserId(int) : List<Task>` — все задачи пользователя.
- `CreateTask(Task) : bool` — создаёт задачу и записывает новый `Id` через `SCOPE_IDENTITY()` (нужно для привязки напоминаний).
- `UpdateTask(Task) : bool` — обновление задачи.
- `DeleteTask(int id) : bool` — удаляет напоминания задачи, затем саму задачу.
- `GetEventsByUserId(int) : List<Event>` — все события пользователя.
- `CreateEvent(Event) : bool` — создание события.
- `UpdateEvent(Event) : bool` — обновление события.
- `DeleteEvent(int id) : bool` — удаление события.
- `GetNotesByUserId(int) : List<Note>` — все заметки пользователя.
- `CreateNote(Note) : bool` — создание заметки.
- `UpdateNote(Note) : bool` — обновление заметки.
- `DeleteNote(int id) : bool` — удаление заметки.
- `GetAllReminders() : List<Reminder>` — все напоминания.
- `CreateReminder(Reminder) : bool` — создание напоминания.
- `UpdateReminder(Reminder) : bool` — обновление напоминания (используется планировщиком при показе).

Приватные методы:

- `EnsureDatabaseIsReady(SqlConnection)` — миграция БД отдельными командами: колонка `Events.Status`, ограничение `CHK_Events_Status`, нормализация приоритетов/статусов задач (`CHK_Tasks_Priority`, `CHK_Tasks_Status`).
- `Query<T>(string, Action<SqlCommand>, Func<SqlDataReader, T>) : List<T>` — универсальный SELECT-хелпер.
- `Execute(string, Action<SqlCommand>, bool showError) : bool` — универсальный INSERT/UPDATE/DELETE-хелпер (возврат — затронуты ли строки).
- `Scalar(string, Action<SqlCommand>, bool showError) : object` — INSERT + `SELECT CAST(SCOPE_IDENTITY() AS INT)` одним подключением.
- `ReadUser/ReadTask/ReadEvent/ReadNote/ReadReminder(SqlDataReader)` — `static` — мапперы строк БД в модели.

### Класс `NotificationService` — DailyPlaner/Services/NotificationService.cs

Toast-уведомления Windows с резервным показом через балун трея.

Поле: `_timers` (`static readonly List<System.Threading.Timer>`) — активные отложенные таймеры уведомлений.

Методы:

- `ShowNotification(string title, string message) : void` — показывает toast через `ToastContentBuilder` (Microsoft.Toolkit.Uwp.Notifications); при сбое — балун трея; потокобезопасно (перенос на UI-поток через `Dispatcher`).
- `ShowReminderNotification(string title, string message, DateTime dueDate) : void` — уведомление о напоминании с датой («Due: гггг-мм-дд чч:мм»).
- `ScheduleNotification(string title, string message, DateTime scheduleTime) : void` — отложенное уведомление через `System.Threading.Timer`; если время прошло — показывает сразу.
- `ShowTrayFallback(string, string) : void` — `private static` — показывает балун трея (`App.TrayIcon`) 4 секунды.

### Класс `ReminderScheduler` — DailyPlaner/Services/ReminderScheduler.cs

Планировщик напоминаний на `DispatcherTimer`.

Поля:

- `_databaseService` (`readonly DatabaseService`) — доступ к БД.
- `_notificationService` (`readonly NotificationService`) — показ уведомлений.
- `_timer` (`readonly DispatcherTimer`) — таймер с интервалом 30 секунд.
- `_shownReminderIds` (`readonly HashSet<int>`) — Id напоминаний, уже показанных в этой сессии.

Методы:

- `ReminderScheduler()` — конструктор: создаёт сервисы и таймер (30 сек), подписывается на `Tick`.
- `Start() : void` — сразу проверяет напоминания и запускает таймер.
- `Stop() : void` — останавливает таймер (используется при выходе).
- `CheckReminders() : void` — показывает активные напоминания с датой в диапазоне от «12 часов назад» до «сейчас», помечает их показанными (`IsShown = false` + `UpdateReminder`).
- `GetPendingReminderCount() : int` — количество будущих активных напоминаний (-1 при ошибке БД).
- `GetNextReminderTime() : DateTime?` — время ближайшего будущего напоминания (`null` при отсутствии/ошибке).

---

## Конвертеры (Converters)

Все конвертеры реализуют `IValueConverter` и используются в XAML для визуального оформления карточек записей.

### `BooleanToVisibilityConverter` — DailyPlaner/Converters/BooleanToVisibilityConverter.cs

- `Convert(object, Type, object, CultureInfo) : object` — `bool` → `Visibility.Visible`/`Collapsed`.
- `ConvertBack(...) : object` — `Visibility` → `bool`.

### `BooleanToTextDecorationConverter` — DailyPlaner/Converters/BooleanToTextDecorationConverter.cs

Три конвертера в одном файле:

- `BooleanToTextDecorationConverter.Convert` — для выполненных записей возвращает зачёркивание (`TextDecorationCollection` с серой пунктирной линией 1.5), иначе `null`. `ConvertBack` возвращает `false`.
- `BooleanToStatusBadgeConverter.Convert` — `bool` → «✔ Выполнена» / «Ожидает». `ConvertBack` — строка содержит «Выполнена» → `true`.
- `BooleanToOpacityConverter.Convert` — выполненное → прозрачность `0.55`, иначе `1.0`.

---

## ViewModel (ViewModels)

### Класс `RelayCommand` — DailyPlaner/ViewModels/RelayCommand.cs

Реализация `ICommand` для привязки команд кнопок к методам ViewModel.

Поля: `_execute` (`readonly Action<object>`) — действие; `_canExecute` (`readonly Predicate<object>`) — условие доступности.

Методы:

- `RelayCommand(Action<object>, Predicate<object> = null)` — конструктор.
- `CanExecute(object) : bool` — `_canExecute == null || _canExecute(parameter)`.
- `Execute(object) : void` — вызывает `_execute`.

Событие: `CanExecuteChanged` — транслируется в `CommandManager.RequerySuggested` (автоматическое обновление доступности кнопок).

### Класс `LoginViewModel` — DailyPlaner/ViewModels/LoginViewModel.cs

Логика окна входа.

Поля: `_databaseService` (`readonly DatabaseService`); `_username`, `_password` (`string`); `_isLoading` (`bool`).

Свойства (с уведомлением `OnPropertyChanged`): `Username`, `Password`, `IsLoading` — данные формы входа.

Команды: `LoginCommand` (с условием `CanExecuteLogin`), `RegisterCommand`.

Методы:

- `CanExecuteLogin(object) : bool` — логин и пароль не пустые.
- `ExecuteLogin(object) : void` — ищет пользователя, сверяет хэш пароля; при успехе открывает `MainWindow`, закрывает окно входа; ошибки показываются в MessageBox.
- `ExecuteRegister(object) : void` — открывает окно регистрации с новым `RegisterViewModel`.
- `OnPropertyChanged(string) : void` — `protected virtual` — событие `PropertyChanged`.

### Класс `RegisterViewModel` — DailyPlaner/ViewModels/RegisterViewModel.cs

Логика окна регистрации.

Поля: `_databaseService` (`readonly DatabaseService`); `_username`, `_password`, `_confirmPassword`, `_email` (`string`); `_selectedGenderId` (`int`); `_genders` (`List<Gender>`).

Свойства: `Username`, `Password`, `ConfirmPassword`, `Email`, `SelectedGenderId`, `Genders`.

Команды: `RegisterCommand` (условие `CanExecuteRegister`), `CancelCommand`.

Методы:

- `RegisterViewModel(DatabaseService)` — конструктор: команды + загрузка списка полов.
- `LocalizeGender(string) : string` — `static` — переводит названия полов из БД («male», «мужской» и т.п.) в «Мужской»/«Женский».
- `LoadGenders() : void` — загружает список полов (кроме «other»/«другое»), локализует их.
- `CanExecuteRegister(object) : bool` — все поля заполнены, пароли совпадают, пол выбран.
- `ExecuteRegister(object) : void` — проверки: пароль ≥ 6 символов, логин свободен; создаёт `User` (RoleId=1) и сохраняет в БД.
- `ExecuteCancel(object) : void` — закрывает окно регистрации (через параметр или поиск открытого окна).
- `CloseRegisterWindow() : void` — находит открытое `RegisterWindow` и закрывает.
- `OnPropertyChanged(string) : void` — `protected virtual`.

### Класс `MainViewModel` — DailyPlaner/ViewModels/MainViewModel.cs

Главная ViewModel приложения: страницы, списки записей, CRUD, поиск, тема, экспорт/импорт, напоминания, выход.

Константы страниц: `PageOverview`, `PageTasks`, `PageEvents`, `PageNotes`, `PageCalendar` (`public const string`).

Поля: `_databaseService`, `_notificationService` (`readonly`), `_reminderScheduler` (`ReminderScheduler`), `_currentUser` (`User`), `_tasks`/`_events`/`_notes` (`ObservableCollection<>`), `_selectedTask`/`_selectedEvent`/`_selectedNote`, `_searchText` (`string`), `_selectedDate` (`DateTime`), `_isDarkTheme` (`bool`).

Свойства:

- `CurrentUser` — при установке загружает данные и запускает планировщик напоминаний.
- `Tasks` — также обновляет `CompletedTasksCount`.
- `Events`, `Notes` — списки записей.
- `ActivePage` — текущая страница интерфейса.
- `SelectedTask`, `SelectedEvent`, `SelectedNote` — выбранные записи.
- `SearchText` — текст поиска (вызывает `ApplyNameSearch`).
- `SelectedDate` — дата для страницы календаря.
- `IsDarkTheme` — состояние темы.
- `CompletedTasksCount` — вычисляемое: число выполненных задач (счётчик на странице «Обзор»).

Команды (17 `ICommand`): `AddTaskCommand`, `EditTaskCommand`, `DeleteTaskCommand`, `CompleteTaskCommand`, `AddEventCommand`, `EditEventCommand`, `DeleteEventCommand`, `CompleteEventCommand`, `AddNoteCommand`, `EditNoteCommand`, `DeleteNoteCommand`, `ClearSearchCommand`, `ToggleThemeCommand`, `ExportToJsonCommand`, `ImportFromJsonCommand`, `TestNotificationCommand`, `LogoutCommand`.

Методы:

- `StartReminderScheduler() : void` — создаёт (один раз) и запускает `ReminderScheduler`.
- `LoadUserData() : void` — загружает задачи/события/заметки текущего пользователя.
- `CanExecuteTaskCommand/EventCommand/NoteCommand(object) : bool` — доступность кнопок (есть выбранные записи).
- `GetSelectedTasks/GetSelectedEvents/GetSelectedNotes(object) : List<>` — выбранные записи из мультиселекта списка либо `Selected*`.
- `PluralForms(int count, string one, string few, string many) : string` — `static` — русская форма множественного числа (1 задача / 2 задачи / 5 задач).
- `ConfirmDelete(int count, string word, string singleText) : bool` — диалог подтверждения удаления (одиночного и массового).
- `ExecuteAddTask(object)` — диалог `TaskDialog` → `CreateTask` → при включённом напоминании `CreateReminder` (использует `task.Id` из `SCOPE_IDENTITY`) → уведомление.
- `ExecuteEditTask(object)` — `TaskDialog` с данными выбранной задачи → `UpdateTask`.
- `ExecuteDeleteTask(object)` — подтверждение → `DeleteTask` для каждой выбранной.
- `ExecuteCompleteTask(object)` — переключает `IsCompleted` выбранной задачи → `UpdateTask` → уведомление.
- `ExecuteAddEvent(object)` — диалог `EventDialog` → `CreateEvent` → отложенное напоминание `ScheduleNotification`.
- `ExecuteEditEvent(object)` — `EventDialog` → `UpdateEvent`.
- `ExecuteDeleteEvent(object)` — подтверждение → `DeleteEvent` для каждой выбранной.
- `ExecuteCompleteEvent(object)` — переключает `IsCompleted` события → `UpdateEvent`.
- `ExecuteAddNote(object)` — диалог `NoteDialog` → `CreateNote`.
- `ExecuteEditNote(object)` — `NoteDialog` → `UpdateNote`.
- `ExecuteDeleteNote(object)` — подтверждение → `DeleteNote` для каждой выбранной.
- `ExecuteClearSearch(object)` — сбрасывает поиск и перезагружает данные.
- `ApplyNameSearch() : void` — `public` — поиск по названию/описанию (задачи/события) или заголовку/содержимому (заметки) на текущей странице.
- `ResetFiltersAndReload() : void` — `public` — очищает `SearchText` и перезагружает данные.
- `RefreshDataForCurrentPage() : void` — обновляет списки с учётом активной страницы (поиск или полная загрузка).
- `ExecuteToggleTheme(object)` — переключает тему через `App.ApplyTheme`.
- `ExecuteExportToJson(object)` — сериализует задачи пользователя в JSON (`Newtonsoft.Json`) через `SaveFileDialog`.
- `ExecuteImportFromJson(object)` — читает JSON через `OpenFileDialog`, создаёт задачи для текущего пользователя.
- `ExecuteTestNotification(object)` — кнопка «Проверить напоминания»: статистика из `ReminderScheduler` + тестовое уведомление.
- `ExecuteLogout(object)` — останавливает планировщик, очищает данные, открывает окно входа в `NavigationWindow`, закрывает все окна (разрешая закрытие главного).
- `OnPropertyChanged(string) : void` — `protected virtual`.

---

## Файл приложения (App)

### Класс `App` — DailyPlaner/App.xaml.cs

Точка входа. Управляет запуском, темой, треем, автозапуском и подключением к базе.

Статические свойства:

- `IsDarkTheme : bool` (private set) — текущая тема.
- `IsExiting : bool` (private set) — признак полного выхода.
- `TrayIcon : System.Windows.Forms.NotifyIcon` (private set) — значок в трее.

Константы:

- `SettingsRegistryKey = "SOFTWARE\\DailyPlanner"` — ветка настроек в реестре.
- `AutoStartRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"` — ветка автозапуска Windows.
- `AppName = "DailyPlanner"` — имя приложения в реестре.

Методы:

- `OnStartup(StartupEventArgs) : void` — `protected override` — трей → тема → проверка БД → автозапуск → окно входа в `NavigationWindow` (1000×650).
- `OnExit(ExitEventArgs) : void` — `protected override` — скрывает и освобождает `TrayIcon`.
- `FindIconFile() : string` — `public static` — ищет `Icon.jpg`: `Assets/` рядом с exe → рядом с exe → `Downloads` пользователя; `null`, если не найден.
- `SetWindowIcon(Window) : void` — `public static` — ставит иконку окна из найденного файла.
- `RestoreFromTray() : void` — `public static` — показывает и активирует главное окно.
- `ExitApp() : void` — `public static` — `IsExiting = true`, завершение приложения.
- `LoadDarkThemeSetting() : bool` — `public static` — тема из реестра.
- `LoadAutoStartSetting() : bool` — `public static` — автозапуск из реестра.
- `SetAutoStart(bool) : void` — `public static` — запись/удаление автозапуска в реестре + сохранение настройки; ошибки показываются в MessageBox.
- `ApplyTheme(bool) : void` — `public static` — сохраняет настройку и перекрашивает кисти в ресурсах приложения и всех открытых окнах.
- `SetupTrayIcon() : void` — `private` — контекстное меню трея («Открыть», «Выход») и значок.
- `LoadTrayIcon() : System.Drawing.Icon` — `private static` — иконка трея из Icon.jpg (клонирование + `DestroyIcon`), при ошибке — системная иконка.
- `CheckAndCreateDatabase() : void` — `private` — `TestConnection()`; при ошибке — MessageBox с текстом про SQL Server.
- `LoadSetting(string) : int` — `private static` — значение настройки из реестра (0/1).
- `SaveSetting(string, bool) : void` — `private static` — запись настройки в реестр.
- `SetThemeBrushes(ResourceDictionary, bool) : void` — `private static` — создаёт кисти темы (светлая: фон `#E8D8C9`, карточка `#FDFBF8`; тёмная: фон `#34445D`, карточка `#4B607F`) и подставляет по ключам `WindowBackgroundBrush`, `LightBackgroundBrush`, `SecondaryBrush`, `SoftBeigeBrush`, `MutedTextBrush` и связанным.

Вложенный класс `Win32`:

- `DestroyIcon(IntPtr) : bool` — `internal static extern` — P/Invoke `user32.dll`, освобождение дескриптора иконки.

---

## Представления (Views)

### `MainWindow` — DailyPlaner/Views/MainWindow.xaml(.cs)

Главное окно: боковая панель (Обзор, Задачи, События, Заметки, Календарь), панель поиска, страницы с карточками записей, страница календаря со списками дня.

Code-behind:

- `AllowClose : bool` — `public` — разрешение закрытия окна (используется при выходе из аккаунта).
- `MainWindow()` / `MainWindow(User)` — конструкторы: иконка окна, `DataContext = new MainViewModel`, галочка автозапуска, стартовая страница «Обзор», обновление списков дня при изменении `SelectedDate`/`Tasks`/`Events`.
- `SidebarNav_Click(object, RoutedEventArgs)` — переключение страниц по `Tag` кнопки.
- `ShowPage(string)` — показывает выбранную страницу, сбрасывает поиск, переключает стили кнопок навигации, управляет панелью поиска.
- `UpdateToolbarForPage(string)` — панель поиска видна только на страницах Задачи/События/Заметки.
- `SearchTextBox_KeyDown(object, KeyEventArgs)` — поиск по нажатию Enter.
- `AutoStartSidebar_Changed(object, RoutedEventArgs)` — галочка автозапуска → `App.SetAutoStart`.
- `LoadHeaderIcon()` — логотип в шапке из `App.FindIconFile()`.
- `RefreshDayLists()` — события и задачи выбранного дня для страницы календаря.
- `DayItem_MouseDoubleClick`, `TasksList_MouseDoubleClick`, `EventsList_MouseDoubleClick`, `NotesList_MouseDoubleClick` — открытие DetailsDialog по двойному клику.
- `ShowTaskDetails/ShowEventDetails/ShowNoteDetails` — вызовы DetailsDialog.
- `OnClosing(CancelEventArgs)` — `protected override` — сворачивание в трей вместо закрытия (пока `AllowClose`/`IsExiting` не установлены), с балуном-подсказкой.

### `LoginWindow` — DailyPlaner/Views/LoginWindow.xaml(.cs)

Страница входа (WPF `Page`, размещается в `NavigationWindow`).

- `LoginWindow()` — конструктор: `DataContext = new LoginViewModel()`, загрузка логотипа.
- `LoadHeaderIcon()` — логотип из `App.FindIconFile()`.
- `PasswordBox_PasswordChanged(object, RoutedEventArgs)` — обновляет `Password` в ViewModel, скрывает водяной знак пароля.

### `RegisterWindow` — DailyPlaner/Views/RegisterWindow.xaml(.cs)

Окно регистрации.

- `ViewModel : RegisterViewModel` — `private` — текущая ViewModel.
- `RegisterWindow()` — конструктор: иконка, ViewModel, подписка на смену пола, логотип.
- `LoadHeaderIcon()` — логотип из `App.FindIconFile()`.
- `GenderComboBox_SelectionChanged(object, SelectionChangedEventArgs)` — скрывает водяной знак пола при выборе.
- `PasswordBox_PasswordChanged` / `ConfirmPasswordBox_PasswordChanged` — обновление пароля/подтверждения в ViewModel и водяных знаков.
- `TextBox_TextChanged()` / `TextBox_TextChanged(object, TextChangedEventArgs)` — заглушки, сгенерированные средой.

---

## Диалоговые окна (Dialogs)

### `TaskDialog` — DailyPlaner/Views/Dialogs/TaskDialog.xaml(.cs)

Диалог создания/редактирования задачи: название, описание, дата, время, приоритет, напоминание.

- `ReminderOffsetMinutes : int[]` (`static readonly`) — смещения напоминаний в минутах (5, 10, 15, 30, 60, 120, 1440).
- `TaskTitle : string` — название из поля.
- `TaskDescription : string` — описание из поля.
- `TaskDueDate : DateTime` (`private set`) — итоговый срок.
- `ReminderEnabled : bool` — включено ли напоминание.
- `ReminderTime : DateTime?` (`private set`) — рассчитанное время напоминания.
- `TaskPriority : string` (`private set`) — выбранный приоритет.
- `TaskDialog(string, string, DateTime)` — конструктор с приоритетом по умолчанию «Средний».
- `TaskDialog(string, string, DateTime, string)` — полный конструктор: заполнение комбобоксов времени (шаг 30 минут), приоритета, смещений напоминания.
- `FillTimeItems(DateTime)` — заполняет комбобокс времени 48 значениями.
- `SelectTime(TimeSpan)` — выбирает ближайший получасовой слот.
- `ReminderCheckBox_Changed(object, RoutedEventArgs)` — показывает/скрывает панель напоминания.
- `ReminderOffsetComboBox_SelectionChanged(...)` — пересчёт времени напоминания.
- `GetDueDateFromControls() : DateTime` — дата + выбранное время.
- `UpdateReminderTime()` — `ReminderTime = срок − смещение`.
- `Save_Click(object, RoutedEventArgs)` — валидация названия, фиксация результатов, `DialogResult = true`.
- `Cancel_Click(object, RoutedEventArgs)` — `DialogResult = false`.

### `EventDialog` — DailyPlaner/Views/Dialogs/EventDialog.xaml(.cs)

Диалог создания/редактирования события: название, описание, место, начало и окончание (дата+время), напоминание.

- `ReminderOffsetMinutes : int[]` (`static readonly`).
- `EventTitle`, `EventDescription`, `EventLocation : string` — значения полей.
- `EventStart`, `EventEnd : DateTime` (`private set`) — начало и окончание.
- `ReminderEnabled : bool`, `ReminderTime : DateTime?`.
- `EventDialog(string, string, string, DateTime, DateTime? end = null)` — конструктор: комбобоксы времени начала и окончания, автосинхронизация даты окончания, смещения напоминания.
- `SyncEndDateToStart()` — дата окончания по умолчанию = дате начала.
- `FillTimeItems(ComboBox)` — 48 слотов по 30 минут.
- `SelectTime(ComboBox, TimeSpan)` — выбор слота.
- `ReminderCheckBox_Changed`, `ReminderOffsetComboBox_SelectionChanged` — панель и пересчёт напоминания.
- `GetStartFromControls()`, `GetEndFromControls() : DateTime` — дата+время начала/окончания.
- `UpdateReminderTime()` — время напоминания от начала события.
- `Save_Click` — валидация (название, окончание не раньше начала), `DialogResult = true`.
- `Cancel_Click` — `DialogResult = false`.
- `TitleBox_TextChanged` — заглушка.

### `NoteDialog` — DailyPlaner/Views/Dialogs/NoteDialog.xaml(.cs)

Диалог создания/редактирования заметки.

- `NoteTitle`, `NoteContent : string` — значения полей.
- `NoteDialog(string, string)` — конструктор, фокус на название.
- `Save_Click` — валидация названия, `DialogResult = true`.
- `Cancel_Click` — `DialogResult = false`.

### `DetailsDialog` — DailyPlaner/Views/Dialogs/DetailsDialog.xaml(.cs)

Оформленное окно подробной информации о записи (вместо MessageBox).

- `DetailsDialog()` — `private` — конструктор.
- `ShowTask(Window, Task) : void` — `static` — карточка задачи: значок, название, описание, срок, приоритет, статус («✅ Выполнена» / «🕐 Ожидает выполнения»).
- `ShowEvent(Window, Event) : void` — `static` — карточка события: начало и окончание, место, статус («✅ Завершено» / «🕐 Запланировано»).
- `ShowNote(Window, Note) : void` — `static` — карточка заметки: текст, дата создания (лишние поля скрыты).
- `Close_Click(object, RoutedEventArgs)` — закрытие окна.

---

## Конфигурационные файлы

### `App.config`

- `DailyPlannerConnection` — строка подключения: SQL Server `PCGl1tch`, база `DailyPlannerDB`, Windows-аутентификация (`Trusted_Connection=True`).

### `packages.config`

NuGet-пакеты:

- `Microsoft.Toolkit.Uwp.Notifications 7.1.3` — toast-уведомления Windows.
- `Microsoft.Windows.SDK.Contracts 10.0.19041.1` — WinRT API для уведомлений на .NET Framework.
- `Newtonsoft.Json 13.0.3` — экспорт/импорт задач в JSON.
- `System.ValueTuple 4.5.0` — кортежи значений (зависимость Newtonsoft на net48).

### `DailyPlaner.csproj`

Проект .NET Framework 4.8 (WPF). Ссылки: стандартные сборки .NET, `System.Windows.Forms` (трей), `System.Drawing` (иконки), WPF-сборки (`WindowsBase`, `PresentationCore`, `PresentationFramework`, `System.Xaml`), NuGet-пакеты выше.

---

## Общая архитектура (для защиты)

Приложение построено по паттерну MVVM:

1. **Models** — классы, отображающие таблицы БД (`Users`, `Tasks`, `Events`, `Notes`, `Reminders`, `Genders`).
2. **Services** — слой работы с данными (`DatabaseService`, ADO.NET + SQL) и инфраструктура (`NotificationService` — уведомления, `ReminderScheduler` — фоновая проверка напоминаний каждые 30 секунд).
3. **ViewModels** — команды и состояние для представлений (`RelayCommand` — реализация `ICommand`; `LoginViewModel`, `RegisterViewModel`, `MainViewModel`).
4. **Views** — XAML-разметка и code-behind (окна, страницы, диалоги), привязки к ViewModel.
5. **Converters** — преобразование данных для отображения (видимость, зачёркивание, статус, прозрачность).

Ключевые сценарии: вход/регистрация (SHA256-хэш пароля), CRUD задач/событий/заметок с подтверждением массового удаления (русские формы множественного числа), поиск по названию, календарь на день, напоминания (toast-уведомления + балун трея), светлая/тёмная тема (кисти в ресурсах), автозапуск (реестр Windows), экспорт/импорт задач в JSON, свёртывание в трей.
