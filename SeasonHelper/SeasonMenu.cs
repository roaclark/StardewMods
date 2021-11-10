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

    internal class SeasonMenu : IClickableMenu
    {
        private const int windowWidth = 632;
        private const int windowHeight= 600;

        private SeasonData data;
        private string selectedSeason = Game1.currentSeason;

        private List<ButtonItem> items = new List<ButtonItem>();

        private bool dismissed = false;

        private float iconScale = 0.75f;
        private int iconPadding = 10;
        private int itemsPerRow = 10;

        private IMonitor Monitor;

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
            List<SeasonData.SeasonObject> seasonCrops = data.getCrops(this.selectedSeason);
            populateObjectList(new Vector2(0, 0), seasonCrops);

            List<SeasonData.SeasonObject> seasonFish = data.getFish(this.selectedSeason);
            populateObjectList(new Vector2(0, Game1.tileSize * 2.5f), seasonFish);

            List<SeasonData.SeasonObject> seasonForage = data.getForage(this.selectedSeason);
            populateObjectList(new Vector2(0, Game1.tileSize * 6), seasonForage);
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

            // FIXME hardcoded name
            //Utility.drawTextWithShadow(
            //    b,
            //    "Crops",
            //    Game1.smallFont,
            //    this.adjustVectorForWindow(new Vector2(0, 0)),
            //    Game1.textColor
            //);

            foreach (ButtonItem item in this.items)
            {
                if (item.Item2.totalStats.done >= item.Item2.totalStats.needed)
                {
                    item.Item1.draw(b, new Color(Color.Gray, 100), 1);
                } else
                {
                    item.Item1.draw(b);
                }
            }

            // TODO season tabs

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
            if (key == Keys.Escape)
            {
                this.dismissed = true;
                Game1.exitActiveMenu();
                return;
            }

            base.receiveKeyPress(key);
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (upperRightCloseButton.containsPoint(x, y))
            {
                this.dismissed = true;
                Game1.exitActiveMenu();
                return;
            }

            foreach(ButtonItem buttonItem in items)
            {
                if (buttonItem.Item1.containsPoint(x, y))
                {
                    // TODO open subwindow with more info
                    SeasonData.SeasonObject obj = buttonItem.Item2;
                    Monitor.Log(obj.prettyPrint(), LogLevel.Info);
                }
            }

            base.receiveLeftClick(x, y, playSound);
        }

        //public override void performHoverAction(int x, int y) { } // TODO

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
