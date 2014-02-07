using System.Configuration;
using StarTrek_KG.Config.Elements;

namespace StarTrek_KG.Config.Collections
{
    public class RegistryNameTypeClasses: ConfigurationElementCollection
    {
        public RegistryNameTypeClassElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as RegistryNameTypeClassElement;
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

        public new RegistryNameTypeClassElement this[string responseString]
        {
            get
            {
                return (RegistryNameTypeClassElement)BaseGet(responseString);
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
            return new RegistryNameTypeClassElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RegistryNameTypeClassElement)element).name;
        }

        //public override ConfigurationElementCollectionType CollectionType
        //{
        //    get
        //    {
        //        return ConfigurationElementCollectionType.BasicMap;
        //    }
        //}
    }
}
