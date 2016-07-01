using Urbbox.SlabAssembler.Repositories;
using System;
using System.Windows;
using Urbbox.SlabAssembler.Core.Models;

namespace Urbbox.SlabAssembler.Views
{
    /// <summary>
    /// Interaction logic for PartWindow.xaml
    /// </summary>
    public partial class PartWindow : Window
    {
        public PartWindow(IPartRepository partRepository, Part part)
        {
            part.Save.Subscribe(x =>
            {
                using (var t = partRepository.StartTransaction()) {
                    if (partRepository.GetById(part.Id) == null)
                        partRepository.Add(part);
                    else
                        partRepository.PartsChanged.Execute(null);
                    t.Commit();
                }
                Close();
            });

            DataContext = part;
            InitializeComponent();
        }
    }
}
