using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace EntityWrapperGenerator.ViewModel
{
    public class TypeViewModel : ObservableObject
    {
        public Type Entity { get; set; }

        private string _className;
        public string ClassName
        {
            get { return _className; }
            set { SetProperty(ref _className, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}