# 🛠 Архитектурные улучшения — Отчёт о внедрении

## ✅ Внедрённые улучшения

### 1. Self-Singleton паттерн (PersistentManager<T>)

**Файл:** `Assets/Scripts/Core/PersistentManager.cs`

Базовый класс для всех постоянных менеджеров. Убирает дублирование кода Awake/DontDestroyOnLoad.

**Использование:**
```csharp
public class SoundManager : PersistentManager<SoundManager>
{
    protected override void OnInit()
    {
        // Инициализация при первом создании
    }
}
```

**Обновлённые менеджеры:**
- ✅ SoundManager
- ✅ LevelProgressManager
- ✅ SearchManager
- ✅ LoadingScreenManager
- ✅ ResetProgressManager
- ✅ InterstitialAdManager

---

### 2. Универсальный ClickHandler

**Файл:** `Assets/Scripts/Core/ClickHandler.cs`

Заменяет 4 устаревших скрипта для обработки кликов.

**Преимущества:**
- Один скрипт для всех типов кликов (фон, UI, 3D-объекты)
- Автоматический поиск SearchZone в родителях
- Проверка завершения зоны перед активацией

**Использование:**
```
1. Добавьте ClickHandler на объект
2. Назначьте Collider2D (для мира) или Image с Raycast Target (для UI)
3. Опционально: назначьте targetZone (или оставьте пустым для автопоиска)
```

---

### 3. Устаревшие скрипты (помечены на удаление)

| Скрипт | Новый файл | Статус |
|--------|-----------|--------|
| BackgroundTrigger | ClickHandler | ⛔ del_BackgroundTrigger.cs |
| UIZoneTrigger | ClickHandler | ⛔ del_UIZoneTrigger.cs |
| ZoomTrigger | ClickHandler | ⛔ del_ZoomTrigger.cs |

**Следующие шаги:**
1. Заменить все ссылки на старые скрипты в сценах
2. Удалить файлы `del_*.cs`

---

### 4. LevelConfig с поддержкой Addressables

**Файл:** `Assets/Scripts/LevelConfig.cs`

Добавлена поддержка Addressable сцен через `AssetReference`.

**Три режима загрузки:**
1. **LevelPrefab** — загрузка префаба (приоритет)
2. **sceneReference** — загрузка сцены через Addressables (рекомендуется)
3. **legacyLevelKey** — старый режим через Resources.Load

**Новые свойства:**
```csharp
public AssetReference sceneReference;  // Ссылка на Addressable сцену
public bool IsAddressableScene;        // Проверка режима
public bool IsPrefabMode;              // Проверка режима
public bool IsLegacyMode;              // Проверка режима
```

---

### 5. Шаблон уровня (temple_lvl)

**Папка:** `Assets/temple_lvl/`

Готовая структура для создания новых уровней.

**Структура:**
```
temple_lvl/
├── Scenes/           # Сцены уровней
├── Prefabs/          # Префабы (ZoomZones, ...)
├── Scripts/          # LevelRoot.cs
├── Configs/          # LevelConfigAddressable.cs
├── Art/
│   ├── Backgrounds/
│   └── Items/
└── README.md         # Инструкция
```

**Компоненты:**
- **LevelRoot.cs** — корневой компонент уровня
- **LevelConfigAddressable.cs** — конфиг для Addressables

---

## 📋 Что НЕ было изменено (по вашему запросу)

### PlayerPrefs и сохранения

**LevelProgressTracker** оставлен без изменений:
- Сохраняет найденные предметы в PlayerPrefs
- Сохраняет завершённые зоны
- Синхронизируется с Yandex Games через LocalStorage

**LevelProgressManager** оставлен без изменений:
- Управляет разблокировкой уровней
- Использует LocalStorage для Yandex Games

---

## 🔄 Миграция: как обновить существующие уровни

### Шаг 1: Заменить старые клики на ClickHandler

**Было:**
```
Background (с BackgroundTrigger)
└── SearchZone
```

**Стало:**
```
Background (с ClickHandler + Collider2D)
└── SearchZone
```

### Шаг 2: Обновить менеджеры в bootstrap

**Было:**
```
BootstrapManager создаёт менеджеры вручную
```

**Стало:**
```
Менеджеры сами создают себя через PersistentManager<T>
```

### Шаг 3: Настроить Addressables для уровней

1. Откройте **Window > Asset Management > Addressables > Groups**
2. Создайте группу `Levels`
3. Перетащите сцены уровней в группу
4. Установите Address = LevelName из LevelConfig

---

## 🎯 Итоговая архитектура

```
┌─────────────────────────────────────────────────────────┐
│                    BOOTSTRAP SCENE                       │
│  ┌────────────────────────────────────────────────────┐ │
│  │ PersistentManagers (Self-Singleton):               │ │
│  │ • GameManager                                      │ │
│  │ • SoundManager : PersistentManager<SoundManager>   │ │
│  │ • LevelProgressManager : PersistentManager<...>    │ │
│  │ • InterstitialAdManager : PersistentManager<...>   │ │
│  │ • ResetProgressManager : PersistentManager<...>    │ │
│  │ • LoadingScreenManager : PersistentManager<...>    │ │
│  │ • SearchManager : PersistentManager<SearchManager> │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    LEVEL SCENE                           │
│  ┌────────────────────────────────────────────────────┐ │
│  │ LevelRoot                                          │ │
│  │ └── LevelConfig (ScriptableObject)                 │ │
│  │     • LevelName                                    │ │
│  │     • sceneReference (Addressable)                 │ │
│  │     • IntroDialog / OutroDialog                    │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │ SearchZone[]                                       │ │
│  │ └── ClickHandler (универсальный клик)              │ │
│  │ └── SearchableItem[]                               │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │ LevelManager (на сцене)                            │ │
│  │ └── Управляет завершением уровня                   │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌────────────────────────────────────────────────────┐ │
│  │ LevelProgressTracker (на сцене)                    │ │
│  │ └── Сохраняет прогресс в PlayerPrefs               │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Новые файлы

| Файл | Назначение |
|------|------------|
| `Scripts/Core/PersistentManager.cs` | Базовый класс для менеджеров |
| `Scripts/Core/ClickHandler.cs` | Универсальный клик |
| `temple_lvl/Scripts/LevelRoot.cs` | Корневой компонент уровня |
| `temple_lvl/Configs/LevelConfigAddressable.cs` | Конфиг для Addressables |
| `temple_lvl/README.md` | Документация шаблона |

---

## ⛔ Файлы на удаление

| Файл | Причина |
|------|---------|
| `Scripts/del_BackgroundTrigger.cs` | Заменён на ClickHandler |
| `Scripts/del_UIZoneTrigger.cs` | Заменён на ClickHandler |
| `Scripts/del_ZoomTrigger.cs` | Устарел |

---

## ✅ Следующие шаги

1. **Протестировать** новые менеджеры в игре
2. **Заменить** старые скрипты в сценах на ClickHandler
3. **Настроить** Addressables для уровней
4. **Удалить** файлы `del_*.cs` после тестирования
5. **Создать** новый уровень через шаблон `temple_lvl`

---

## 🔧 PlayerPrefs структура

Сохраняемые данные (не изменены):

```
PlayerPrefs:
├── LevelProgress (JSON) — разблокировка уровней
├── LevelProgress_{LevelName} — найденные предметы
└── LevelProgress_{LevelName}_completedZones — завершённые зоны

LocalStorage (Yandex Games):
└── LevelProgress (JSON) — синхронизация прогресса
```
