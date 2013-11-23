﻿
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Playfield;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void NewGame()
        {
            var game = new Game();
            
            Assert.IsInstanceOf<Map>(game.Map);
        }
    }
}