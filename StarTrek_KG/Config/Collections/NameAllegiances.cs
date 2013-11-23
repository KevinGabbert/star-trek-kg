using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class NameAllegiances: ConfigurationElementCollection
    {
        public NameAllegiance this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameAllegiance;
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

        public new NameAllegiance this[string responseString]
        {
            get
            {
                return (NameAllegiance)BaseGet(responseString);
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
            return new NameAllegiance();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameAllegiance)element).name;
        }
    }
}
