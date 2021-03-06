﻿using System.ComponentModel;
using HSMClientWPFControls.ConnectorInterface;

namespace HSMClient
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : System.Windows.Window
    {
        private readonly SettingsWindowViewModel _viewModel;
        public SettingsWindow(ISettingsConnector settingsConnector)
        {
            _viewModel = new SettingsWindowViewModel(settingsConnector);
            this.DataContext = _viewModel;
            InitializeComponent();
        }


        private void settingsWindow_Closing(object sender, CancelEventArgs e)
        {
            this.Owner = null;
            _viewModel.Dispose();
        }
    }
}
