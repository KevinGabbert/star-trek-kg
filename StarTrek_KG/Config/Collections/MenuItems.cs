using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class MenuItems : RegistryNameTypeClasses
    {
        public new RegistryNameTypeClass this[int index]
        {
            get
            {
                return base.BaseGet(index) as RegistryNameTypeClass;
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

        public new RegistryNameTypeClass this[string responseString]
        {
            get
            {
                return (RegistryNameTypeClass)BaseGet(responseString);
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
            return new RegistryNameTypeClass();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RegistryNameTypeClass)element).name;
        }
    }
}
