using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class FactionThreats: SeverityValues
    {
        public SeverityValueTranslationElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as SeverityValueTranslationElement;
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

        public new SeverityValueTranslationElement this[string responseString]
        {
            get
            {
                return (SeverityValueTranslationElement)BaseGet(responseString);
            }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }

                base.BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SeverityValueTranslationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeverityValueTranslationElement)element).value;
        }
    }
}
