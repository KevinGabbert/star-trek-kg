using System;
using System.Linq;
using StarTrek_KG.Config;
using StarTrek_KG.Enums;
using StarTrek_KG.Exceptions;
using StarTrek_KG.Interfaces;

namespace StarTrek_KG.Subsystem
{
    public abstract class SubSystem_Base: System, ISubsystem
    {
        #region Properties

            public int Damage { get; set; }
            public int MaxTransfer { get; set; }
            public SubsystemType Type { get; set; }

        #endregion

        public abstract void OutputDamagedMessage();
        public abstract void OutputRepairedMessage();
        public abstract void OutputMalfunctioningMessage();

        public abstract void Controls(string command);

        public virtual bool Repair()
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
            this.Damage = 1 + (Utility.Random).Next(StarTrekKGSettings.GetSetting<int>("DamageSeed"));

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
            return (Utility.Random).Next(seed);
        }

        public int TransferredFromUser()
        {
            throw new global::System.NotImplementedException();
        }

        public void AddEnergy(int transfer, bool adding)
        {
            if (adding)
            {
                this.Energy += transfer;
                this.Map.Playership.Energy -= transfer;
            }
            else
            {
                this.Energy -=  transfer;
                this.Map.Playership.Energy += transfer;
            }
        }

        public ISubsystem For(Ship ship)
        {
            if (ship == null)
            {
                throw new GameConfigException("Ship not set up (ISubsystem). Add a Friendly to your GameConfig"); //todo: make this a custom exception
            }

            return ship.Subsystems.Single(s => s.Type == this.Type);
        }
    }
}
