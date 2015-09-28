using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using StarTrek_KG.Actors;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;
using StarTrek_KG.TypeSafeEnums;

namespace StarTrek_KG.Subsystem
{
    //todo: make it so that a subsystem can be shut off at will
    //todo: subsystems use energy
    //todo: introduce the concept of damage control.  Repairs can be prioritized through a panel

    /// <summary>
    /// Subsystems exist to take damage.  any workflow activity needs to move out.
    /// </summary>
    public abstract class SubSystem_Base: Actors.System, ISubsystem
    {
        #region Properties

            public int Damage { get; set; }
            public int MaxTransfer { get; set; }
            public SubsystemType Type { get; set; }

        #endregion

        protected List<string> _myPanel; 

        protected SubSystem_Base(Ship shipConnectedTo, Game game)
        {
            this.Game = game;
            this.Initialize();

            this.ShipConnectedTo = shipConnectedTo;
        }

        protected virtual void OutputDamagedMessage()
        {
            this.Game.Interact.Line(this.Type + " Damaged.");
        }

        protected virtual void OutputRepairedMessage()
        {
            this.Game.Interact.Line(this.Type + " Repaired.");
        }

        //public virtual void OutputMalfunctioningMessage()
        //{
        //    this.Game.Write.Line(this.Type + " Malfunctioning.");
        //}

        private void Initialize()
        {   

        }

        public virtual List<string> Controls(string command)
        {
            this.Game.Interact.Output.Queue.Clear();
            return new List<string>();
        }

        public virtual bool PartialRepair()
        {
            if (this.Damage > 0)
            {
                this.Damage--;
                if (this.Damage == 0)
                {
                    this.OutputRepairedMessage();
                }

                return true;
            }
            return false;
        }

        public virtual void FullRepair()
        {
            this.Damage = 0; //TODO: (this.COnfig).GetSetting<int>("this.name");

            //todo: if this system is not debug, then repair
            this.OutputRepairedMessage();
        }

        public bool Damaged()
        {
            if (this.Damage > 0)
            {
                this.OutputDamagedMessage();
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is where a subsystem takes its damage.  
        /// The amount of damage is random from within the seed defined in app.config
        /// </summary>
        public void TakeDamage()
        {
            this.Damage = 1 + (Utility.Utility.Random).Next(this.Game.Config.GetSetting<int>("DamageSeed"));

            //todo: if number is small, then this.OutputMalfunctioningMessage.. else...
            this.OutputDamagedMessage();

            //todo: this might be something to do if shields are up..
            //const int subsystemCount = 7; //todo: how can we find this out at this point?

            //if (this.GetNext(subsystemCount) > 0) //todo: this should be ship.subsystem.count
            //{
            //    return;
            //}

            //this.Damage = 1 + (Utility.Random).Next(AppConfig.Setting<int>("DamageSeed"));

            //if ((int)this.Type < 0)
            //{
            //    if (this.GetNext(subsystemCount) == (int)this.Type)
            //    {
            //        this.OutputMalfunctioningMessage();
            //    }
            //}
        }

        public int GetNext(int seed)
        {
            return (Utility.Utility.Random).Next(seed);
        }

        public int TransferredFromUser()
        {
            throw new global::System.NotImplementedException();
        }

        protected void AddEnergy(int transfer, bool adding)
        {
            if (adding)
            {
                //todo: why add to both objects?? Are they not the same?
                this.Energy += transfer;
                this.ShipConnectedTo.Energy -= transfer;
            }
            else
            {
                //todo: why add to both objects?? Are they not the same?
                this.Energy -=  transfer;
                this.ShipConnectedTo.Energy += transfer;
            }
        }

        public void SetEnergy(int transfer)
        {
            //todo: why add to both objects??
            this.Energy = transfer;
        }

        public ISubsystem For(Ship ship, Game game)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (ISubsystem). ");
            }

            var subSystem = ship.Subsystems.Single(s => s.Type == this.Type);
            subSystem.Game = game;

            return subSystem;
        }

        protected static ISubsystem For(IShip ship, SubsystemType subsystemType)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up with Subsystem: " + subsystemType);
            }

            var subSystemToReturn = ship.Subsystems.Single(s => s.Type == subsystemType);

            return subSystemToReturn;
        }

        internal static ISubsystem GetSubsystemFor(IShip ship, SubsystemType subscriberPromptSubSystem)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not passed.");
            }

            if (subscriberPromptSubSystem == null)
            {
                throw new GameConfigException("subsystem not passed");
            }

            var subSystemToReturn = ship.Subsystems.Single(s => s.Type == subscriberPromptSubSystem);

            return subSystemToReturn;
        }


        #region Menu

        /// <summary>
        /// This should provent recognition of a command that is not found in the first level menu
        /// </summary>
        /// <param name="command"></param>
        /// <param name="promptInteraction"></param>
        /// <returns></returns>
        protected bool InFirstLevelMenu(string command, IInteraction promptInteraction)
        {
            bool retVal = true;

            if (promptInteraction.Subscriber.PromptInfo.Level == 1 && command != "")
            {
                var search = $"{command}  = "; //todo: modify to use MenuItem "divider" in web.config  -  <MenuItem promptLevel="1" name="ship" description="Exit back to the Ship Panel" ordinalPosition ="1" divider=" = "></MenuItem>
                retVal = this._myPanel.Any(p => p.Contains(search)); 
            }

            return retVal;
        }

        #endregion
    }
}
