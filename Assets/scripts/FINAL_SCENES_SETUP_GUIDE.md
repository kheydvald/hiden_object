# 📖 ФИНАЛЬНОЕ РУКОВОДСТВО ПО НАСТРОЙКЕ СЦЕН

## 🎯 Быстрый старт

### ⚠️ КРИТИЧЕСКИ ВАЖНО: Изменения в bootstrap.unity

**Перед началом настройки:**

1. **Откройте `bootstrap.unity`**
2. **Удалите** отдельные объекты:
   - `GameManager` (объект)
   - `LevelProgressManager` (объект)
3. **Оставьте только** `BootstrapManager` (объект)
4. **Настройте BootstrapManager** в инспекторе:
   - `firstSceneName`: "main_menu"
   - `persistentManagersPrefab`: (оставьте пустым)

> **Почему?** BootstrapManager автоматически создаст GameManager и LevelProgressManager при запуске. Дубликаты приведут к ошибкам!

---

## 📁 СТРУКТУРА СЦЕН (Build Settings)

Откройте `File → Build Settings` и добавьте сцены в **строгом порядке**:

```
Index 0: Assets/Scenes/bootstrap.unity        (загрузочная)
Index 1: Assets/Scenes/main_menu.unity        (главное меню)
Index 2: Assets/Scenes/select_level.unity     (выбор уровней)
Index 3: Assets/Scenes/lvl1_office.unity      (уровень 1)
Index 4: Assets/Scenes/lvl2_*.unity           (уровень 2)
...
Index N: Assets/Scenes/lvlN_*.unity           (уровень N)
```

---

## 🔧 НАСТРОЙКА СЦЕН

### 1️⃣ bootstrap.unity (загрузочная сцена)

**Что должно быть на сцене:**
```
BootstrapManager (GameObject)
└── Компонент: BootstrapManager
    ├── firstSceneName: "main_menu"
    └── persistentManagersPrefab: (пусто)
```

**Что происходит:**
- При запуске BootstrapManager создаёт:
  - `PersistentManagers` (объект)
    - `GameManager` (автоматически)
    - `SoundManager` (автоматически)
    - `LevelProgressManager` (автоматически)
    - `InterstitialAdManager` (автоматически)
    - `ResetProgressManager` (опционально, закомментировано)
- Все менеджеры получают `DontDestroyOnLoad`
- Загружается сцена `main_menu`

**⚠️ ВАЖНО:**
- ❌ НЕ создавайте GameManager вручную
- ❌ НЕ создавайте SoundManager вручную
- ❌ НЕ создавайте LevelProgressManager вручную
- ❌ НЕ создавайте InterstitialAdManager вручную
- ✅ Оставьте только BootstrapManager
- ℹ️ ResetProgressManager создаётся при необходимости (для кнопки сброса прогресса)

---

### 2️⃣ main_menu.unity (главное меню)

**Что должно быть на сцене:**
```
Canvas
├── Panel_MainMenu (Image)
│   ├── Button_Play (Button)
│   │   └── Компонент: MenuPlayButton
│   ├── Button_Settings (Button) — опционально
│   ├── Button_Exit (Button) — опционально
│   └── Button_ResetProgress (Button) — опционально (сброс прогресса)
│       └── Компонент: ResetProgressButton
└── MenuController (GameObject, пустой)
    └── Компонент: MenuController
        ├── menuPanel: Panel_MainMenu
        └── musicSource: (AudioSource, опционально)

ResetProgressManager (GameObject) — опционально (для сброса прогресса)
└── Компонент: ResetProgressManager

EventSystem (GameObject)
```

**Настройка ResetProgressManager (для сброса прогресса):**

1. Создайте пустой объект `ResetProgressManager`
2. Добавьте компонент `ResetProgressManager`
3. Не требует дополнительной настройки

**ИЛИ** раскомментируйте создание в BootstrapManager:
```csharp
// В BootstrapManager.cs раскомментируйте:
GameObject resetManagerObject = new GameObject("ResetProgressManager");
resetManagerObject.transform.SetParent(managersObject.transform);
resetManagerObject.AddComponent<ResetProgressManager>();
```

