using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace EntityWrapperGenerator.ViewModel
{
    public sealed class MainViewModel : ObservableObject
    {
        #region Constants

        private static readonly string Tab1 = "\t";
        private static readonly string Tab2 = "\t\t";
        private static readonly string Tab3 = "\t\t\t";
        private static readonly string Tab4 = "\t\t\t\t";
        private static readonly string Tab5 = "\t\t\t\t\t";
        private static readonly string Tab6 = "\t\t\t\t\t\t";
        private static readonly string WhiteSpace = " ";
        private static readonly string Using = "using";
        private static readonly string Namespace = "namespace";
        private static readonly string Public = "public";
        private static readonly string Class = "class";
        private static readonly string BlockStart = "{";
        private static readonly string BlockEnd = "}";
        private static readonly string EndOfStatement = ";";
        private static readonly string RegionStart = "#region";
        private static readonly string RegionEnd = "#endregion";
        private static readonly string Return = "return";
        private static readonly string Get = "get";
        private static readonly string Set = "set";
        private static readonly string If = "if";
        private static readonly string On = "On";
        private static readonly string AddConditionalIsDirty = "AddConditionalIsDirty";
        private static readonly string RaisePropertyChanged = "RaisePropertyChanged";
        private static readonly string Initialised = "Initialised";
        private static readonly string Created = "Created";
        private static readonly string Default = "default";
        private static readonly string Changed = "Changed";
        private static readonly string Changing = "Changing";
        private static readonly string Ref = "ref";
        private static readonly string Value = "value";
        private static readonly string Properties = "Properties";
        private static readonly string Constructor = "Constructor";
        private static readonly string Methods = "Methods";
        private static readonly string ViewModel = "ViewModel";
        private static readonly string Partial = "partial";
        private static readonly string Void = "void";
        private static readonly string ParenthesesOpen = "(";
        private static readonly string ParenthesesClose = ")";
        private static readonly string Comma = ",";
        private static readonly string Colon = ":";
        private static readonly string New = "new";
        private static readonly string Base = "base";
        private static readonly string This = "this";


        #endregion

        #region Properties

        private ObservableCollection<TypeViewModel> _typeListInternal;

        private EntitiesConfig _selectedTypeConfig = null;

        public string FolderPath { get; set; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value) && TypeList != null)
                {
                    TypeList.Refresh();
                }
            }
        }

        private ICollectionView _typeList;
        public ICollectionView TypeList
        {
            get { return _typeList; }
            set { SetProperty(ref _typeList, value); }
        }

        private TypeViewModel _selectedType;
        public TypeViewModel SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    RaiseCommandsNotifyCanExecuteChanged();
                    if (SelectedType != null && EntityWrapperSettings.Default.EntitiesConfig != null && EntityWrapperSettings.Default.EntitiesConfig.Any(x => x.Namespace == SelectedType.Entity.Namespace))
                    {
                        _selectedTypeConfig = EntityWrapperSettings.Default.EntitiesConfig.First(x => x.Namespace == SelectedType.Entity.Namespace);
                    }
                    else
                    {
                        _selectedTypeConfig = null;
                    }
                }
            }
        }

        private string _generatedCode;
        public string GeneratedCode
        {
            get { return _generatedCode; }
            set
            {
                if (SetProperty(ref _generatedCode, value))
                {
                    RaiseCommandsNotifyCanExecuteChanged();
                }
            }
        }

        private RelayCommand _generateClassCommand;

        public RelayCommand GenerateClassCommand
        {
            get { return _generateClassCommand ?? (_generateClassCommand = new RelayCommand(GenerateClass, CanGenerateClass)); }
        }


        private RelayCommand _copyToClipboardCommand;

        public RelayCommand CopyToClipboardCommand
        {
            get { return _copyToClipboardCommand ?? (_copyToClipboardCommand = new RelayCommand(CopyToClipboard, CanCopyToClipboard)); }
        }

        private RelayCommand _saveToFileCommand;

        public RelayCommand SaveToFileCommand
        {
            get { return _saveToFileCommand ?? (_saveToFileCommand = new RelayCommand(SaveToFile, CanSaveToFile)); }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _typeListInternal = new ObservableCollection<TypeViewModel>();
            TypeList = CollectionViewSource.GetDefaultView(_typeListInternal);
            TypeList.Filter += OnFilterRequested;

        }

        #endregion

        /// <summary>
        /// Raises the commands notify can execute changed.
        /// </summary>
        private void RaiseCommandsNotifyCanExecuteChanged()
        {
            if (_copyToClipboardCommand != null)
            {
                _copyToClipboardCommand.NotifyCanExecuteChanged();
            }
            if (_generateClassCommand != null)
            {
                _generateClassCommand.NotifyCanExecuteChanged();
            }
            if (_saveToFileCommand != null)
            {
                _saveToFileCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Called when [filter requested].
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool OnFilterRequested(object obj)
        {
            TypeViewModel item = obj as TypeViewModel;

            if (item != null && !string.IsNullOrWhiteSpace(item.ClassName) && !string.IsNullOrWhiteSpace(SearchText))
            {
                return item.ClassName.ToLower().StartsWith(SearchText.ToLower());
            }

            return true;
        }

        /// <summary>
        /// Adds the items.
        /// </summary>
        /// <param name="types">The types.</param>
        public void AddItems(List<TypeViewModel> types)
        {
            _typeListInternal.Clear();
            types.ForEach(t => _typeListInternal.Add(t));

        }

        private bool CanGenerateClass()
        {
            return SelectedType != null;
        }

        private void GenerateClass()
        {
            string defaultNamespace = _selectedTypeConfig != null && !string.IsNullOrEmpty(_selectedTypeConfig.ViewModelNamespace) ? _selectedTypeConfig.ViewModelNamespace : "Enter_Namespace_Here";
            StringBuilder stringBuilder = new StringBuilder();
            HashSet<string> nameSpaces = new HashSet<string>
            {
                "VShips.Framework.Common.Services",
                "VShips.Framework.Common.ViewModel",
                SelectedType.Entity.Namespace
            };

            var props = SelectedType.Entity.GetProperties().Where(p => !p.PropertyType.IsGenericType || p.PropertyType.Name == ExtensionMethods.NullableTypeName).ToList();
            List<string> propsNameSpaces = props.Select(p => p.PropertyType.Namespace).Distinct().ToList();
            propsNameSpaces.ForEach(n =>
            {
                if (!nameSpaces.Contains(n))
                {
                    nameSpaces.Add(n);
                }
            });
            stringBuilder.AddInLoop(nameSpaces.OrderBy(x => x), (s, nameSpace) =>
            {
                s.Append(Using).Append(WhiteSpace).Append(nameSpace).Append(EndOfStatement).Append(Environment.NewLine);
            })
            .Append(Environment.NewLine).Append(Namespace).Append(WhiteSpace).Append(defaultNamespace)
            .Append(Environment.NewLine).Append(BlockStart)
                .Append(Environment.NewLine).Append(Tab1).Append(Public).Append(WhiteSpace).Append(Partial).Append(WhiteSpace).Append(Class).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(" : BaseEntityWrapper<").Append(SelectedType.Entity.Name).Append(">")
                .Append(Environment.NewLine).Append(Tab1).Append(BlockStart)
                    .Append(Environment.NewLine).Append(Tab2).Append(RegionStart).Append(WhiteSpace).Append(Properties) // #region Properties
                    .AddInLoop(props, (s, p) =>
                    {
                        string typeName = p.PropertyType.GetTypeName();

                        s.Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(typeName).Append(WhiteSpace).Append(p.Name)
                        .Append(Environment.NewLine).Append(Tab2).Append(BlockStart)
                            // Entity Getter
                            .Append(Environment.NewLine).Append(Tab3).Append(Get).Append(WhiteSpace).Append(BlockStart).Append(WhiteSpace).Append(Return).Append(WhiteSpace).Append("Entity != null ? Entity.").Append(p.Name).Append(WhiteSpace).Append(":").Append(WhiteSpace).Append(Default).Append(ParenthesesOpen).Append(typeName).Append(ParenthesesClose).Append(EndOfStatement).Append(WhiteSpace).Append(BlockEnd)
                            // Entity Setter
                            .Append(Environment.NewLine).Append(Tab3).Append(Set)
                            .Append(Environment.NewLine).Append(Tab3).Append(BlockStart)
                                .Append(Environment.NewLine).Append(Tab4).Append(If).Append(WhiteSpace).Append(ParenthesesOpen).Append("Entity != null").Append(ParenthesesClose)
                                .Append(Environment.NewLine).Append(Tab4).Append(BlockStart)
                                    .Append(Environment.NewLine).Append(Tab5).Append(On).Append(p.Name).Append(Changing).Append(ParenthesesOpen).Append(Ref).Append(WhiteSpace).Append(Value).Append(ParenthesesClose).Append(EndOfStatement)
                                    .Append(Environment.NewLine).Append(Tab5).Append(If).Append(WhiteSpace).Append(ParenthesesOpen).Append("!Equals").Append(ParenthesesOpen).Append("Entity.").Append(p.Name).Append(Comma).Append(WhiteSpace).Append(Value).Append(ParenthesesClose).Append(ParenthesesClose)
                                    .Append(Environment.NewLine).Append(Tab5).Append(BlockStart)
                                        .Append(Environment.NewLine).Append(Tab6).Append("Entity.").Append(p.Name).Append(WhiteSpace).Append("=").Append(WhiteSpace).Append(Value).Append(EndOfStatement)
                                        .Append(Environment.NewLine).Append(Tab6).Append(AddConditionalIsDirty).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                                        .Append(Environment.NewLine).Append(Tab6).Append(On).Append(p.Name).Append(Changed).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                                    .Append(Environment.NewLine).Append(Tab5).Append(BlockEnd)
                                    .Append(Environment.NewLine).Append(Tab5).Append(RaisePropertyChanged).Append(ParenthesesOpen).Append(ParenthesesOpen).Append(ParenthesesClose).Append(WhiteSpace).Append("=>").Append(WhiteSpace).Append(p.Name).Append(ParenthesesClose).Append(EndOfStatement)
                                .Append(Environment.NewLine).Append(Tab4).Append(BlockEnd)
                            .Append(Environment.NewLine).Append(Tab3).Append(BlockEnd)
                        .Append(Environment.NewLine).Append(Tab2).Append(BlockEnd);
                    })
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(RegionEnd)
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(RegionStart).Append(WhiteSpace).Append(Constructor)
                // Constructor for Edit Entity with StartMarkingDirty = true
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(ParenthesesOpen).Append(SelectedType.Entity.Name).Append(WhiteSpace).Append("entity, INavigationContext context").Append(ParenthesesClose)
                    .Append(Environment.NewLine).Append(Tab3).Append(Colon).Append(WhiteSpace).Append(This).Append(ParenthesesOpen).Append("entity, context, true").Append(ParenthesesClose).Append(WhiteSpace).Append(BlockStart).Append(WhiteSpace).Append(BlockEnd)
                // Constructor for Edit Entity with StartMarkingDirty pass
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(ParenthesesOpen).Append(SelectedType.Entity.Name).Append(WhiteSpace).Append("entity, INavigationContext context, bool isStartMarkingDirty").Append(ParenthesesClose)
                    .Append(Environment.NewLine).Append(Tab3).Append(Colon).Append(WhiteSpace).Append(Base).Append(ParenthesesOpen).Append("entity, context, false, isStartMarkingDirty, true").Append(ParenthesesClose)
                .Append(Environment.NewLine).Append(Tab2).Append(BlockStart)
                    .Append(Environment.NewLine).Append(Tab3).Append(On).Append(Initialised).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                .Append(Environment.NewLine).Append(Tab2).Append(BlockEnd)
                // Constructor for Add Entity with StartMarkingDirty = true
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(ParenthesesOpen).Append("INavigationContext context").Append(ParenthesesClose)
                    .Append(Environment.NewLine).Append(Tab3).Append(Colon).Append(WhiteSpace).Append(This).Append(ParenthesesOpen).Append("context, true").Append(ParenthesesClose).Append(WhiteSpace).Append(BlockStart).Append(WhiteSpace).Append(BlockEnd)
                // Constructor for Add Entity with StartMarkingDirty pass
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(ParenthesesOpen).Append("INavigationContext context, bool isStartMarkingDirty").Append(ParenthesesClose)
                    .Append(Environment.NewLine).Append(Tab3).Append(Colon).Append(WhiteSpace).Append(Base).Append(ParenthesesOpen).Append(New).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ParenthesesOpen).Append(ParenthesesClose).Append(Comma).Append(WhiteSpace).Append("context, true, isStartMarkingDirty, true").Append(ParenthesesClose)
                .Append(Environment.NewLine).Append(Tab2).Append(BlockStart)
                    .Append(Environment.NewLine).Append(Tab3).Append(On).Append(Created).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                    .Append(Environment.NewLine).Append(Tab3).Append(On).Append(Initialised).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                .Append(Environment.NewLine).Append(Tab2).Append(BlockEnd)
                // Dummy Constructor
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Public).Append(WhiteSpace).Append(SelectedType.Entity.Name).Append(ViewModel).Append(ParenthesesOpen).Append(ParenthesesClose).Append(WhiteSpace).Append(BlockStart).Append(WhiteSpace).Append(BlockEnd)
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(RegionEnd)
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(RegionStart).Append(WhiteSpace).Append(Methods)
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Partial).Append(WhiteSpace).Append(Void).Append(WhiteSpace).Append(On).Append(Created).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                .Append(Environment.NewLine).Append(Tab2).Append(Partial).Append(WhiteSpace).Append(Void).Append(WhiteSpace).Append(On).Append(Initialised).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement)
                .AddInLoop(props, (s, p) =>
                {
                    string typeName = p.PropertyType.GetTypeName();

                    s.Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(Partial).Append(WhiteSpace).Append(Void).Append(WhiteSpace).Append(On).Append(p.Name).Append(Changing).Append(ParenthesesOpen).Append(Ref).Append(WhiteSpace).Append(typeName).Append(WhiteSpace).Append(Value).Append(ParenthesesClose).Append(EndOfStatement)
                     .Append(Environment.NewLine).Append(Tab2).Append(Partial).Append(WhiteSpace).Append(Void).Append(WhiteSpace).Append(On).Append(p.Name).Append(Changed).Append(ParenthesesOpen).Append(ParenthesesClose).Append(EndOfStatement);
                })
                .Append(Environment.NewLine).Append(Environment.NewLine).Append(Tab2).Append(RegionEnd)
            .Append(Environment.NewLine).Append(Tab1).Append(BlockEnd)
            .Append(Environment.NewLine).Append(BlockEnd);

            GeneratedCode = stringBuilder.ToString();
        }

        private bool CanCopyToClipboard()
        {
            return !string.IsNullOrWhiteSpace(GeneratedCode);
        }

        private void CopyToClipboard()
        {
            System.Windows.Clipboard.SetText(GeneratedCode);
        }

        private bool CanSaveToFile()
        {
            return !string.IsNullOrWhiteSpace(GeneratedCode) && SelectedType != null;
        }

        private void SaveToFile()
        {
            if (_selectedTypeConfig != null && !string.IsNullOrWhiteSpace(_selectedTypeConfig.ViewModelSaveFolderPath) && Directory.Exists(_selectedTypeConfig.ViewModelSaveFolderPath))
            {
                string filePath = Path.Combine(_selectedTypeConfig.ViewModelSaveFolderPath, SelectedType.Entity.Name + ViewModel + ".cs");

                File.WriteAllText(filePath, GeneratedCode);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    InitialDirectory = FolderPath,
                    FileName = SelectedType.Entity.Name + ViewModel + ".cs"
                };

                if (saveFileDialog.ShowDialog().GetValueOrDefault())
                {
                    File.WriteAllText(saveFileDialog.FileName, GeneratedCode);
                }
            }

        }
    }
}