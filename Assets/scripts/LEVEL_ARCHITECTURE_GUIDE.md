# 🎯 АРХИТЕКТУРА УРОВНЕЙ (ДЛЯ YANDEX GAMES)

## ✅ РЕКОМЕНДУЕМЫЙ ПОДХОД: Уровень сразу в сцене

Для вашей игры (Hidden Object, Яндекс Игры) **рекомендую** делать каждый уровень отдельной сценой **без префабов**.

---

## 📁 Структура сцены уровня

```
lvl1_office.unity
├── Canvas
│   ├── BackgroundContainer (RectTransform) - ПУСТОЙ
│   ├── ZoomContainer (RectTransform) - ПУСТОЙ
│   ├── GameUI (RectTransform, скрыт)
│   │   ├── WordsPanel
│   │   │   └── Content (VerticalLayoutGroup)
│   │   ├── HintButton
│   │   └── CloseZoomButton
│   └── StoryPanel (скрыт)
│       └── StoryDialog
│
├── LevelRoot (GameObject)
│   ├── LevelManager
│   │   └── currentLevelConfig: LVLconfig_1
│   └── LevelProgressTracker
│
├── Background (Sprite Renderer или Image)
│   └── Спрайт фона уровня
│
├── SearchZone1 (не-зум зона)
│   ├── SearchZone
│   │   ├── useZoom: false
│   │   ├── autoActivateOnStart: true
│   │   └── items: [HiddenItemData 1, 2, 3]
│   ├── BoxCollider2D (на весь фон или зону)
│   └── SearchableItem (дочерние, невидимые)
│       ├── itemData: HiddenItemData
│       └── BoxCollider2D
│
├── SearchZone2 (зум-зона)
│   ├── SearchZone
│   │   ├── useZoom: true
│   │   ├── autoActivateOnStart: false
│   │   └── items: [HiddenItemData 4, 5]
│   ├── BoxCollider2D (на зону)
│   └── ZoomZonePrefab (префаб, скрыт)
│       └── Детализированный спрайт зума
│           └── SearchableItem (дочерние)
│
└── DialogManager
    └── storyDialog: StoryDialog
```

---

## 🔧 Изменения в GameManager

Теперь `GameManager.LoadLevel()` будет просто загружать сцену уровня, а не инстанцировать префаб:

### Вариант 1: Загрузка сцены уровня (рекомендуется)

```csharp
public void LoadLevel(LevelConfig levelConfig)
{
    if (levelConfig == null) return;

    // Просто загружаем сцену уровня
    string sceneName = levelConfig.LevelName; // "lvl1_office"
    SceneManager.LoadScene(sceneName);
}
```

### Вариант 2: Оставить префабы (если нужно)

```csharp
public void LoadLevel(LevelConfig levelConfig)
{
    if (levelConfig.LevelPrefab != null)
    {
        // Инстанцируем префаб (старый способ)
        Instantiate(levelConfig.LevelPrefab, backgroundContainer);
    }
    else
    {
        // Загружаем сцену (новый способ)
        SceneManager.LoadScene(levelConfig.LevelName);
    }
}
```

---

## 📋 Настройка LevelConfig

### Для сцен (рекомендуется):
```csharp
LevelConfig: LVLconfig_1
├── LevelName: "lvl1_office"  // Имя сцены
├── LevelPrefab: (пусто)       // Не используется
├── GameUI: (пусто)            // Уже на сцене
├── IntroDialog: "Найди 5 предметов..."
├── OutroDialog: "Молодец!"
└── TotalItemsCount: 5
```

### Для префабов (если нужно):
```csharp
LevelConfig: LVLconfig_1
├── LevelName: "Level_BG1"
├── LevelPrefab: Level_BG1.prefab  // Префаб уровня
├── GameUI: GameUI.prefab          // Префаб UI
├── IntroDialog: "Найди 5 предметов..."
├── OutroDialog: "Молодец!"
└── TotalItemsCount: 5
```

