using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class NameHeaders : ConfigurationElementCollection
    {
        public NameHeader this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameHeader;
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

        public new NameHeader this[string responseString]
        {
            get
            {
                return (NameHeader)BaseGet(responseString);
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
            return new NameHeader();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameHeader)element).name;
        }
    }
}
