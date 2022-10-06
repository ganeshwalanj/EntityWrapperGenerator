using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EntityWrapperGenerator
{
    public class EntityWrapperSettings
    {
        #region Singleton Setup

        private static readonly EntityWrapperSettings _settings = null;

        public static EntityWrapperSettings Default
        {
            get { return _settings; }
        }

        static EntityWrapperSettings()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EntityWrapperSettings.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show("EntityWrapperSettings.json missing", "Error");
                App.Current.Shutdown();
            }
            else
            {
                try
                {
                    _settings = JsonConvert.DeserializeObject<EntityWrapperSettings>(File.ReadAllText(filePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    Application.Current.Shutdown();
                }
            }
        }

        private EntityWrapperSettings()
        {

        }

        #endregion

        #region Properties

        public bool ShowInstruction { get; set; }
        public string ShellDebugFolderPath { get; set; }
        public List<EntitiesConfig> EntitiesConfig { get; set; }

        #endregion
    }

    public class EntitiesConfig
    {
        public string Namespace { get; set; }
        public string ViewModelNamespace { get; set; }
        public string ViewModelSaveFolderPath { get; set; }
    }
}
