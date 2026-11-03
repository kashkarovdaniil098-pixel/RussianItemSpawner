
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RussianItemSpawner
{
    public class ModEntry : Mod
    {
        private bool menuOpen;
        private int page;
        private int selected = -1;
        private int quantity = 10;
        private string searchLetter = "";

        private const int PerPage = 6;

        private readonly List<(string name, string id)> items = new();

        private static readonly string[] Alphabet =
        {
            "Все", "А", "Б", "В", "Г", "Д", "Е", "Ё",
            "Ж", "З", "И", "Й", "К", "Л", "М", "Н",
            "О", "П", "Р", "С", "Т", "У", "Ф", "Х",
            "Ц", "Ч", "Ш", "Щ", "Э", "Ю", "Я"
        };

        private readonly Rectangle openButton =
            new Rectangle(16, 16, 125, 46);

        public override void Entry(IModHelper helper)
        {
            LoadItems();

            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Monitor.Log(
                "Русский каталог загружен. Предметов: " + items.Count,
                LogLevel.Info
            );
        }

        private void LoadItems()
        {
            try
            {
                var data = Helper.GameContent.Load<
                    Dictionary<string,
                    StardewValley.GameData.Objects.ObjectData>
                >("Data/Objects");

                items.Clear();

                foreach (var entry in data)
                {
                    if (entry.Value == null)
                        continue;

                    string id = "(O)" + entry.Key;

                    try
                    {
                        Item item = ItemRegistry.Create(id);
                        items.Add((item.DisplayName, id));
                    }
                    catch
                    {
                        items.Add((entry.Value.Name, id));
                    }
                }

                items.Sort((a, b) =>
                    string.Compare(
                        a.name,
                        b.name,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                );
            }
            catch (Exception ex)
            {
                Monitor.Log(
                    "Ошибка загрузки предметов: " + ex.Message,
                    LogLevel.Error
                );
            }
        }

        private List<(string name, string id)> GetItems()
        {
            if (searchLetter == "")
                return items;

            return items.Where(x =>
                x.name.StartsWith(
                    searchLetter,
                    StringComparison.CurrentCultureIgnoreCase
                )
            ).ToList();
        }

        private void DrawBox(
            SpriteBatch b,
            Rectangle area,
            Color color)
        {
            b.Draw(Game1.staminaRect, area, color);
        }

        private void OnRenderedHud(
            object? sender,
            RenderedHudEventArgs e)
        {
            SpriteBatch b = e.SpriteBatch;

            // Кнопка открытия
            DrawBox(
                b,
                openButton,
                new Color(79, 116, 62)
            );

            DrawBox(
                b,
                new Rectangle(19, 19, 119, 40),
                new Color(112, 143, 76)
            );

            b.DrawString(
                Game1.smallFont,
                "ПРЕДМЕТЫ",
                new Vector2(29, 31),
                Color.White
            );

            if (!menuOpen)
                return;

            // Основная панель
            DrawBox(
                b,
                new Rectangle(16, 70, 430, 460),
                new Color(37, 49, 37)
            );

            DrawBox(
                b,
                new Rectangle(22, 76, 418, 43),
                new Color(126, 91, 53)
            );

            b.DrawString(
                Game1.dialogueFont,
                "КАТАЛОГ ПРЕДМЕТОВ",
                new Vector2(34, 80),
                Color.White
            );

            // Закрытие
            DrawBox(
                b,
                new Rectangle(397, 80, 30, 30),
                Color.DarkRed
            );

            b.DrawString(
                Game1.smallFont,
                "X",
                new Vector2(407, 86),
                Color.White
            );

            // Алфавит
            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 25 + (i % 10) * 40;
                int y = 125 + (i / 10) * 29;

                bool active =
                    searchLetter == Alphabet[i] ||
                    (searchLetter == "" && i == 0);

                DrawBox(
                    b,
                    new Rectangle(x, y, 36, 25),
                    active
                        ? new Color(65, 135, 70)
                        : new Color(105, 91, 67)
                );

                b.DrawString(
                    Game1.smallFont,
                    Alphabet[i],
                    new Vector2(x + 5, y + 3),
                    Color.White
                );
            }

            List<(string name, string id)> list = GetItems();

            int maxPage = Math.Max(
                0,
                (list.Count - 1) / PerPage
            );

            page = Math.Min(page, maxPage);

            b.DrawString(
                Game1.smallFont,
                "Предметов: " + list.Count +
                "   Страница: " + (page + 1) +
                "/" + (maxPage + 1),
                new Vector2(30, 218),
                Color.White
            );

            // Карточки предметов
            for (int i = 0; i < PerPage; i++)
            {
                int index = page * PerPage + i;

                if (index >= list.Count)
                    break;

                int x = 27 + (i % 3) * 135;
                int y = 245 + (i / 3) * 82;

                Rectangle card =
                    new Rectangle(x, y, 125, 74);

                DrawBox(
                    b,
                    card,
                    selected == i
                        ? new Color(83, 139, 81)
                        : new Color(100, 88, 67)
                );

                // Настоящая игровая иконка
                try
                {
                    Item item = ItemRegistry.Create(
                        list[index].id
                    );

                    item.drawInMenu(
                        b,
                        new Vector2(x + 45, y + 5),
                        0.5f
                    );
                }
                catch (Exception ex)
                {
                    Monitor.Log(
                        "Не удалось нарисовать предмет: " + ex.Message,
                        LogLevel.Trace
                    );
                }

                string name = list[index].name;

                if (name.Length > 17)
                    name = name.Substring(0, 17);

                b.DrawString(
                    Game1.smallFont,
                    name,
                    new Vector2(x + 5, y + 46),
                    Color.White
                );
            }

            // Стрелки страниц
            DrawBox(
                b,
                new Rectangle(28, 420, 65, 34),
                new Color(126, 91, 53)
            );

            DrawBox(
                b,
                new Rectangle(350, 420, 65, 34),
                new Color(126, 91, 53)
            );

            b.DrawString(
                Game1.smallFont,
                "<",
                new Vector2(53, 425),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                ">",
                new Vector2(375, 425),
                Color.White
            );

            // Количество
            DrawBox(
                b,
                new Rectangle(105, 420, 45, 34),
                Color.DarkRed
            );

            DrawBox(
                b,
                new Rectangle(295, 420, 45, 34),
                Color.DarkGreen
            );

            b.DrawString(
                Game1.smallFont,
                "-",
                new Vector2(121, 425),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "+",
                new Vector2(311, 425),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "Количество: " + quantity,
                new Vector2(160, 425),
                Color.White
            );

            // Получить предмет
            DrawBox(
                b,
                new Rectangle(125, 470, 210, 42),
                new Color(63, 135, 72)
            );

            b.DrawString(
                Game1.dialogueFont,
                "ПОЛУЧИТЬ",
                new Vector2(164, 474),
                Color.White
            );
        }

        private void OnButtonPressed(
            object? sender,
            ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft)
                return;

            Point p = e.Cursor.ScreenPixels.ToPoint();

            if (openButton.Contains(p))
            {
                menuOpen = !menuOpen;
                return;
            }

            if (!menuOpen)
                return;

            // Закрыть меню
            if (new Rectangle(397, 80, 30, 30).Contains(p))
            {
                menuOpen = false;
                return;
            }

            // Выбрать букву
            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 25 + (i % 10) * 40;
                int y = 125 + (i / 10) * 29;

                if (new Rectangle(x, y, 36, 25).Contains(p))
                {
                    searchLetter = i == 0
                        ? ""
                        : Alphabet[i];

                    page = 0;
                    selected = -1;
                    return;
                }
            }

            List<(string name, string id)> list = GetItems();

            int maxPage = Math.Max(
                0,
                (list.Count - 1) / PerPage
            );

            // Предыдущая страница
            if (new Rectangle(28, 420, 65, 34).Contains(p))
            {
                page = Math.Max(0, page - 1);
                selected = -1;
                return;
            }

            // Следующая страница
            if (new Rectangle(350, 420, 65, 34).Contains(p))
            {
                page = Math.Min(maxPage, page + 1);
                selected = -1;
                return;
            }

            // Выбор предмета
            for (int i = 0; i < PerPage; i++)
            {
                int x = 27 + (i % 3) * 135;
                int y = 245 + (i / 3) * 82;

                int index = page * PerPage + i;

                if (index < list.Count &&
                    new Rectangle(x, y, 125, 74).Contains(p))
                {
                    selected = i;
                    return;
                }
            }

            // Уменьшить количество
            if (new Rectangle(105, 420, 45, 34).Contains(p))
            {
                quantity = Math.Max(1, quantity - 1);
                return;
            }

            // Увеличить количество
            if (new Rectangle(295, 420, 45, 34).Contains(p))
            {
                quantity = Math.Min(999, quantity + 1);
                return;
            }

            // Выдать предмет
            if (new Rectangle(125, 470, 210, 42).Contains(p) &&
                Context.IsWorldReady &&
                selected >= 0)
            {
                int index = page * PerPage + selected;

                if (index >= list.Count)
                    return;

                try
                {
                    Item item = ItemRegistry.Create(
                        list[index].id,
                        quantity
                    );

                    Game1.player.addItemToInventory(item);

                    Monitor.Log(
                        "Получен предмет: " + list[index].name,
                        LogLevel.Info
                    );
                }
                catch (Exception ex)
                {
                    Monitor.Log(
                        "Ошибка выдачи: " + ex.Message,
                        LogLevel.Error
                    );
                }
            }
        }
    }
}
