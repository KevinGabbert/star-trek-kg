using System;
using System.Collections.Specialized;
using System.Configuration;

namespace StarTrek_KG.Config
{
    public static class AppConfig
    {
        private static readonly NameValueCollection _appSettings;

        public static NameValueCollection Settings
        {
            get { return _appSettings; }
        }

        static AppConfig ()
        {
            _appSettings = ConfigurationManager.AppSettings;
        }

        public static T Setting<T>(string key)
        {
            T setting;
            string value = Settings[key];

            if(string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException("Unable to retrieve value for: " + key);
            }

            try
            {
                //user is passing in the data type (T), so typecast it here to the provided type so they don't have to.
                setting = (T) Convert.ChangeType(value, typeof (T));
            }
            catch(Exception)
            {
                throw new ConfigurationErrorsException(string.Format("Unable to cast config setting {0}. Check to make sure setting is filled out correctly.", key));
            }

            return setting;
        }
    }
}
