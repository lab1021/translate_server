using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace translate_server
{
    public class ConfigurationOperation
    {
        /// <summary>
        /// the first database
        /// </summary>
        private string _db_first;
        /// <summary>
        /// the current database
        /// </summary>
        private string _db_current;
        /// <summary>
        /// the last database
        /// </summary>
        private string _db_last;

        private string _user_db;

        private string _config_path;
        private string _config_file;
        private static XmlDocument Config;
        private static XmlElement root;
        public string Db_first
        {
            get { return _db_first; }
        }
        public string Db_current
        {
            get { return _db_current; }
        }
        public string Db_last
        {
            get { return _db_last; }
        }
        public string User_db
        {
            get { return _user_db; }
        }

        public ConfigurationOperation(string config_path)
        {
            _config_path = config_path;
            _config_file = _config_path+"configuration.xml";
            Config = new XmlDocument();
            Config.Load(_config_file);
            root = Config.DocumentElement;
        }
        public void Load()
        {
            XmlElement db_path = (XmlElement)root.SelectSingleNode("/configuration/databasepath");
            _db_first = _config_path + db_path.GetElementsByTagName("first").Item(0).InnerText;
            _db_current = _config_path + db_path.GetElementsByTagName("current").Item(0).InnerText;
            _db_last = _config_path + db_path.GetElementsByTagName("last").Item(0).InnerText;

            XmlElement user_path = (XmlElement)root.SelectSingleNode("/configuration");
            _user_db = _config_path + user_path.GetElementsByTagName("userdb").Item(0).InnerText;
        }
        public void Reset_Db_current(string db_current)
        {
            _db_current = db_current;
            XmlElement db_path = (XmlElement)root.SelectSingleNode("/configuration/databasepath");
            db_path.GetElementsByTagName("current").Item(0).InnerText = db_current.Substring(_config_path.Length);
            Config.Save(_config_file);
        }
    }
}
