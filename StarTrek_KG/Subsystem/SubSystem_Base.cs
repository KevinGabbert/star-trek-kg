using System.Collections.Generic;
using System.Linq;
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
            public SubsystemType Type { get; set; }

        #endregion

        protected List<string> _myPanel; 

        protected SubSystem_Base(IShip shipConnectedTo)
        {
            this.ShipConnectedTo = shipConnectedTo;
            this.Initialize();
        }

        #region Output Messages

        private void OutputDamagedMessage()
        {
            this.ShipConnectedTo.OutputLine($"{this.Type} Damaged.");
        }

        private void OutputRepairedMessage()
        {
            this.ShipConnectedTo.OutputLine($"{this.Type} Repaired.");
        }

        //public virtual void OutputMalfunctioningMessage()
        //{
        //    this.Game.Write.Line(this.Type + " Malfunctioning.");
        //}

        #endregion

        private void Initialize()
        {   

        }

        public virtual List<string> Controls(string command)
        {
            this.ShipConnectedTo.Map.Game.Interact.Output.Queue.Clear();
            return new List<string>();
        }

        #region Repair

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

        #endregion

        #region Damage

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
            this.Damage = 1 + Utility.Utility.Random.Next(this.ShipConnectedTo.Map.Game.Config.GetSetting<int>("DamageSeed"));

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

        #endregion

        #region Energy

        public int TransferredFromUser()
        {
            throw new System.NotImplementedException();
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

        #endregion

        #region Synctactic sugar

        public ISubsystem For(IShip ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (ISubsystem). ");
            }

            ISubsystem subSystem = ship.Subsystems.Single(s => s.Type == this.Type);

            return subSystem;
        }

        protected static ISubsystem For(IShip ship, SubsystemType subsystemType)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up");
            }

            ISubsystem subSystemToReturn = ship.Subsystems.SingleOrDefault(s => s.Type == subsystemType);

            if (subSystemToReturn == null)
            {
                throw new GameConfigException($"Ship not set up with Subsystem: {subsystemType}");
            }

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

        #endregion

        #region Menu

        /// <summary>
        /// This should provent recognition of a command that is not found in the first level menu
        /// </summary>
        /// <param name="command"></param>
        /// <param name="promptInteraction"></param>
        /// <returns></returns>
        protected bool InFirstLevelMenu(string command)
        {
            bool retVal = true;

            if (this.ShipConnectedTo.Map.Game.Interact.Subscriber.PromptInfo.Level == 1 && command != "")
            {
                var search = $"{command}  = ";
                //todo: modify to use MenuItem "divider" in web.config  -  <MenuItem promptLevel="1" name="ship" description="Exit back to the Ship Panel" ordinalPosition ="1" divider=" = "></MenuItem>

                if (this._myPanel != null)
                {
                    retVal = this._myPanel.Any(p => p.Contains(search));
                }
            }

            return retVal;
        }

        #endregion

        public int GetNext(int seed)
        {
            return Utility.Utility.Random.Next(seed);
        }

        public bool NotRecognized(string command)
        {
            bool recognized = this.InFirstLevelMenu(command);
            return !recognized;
        }
    }
}
