# 📋 РУКОВОДСТВО ПО ОБНОВЛЕНИЮ ПРОЕКТА

## ✅ Что было сделано

### 1. **Объединение конфигов**
- `LevelConfig` + `LevelUnlockData` → **единый `LevelConfig`**
- Удалены файлы `Level1Data 1-9.asset` (избыточны)
- Удалён скрипт `LevelUnlockData.cs`

### 2. **Экран загрузки**
- Создан префаб `Assets/Prefab/UI/LoadingScreen.prefab`
- Создан скрипт `LoadingScreenManager.cs`
- `BootstrapManager` теперь создаёт `LoadingScreenManager`
- `GameManager` использует `LoadingScreenManager.Instance` для показа/скрытия экрана загрузки

### 3. **Упрощение архитектуры**
- Удалены лишние файлы документации
- Удалён `ClickDetector_old_for delete.cs`
- `LevelProgressManager` теперь использует `List<LevelConfig>` вместо `List<LevelUnlockData>`
- `LevelButton` использует спрайты напрямую из `LevelConfig`
- `ResetProgressManager.ClearLevelConfigsCache()` переименован

---

## 🔧 ЧТО НУЖНО СДЕЛАТЬ В UNITY

### Шаг 1: Откройте проект в Unity

После импорта изменений Unity перекомпилирует скрипты.

### Шаг 2: Обновите LevelConfig

Для каждого уровня (`LVLconfig_1` ... `LVLconfig_9`):

1. Откройте `Assets/scripts/config/LVLconfig_N.asset`
2. В инспекторе появятся новые поля:
   - **Level Unlock Settings**:
     - `requiredPreviousLevels` — зависимости (какие уровни должны быть пройдены)
   - **Level Button Sprites**:
     - `unlockedSprite` — спрайт разблокированного уровня
     - `lockedSprite` — спрайт заблокированного уровня
     - `completedSprite` — спрайт пройденного уровня
     - `lockIcon` — иконка замка

3. **Перенесите данные** из старых `Level1Data N.asset`:
   - Откройте `Level1Data 1.asset` (если ещё не удалили)
   - Скопируйте значения:
     - `requiredPreviousLevels` → `requiredPreviousLevels`
     - `unlockedSprite` → `unlockedSprite`
     - `lockedSprite` → `lockedSprite`
     - `completedSprite` → `completedSprite`
     - `lockIcon` → `lockIcon`

4. **Назначьте LevelPrefab**:
   - В `LevelConfig` в поле `LevelPrefab` перетащите префаб уровня из `Assets/Prefab/BG/`
   - Например: `Level_BG1.prefab` → `LVLconfig_1`

### Шаг 3: Обновите LevelProgressManager

1. Найдите объект `LevelProgressManager` на сцене `bootstrap.unity` (создаётся автоматически)
2. В инспекторе в поле `levelConfigs` добавьте все `LevelConfig`:
   - Нажмите `+` для каждого уровня
   - Перетащите `LVLconfig_1`, `LVLconfig_2`, etc.

**ИЛИ** (если используете префаб менеджеров):
1. Откройте префаб `PersistentManagers` (если есть)
2. Обновите `LevelProgressManager.levelConfigs`

### Шаг 4: Настройте экран загрузки

1. Откройте `bootstrap.unity`
2. Найдите объект `LoadingScreenManager` (создаётся автоматически)
3. В поле `loadingScreenPrefab` назначьте `Assets/Prefab/UI/LoadingScreen.prefab`

**ИЛИ** оставьте пустым — экран загрузки будет создан программно.

### Шаг 5: Проверьте кнопки уровней

1. Откройте сцену `select_level.unity`
2. Для каждой кнопки уровня:
   - Убедитесь, что `LevelButton.levelConfig` назначен
   - Убедитесь, что `levelImage`, `lockOverlay`, `lockIcon` назначены (опционально)

### Шаг 6: Настройте Addressables

1. Откройте `Window → Asset Management → Addressables → Groups`
2. Создайте группу `Levels` (если ещё не создана)
3. Перетащите префабы уровней (`Level_BG1.prefab`, etc.) в группу
4. Выполните `Build → New Build → Default Build Script`

### Шаг 7: Проверка работы

1. Запустите игру
2. Проверьте:
   - ✅ Загрузочная сцена → главное меню
   - ✅ Кнопка "Играть" → экран выбора уровней
   - ✅ Кнопки уровней показывают спрайты (разблокирован/заблокирован)
   - ✅ При клике на уровень показывается экран загрузки
   - ✅ Уровень загружается
   - ✅ При завершении уровня возвращается экран выбора уровней

