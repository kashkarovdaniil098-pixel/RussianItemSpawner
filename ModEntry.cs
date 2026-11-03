

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;

namespace RussianItemSpawner
{
    public class ModEntry : Mod
    {
        private bool menuOpen;
        private int selectedItem;
        private int quantity = 10;
        private int page;
        private char searchLetter = '\0';

        private const int ItemsPerPage = 6;

        private readonly List<(string name, string id)> items = new();

        private static readonly string Alphabet =
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        private static readonly Rectangle OpenButton =
            new Rectangle(16, 16, 118, 44);

        private static readonly Rectangle Panel =
            new Rectangle(16, 68, 390, 430);

        private static readonly Rectangle CloseButton =
            new Rectangle(360, 76, 34, 30);

        private static readonly Rectangle GetButton =
            new Rectangle(110, 450, 180, 38);

        public override void Entry(IModHelper helper)
        {
            LoadItems();

            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Monitor.Log(
                "Russian Item Spawner загружен. Предметов: " + items.Count,
                LogLevel.Info
            );
        }

        private void LoadItems()
        {
            try
            {
                var data = Helper.GameContent.Load<
                    Dictionary<string, ObjectData>
                >("Data/Objects");

                items.Clear();

                foreach (var entry in data)
                {
                    if (entry.Value == null ||
                        string.IsNullOrWhiteSpace(entry.Value.Name))
                        continue;

                    items.Add((
                        entry.Value.Name,
                        "(O)" + entry.Key
                    ));
                }

                items.Sort((a, b) =>
                    string.Compare(
                        a.name,
                        b.name,
                        StringComparison.CurrentCultureIgnoreCase
                    )
                );

                Monitor.Log(
                    "Загружено объектов: " + items.Count,
                    LogLevel.Info
                );
            }
            catch (Exception ex)
            {
                Monitor.Log(
                    "Не удалось загрузить каталог: " + ex.Message,
                    LogLevel.Error
                );
            }
        }

        private List<(string name, string id)> GetFilteredItems()
        {
            if (searchLetter == '\0')
                return items;

            return items
                .Where(item =>
                    item.name.Length > 0 &&
                    char.ToUpperInvariant(item.name[0]) == searchLetter
                )
                .ToList();
        }

        private void OnRenderedHud(
            object? sender,
            RenderedHudEventArgs e
        )
        {
            SpriteBatch b = e.SpriteBatch;

            DrawBox(b, OpenButton, new Color(45, 115, 65));

            b.DrawString(
                Game1.smallFont,
                "ITEM",
                new Vector2(44, 27),
                Color.White
            );

            if (!menuOpen)
                return;

            DrawBox(b, Panel, new Color(35, 38, 42));

            b.DrawString(
                Game1.smallFont,
                "ITEM SPAWNER",
                new Vector2(28, 82),
                Color.White
            );

            DrawBox(b, CloseButton, Color.DarkRed);

            b.DrawString(
                Game1.smallFont,
                "X",
                new Vector2(371, 82),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "Выбери букву:",
                new Vector2(28, 110),
                Color.White
            );

            DrawBox(
                b,
                new Rectangle(25, 128, 36, 25),
                searchLetter == '\0' ? Color.DarkGreen : Color.Gray
            );

            b.DrawString(
                Game1.smallFont,
                "ВС",
                new Vector2(32, 132),
                Color.White
            );

            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 65 + (i % 8) * 40;
                int y = 128 + (i / 8) * 28;

                Rectangle r = new Rectangle(x, y, 36, 25);

                DrawBox(
                    b,
                    r,
                    searchLetter == Alphabet[i]
                        ? Color.DarkGreen
                        : Color.Gray
                );

                b.DrawString(
                    Game1.smallFont,
                    Alphabet[i].ToString(),
                    new Vector2(x + 9, y + 3),
                    Color.White
                );
            }

            var filtered = GetFilteredItems();

            int maxPage = Math.Max(
                0,
                (filtered.Count - 1) / ItemsPerPage
            );

            page = Math.Min(page, maxPage);

            DrawBox(
                b,
                new Rectangle(25, 235, 45, 25),
                Color.Gray
            );

            b.DrawString(
                Game1.smallFont,
                "<",
                new Vector2(40, 237),
                Color.White
            );

            DrawBox(
                b,
                new Rectangle(340, 235, 45, 25),
                Color.Gray
            );

