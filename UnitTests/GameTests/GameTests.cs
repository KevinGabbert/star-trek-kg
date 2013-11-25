
using System;
using NUnit.Framework;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Playfield;

namespace UnitTests.GameTests
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void NewGame()
        {
            //interesting.. this test causes MapWith3HostilesTheConfigWayAddedInDescendingOrder2() to fail when running after this one does

            var game = new Game();
            
            Assert.IsInstanceOf<Map>(game.Map);

            StarTrekKGSettings.Get = null;

            game.Map = null;
            GC.Collect();
            game = null;
            GC.Collect();
        }
    }
}
