# ADDRESSABLES — Краткое руководство по миграции на AssetReference

Это простая инструкция для перехода с строковых ключей на `AssetReference` и использования новой plug-and-play системы.

## Коротко — что изменилось
- `LevelConfig` теперь содержит `AssetReference LevelPrefab` и опционально `AssetReference GameUI`.
- `GameManager` поддерживает `LoadLevel(LevelConfig)` и автоматически инстанцирует/освобождает Addressables.
- `ItemTriggerZone` поддерживает `AssetReference zoomZoneReference` с fallback на `detailPrefab`.
- `LevelButton` теперь требует `Button` и вызывает `LoadLevel(LevelConfig)`.
- `LevelConfigEditor` добавляет кнопку `Apply LevelManager to Prefab` для автодобавления `LevelManager`.
- `BuildAddressablesAutomatically` — опция для автоматической сборки Addressables (настраиваемо).

## Шаги миграции

1) Добавление префабов в Addressables
   - Откройте Window > Asset Management > Addressables > Groups
   - Создайте группы (`UI`, `Levels`, `ZoomZones` и т.д.) и перетащите префабы туда

2) Обновление `LevelConfig`
   - Для каждого уровня в инспекторе `LevelConfig` вместо текстового ключа назначьте `LevelPrefab` (перетащите префаб)
   - Опционально назначьте `GameUI` в `LevelConfig` если UI специфичен для уровня
   - При желании используйте поле `legacyLevelKey` временно для совместимости

3) Префаб уровня
   - Откройте префаб уровня и нажмите в `LevelConfig` кнопку `Apply LevelManager to Prefab` — это добавит `LevelManager` автоматически

4) Триггеры и зум-зоны
   - Заменяйте `ZoomTrigger` на `ItemTriggerZone` (если вы всё ещё используете ZoomTrigger)
   - В `ItemTriggerZone` назначьте `zoomZoneReference` (AssetReference) на префаб зум-зоны
   - Если нужно, оставьте `detailPrefab` как fallback для старых данных

5) Кнопки уровней
   - На кнопках используйте компонент `LevelButton` и назначьте соответствующий `LevelConfig`

6) Сборка Addressables
   - После добавления всех префабов выполните: Addressables > Build > New Build > Default Build Script
   - Опционально включите `BuildAddressablesAutomatically` если хотите собирать автоматически перед Play Mode

## Совместимость и откат
- Поля `legacyLevelKey` и старые строки поддерживаются временно — рекомендуется полностью перейти на `AssetReference`.
- `ZoomTrigger` помечен как устаревший; `ItemTriggerZone` покрывает весь функционал и лучше поддерживает UI-контейнеры.

## Быстрые проверки после миграции
- Убедитесь, что в `LevelConfig` назначен `LevelPrefab` для каждого уровня.
- Проверьте, что `LevelManager` присутствует в корне префаба уровня.
- Убедитесь, что `ItemTriggerZone.zoomContainer` назначен (Panel в Canvas), если используете UI-режим зума.
- Выполните Build Addressables и запустите сцену — ошибки в консоли укажут отсутствующие ссылки.

Если хочешь, могу:
- автоматически заменить `ZoomTrigger` компонент на `ItemTriggerZone` в префабах/сценах (запрос требует правки YAML префабов), или
- запустить скрипт-рефактор (создать утилиту Editor) для массовой миграции.

— Скажи, что делать дальше: автоматическая замена компонентов или только генерация миграционного гайда?

