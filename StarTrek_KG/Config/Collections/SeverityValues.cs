using System.Configuration;
using E = StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class SeverityValues : ConfigurationElementCollection
    {
        public E.SeverityValueTranslationElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as E.SeverityValueTranslationElement;
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

        public new E.SeverityValueTranslationElement this[string responseString]
        {
            get
            {
                return (E.SeverityValueTranslationElement)BaseGet(responseString);
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
            return new E.SeverityValueTranslationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((E.SeverityValueTranslationElement)element).value;
        }
    }
}