            b.DrawString(
                Game1.smallFont,
                ">",
                new Vector2(355, 237),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "Предметы: " + filtered.Count +
                "   Страница: " + (page + 1) +
                "/" + (maxPage + 1),
                new Vector2(80, 240),
                Color.White
            );

            for (int i = 0; i < ItemsPerPage; i++)
            {
                int index = page * ItemsPerPage + i;

                if (index >= filtered.Count)
                    break;

                Rectangle r = new Rectangle(
                    28,
                    265 + i * 26,
                    350,
                    24
                );

                DrawBox(
                    b,
                    r,
                    i == selectedItem
                        ? Color.SteelBlue
                        : Color.Gray
                );

                b.DrawString(
                    Game1.smallFont,
                    filtered[index].name,
                    new Vector2(r.X + 8, r.Y + 3),
                    Color.White
                );
            }

            DrawBox(
                b,
                new Rectangle(28, 425, 55, 28),
                Color.DarkRed
            );

            DrawBox(
                b,
                new Rectangle(320, 425, 55, 28),
                Color.DarkGreen
            );

            b.DrawString(
                Game1.smallFont,
                "-",
                new Vector2(48, 428),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "+",
                new Vector2(339, 428),
                Color.White
            );

            b.DrawString(
                Game1.smallFont,
                "Количество: " + quantity,
                new Vector2(110, 430),
                Color.White
            );

            DrawBox(b, GetButton, Color.DarkGreen);

            b.DrawString(
                Game1.smallFont,
                "ПОЛУЧИТЬ",
                new Vector2(155, 460),
                Color.White
            );
        }

        private void OnButtonPressed(
            object? sender,
            ButtonPressedEventArgs e
        )
        {
            if (e.Button != SButton.MouseLeft)
                return;

            Point p = e.Cursor.ScreenPixels.ToPoint();

            if (OpenButton.Contains(p))
            {
                menuOpen = !menuOpen;
                return;
            }

            if (!menuOpen)
                return;

            if (CloseButton.Contains(p))
            {
                menuOpen = false;
                return;
            }

            if (new Rectangle(25, 128, 36, 25).Contains(p))
            {
                searchLetter = '\0';
                page = 0;
                selectedItem = 0;
                return;
            }

            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 65 + (i % 8) * 40;
                int y = 128 + (i / 8) * 28;

                Rectangle r = new Rectangle(x, y, 36, 25);

                if (r.Contains(p))
                {
                    searchLetter = Alphabet[i];
                    page = 0;
                    selectedItem = 0;
                    return;
                }
            }

            var filtered = GetFilteredItems();

            int maxPage = Math.Max(
                0,
                (filtered.Count - 1) / ItemsPerPage
            );

            if (new Rectangle(25, 235, 45, 25).Contains(p))
            {
                page = Math.Max(0, page - 1);
                selectedItem = 0;
                return;
            }

            if (new Rectangle(340, 235, 45, 25).Contains(p))
            {
                page = Math.Min(maxPage, page + 1);
                selectedItem = 0;
                return;
            }

            for (int i = 0; i < ItemsPerPage; i++)
            {
                Rectangle r = new Rectangle(
                    28,
                    265 + i * 26,
                    350,
                    24
                );

                int index = page * ItemsPerPage + i;

                if (r.Contains(p) && index < filtered.Count)
                {
                    selectedItem = i;
                    return;
                }
            }

            if (new Rectangle(28, 425, 55, 28).Contains(p))
            {
                quantity = Math.Max(1, quantity - 1);
                return;
            }

            if (new Rectangle(320, 425, 55, 28).Contains(p))
            {
                quantity = Math.Min(999, quantity + 1);
                return;
            }

            if (GetButton.Contains(p) &&
                Context.IsWorldReady &&
                filtered.Count > 0)
            {
                int index = page * ItemsPerPage + selectedItem;

                if (index < 0 || index >= filtered.Count)
                    return;

                try
                {
                    var selected = filtered[index];

                    var item = ItemRegistry.Create(
                        selected.id,
                        quantity
                    );

                    Game1.player.addItemToInventory(item);

                    Monitor.Log(
                        "Выдан предмет: " + selected.name,
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

        private static void DrawBox(
            SpriteBatch b,
            Rectangle area,
            Color color
        )
        {
            b.Draw(Game1.staminaRect, area, color);
        }
    }
}