---

## 🗑️ УДАЛЁННЫЕ ФАЙЛЫ

Можете безопасно удалить эти файлы (если ещё не удалили):

```
❌ Assets/scripts/LevelUnlockData.cs
❌ Assets/scripts/config/Level1Data 1-9.asset
❌ Assets/scripts/config/Level1Data 1-9.asset.meta
❌ Assets/scripts/ADDRESSABLES_SETUP.md
❌ Assets/scripts/LEVEL_UNLOCK_DATA_WARNING.md
❌ Assets/scripts/QUICK_FIXES.md
❌ Assets/scripts/SCENES_SETUP_GUIDE.md
❌ Assets/scripts/RESET_PROGRESS_MANAGER_GUIDE.md
❌ Assets/scripts/TEST_RESET_PROGRESS.md
❌ ClickDetector_old_for delete.cs
```

---

## 📝 НОВАЯ СТРУКТУРА LevelConfig

```
LevelConfig
├── Level Info
│   └── LevelName: string
├── Level Prefab
│   └── LevelPrefab: GameObject
├── UI
│   └── GameUI: GameObject
├── Dialogs
│   ├── IntroDialog: string
│   └── OutroDialog: string
├── Level Settings
│   ├── TotalItemsCount: int
│   └── OutroDelay: float
├── Level Unlock Settings          ← НОВОЕ!
│   └── requiredPreviousLevels: List<string>
├── Level Button Sprites           ← НОВОЕ!
│   ├── unlockedSprite: Sprite
│   ├── lockedSprite: Sprite
│   ├── completedSprite: Sprite
│   └── lockIcon: Sprite
└── Legacy
    └── legacyLevelKey: string
```

---

## 🐛 ВОЗМОЖНЫЕ ПРОБЛЕМЫ И РЕШЕНИЯ

### ❌ "LevelUnlockData not found"

**Причина:** Скрипт `LevelUnlockData.cs` удалён.

**Решение:** Используйте `LevelConfig` вместо него.

### ❌ "levelUnlockData не существует"

**Причина:** В `LevelProgressManager` поле переименовано.

**Решение:**
- Старое: `levelUnlockData`
- Новое: `levelConfigs`

Переназначьте в инспекторе.

### ❌ Кнопки уровней не показывают спрайты

**Причина:** Не назначены спрайты в `LevelConfig`.

**Решение:**
1. Откройте `LVLconfig_N`
2. Назначьте спрайты в поля:
   - `unlockedSprite`
   - `lockedSprite`
   - `completedSprite`
   - `lockIcon`

### ❌ Экран загрузки не показывается

**Причина:** Не назначен префаб или `LoadingScreenManager` не создан.

**Решение:**
1. Проверьте, что `BootstrapManager` создаёт `LoadingScreenManager`
2. В `GameManager.LoadLevel()` вызывается `ShowLoadingScreen(true)`

---

## 📊 ИТОГОВАЯ АРХИТЕКТУРА

```
bootstrap.unity
└── BootstrapManager
    └── Создаёт PersistentManagers:
        ├── GameManager
        ├── SoundManager
        ├── LevelProgressManager
        ├── InterstitialAdManager
        ├── ResetProgressManager
        └── LoadingScreenManager ← НОВЫЙ!

main_menu.unity
└── Canvas
    └── Panel_MainMenu
        └── Button_Play → MenuPlayButton

select_level.unity
└── Canvas
    └── Panel_LevelSelect
        ├── Button_Back → MenuBackButton
        └── Grid_Levels
            └── Button_LevelN → LevelButton (levelConfig: LVLconfig_N)

lvlN.unity (уровень)
└── LevelRoot
    ├── LevelManager (currentLevelConfig: LVLconfig_N)
    └── LevelProgressTracker
```

---

## ✅ ЧЕК-ЛИСТ ПРОВЕРКИ

- [ ] Все `LevelConfig` обновлены (спрайты, зависимости)
- [ ] `LevelProgressManager.levelConfigs` назначен
- [ ] `LevelPrefab` назначен в каждом `LevelConfig`
- [ ] `LoadingScreenManager` создан в bootstrap
- [ ] Addressables настроены (префабы уровней добавлены в группу)
- [ ] Кнопки уровней работают
- [ ] Экран загрузки показывается
- [ ] Уровни загружаются и завершаются

---

**Если всё работает — готово!** 🎉
