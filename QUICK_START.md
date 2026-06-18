# 🎯 КРАТКАЯ ИНСТРУКЦИЯ ПО НАСТРОЙКЕ СЦЕН

## 📋 ЧТО И ГДЕ ДОЛЖНО НАХОДИТЬСЯ

---

## 1️⃣ bootstrap.unity (Build Index 0)

**Пустая сцена с одним объектом:**

```
BootstrapManager (GameObject)
└── Компонент: BootstrapManager
    ├── firstSceneName: "main_menu"
    └── persistentManagersPrefab: (пусто)
```

**Что делает:**
- Создаёт `GameManager`, `LevelProgressManager`, `InterstitialAdManager`
- Все менеджеры получают `DontDestroyOnLoad`
- Загружает `main_menu.unity`

---

## 2️⃣ main_menu.unity (Build Index 1)

**Структура:**
```
Canvas
├── Panel_MainMenu (Image)
│   ├── Title (Text)
│   ├── Button_Play (Button)
│   ├── Button_Settings (Button) — опционально
│   └── Button_Exit (Button) — опционально
└── MenuController (GameObject)
    └── Компонент: MenuController
        ├── menuPanel: Panel_MainMenu
        └── musicSource: (опционально)
```

**Настройка кнопок:**

**Button_Play → OnClick:**
```
Object: GameManager (Instance)
Function: GameManager.ShowLevelSelect()
```

**Button_Exit → OnClick:**
```
Object: MenuController
Function: MenuController.ToggleMusic() — или другое
```

---

## 3️⃣ select_level.unity (Build Index 2)

**Структура:**
```
Canvas
├── Panel_LevelSelect (Image)
│   ├── Title (Text)
│   ├── Button_Back (Button)
│   └── Grid_Levels (RectTransform + GridLayoutGroup)
│       ├── Button_Level1
│       │   └── Компонент: LevelButton
│       ├── Button_Level2
│       │   └── Компонент: LevelButton
│       └── ... ( Button_Level3 - Button_Level15)
└── LevelSelectionManager (GameObject)
    └── Компонент: LevelSelectionManager
        └── levelButtons: [Button_Level1, Button_Level2, ...]
```

**Настройка LevelButton (на каждой кнопке):**
```
Компонент: LevelButton
├── levelConfig: [LVLconfig_1] ← ScriptableObject
├── levelImage: [Image на кнопке]
├── lockOverlay: [GameObject замка]
└── lockIcon: [GameObject иконки замка]
```

**Button_Back → OnClick:**
```
Object: GameManager (Instance)
Function: GameManager.ShowMainMenu()
```

---

## 4️⃣ lvl1_office.unity (и все уровни, Build Index 3+)

**Структура:**
```
Canvas
├── BackgroundContainer (RectTransform)
├── ZoomContainer (RectTransform)
├── GameUI (RectTransform, изначально скрыт)
│   ├── WordsPanel (RectTransform)
│   │   └── Content (VerticalLayoutGroup)
│   ├── HintButton (Button + HintButtonController)
│   └── CloseZoomButton (Button + CloseZoomButton)
└── StoryPanel (RectTransform, изначально скрыт)
    └── StoryDialog (StoryDialog)

LevelRoot (GameObject)
├── Компонент: LevelManager
│   └── currentLevelConfig: [LVLconfig_1] ← назначить!
└── Компонент: LevelProgressTracker

DialogManager (GameObject)
└── Компонент: DialogManager
    └── storyDialog: [StoryDialog на сцене]

SearchZone1 (GameObject)
├── Компонент: SearchZone
│   ├── useZoom: false
│   ├── autoActivateOnStart: true
│   ├── items: [HiddenItemData 1, HiddenItemData 2, ...]
│   └── zoomZoneReference: (пусто)
├── BoxCollider2D
└── SearchableItem (дочерние)
    └── itemData: [HiddenItemData]

SearchZone2 (GameObject) — зум-зона
├── Компонент: SearchZone
│   ├── useZoom: true
│   ├── autoActivateOnStart: false
│   ├── items: [HiddenItemData 1, ...]
│   └── zoomZoneReference: [префаб зума]
└── BoxCollider2D
```