---

## 🎮 Поток игры (со сценами)

```
1. Запуск
   ↓
2. bootstrap.unity
   → Создаёт менеджеры
   → Загружает main_menu
   ↓
3. main_menu.unity
   → Игрок нажимает "Играть"
   → GameManager.ShowLevelSelect()
   ↓
4. select_level.unity
   → Игрок выбирает уровень
   → GameManager.LoadLevel("lvl1_office")
   ↓
5. lvl1_office.unity (загружается вся сцена)
   → LevelManager.Initialize()
   → SearchManager.ActivateZone() для не-зум зон
   → Игрок ищет предметы
   → LevelManager.CompleteLevel()
   → GameManager.ShowLevelSelect()
   ↓
6. select_level.unity (возврат)
```

---

## ✅ Преимущества для Яндекс Игр

### 1. Быстрая загрузка
- ❌ **Префабы:** Загрузка → Инстанцирование → Поиск ссылок
- ✅ **Сцены:** Загрузка сцены → Всё готово

### 2. Меньше багов
- ❌ **Префабы:** Могут потеряться ссылки, не назначиться UI
- ✅ **Сцены:** Всё назначено в инспекторе, работает сразу

### 3. Удобная разработка
- ❌ **Префабы:** Нужно открывать префаб для редактирования
- ✅ **Сцены:** Видите весь уровень целиком, легко тестировать

### 4. Оптимизация
- ❌ **Префабы:** Дублирование UI на каждый уровень
- ✅ **Сцены:** UI один на сцену, меньше памяти

---

## 🔧 Как перейти на сцены

### Шаг 1: Создайте сцены уровней
```
1. Откройте lvl_template.unity
2. Сохраните как lvl1_office.unity
3. Настройте фон, зоны, предметы
4. Повторите для всех уровней
```

### Шаг 2: Настройте LevelConfig
```
1. Откройте LVLconfig_1
2. LevelName: "lvl1_office" (имя сцены)
3. LevelPrefab: (оставьте пустым)
```

### Шаг 3: Добавьте сцены в Build Settings
```
File → Build Settings → Add Open Scenes
Порядок:
  0. bootstrap.unity
  1. main_menu.unity
  2. select_level.unity
  3. lvl1_office.unity
  4. lvl2_*.unity
  ...
```

### Шаг 4: Исправьте GameManager.LoadLevel()
```csharp
// В GameManager.cs замените LoadLevel(LevelConfig) на:

public void LoadLevel(LevelConfig levelConfig)
{
    if (levelConfig == null)
    {
        Debug.LogError("GameManager: LevelConfig is null!");
        return;
    }

    // Загружаем сцену уровня по имени
    string sceneName = levelConfig.LevelName;
    
    if (!string.IsNullOrEmpty(sceneName))
    {
        ShowLoadingScreen(true);
        UnloadCurrentLevel();
        SceneManager.LoadScene(sceneName);
        ShowLoadingScreen(false);
    }
    else
    {
        Debug.LogError("GameManager: LevelName is empty!");
    }
}
```

---

## 🐛 Отладка

### Включите DEBUG логи
```
Edit → Project Settings → Player
Scripting Define Symbols: DEBUG
```

### Проверьте загрузку
```
[GameManager] LoadLevel: lvl1_office
[LevelManager] Initialize() called
[SearchManager] Activating zone 'SearchZone1'
```

---

## 📊 Итоговая рекомендация

| Критерий | Префабы | Сцены |
|----------|---------|-------|
| **Быстрый лоад** | ❌ | ✅ |
| **Удобство** | ❌ | ✅ |
| **Меньше багов** | ❌ | ✅ |
| **DLC** | ✅ | ❌ |
| **Память** | ❌ | ✅ |

**Для Яндекс Игр:** Используйте **сцены** ✅

---

**Версия:** 1.0  
**Дата:** 2 марта 2026
