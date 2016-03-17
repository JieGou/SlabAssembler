using System;
using System.Windows;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            DataContext = new LogWindowViewModel();
            InitializeComponent();
        }

        public void SetLogMessage(string log)
        {
            ((LogWindowViewModel)DataContext).LogMessage = log;
        }

        public void SetResultTitle(string title)
        {
            ((LogWindowViewModel)DataContext).ResultsMessage = title;
        }
    }
}
