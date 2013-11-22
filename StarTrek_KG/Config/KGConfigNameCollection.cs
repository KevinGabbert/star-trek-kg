﻿using System.Configuration;

namespace StarTrek_KG.Config
{
    public class KGConfigNameCollection: ConfigurationElementCollection
    {
        public NameElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameElement;
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

        public new NameElement this[string responseString]
        {
            get
            {
                return (NameElement)BaseGet(responseString);
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
            return new NameElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameElement)element).name;
        }
    }
}
