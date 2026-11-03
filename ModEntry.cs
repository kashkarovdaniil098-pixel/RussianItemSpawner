
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
        private char letter = '\0';

        private const int PerPage = 6;

        private readonly List<(string name, string id)> items = new();

        private static readonly string Alphabet =
            "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        private static readonly Rectangle OpenButton =
            new Rectangle(16, 16, 118, 44);

        private static readonly Rectangle Panel =
            new Rectangle(16, 68, 390, 430);

        private static readonly Rectangle CloseButton =
            new Rectangle(360, 76, 34, 30);

        public override void Entry(IModHelper helper)
        {
            LoadItems();

            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Monitor.Log("ITEM PRO загружен. Предметов: " + items.Count,
                LogLevel.Info);
        }

        private void LoadItems()
        {
            try
            {
                var data = Helper.GameContent.Load<
                    Dictionary<string, StardewValley.GameData.Objects.ObjectData>
                >("Data/Objects");

                items.Clear();

                foreach (var entry in data)
                {
                    if (entry.Value == null ||
                        string.IsNullOrWhiteSpace(entry.Value.Name))
                        continue;

                    items.Add((entry.Value.Name, "(O)" + entry.Key));
                }

                items.Sort((a, b) =>
                    string.Compare(a.name, b.name,
                        StringComparison.CurrentCultureIgnoreCase));
            }
            catch (Exception ex)
            {
                Monitor.Log("Ошибка каталога: " + ex.Message,
                    LogLevel.Error);
            }
        }

        private List<(string name, string id)> Filtered()
        {
            if (letter == '\0')
                return items;

            return items.Where(x =>
                x.name.Length > 0 &&
                char.ToUpperInvariant(x.name[0]) == letter
            ).ToList();
        }

        private void DrawBox(SpriteBatch b, Rectangle r, Color color)
        {
            b.Draw(Game1.staminaRect, r, color);
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            SpriteBatch b = e.SpriteBatch;

            DrawBox(b, OpenButton, new Color(45, 115, 65));
            b.DrawString(Game1.smallFont, "ITEM",
                new Vector2(44, 27), Color.White);

            if (!menuOpen)
                return;

            DrawBox(b, Panel, new Color(35, 38, 42));

            b.DrawString(Game1.smallFont, "ITEM PRO",
                new Vector2(28, 82), Color.White);

            DrawBox(b, CloseButton, Color.DarkRed);
            b.DrawString(Game1.smallFont, "X",
                new Vector2(371, 82), Color.White);

            DrawBox(b, new Rectangle(25, 112, 38, 27),
                letter == '\0' ? Color.DarkGreen : Color.Gray);
            b.DrawString(Game1.smallFont, "ВС",
                new Vector2(30, 116), Color.White);

            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 65 + (i % 8) * 40;
                int y = 112 + (i / 8) * 27;

                Rectangle r = new Rectangle(x, y, 36, 25);
                DrawBox(b, r, letter == Alphabet[i]
                    ? Color.DarkGreen : Color.Gray);

                b.DrawString(Game1.smallFont, Alphabet[i].ToString(),
                    new Vector2(x + 9, y + 3), Color.White);
            }

            var list = Filtered();
            int maxPage = Math.Max(0, (list.Count - 1) / PerPage);
            page = Math.Min(page, maxPage);

            b.DrawString(Game1.smallFont,
                "Предметов: " + list.Count +
                "   Страница: " + (page + 1) + "/" + (maxPage + 1),
                new Vector2(28, 220), Color.White);

            for (int i = 0; i < PerPage; i++)
            {
                int index = page * PerPage + i;
                if (index >= list.Count)
                    break;

                int x = 28 + (i % 3) * 120;
                int y = 245 + (i / 3) * 78;

                Rectangle card = new Rectangle(x, y, 110, 70);
                DrawBox(b, card,
                    selected == i ? Color.SteelBlue : Color.Gray);

                try
                {
                    var item = ItemRegistry.Create(list[index].id);
                    Texture2D texture = item.GetTexture();
                    Rectangle source = item.GetSourceRect();

                    b.Draw(texture,
                        new Rectangle(x + 38, y + 3, 32, 32),
                        source, Color.White);
                }
                catch
                {
                    // Если текстура недоступна, остаётся карточка с названием.
                }

                b.DrawString(Game1.smallFont,
                    list[index].name.Length > 15
                        ? list[index].name.Substring(0, 15)
                        : list[index].name,
                    new Vector2(x + 4, y + 42), Color.White);
            }

            DrawBox(b, new Rectangle(28, 410, 65, 35), Color.Gray);
            DrawBox(b, new Rectangle(315, 410, 65, 35), Color.DarkGreen);

            b.DrawString(Game1.smallFont, "<",
                new Vector2(54, 416), Color.White);
            b.DrawString(Game1.smallFont, ">",
                new Vector2(340, 416), Color.White);

            DrawBox(b, new Rectangle(100, 410, 55, 35), Color.DarkRed);
            DrawBox(b, new Rectangle(255, 410, 55, 35), Color.DarkGreen);

            b.DrawString(Game1.smallFont, "-",
                new Vector2(120, 416), Color.White);
            b.DrawString(Game1.smallFont, "+",
                new Vector2(275, 416), Color.White);

            b.DrawString(Game1.smallFont, "Кол-во: " + quantity,
                new Vector2(160, 418), Color.White);

            DrawBox(b, new Rectangle(110, 455, 180, 35),
                Color.DarkGreen);
            b.DrawString(Game1.smallFont, "ПОЛУЧИТЬ",
                new Vector2(155, 463), Color.White);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
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

            if (new Rectangle(25, 112, 38, 27).Contains(p))
            {
                letter = '\0';
                page = 0;
                selected = -1;
                return;
            }

            for (int i = 0; i < Alphabet.Length; i++)
            {
                int x = 65 + (i % 8) * 40;
                int y = 112 + (i / 8) * 27;

                if (new Rectangle(x, y, 36, 25).Contains(p))
                {
                    letter = Alphabet[i];
                    page = 0;
                    selected = -1;
                    return;
                }
            }

            var list = Filtered();
            int maxPage = Math.Max(0, (list.Count - 1) / PerPage);

            if (new Rectangle(28, 410, 65, 35).Contains(p))
            {
                page = Math.Max(0, page - 1);
                selected = -1;
                return;
            }

            if (new Rectangle(315, 410, 65, 35).Contains(p))
            {
                page = Math.Min(maxPage, page + 1);
                selected = -1;
                return;
            }

            for (int i = 0; i < PerPage; i++)
            {
                int x = 28 + (i % 3) * 120;
                int y = 245 + (i / 3) * 78;
                int index = page * PerPage + i;

                if (index < list.Count &&
                    new Rectangle(x, y, 110, 70).Contains(p))
                {
                    selected = i;
                    return;
                }
            }

            if (new Rectangle(100, 410, 55, 35).Contains(p))
            {
                quantity = Math.Max(1, quantity - 1);
                return;
            }

            if (new Rectangle(255, 410, 55, 35).Contains(p))
            {
                quantity = Math.Min(999, quantity + 1);
                return;
            }

            if (new Rectangle(110, 455, 180, 35).Contains(p) &&
                Context.IsWorldReady &&
                selected >= 0)
            {
                int index = page * PerPage + selected;

                if (index >= list.Count)
                    return;

                try
                {
                    var item = ItemRegistry.Create(
                        list[index].id, quantity);

                    Game1.player.addItemToInventory(item);

                    Monitor.Log("Выдано: " + list[index].name,
                        LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Monitor.Log("Ошибка выдачи: " + ex.Message,
                        LogLevel.Error);
                }
            }
        }
    }
}


