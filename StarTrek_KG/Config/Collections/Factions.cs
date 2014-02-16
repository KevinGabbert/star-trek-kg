using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class Factions : NameAllegianceDesignators
    {
        public Faction this[int index]
        {
            get
            {
                return base.BaseGet(index) as Faction;
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

        public new Faction this[string responseString]
        {
            get
            {
                return (Faction)BaseGet(responseString);
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
            return new Faction();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Faction)element).name;
        }

        //public override ConfigurationElementCollectionType CollectionType
        //{
        //    get
        //    {
        //        return ConfigurationElementCollectionType.BasicMap;
        //    }
        //}
    }
}
