
using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class KGFactionCollection : KGNameAllegianceCollection
    {
        public FactionElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as FactionElement;
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

        public new FactionElement this[string responseString]
        {
            get
            {
                return (FactionElement)BaseGet(responseString);
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
            return new FactionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FactionElement)element).name;
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
