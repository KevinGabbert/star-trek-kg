﻿using System.Configuration;

namespace StarTrek_KG.Config
{
    public class KGConfigNameValueCollection : ConfigurationElementCollection
    {
        public NameValueElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as NameValueElement;
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

        public new NameValueElement this[string responseString]
        {
            get
            {
                return (NameValueElement)BaseGet(responseString);
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
            return new NameValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NameValueElement)element).name;
        }
    }
}
