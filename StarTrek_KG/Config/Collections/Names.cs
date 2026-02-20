using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class Names: ConfigurationElementCollection
    {
        public Name this[int index]
        {
            get
            {
                return base.BaseGet(index) as Name;
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

        public new Name this[string responseString]
        {
            get
            {
                return (Name)BaseGet(responseString);
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
            return new Name();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Name)element).name;
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
