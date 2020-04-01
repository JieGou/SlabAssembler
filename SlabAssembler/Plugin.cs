using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using ReactiveUI;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Urbbox.SlabAssembler.Core;
using Urbbox.SlabAssembler.Managers;
using Urbbox.SlabAssembler.Repositories;

namespace Urbbox.SlabAssembler
{
    public static class Plugin
    {
        /// <summary>
        /// 面板
        /// </summary>
        private static PaletteSet _mainPallet;

        private static IAlgorythimRepository _algorythimRepository;
        private static IPartRepository _partRepository;

        /// <summary>
        /// 面板初始大小
        /// </summary>
        private static Size _initialSize = new Size(325, 600);

        /// <summary>
        /// 打开面板命令
        /// </summary>

        [CommandMethod("UrbSlab")]
        public static void ShowPallet()
        {
            if (_mainPallet != null)
            {
                return;
            }

            _mainPallet = InitializeMainPallet();
            _mainPallet.Size = _initialSize;

            _mainPallet.StateChanged += _mainPallet_StateChanged;
            _algorythimRepository = new AlgorythimRepository();
            _partRepository = new PartRepository();

            var especificationsView = new Views.EspecificationsControl(_partRepository);
            var algorythimView = new Views.AlgorythimControl(_partRepository, _algorythimRepository);
            var partsView = new Views.PartsListControl(_partRepository);
            var helper = new BuildingProcessHelper();
            var prop = new SlabProperties { Algorythim = algorythimView.ViewModel, Parts = especificationsView.ViewModel };

            especificationsView.ViewModel.WhenAnyValue(x => x.SelectedModulation).Subscribe(m => algorythimView.ViewModel.SelectedModulation = m);
            especificationsView.ViewModel.DrawSlab.Subscribe(async _ =>
            {
                try
                {
                    prop.MaxPoint = helper.GetMaxPoint(especificationsView.ViewModel.SelectedOutline);
                    prop.StartPoint = helper.GetStartPoint(especificationsView.ViewModel.SelectedOutline, especificationsView.ViewModel.SpecifyStartPoint);

                    using (var builder = new SlabBuilder(_partRepository, prop))
                    {
                        await builder.Start();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    MessageBox.Show($"{e.Message}\n\n{e.StackTrace}");
                }
            });

            especificationsView.ViewModel.SelectOutline.Subscribe(_ =>
            {
                try
                {
                    especificationsView.ViewModel.SelectedOutline = helper.SelectOutline();
                }
                catch (ArgumentException) { MessageBox.Show("选择一个有效的轮廓。（闭合的折线）"); }
            });

            _mainPallet.Add("规格", GetElementHost(especificationsView));
            _mainPallet.Add("算法", GetElementHost(algorythimView));
            _mainPallet.Add("零件图", GetElementHost(partsView));
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
            if (e.NewState == StateEventIndex.Hide)
            {
                _mainPallet = null;
                _algorythimRepository = null;
                _partRepository = null;
            }
        }

        /// <summary>
        /// Palette面板初始化
        /// </summary>
        /// <returns></returns>
        private static PaletteSet InitializeMainPallet()
        {
            return new PaletteSet("Urbbox | 平板创建")
            {
                Size = _initialSize,
                DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right),
                MinimumSize = new Size(325, 500),
                Visible = true,
                KeepFocus = true,
            };
        }
    }
}