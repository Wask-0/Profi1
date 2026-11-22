# Labyrinthino (Unity 6000.5, URP, uGUI, портрет, планшет)
Конкурсная игра WorldSkills «Разработчик мобильных игр». ТЗ/критерии: Docs/Requirements.md. ГДД (выведен, оригинала нет): Docs/GDD.md.

## Структура
- Assets/Project/Scripts/{Core,Data,Logic,UI,MiniGames,Audio} (asmdef Labyrinthino.Game); Editor/ — генератор сцены (меню Labyrinthino); Tests/ — EditMode.
- Assets/Project/{Art,Audio,Scenes}; ассеты генерирует Tools/gen_assets.py (Pillow, wave).
- Логика мини-игр — чистые C# модели (Logic/), View — MonoBehaviour (MiniGames/).

## Правила кода (оцениваются!)
- XML `<summary>` + `<param>`/`<returns>` у каждого класса и метода; закомментированного кода нет.
- MS-конвенция: PascalCase публичное, _camelCase приватные поля, имена по смыслу, англ. имена ресурсов.
- Весь UI — Canvas + uGUI, шрифты Lobster/Nunito (не стандартные).
- Сцена собирается `Labyrinthino/Build Scene` — править Editor/SceneBuilder.cs, не руками.

## Работа
- Unity открыт, управляется через UnityMCP; после правок скриптов — read_console на ошибки.
- Не читать Library/, .meta, бинарные ассеты. Тесты: EditMode (Tests/).