**Настройка кнопок:**

#### 🔴 Button_Play (Играть)

**Вариант A: Через MenuPlayButton (рекомендуется)**

1. Добавьте компонент `MenuPlayButton` на объект `Button_Play`
2. Убедитесь, что на объекте есть компонент `Button`
3. В инспекторе кнопки в событии `OnClick()` добавьте:
   ```
   Object: Button_Play (этот же объект)
   Function: MenuPlayButton.OnClick()
   ```

**Вариант B: Через GameManager.Instance (если хотите напрямую)**

1. В инспекторе кнопки в событии `OnClick()` добавьте:
   - Нажмите `+` для добавления события
   - Object: перетащите любой объект сцены (например, Canvas)
   - Function: `GameManager.Instance.ShowLevelSelect()` **НЕ сработает!**
   
   **Правильный способ:**
   - Используйте Вариант A с MenuPlayButton

#### 🔙 Button_Back / Button_Settings

- Используйте `MenuController` для управления меню
- Настройте `menuPanel` в инспекторе MenuController

---

### 3️⃣ select_level.unity (выбор уровней)

**Что должно быть на сцене:**
```
Canvas
├── Panel_LevelSelect (Image)
│   ├── Button_Back (Button)
│   │   └── Компонент: MenuBackButton
│   └── Grid_Levels (RectTransform с GridLayoutGroup)
│       ├── Button_Level1 (Button)
│       │   └── Компонент: LevelButton
│       │       ├── levelConfig: LVLconfig_1
│       │       ├── levelImage: (Image на кнопке)
│       │       ├── lockOverlay: (GameObject блокировки)
│       │       └── lockIcon: (иконка замка)
│       ├── Button_Level2 (Button)
│       │   └── Компонент: LevelButton
│       └── ... (кнопки для всех уровней)
└── LevelSelectionManager (GameObject, пустой)
    └── Компонент: LevelSelectionManager
        └── levelButtons: [массив ссылок на LevelButton]

EventSystem (GameObject)
```

**Настройка Button_Back:**

1. Добавьте компонент `MenuBackButton` на объект `Button_Back`
2. Убедитесь, что на объекте есть компонент `Button`
3. В событии `OnClick()` кнопки добавьте:
   ```
   Object: Button_Back (этот же объект)
   Function: MenuBackButton.OnClick()
   ```

**Настройка LevelButton:**

Для каждой кнопки уровня:

1. Добавьте компонент `LevelButton` на объект кнопки
2. Убедитесь, что на объекте есть компонент `Button`
3. Назначьте поля в инспекторе:
   - `levelConfig` → ScriptableObject LevelConfig (например, LVLconfig_1)
   - `levelImage` → Image на кнопке (для спрайтов)
   - `lockOverlay` → GameObject блокировки (если есть)
   - `lockIcon` → Иконка замка (если есть)

**Настройка LevelSelectionManager:**

1. Создайте пустой объект `LevelSelectionManager`
2. Добавьте компонент `LevelSelectionManager`
3. В поле `levelButtons` добавьте все кнопки уровней:
   - Нажмите `+` для каждого уровня
   - Перетащите объект кнопки уровня (Button_Level1, Button_Level2, ...)

---

### 4️⃣ lvl1_office.unity (шаблон уровня)

