using FontAwesome5.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace FontAwesome5.Net40.Example.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            AllIcons = Enum.GetValues(typeof(EFontAwesomeIcon)).Cast<EFontAwesomeIcon>().Where(x => x != EFontAwesomeIcon.None)
                        .OrderByDescending(i => i.GetStyle()).ThenBy(i => i.GetLabel()).ToArray();

            SelectedIcon = AllIcons.First();

            FlipOrientations = Enum.GetValues(typeof(EFlipOrientation)).Cast<EFlipOrientation>().ToArray();
            SpinDuration = 5;
            PulseDuration = 5;
            FontSize = 30;
            Rotation = 0;

            Visibilities = Enum.GetValues(typeof(Visibility)).Cast<Visibility>().ToArray();
            Visibility = Visibility.Visible;
        }

        public Visibility Visibility { get; set; }
        public IEnumerable<Visibility> Visibilities { get; }

        //public EFontAwesomeIcon SelectedIcon { get; set; }
        private EFontAwesomeIcon _selectedIcon;
        public EFontAwesomeIcon SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                if (_selectedIcon != value)
                {
                    _selectedIcon = value;
                    RaisePropertyChanged(nameof(SelectedIcon));
                }
            }
        }



        public bool SpinIsEnabled { get; set; }
        public double SpinDuration { get; set; }
        public bool PulseIsEnabled { get; set; }
        public double PulseDuration { get; set; }
        public EFlipOrientation FlipOrientation { get; set; }
        public double FontSize { get; set; }
        public double Rotation { get; set; }

        public IEnumerable<EFlipOrientation> FlipOrientations { get; }
        public IEnumerable<EFontAwesomeIcon> AllIcons { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
