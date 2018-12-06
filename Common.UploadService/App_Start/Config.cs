using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Common.UploadService
{
    public static class Config
    {
        private static bool noCache = false;
        private static JObject BuildItems()
        {
            var configJsonPath = ConfigurationManager.AppSettings["uploadconfigfilepath"];
            var json = File.ReadAllText(HttpContext.Current.Server.MapPath(configJsonPath) ?? throw new InvalidOperationException("需要在配置文件中添加上传文件配置文件信息，结点名称为：uploadconfigfilepath"));
            return JObject.Parse(json);
        }

        public static JObject Items
        {
            get
            {
                if (noCache || _items == null)
                {
                    _items = BuildItems();
                }
                return _items;
            }
        }
        private static JObject _items;


        public static T GetValue<T>(string key)
        {
            return Items[key].Value<T>();
        }

        public static String[] GetStringList(string key)
        {
            return Items[key].Select(x => x.Value<String>()).ToArray();
        }

        public static String GetString(string key)
        {
            return GetValue<String>(key);
        }

        public static int GetInt(string key)
        {
            return GetValue<int>(key);
        }
    }
}