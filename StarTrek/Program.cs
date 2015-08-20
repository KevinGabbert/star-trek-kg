/*
 * Star Trek KG
 * Copyright (C) 2014, Kevin Gabbert
 * 
 * This file is part of Star Trek KG.
 *
 * This version is a re-architecting of Super Star Trek, by Michael Birken,
 * which itself is a rewrite of Creative Computing's version of Mike Mayfield’s 
 * original public domain program.  His version can be found here:
 * http://www.codeproject.com/Articles/28228/Star-Trek-1971-Text-Game
 * 
 * Star Trek KG is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published 
 * by the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * Star Trek KG is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using System.Drawing;
using StarTrek_KG;
using StarTrek_KG.Config;
using StarTrek_KG.Utility;

namespace StarTrek_Console {

  public static class Program {

    public static void Main(string[] args)
    {
        System.Console.Title = "Star Trek KG";

        ConsoleHelper.SetConsoleIcon(SystemIcons.Shield);

        var settingsForWholeGame = (new StarTrekKGSettings());

        (new Game(settingsForWholeGame)).Run();
    }
  }
}