---

## 🔧 НАСТРОЙКА LevelConfig

**Создание:**
1. В Project окне: `Right Click → Create → Configs → Level Config`
2. Назовите `LVLconfig_N`

**Заполнение:**
```
LevelName: "Level_BG1"
LevelPrefab: [префаб Level_BG1 из Assets/Prefab/BG/]
GameUI: (пусто, или префаб GameUI)
IntroDialog: "Привет! Найди 5 предметов..."
OutroDialog: "Молодец! Следующий уровень открыт."
TotalItemsCount: 5
OutroDelay: 2
legacyLevelKey: (пусто)
```

---

## 📦 ADDRESSABLES НАСТРОЙКА

### 1. Откройте Addressables
`Window → Asset Management → Addressables → Groups`

### 2. Создайте группу "Levels"
- Правой кнопкой → `Create Group`
- Назовите "Levels"

### 3. Добавьте префабы
- Перетащите `Level_BG1`, `Level_BG2`, ... `Level_BG9` в группу
- Для каждого установите:
  - **Address**: `Level_BG1`, `Level_BG2`, ...
  - **Labels**: добавьте метку "Level"

### 4. Build Addressables
`Window → Asset Management → Addressables → Build → New Build`

---

## ✅ ЧЕК-ЛИСТ ПЕРЕД ЗАПУСКОМ

### Build Settings:
- [ ] `bootstrap.unity` — индекс 0
- [ ] `main_menu.unity` — индекс 1
- [ ] `select_level.unity` — индекс 2
- [ ] `lvl1_office.unity` — индекс 3
- [ ] `lvl2_*.unity` — индекс 4
- [ ] ... все уровни

### Настройка уровней:
- [ ] На `LevelRoot` назначен `LevelManager`
- [ ] В `LevelManager` назначен `currentLevelConfig`
- [ ] `LevelConfig` создан и заполнен
- [ ] `LevelConfig.LevelPrefab` назначен
- [ ] SearchZone имеют `items` или дочерние `SearchableItem`
- [ ] Зум-зоны имеют `zoomZoneReference`

### Настройка кнопок уровней:
- [ ] На каждой кнопке `LevelButton`
- [ ] `levelConfig` назначен
- [ ] `levelImage` назначено
- [ ] `lockOverlay` и `lockIcon` назначены (если есть)

### UI:
- [ ] `WordsPanel` имеет дочерний `Content` с `VerticalLayoutGroup`
- [ ] `HintButton` имеет компонент `HintButtonController`
- [ ] `CloseZoomButton` имеет компонент `CloseZoomButton`
- [ ] `DialogManager.storyDialog` назначен

---

## 🎮 ПОТОК ИГРЫ

```
Запуск
  ↓
bootstrap.unity
  → Создаёт менеджеры (GameManager, LevelProgressManager...)
  → Загружает main_menu
  ↓
main_menu.unity
  → Игрок нажимает "Играть"
  → GameManager.ShowLevelSelect()
  ↓
select_level.unity
  → LevelSelectionManager обновляет кнопки
  → Игрок нажимает кнопку уровня
  → GameManager.LoadLevel(levelConfig)
  ↓
lvlN.unity
  → GameManager загружает префаб уровня в BackgroundContainer
  → LevelManager.Initialize()
  → SearchManager.ActivateZone() для зон с autoActivateOnStart
  → Игрок ищет предметы
  → SearchManager.NotifyItemFound() × N
  → LevelManager.CompleteLevel()
  → GameManager.OnLevelCompleted()
  → GameManager.ShowLevelSelect()
  ↓
select_level.unity (возврат)
```

