using System.Linq;
using StarTrek_KG.Actors;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

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
            public double MaxTransfer { get; set; }
            public SubsystemType Type { get; set; }

        #endregion

        public abstract void OutputDamagedMessage();
        public abstract void OutputRepairedMessage();
        public abstract void OutputMalfunctioningMessage();

        public virtual void Controls(string command)
        {
            //todo: can Prompt be moved out of here?
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
            this.Damage = 0; //TODO: StarTrekKGSettings.GetSetting<int>("this.name");
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
            this.Damage = 1 + (Utility.Utility.Random).Next(StarTrekKGSettings.GetSetting<int>("DamageSeed"));

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

        public void AddEnergy(int transfer, bool adding)
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

        public ISubsystem For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (ISubsystem). ");
            }

            return ship.Subsystems.Single(s => s.Type == this.Type);
        }
    }
}
