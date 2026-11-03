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

        // Кнопка в левом верхнем углу.
        private static readonly Rectangle OpenButton = new Rectangle(16, 16, 118, 44);
        private static readonly Rectangle Panel = new Rectangle(16, 68, 300, 150);
        private static readonly Rectangle StoneButton = new Rectangle(30, 132, 270, 42);
        private static readonly Rectangle CloseButton = new Rectangle(270, 78, 34, 30);

        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            Monitor.Log("RussianItemSpawner загружен. Кнопка ITEM находится слева сверху.", LogLevel.Info);
        }

        private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            SpriteBatch b = e.SpriteBatch;

            // Постоянная кнопка открытия.
            DrawBox(b, OpenButton, new Color(45, 115, 65));
            b.DrawString(Game1.smallFont, "ITEM", new Vector2(OpenButton.X + 28, OpenButton.Y + 11), Color.White);

            if (!menuOpen)
                return;

            DrawBox(b, Panel, new Color(35, 38, 42) * 0.96f);
            b.DrawString(Game1.smallFont, "ITEM SPAWNER", new Vector2(30, 82), Color.White);
            DrawBox(b, CloseButton, new Color(150, 55, 55));
            b.DrawString(Game1.smallFont, "X", new Vector2(281, 82), Color.White);

            DrawBox(b, StoneButton, new Color(65, 105, 145));
            b.DrawString(Game1.smallFont, "Выдать камень x10", new Vector2(43, 142), Color.White);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft)
                return;

            Point cursor = e.Cursor.ScreenPixels.ToPoint();

            if (OpenButton.Contains(cursor))
            {
                menuOpen = !menuOpen;
                return;
            }

            if (!menuOpen)
                return;

            if (CloseButton.Contains(cursor))
            {
                menuOpen = false;
                return;
            }

            if (StoneButton.Contains(cursor) && Context.IsWorldReady)
            {
                Item stones = ItemRegistry.Create("(O)390", 10);
                Game1.player.addItemToInventory(stones);
                Monitor.Log("Выдано: камень x10.", LogLevel.Info);
            }
        }

        private static void DrawBox(SpriteBatch b, Rectangle area, Color color)
        {
            b.Draw(Game1.staminaRect, area, color);
        }
    }
}
