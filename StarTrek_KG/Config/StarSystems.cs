using System.Configuration;

namespace StarTrek_KG.Config
{
    public class StarSystems : ConfigurationElementCollection
    {
        public StarSystem this[int index]
        {
            get
            {
                return base.BaseGet(index) as StarSystem;
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

        public new StarSystem this[string responseString]
        {
            get
            {
                return (StarSystem)BaseGet(responseString);
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
            return new StarSystem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StarSystem)element).name;
        }
    }
}
