using System.Configuration;
using E = StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class NameValues : ConfigurationElementCollection
    {
        public E.NameValueElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as E.NameValueElement;
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

        public new E.NameValueElement this[string responseString]
        {
            get
            {
                return (E.NameValueElement)BaseGet(responseString);
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
            return new E.NameValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((E.NameValueElement)element).name;
        }
    }
}
