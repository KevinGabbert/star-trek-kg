using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class FactionThreats: SeverityValues
    {
        public SeverityValueElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as SeverityValueElement;
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

        public new SeverityValueElement this[string responseString]
        {
            get
            {
                return (SeverityValueElement)BaseGet(responseString);
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
            return new SeverityValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeverityValueElement)element).value;
        }
    }
}