**Что должно быть на сцене:**
```
Canvas
├── BackgroundContainer (RectTransform)
│   └── Сюда GameManager будет загружать префаб уровня
├── ZoomContainer (RectTransform)
│   └── Сюда загружаются зум-объекты
├── GameUI (RectTransform, изначально скрыт)
│   ├── WordsPanel (RectTransform)
│   │   └── Content (VerticalLayoutGroup)
│   ├── HintButton (Button)
│   │   └── Компонент: HintButtonController
│   └── CloseZoomButton (Button)
│       └── Компонент: CloseZoomButton
└── StoryPanel (RectTransform, изначально скрыт)
    └── StoryDialog (StoryDialog)

LevelRoot (GameObject)
├── Компонент: LevelManager
│   └── currentLevelConfig: LVLconfig_1
└── Компонент: LevelProgressTracker

DialogManager (GameObject)
└── Компонент: DialogManager
    └── storyDialog: StoryDialog

SearchZone1 (GameObject)
├── Компонент: SearchZone
│   ├── useZoom: false
│   ├── autoActivateOnStart: true
│   ├── items: [HiddenItemData 1, HiddenItemData 2, ...]
│   ├── wordsPanelOverride: (пусто)
│   └── itemWordPrefabOverride: (пусто)
├── BoxCollider2D (для клика)
└── SearchableItem (дочерние для не-зум зон)
    └── Компонент: SearchableItem
        └── itemData: HiddenItemData

SearchZone2 (GameObject) — для зум-зон
├── Компонент: SearchZone
│   ├── useZoom: true
│   ├── autoActivateOnStart: false
│   ├── zoomZoneReference: BG1_zzokno (префаб зума)
│   └── items: [HiddenItemData 1, HiddenItemData 2, ...]
└── BoxCollider2D
```

**Настройка LevelManager:**

1. Найдите объект `LevelRoot` (или создайте пустой)
2. Добавьте компонент `LevelManager`
3. Назначьте `currentLevelConfig` → ScriptableObject LevelConfig

**Настройка LevelProgressTracker:**

1. Добавьте компонент `LevelProgressTracker` на `LevelRoot`
2. Не требует дополнительной настройки

**Настройка DialogManager:**

1. Создайте пустой объект `DialogManager`
2. Добавьте компонент `DialogManager`
3. Назначьте `storyDialog` → StoryDialog на сцене

**Настройка SearchZone (не-зум зона):**

```
useZoom: false
autoActivateOnStart: true
items: [HiddenItemData 1, HiddenItemData 2, ...]
```

Добавьте дочерние объекты `SearchableItem` с `BoxCollider2D`

**Настройка SearchZone (зум-зона):**

```
useZoom: true
autoActivateOnStart: false
zoomZoneReference: BG1_zzokno (префаб зума)
items: [HiddenItemData 1, HiddenItemData 2, ...]
```

---

## 🎮 ПОТОК ИГРЫ

```
1. Запуск игры
   ↓
2. bootstrap.unity
   → BootstrapManager.Awake()
   → Создаёт PersistentManagers
     → GameManager (DontDestroyOnLoad)
     → SoundManager (DontDestroyOnLoad)
     → LevelProgressManager (DontDestroyOnLoad)
     → InterstitialAdManager (DontDestroyOnLoad)
     → ResetProgressManager (DontDestroyOnLoad)
   → BootstrapManager.Start()
   → Загружает main_menu
   ↓
3. main_menu.unity
   → GameManager.Instance уже существует
   → SoundManager.Instance уже существует
   → GameManager.Start()
   → Игрок нажимает "Играть"
   → MenuPlayButton.OnClick()
   → GameManager.Instance.ShowLevelSelect()
   ↓
4. select_level.unity
   → LevelSelectionManager.Start()
   → Обновляет кнопки уровней
   → Игрок нажимает кнопку уровня
   → LevelButton.OnClick()
   → GameManager.Instance.LoadLevel(levelConfig)
   ↓
5. lvlN.unity
   → GameManager загружает префаб уровня в BackgroundContainer
   → LevelManager.Initialize()
   → SearchManager.ActivateZone() для не-зум зон
   → Игрок ищет предметы
   → SearchableItem.OnFound()
   → SearchManager.NotifyItemFound()
   → LevelProgressTracker.RegisterItemFound()
   → LevelManager.ItemFound() × N
   → LevelManager.CompleteLevel()
   → GameManager.OnLevelCompleted()
   → GameManager.ShowLevelSelect()
   ↓
6. select_level.unity (возврат)
```

---