---

## 📝 СОЗДАНИЕ НОВОГО УРОВНЯ

1. **Скопируйте `lvl_template.unity`** → назовите `lvlN_название.unity`

2. **Откройте сцену**, настройте:
   - Добавьте фон (Image на BackgroundContainer)
   - Добавьте SearchZone (с BoxCollider2D)
   - Добавьте предметы (SearchableItem или в items массив)

3. **Создайте префаб уровня:**
   - Выделите корневой объект (фон + зоны)
   - Перетащите в `Assets/Prefab/BG/`
   - Назовите `Level_BGN`

4. **Создайте LevelConfig:**
   - `Create → Configs → Level Config`
   - Назовите `LVLconfig_N`
   - Назначьте `LevelPrefab` → префаб уровня
   - Заполните `IntroDialog`, `OutroDialog`, `TotalItemsCount`

5. **Добавьте кнопку в select_level:**
   - Скопируйте существующую кнопку
   - Назначьте новый `LevelConfig`
   - Обновите `LevelSelectionManager.levelButtons`

6. **Добавьте сцену в Build Settings:**
   - `File → Build Settings → Add Open Scenes`

7. **Добавьте префаб в Addressables:**
   - Перетащите в группу "Levels"
   - Установите Address = имени префаба

---

## 🐛 ОТЛАДКА

### Включите DEBUG логи:
`Edit → Project Settings → Player → Scripting Define Symbols`
Добавьте: `DEBUG`

### Проверьте консоль:
```
[BootstrapManager] Persistent managers created successfully
[GameManager] ShowMainMenu: Loading main menu scene
[GameManager] ShowLevelSelect: Loading level select scene
[GameManager] LoadLevel: Level_BG1
[LevelManager] Initialize() called
[LevelManager] Found N SearchZone(s) in level
[SearchManager] Activating zone 'SearchZone1'
```

---

## 📞 ЕСЛИ ПРОБЛЕМЫ

| Проблема | Решение |
|----------|---------|
| LevelManager не инициализируется | Проверьте `currentLevelConfig` в инспекторе |
| WordsPanel не показывается | Проверьте `SearchManager.SetWordsPanel()` вызван |
| SearchZone не активируется | Проверьте `HasItemsToFind()` и `IsZoneCompleted()` |
| Кнопка уровня не работает | Проверьте `LevelConfig.LevelPrefab` назначен |
| Прогресс не сохраняется | Проверьте `LevelProgressManager` в bootstrap |

---

## 📂 СТРУКТУРА ПРОЕКТА

```
Assets/
├── Scenes/
│   ├── bootstrap.unity          ← загрузочная
│   ├── main_menu.unity          ← меню
│   ├── select_level.unity       ← выбор уровней
│   ├── lvl_template.unity       ← шаблон для копирования
│   ├── lvl1_office.unity        ← уровень 1
│   ├── lvl2_*.unity             ← уровень 2
│   └── ...
├── Prefab/
│   ├── BG/
│   │   ├── Level_BG1.prefab
│   │   ├── Level_BG2.prefab
│   │   └── ...
│   ├── UI/
│   │   ├── GameUI.prefab
│   │   ├── ItemWordUI.prefab
│   │   └── ...
│   └── zoom/
│       └── ...
├── scripts/
│   ├── config/
│   │   ├── LVLconfig_1.asset
│   │   ├── LVLconfig_2.asset
│   │   └── ...
│   ├── item/
│   │   └── BG1/, BG2/, ... (HiddenItemData)
│   ├── BootstrapManager.cs
│   ├── GameManager.cs
│   ├── LevelManager.cs
│   ├── LevelUIManager.cs
│   ├── SearchManager.cs
│   └── ...
└── AddressableAssetsData/
    └── ... (настройки Addressables)
```

---

**Готово!** Теперь игра разделена на сцены с использованием Addressables для загрузки уровней.
