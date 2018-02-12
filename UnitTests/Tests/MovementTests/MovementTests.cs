using System;
using System.Collections.Generic;
using NUnit.Framework;
using StarTrek_KG.Actors;
using StarTrek_KG.Enums;
using System.Linq;

namespace UnitTests.Tests.MovementTests
{
    [TestFixture]
    public class MovementTests: Movement_Base
    {
        [Ignore("")]// not working yet
        [Test]
        public void TravelAlongCourse_BugVerification()
        {
            var testMovement = new Movement( this.Game.Map.Playership);

            double x = 31.5084577259018;
            double y = 31.5084577259018;

            //testMovement.TravelAlongCourse(0, 0, -0.00565685424949238, new Coordinate(), -0.00565685424949238, ref x, ref y );
            //.Encountered obstacle within Region. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018
            //.Encountered obstacle within Region. X:0Y:0 Friendlyvx: -0.00565685424949238 vy: -0.00565685424949238 x: 31.5084577259018 y: 31.5084577259018 lastsectX: 0 lastSectY: 0
        }

        [Ignore(reason: "Mod MoveSector to be able to move to a new region")]
        [Test]
        public void MoveSector_ToNewRegion()
        {
            List<int> directions = Enum.GetValues(typeof(NavDirection)).Cast<int>().ToList();

            for (int distance = 1; distance < 8; distance++)
            {
                foreach (int direction in directions)
                {
                    base.Move_Sector((NavDirection)direction, distance);
                    this.reset();
                }
            }
        }

    }
}