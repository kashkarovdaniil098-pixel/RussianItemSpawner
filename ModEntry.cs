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
        private int category = 0;
        private int selectedItem = 0;
        private int quantity = 10;

        private static readonly Rectangle OpenButton =
            new Rectangle(16, 16, 118, 44);

        private static readonly Rectangle Panel =
            new Rectangle(16, 68, 390, 390);

        private static readonly Rectangle CloseButton =
            new Rectangle(360, 76, 34, 30);

        private readonly string[] categories =
        {
            "Ресурсы", "Еда", "Семена", "Инструменты"
        };

        private readonly (string name, string id)[][] items =
        {
            new[] {
                ("Камень", "(O)390"),
                ("Дерево", "(O)388"),
                ("Уголь", "(O)382"),
                ("Медная руда", "(O)378"),
                ("Волокно", "(O)771")
            },
            new[] {
                ("Яйцо", "(O)176"),
                ("Молоко", "(O)184"),
                ("Сыр", "(O)424"),
                ("Хлеб", "(O)216"),
                ("Яблоко", "(O)613")
            },
            new[] {
                ("Семена пастернака", "(O)472"),
                ("Семена цветной капусты", "(O)474"),
                ("Семена картофеля", "(O)475"),
                ("Семена капусты", "(O)477")
            },
            new[] {
                ("Кирка", "(T)Pickaxe"),
                ("Топор", "(T)Axe"),
                ("Мотыга", "(T)Hoe"),
                ("Лейка", "(T)WateringCan")
            }
        };

        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Monitor.Log("Russian Item Spawner загружен!", LogLevel.Info);
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
            b.DrawString(Game1.smallFont, "ITEM SPAWNER",
                new Vector2(28, 82), Color.White);

            DrawBox(b, CloseButton, Color.DarkRed);
            b.DrawString(Game1.smallFont, "X",
                new Vector2(371, 82), Color.White);

            for (int i = 0; i < categories.Length; i++)
            {
                Rectangle r = new Rectangle(25 + i * 92, 120, 88, 35);
                DrawBox(b, r, i == category ? Color.DarkGreen : Color.Gray);
                b.DrawString(Game1.smallFont, categories[i],
                    new Vector2(r.X + 3, r.Y + 9), Color.White);
            }

            for (int i = 0; i < items[category].Length; i++)
            {
                Rectangle r = new Rectangle(28, 165 + i * 42, 350, 36);
                DrawBox(b, r, i == selectedItem ? Color.SteelBlue : Color.Gray);
                b.DrawString(Game1.smallFont, items[category][i].name,
                    new Vector2(r.X + 10, r.Y + 9), Color.White);
            }

            DrawBox(b, new Rectangle(28, 365, 55, 32), Color.DarkRed);
            DrawBox(b, new Rectangle(320, 365, 55, 32), Color.DarkGreen);

            b.DrawString(Game1.smallFont, "-",
                new Vector2(48, 370), Color.White);
            b.DrawString(Game1.smallFont, "+",
                new Vector2(339, 370), Color.White);

            b.DrawString(Game1.smallFont, "Количество: " + quantity,
                new Vector2(110, 373), Color.White);

            DrawBox(b, new Rectangle(110, 410, 180, 38), Color.DarkGreen);
            b.DrawString(Game1.smallFont, "ПОЛУЧИТЬ",
                new Vector2(155, 420), Color.White);
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

            for (int i = 0; i < categories.Length; i++)
            {
                Rectangle r = new Rectangle(25 + i * 92, 120, 88, 35);

                if (r.Contains(p))
                {
                    category = i;
                    selectedItem = 0;
                    return;
                }
            }

            for (int i = 0; i < items[category].Length; i++)
            {
                Rectangle r = new Rectangle(28, 165 + i * 42, 350, 36);

                if (r.Contains(p))
                {
                    selectedItem = i;
                    return;
                }
            }

            if (new Rectangle(28, 365, 55, 32).Contains(p))
                quantity = Math.Max(1, quantity - 1);

            if (new Rectangle(320, 365, 55, 32).Contains(p))
                quantity = Math.Min(999, quantity + 1);

            if (new Rectangle(110, 410, 180, 38).Contains(p)
                && Context.IsWorldReady)
            {
                try
                {
                    var item = ItemRegistry.Create(
                        items[category][selectedItem].id, quantity);

                    Game1.player.addItemToInventory(item);

                    Monitor.Log("Предмет выдан: " +
                        items[category][selectedItem].name,
                        LogLevel.Info);
                }
                catch (System.Exception ex)
                {
                    Monitor.Log("Ошибка выдачи предмета: " + ex.Message,
                        LogLevel.Error);
                }
            }
        }

        private static void DrawBox(
            SpriteBatch b, Rectangle area, Color color)
        {
            b.Draw(Game1.staminaRect, area, color);
        }
    }
}
