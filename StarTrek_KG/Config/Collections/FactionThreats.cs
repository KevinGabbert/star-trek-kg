using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class FactionThreats: SeverityValues
    {
        public new SeverityValueTranslation this[int index]
        {
            get
            {
                return base.BaseGet(index) as SeverityValueTranslation;
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

        public new SeverityValueTranslation this[string responseString]
        {
            get
            {
                return (SeverityValueTranslation)BaseGet(responseString);
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
            return new SeverityValueTranslation();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeverityValueTranslation)element).value;
        }
    }
}
