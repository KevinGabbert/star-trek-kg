using System.Configuration;

namespace StarTrek_KG.Config.Collections
{
    public class KGFactionShipsCollection: KGNameCollection
    {
        public FactionShip this[int index]
        {
            get
            {
                return base.BaseGet(index) as FactionShip;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new FactionShip this[string responseString]
        {
            get
            {
                return (FactionShip)BaseGet(responseString);
            }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }

                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FactionShip();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FactionShip)element).name;
        }
    }
}
