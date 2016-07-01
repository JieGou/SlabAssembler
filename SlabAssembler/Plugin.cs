using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Urbbox.SlabAssembler.Core;
using System;
using ReactiveUI;
using Urbbox.SlabAssembler.Managers;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler
{
    public static class Plugin
    {
        private static PaletteSet _mainPallet;
        private static IAlgorythimRepository _algorythimRepository;
        private static IPartRepository _partRepository;

        [CommandMethod("URBLAJE")]
        public static void ShowPallet()
        {
            if (_mainPallet != null) return;
            _mainPallet = InitializeMainPallet();
            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _algorythimRepository = new AlgorythimRepository();
            _partRepository = new PartRepository();

            var especificationsView = new Views.EspecificationsControl(_partRepository);
            var algorythimView = new Views.AlgorythimControl(_partRepository, _algorythimRepository);
            var partsView = new Views.PartsListControl(_partRepository);
            var helper = new BuildingProcessHelper();
            var prop = new SlabProperties {
                Algorythim = algorythimView.ViewModel,
                Parts = especificationsView.ViewModel
            };

            especificationsView.ViewModel.WhenAnyValue(x => x.SelectedModulation).Subscribe(m => algorythimView.ViewModel.SelectedModulation = m);
            especificationsView.ViewModel.DrawSlab.Subscribe(async _ =>
            {
                using (var builder = new SlabBuilder(_partRepository))
                {
                    try
                    {
                        prop.MaxPoint = helper.GetMaxPoint(especificationsView.ViewModel.SelectedOutline);
                        prop.StartPoint = helper.GetStartPoint(especificationsView.ViewModel.SelectedOutline, especificationsView.ViewModel.SpecifyStartPoint);
                        await builder.Start(prop);
                    }
                    catch (NullReferenceException) { }
                    catch (OperationCanceledException) { }
                    catch (Autodesk.AutoCAD.Runtime.Exception e) { MessageBox.Show($"{e.Message}\n\n{e.StackTrace}"); }
                }
            });

            especificationsView.ViewModel.SelectOutline.Subscribe(_ =>
            {
                try
                {
                    especificationsView.ViewModel.SelectedOutline = helper.SelectOutline();
                } catch (ArgumentException) { MessageBox.Show("Selecione um contorno válido. (Polilinha fechada)"); }
            });

            _mainPallet.Add("Especificações", GetElementHost(especificationsView));
            _mainPallet.Add("Algoritmo", GetElementHost(algorythimView));
            _mainPallet.Add("Peças", GetElementHost(partsView));
        }

        private static ElementHost GetElementHost(System.Windows.UIElement element)
        {
            return new ElementHost
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Child = element
            };
        }

        private static void _mainPallet_StateChanged(object sender, PaletteSetStateEventArgs e)
        {
            if (e.NewState == StateEventIndex.Hide) { 
                _mainPallet = null;
                _algorythimRepository = null;
                _partRepository = null;
            }
        }

        private static PaletteSet InitializeMainPallet()
        {
            return new PaletteSet("Urbbox | Construtor de Lajes")
            {
                Size = new Size(325, 600),
                DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right),
                MinimumSize = new Size(325, 500),
                Visible = true,
                KeepFocus = true,
            };
        }
    }
}