## ⚠️ ЧЕК-ЛИСТ ПЕРЕД ЗАПУСКОМ

### bootstrap.unity
- [ ] Удалены объекты GameManager и LevelProgressManager
- [ ] Оставлен только BootstrapManager
- [ ] В BootstrapManager.firstSceneName = "main_menu"
- [ ] persistentManagersPrefab = (пусто)

### main_menu.unity
- [ ] Button_Play имеет компонент MenuPlayButton
- [ ] Button_Play.OnClick() вызывает MenuPlayButton.OnClick()
- [ ] Есть MenuController (опционально)
- [ ] Есть EventSystem

### select_level.unity
- [ ] Button_Back имеет компонент MenuBackButton
- [ ] Button_Back.OnClick() вызывает MenuBackButton.OnClick()
- [ ] Все кнопки уровней имеют компонент LevelButton
- [ ] LevelButton.levelConfig назначен для каждой кнопки
- [ ] Есть LevelSelectionManager с массивом levelButtons
- [ ] Есть EventSystem

### lvlN.unity (каждый уровень)
- [ ] Есть Canvas с BackgroundContainer и ZoomContainer
- [ ] Есть GameUI с WordsPanel (Content внутри)
- [ ] Есть LevelRoot с LevelManager и LevelProgressTracker
- [ ] LevelManager.currentLevelConfig назначен
- [ ] Есть DialogManager с storyDialog
- [ ] SearchZone настроены (useZoom, autoActivateOnStart, items)
- [ ] SearchableItem имеют BoxCollider2D

### Build Settings
- [ ] bootstrap.unity — Index 0
- [ ] main_menu.unity — Index 1
- [ ] select_level.unity — Index 2
- [ ] Уровни — Index 3+

---

## 🐛 ОТЛАДКА

### Включение DEBUG логов

1. `Edit → Project Settings → Player`
2. В разделе `Scripting Define Symbols` добавьте: `DEBUG`
3. Нажмите `Apply`

### Проверка работы

**Консоль при запуске:**
```
[BootstrapManager] Persistent managers created successfully
[GameManager] Start: GameUI найден: GameUI (isPrefab=False)
[SearchManager] WordsPanel automatically assigned: WordsPanel
```

**При нажатии "Играть":**
```
[MenuPlayButton] OnClick()
[GameManager] ShowLevelSelect: Loading level select scene
```

**При выборе уровня:**
```
[LevelButton] Клик! Загружаем уровень: Level_BG1
[GameManager] Начинаем загрузку уровня: 'Level_BG1'
[LevelManager] Initialize() called
[SearchManager] Activating zone 'SearchZone1'
```

### Частые проблемы

**❌ "GameManager.Instance is null"**

**Причина:** bootstrap.unity не загружается первой или менеджеры не созданы.

**Решение:**
1. Проверьте, что bootstrap.unity — Index 0 в Build Settings
2. Удалите GameManager и LevelProgressManager из bootstrap.unity
3. Оставьте только BootstrapManager

---

**❌ Кнопка Play не работает**

**Причина:** Неправильная ссылка на GameManager в OnClick.

**Решение:**
1. Добавьте компонент `MenuPlayButton` на Button_Play
2. В OnClick() кнопки выберите:
   - Object: Button_Play
   - Function: MenuPlayButton.OnClick()

---

**❌ LevelButton не видит LevelConfig**

**Причина:** Не назначен levelConfig в инспекторе.

**Решение:**
1. Выделите кнопку уровня
2. В инспекторе LevelButton назначьте `levelConfig` → LVLconfig_N

---

**❌ WordsPanel не показывается**

**Причина:** SearchManager не нашёл WordsPanel.

**Решение:**
1. Проверьте, что в GameUI есть RectTransform с именем "WordsPanel"
2. Внутри WordsPanel должен быть дочерний объект "Content" с VerticalLayoutGroup
3. Проверьте, что SearchManager.SetWordsPanel() вызывается

---

## 📞 ЕСЛИ ЧТО-ТО ПОШЛО НЕ ТАК

