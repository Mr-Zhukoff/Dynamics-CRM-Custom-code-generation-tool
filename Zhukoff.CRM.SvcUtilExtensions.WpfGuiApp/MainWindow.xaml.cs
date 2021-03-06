﻿using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp.ViewModel;

namespace Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnectToCrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginWindow loginWnd = new LoginWindow();
                bool? result = loginWnd.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var vm = (MainViewModel)DataContext;
                    vm.ConnectToCrmCommand.Execute(null);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
