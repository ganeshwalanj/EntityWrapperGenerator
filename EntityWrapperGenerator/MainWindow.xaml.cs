using EntityWrapperGenerator.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace EntityWrapperGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _mainVM = new MainViewModel();
        public MainWindow()
        {
            DataContext = _mainVM;
            this.FontFamily = new FontFamily(new Uri("pack://application:,,,/JetBrainsMono-Medium"), "JetBrainsMono");
            InitializeComponent();
            Loaded += OnWindowsLoaded;
            Console.WriteLine(EntityWrapperSettings.Default.EntitiesConfig);
        }

        private void OnWindowsLoaded(object sender, RoutedEventArgs e)
        {
            if (EntityWrapperSettings.Default.ShowInstruction)
            {
                MessageBox.Show("Select Folder" + Environment.NewLine + @"~\Source\VShips.Framework.Shell\bin\Debug\" + Environment.NewLine + "from your machine.", "EntityWrapper Generator Tool - Get Started Instruction");
            }
            if (!string.IsNullOrWhiteSpace(EntityWrapperSettings.Default.ShellDebugFolderPath) && Directory.Exists(EntityWrapperSettings.Default.ShellDebugFolderPath))
            {
                OnFolderSelection(EntityWrapperSettings.Default.ShellDebugFolderPath);
            }
            else
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    OnFolderSelection(dialog.FileName);
                }
            }
        }

        private void OnFolderSelection(string folderPath) 
        {
            _mainVM.FolderPath = folderPath;
            string fileName = folderPath + "\\VShips.Framework.Common.dll";
            if (File.Exists(fileName))
            {
                try
                {
                    var dll = Assembly.LoadFrom(fileName);

                    var types = dll.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith("VShips.Framework.Common.Model")
                                                          && !t.IsGenericType && !t.IsNested && !t.IsInterface).ToList();

                    _mainVM.AddItems(types.Select(t => new TypeViewModel { ClassName = t.Name, Entity = t }).OrderBy(t => t.ClassName).ToList());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
            else
            {
                MessageBox.Show("VShips.Framework.Common.dll is Not Found!");
                Close();
            }
        }
    }
}