1. **Проверьте консоль на ошибки** — красные сообщения укажут на проблему
2. **Проверьте иерархию сцен** — все ли объекты на месте
3. **Проверьте ссылки в инспекторе** — нет ли `Missing Reference`
4. **Убедитесь, что сцены в Build Settings в правильном порядке**
5. **Перезапустите Unity** — иногда помогает перезагрузка
6. **Очистите кэш** — `Edit → Preferences → External Tools → Regenerate project files`

---

## 🎯 СОЗДАНИЕ НОВОГО УРОВНЯ

### Шаг 1: Создайте LevelConfig

1. В Project окне: `Right Click → Create → Configs → Level Config`
2. Назовите `LVLconfig_N`
3. Заполните:
   ```
   LevelName: "Level_BGN"
   LevelPrefab: (пока пусто)
   IntroDialog: "Текст интро..."
   OutroDialog: "Текст аутро..."
   TotalItemsCount: 5
   ```

### Шаг 2: Создайте сцену уровня

1. `File → New Scene`
2. Сохраните как `Assets/Scenes/lvlN_название.unity`
3. Скопируйте структуру из `lvl_template.unity` или `lvl1_office.unity`

### Шаг 3: Настройте уровень

1. Добавьте LevelManager и назначьте `currentLevelConfig`
2. Добавьте SearchZone с предметами
3. Настройте UI (GameUI, WordsPanel, DialogManager)

### Шаг 4: Создайте префаб уровня

1. В сцене уровня выделите корневой объект (фон + зоны)
2. Перетащите в `Assets/Prefab/BG/`
3. Назовите `Level_BGN`
4. В LevelConfig назначьте `LevelPrefab` → этот префаб

### Шаг 5: Добавьте сцену в Build Settings

`File → Build Settings → Add Open Scenes`

### Шаг 6: Добавьте кнопку в select_level

1. Откройте `select_level.unity`
2. Скопируйте существующую кнопку уровня
3. Назначьте новый `LevelConfig` в LevelButton
4. Добавьте кнопку в `LevelSelectionManager.levelButtons`

---

## 📄 ИТОГОВАЯ СТРУКТУРА ФАЙЛОВ

```
Assets/
├── Scenes/
│   ├── bootstrap.unity          # Загрузочная (только BootstrapManager)
│   ├── main_menu.unity          # Главное меню
│   ├── select_level.unity       # Выбор уровней
│   ├── lvl1_office.unity        # Уровень 1
│   ├── lvl2_*.unity             # Уровень 2
│   └── ...
├── scripts/
│   ├── BootstrapManager.cs      # Создаёт менеджеры
│   ├── GameManager.cs           # Главный менеджер (singleton)
│   ├── LevelManager.cs          # Менеджер уровня
│   ├── SearchManager.cs         # Менеджер поиска
│   ├── LevelProgressManager.cs  # Прогресс уровней (singleton)
│   ├── LevelProgressTracker.cs  # Трекер прогресса на уровне
│   ├── MenuPlayButton.cs        # Кнопка "Играть"
│   ├── MenuBackButton.cs        # Кнопка "Назад"
│   ├── LevelButton.cs           # Кнопка уровня
│   ├── LevelSelectionManager.cs # Менеджер выбора уровней
│   ├── MenuController.cs        # Контроллер меню (опционально)
│   └── ...
└── Prefab/
    └── BG/
        ├── Level_BG1.prefab
        ├── Level_BG2.prefab
        └── ...
```

---

## ✅ ПРОВЕРКА РАБОТОСПОСОБНОСТИ

1. **Запустите игру** (Play в Unity)
2. **Должно загрузиться главное меню**
3. **Нажмите "Играть"** — должен открыться экран выбора уровней
4. **Нажмите кнопку уровня** — должен загрузиться уровень
5. **Найдите предметы** — слова должны исчезать из списка
6. **Завершите уровень** — должен вернуться экран выбора уровней

Если всё работает — **поздравляю!** 🎉
