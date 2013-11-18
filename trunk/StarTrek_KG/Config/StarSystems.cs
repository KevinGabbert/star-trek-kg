using System.Configuration;

namespace StarTrek_KG.Config
{
    public class StarSystems : ConfigurationElementCollection
    {
        public StarSystem this[int index]
        {
            get
            {
                return base.BaseGet(index) as StarSystem;
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

        protected override ConfigurationElement CreateNewElement()
        {
            return new StarSystem();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StarSystem)element).Name;
        }
    }


    //public class Company : ConfigurationElement
    //{

    //    [ConfigurationProperty("name", IsRequired = true)]
    //    public string Name
    //    {
    //        get
    //        {
    //            return this["name"] as string;
    //        }
    //    }
    //    [ConfigurationProperty("code", IsRequired = true)]
    //    public string Code
    //    {
    //        get
    //        {
    //            return this["code"] as string;
    //        }
    //    }
    //}
    //public class Companies: ConfigurationElementCollection
    //{
    //    public Company this[int index]
    //    {
    //        get
    //        {
    //            return base.BaseGet(index) as Company;
    //        }
    //        set
    //        {
    //            if (base.BaseGet(index) != null)
    //            {
    //                base.BaseRemoveAt(index);
    //            }
    //            this.BaseAdd(index, value);
    //        }
    //    }


    //    protected override ConfigurationElement CreateNewElement()
    //    {
    //        return new Company();
    //    }

    //    protected override object GetElementKey(ConfigurationElement element)
    //    {
    //        return ((Company)element).Name;
    //    }
    //}
    //public class RegisterCompaniesConfig: ConfigurationSection
    //{

    //    public static RegisterCompaniesConfig GetConfig()
    //    {
    //        return (RegisterCompaniesConfig)ConfigurationManager.GetSection("RegisterCompanies") ?? new RegisterCompaniesConfig();
    //    }

    //    [ConfigurationProperty("Companies")]
    //    public Companies Companies
    //    {
    //        get
    //        {
    //            object o = this["Companies"];
    //            return o as Companies;
    //        }
    //    }

    //}

}
