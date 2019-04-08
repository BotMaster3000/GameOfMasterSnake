using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameOfMasterSnake.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameOfMasterSnake.Enums;
using GameOfMasterSnake.Interfaces;

namespace GameOfMasterSnake.Map.Tests
{
    [TestClass]
    public class TileTests
    {
        private const int XPOS = 10;
        private const int YPOS = 15;

        [TestMethod]
        public void ConstructorTest()
        {
            ITile tile = new Tile(XPOS, YPOS);
            Assert.AreEqual(XPOS, tile.XPos);
            Assert.AreEqual(YPOS, tile.YPos);
        }

        [TestMethod]
        public void SetValueTest_HasChangedShouldBeTrue()
        {
            const TileValues value = TileValues.Food;
            ITile tile = new Tile(XPOS, YPOS);
            tile.SetValue(value);
            Assert.AreEqual(value, tile.Value);
            Assert.AreEqual(true, tile.HasChanged);
        }
    }
}