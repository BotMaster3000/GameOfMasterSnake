using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameOfMasterSnake.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Interfaces;
using GameOfMasterSnake.Enums;

namespace GameOfMasterSnake.Map.Tests
{
    [TestClass]
    public class GameMapTests
    {
        [TestMethod]
        public void ConstructorTest_WidthAndHeightAreSet()
        {
            const int width = 100;
            const int height = 150;
            IGameMap map = new GameMap(width, height);

            Assert.AreEqual(width, map.Width);
            Assert.AreEqual(height, map.Height);
        }

        [TestMethod]
        public void ConstructorTest_TilesArrayGetsInitializedCorrectly()
        {
            const int width = 123;
            const int height = 412;
            GameMap map = new GameMap(width, height);
            Assert.AreEqual(width * height, map.Tiles.Length);
            foreach (Tile tile in map.Tiles)
            {
                Assert.IsNotNull(tile);
            }
        }

        [TestMethod]
        public void GetTileTest()
        {
            const int width = 312;
            const int height = 562;
            const int xPos = 123;
            const int yPos = 31;
            GameMap map = new GameMap(width, height);
            ITile tile = map.GetTile(xPos, yPos);

            Assert.AreEqual(xPos, tile.XPos);
            Assert.AreEqual(yPos, tile.YPos);
        }

        [TestMethod]
        public void SetTileValueTest()
        {
            const int width = 512;
            const int height = 162;
            const int xPos = 412;
            const int yPos = 123;
            GameMap map = new GameMap(width, height);

            TileValues oldValue = map.GetTile(xPos, yPos).Value;
            TileValues newValue = oldValue == TileValues.Food ? TileValues.Snake : TileValues.Food;
            map.SetTileValue(newValue, xPos, yPos);
            Assert.AreEqual(newValue, map.GetTile(xPos, yPos).Value);
        }
    }
}