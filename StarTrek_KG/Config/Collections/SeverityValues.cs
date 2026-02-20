using System.Configuration;
using E = StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class SeverityValues : ConfigurationElementCollection
    {
        public E.SeverityValueTranslation this[int index]
        {
            get
            {
                return base.BaseGet(index) as E.SeverityValueTranslation;
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

        public new E.SeverityValueTranslation this[string responseString]
        {
            get
            {
                return (E.SeverityValueTranslation)BaseGet(responseString);
            }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }

                this.BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new E.SeverityValueTranslation();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((E.SeverityValueTranslation)element).value;
        }
    }
}
