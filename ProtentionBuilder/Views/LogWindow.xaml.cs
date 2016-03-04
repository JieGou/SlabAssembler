using System;
using System.Windows;
using Urbbox.AutoCAD.ProtentionBuilder.ViewModels;

namespace Urbbox.AutoCAD.ProtentionBuilder.Views
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
