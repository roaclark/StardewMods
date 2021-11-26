using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace SeasonHelper
{
    using ButtonItem = Tuple<ClickableTextureComponent, SeasonData.SeasonObject>;
    using SeasonButton = Tuple<ClickableTextureComponent, string>;

    internal class SeasonMenu : IClickableMenu
    {
        private const int windowWidth = 632;
        private const int windowHeight= 600;

        private SeasonData data;
        private string selectedSeason = Game1.currentSeason;

        private List<ButtonItem> items = new List<ButtonItem>();
        private List<SeasonButton> seasonButtons = new List<SeasonButton>();

        private bool dismissed = false;

        private float iconScale = 0.75f;
        private int iconPadding = 10;
        private int itemsPerRow = 10;

        private IMonitor Monitor;

        private SeasonData.SeasonObject selectedObject;

        public SeasonMenu(IMonitor monitor, SeasonData data)
            : base(
                  Game1.viewport.Width / 2 - (windowWidth + IClickableMenu.borderWidth * 2) / 2,
                  Game1.viewport.Height / 2 - (windowHeight + IClickableMenu.borderWidth * 2) / 2,
                  windowWidth + IClickableMenu.borderWidth * 2,
                  windowHeight + IClickableMenu.borderWidth * 2,
                  true
              )
        {
            this.Monitor = monitor;
            this.data = data;
            this.createButtons();
        }

        private void createButtons()
        {
            items = new List<ButtonItem>();
            seasonButtons = new List<SeasonButton>();

            List<SeasonData.SeasonObject> seasonCrops = data.getCrops(this.selectedSeason);
            populateObjectList(new Vector2(0, 0), seasonCrops);

            int cropRows = Convert.ToInt32(Math.Ceiling(1f * seasonCrops.Count / itemsPerRow));

            List<SeasonData.SeasonObject> seasonFish = data.getFish(this.selectedSeason);
            populateObjectList(new Vector2(0, Game1.tileSize * (cropRows + 0.5f)), seasonFish);

            int fishRows = Convert.ToInt32(Math.Ceiling(1f * seasonFish.Count / itemsPerRow));

            List<SeasonData.SeasonObject> seasonForage = data.getForage(this.selectedSeason);
            populateObjectList(new Vector2(0, Game1.tileSize * (cropRows + fishRows + 1)), seasonForage);

            for (int i = 0; i < 4; i++)
            {
                seasonButtons.Add(new SeasonButton(
                    new ClickableTextureComponent(
                        SeasonData.seasons[i],
                        this.adjustRectangleForWindow(new Rectangle(
                            -90,
                            i * 40 - 30,
                            12 * Game1.pixelZoom,
                            8 * Game1.pixelZoom
                        )),
                        "",
                        "",
                        Game1.mouseCursors,
                        new Rectangle(406, 441 + i * 8, 12, 8),
                        Game1.pixelZoom
                    ),
                    SeasonData.seasons[i]
                ));
            }
        }

        private void populateObjectList(Vector2 offset, List<SeasonData.SeasonObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                int row = i / itemsPerRow;
                int col = i % itemsPerRow;
                int iconSize = (int)(this.iconScale * Game1.tileSize);
                int paddedIconSize = iconSize + this.iconPadding;
                SeasonData.SeasonObject obj = objects[i];
                items.Add(new ButtonItem(
                    new ClickableTextureComponent(
                        obj.objectIndex.ToString(),
                        this.adjustRectangleForWindow(new Rectangle(
                            (int)offset.X + col * paddedIconSize,
                            (int)offset.Y + row * paddedIconSize,
                            iconSize,
                            iconSize
                        )),
                        "",
                        "",
                        Game1.objectSpriteSheet,
                        getObjectBounds(obj.objectIndex),
                        (int)(Game1.pixelZoom * 0.75)
                    ),
                    obj
                ));
            }
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);


            if (selectedObject == null)
            {

                foreach (ButtonItem item in this.items)
                {
                    SeasonData.SeasonObject obj = item.Item2;
                    ClickableTextureComponent button = item.Item1;
                    if (obj.totalStats.done >= obj.totalStats.needed)
                    {
                        button.draw(b, new Color(Color.Gray, 100), 1);
                    }
                    else
                    {
                        button.draw(b);
                    }
                }

                foreach (ButtonItem item in this.items)
                {
                    SeasonData.SeasonObject obj = item.Item2;
                    ClickableTextureComponent button = item.Item1;

                    if (button.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        drawHoverText(b, " " + obj.totalStats.done.ToString() + " / " + obj.totalStats.needed + " ", Game1.smallFont);
                    }
                }
            }
            else
            {
                Utility.drawTextWithShadow(
                    b,
                    "Total: " + selectedObject.totalStats.done + "/" + selectedObject.totalStats.needed,
                    Game1.smallFont,
                    this.adjustVectorForWindow(new Vector2(0, 0)),
                    Game1.textColor
                );

                int i = 0;
                foreach (var entry in selectedObject.tasks)
                {
                    int needed = entry.Value.needed;
                    int done = entry.Value.done;
                    Utility.drawTextWithShadow(
                        b,
                        entry.Key + ": " + done + "/" + needed,
                        Game1.smallFont,
                        this.adjustVectorForWindow(new Vector2(0, (i + 1.5f) * Game1.smallFont.LineSpacing * 1.5f)),
                        Game1.textColor
                    );
                    i += 1;
                }
            }

            foreach (SeasonButton seasonButton in this.seasonButtons)
            {
                ClickableTextureComponent button = seasonButton.Item1;
                button.draw(b);
            }

            this.drawMouse(b);
        }

        public override bool readyToClose()
        {
            return this.dismissed;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            this.xPositionOnScreen = Game1.viewport.Width / 2 - (windowWidth + IClickableMenu.borderWidth * 2) / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - (windowHeight + IClickableMenu.borderWidth * 2) / 2 - Game1.tileSize;
            this.createButtons();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape || key == Keys.Q)
            {
                close();
                return;
            }

            base.receiveKeyPress(key);
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (upperRightCloseButton.containsPoint(x, y))
            {
                close();
                return;
            }

            if (selectedObject == null)
            {
                foreach (ButtonItem buttonItem in items)
                {
                    if (buttonItem.Item1.containsPoint(x, y))
                    {
                        SeasonData.SeasonObject obj = buttonItem.Item2;
                        this.selectedObject = obj;
                        return;
                    }
                }

            }

            foreach (SeasonButton seasonButton in seasonButtons)
            {
                if (seasonButton.Item1.containsPoint(x, y))
                {
                    this.selectedObject = null;
                    selectedSeason = seasonButton.Item2;
                    createButtons();
                    return;
                }
            }

            base.receiveLeftClick(x, y, playSound);
        }

        public override void performHoverAction(int x, int y)
        {
            foreach(ButtonItem item in items)
            {
                item.Item1.tryHover(x, y);
            }

            base.performHoverAction(x, y);
        }

        private void close()
        {
            if (selectedObject != null)
            {
                selectedObject = null;
            }
            else
            {
                this.dismissed = true;
                Game1.exitActiveMenu();
            }
        }

        private Rectangle getObjectBounds(int objectIndex)
        {
            return Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, objectIndex, 16, 16);
        }


        private Vector2 adjustVectorForWindow(Vector2 vector)
        {
            float x = vector.X + this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            float y = vector.Y + this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
            return new Vector2(x, y);
        }

        private Rectangle adjustRectangleForWindow(Rectangle rect)
        {
            int x = rect.X + this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            int y = rect.Y + this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
            return new Rectangle(x, y, rect.Width, rect.Height);
        }
    }
}
