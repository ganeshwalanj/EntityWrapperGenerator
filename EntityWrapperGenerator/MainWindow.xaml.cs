using EntityWrapperGenerator.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
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
            InitializeComponent();

            DataContext = _mainVM;


            Loaded += OnWindowsLoaded;
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

        private List<FileInfo> GetAllDLLs(DirectoryInfo dirInfo, List<FileInfo> files = null) 
        {
            if (files == null)
            {
                files = new List<FileInfo>();
            }

            files.AddRange(dirInfo.GetFiles().Where(f => Path.GetExtension(f.FullName).ToLower() == ".dll" && Path.GetFileNameWithoutExtension(f.FullName).StartsWith("VShips")));

            try
            {
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    GetAllDLLs(dir, files);
                }
            }
            catch { }

            return files;
        }

        private void OnFolderSelection(string folderPath)
        {
            _mainVM.FolderPath = folderPath;

            try
            {
                var dlls = GetAllDLLs(new DirectoryInfo(folderPath));
                List<Assembly> asms = new List<Assembly>();
                foreach (var file in dlls)
                {
                    try
                    {
                        var dll = Assembly.LoadFrom(file.FullName);

                        if (dll.GetReferencedAssemblies().Any(asm => asm.Name.Contains("Spire")))
                        {
                            asms.Add(dll);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                _mainVM.AddItems(asms.Select(asm => new TypeViewModel { ClassName = asm.FullName }).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }
    }
}