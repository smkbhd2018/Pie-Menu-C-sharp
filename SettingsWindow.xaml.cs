using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PieOverlay
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow _parent;
        private readonly List<TextBox> _nameBoxes = new();
        private readonly List<TextBox> _hotBoxes  = new();

        public SettingsWindow(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;

            SldCount.Value = _parent.ItemNames.Count;
            SldRadius.Value = _parent.Radius;
            CmbMode.SelectedIndex = (int)_parent.Behavior;
            SldDead.Value = _parent.DeadZoneRadius;
            SldFont.Value = _parent.ItemFontSize;
            TxtFont.Text = _parent.ItemFontFamily.Source;

            if (_parent.ItemColor is SolidColorBrush scb)
                TxtColor.Text = scb.Color.ToString();
            else
                TxtColor.Text = "";

            RebuildItemRows((int)SldCount.Value);
        }

        private void RebuildItemRows(int count)
        {
            ItemsPanel.Children.Clear();
            _nameBoxes.Clear();
            _hotBoxes.Clear();
            for (int i = 0; i < count; i++)
            {
                var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,2,0,2) };
                sp.Children.Add(new Label { Content = $"Item {i+1}:", Width = 60, VerticalAlignment = VerticalAlignment.Center });

                var name = new TextBox { Width = 120, Margin = new Thickness(4,0,4,0) };
                if (i < _parent.ItemNames.Count) name.Text = _parent.ItemNames[i];
                sp.Children.Add(name);
                _nameBoxes.Add(name);

                var hot = new TextBox { Width = 120, Margin = new Thickness(4,0,4,0) };
                if (i < _parent.ItemHotkeys.Count) hot.Text = _parent.ItemHotkeys[i];
                sp.Children.Add(hot);
                _hotBoxes.Add(hot);

                ItemsPanel.Children.Add(sp);
            }
            TxtCount.Text = count.ToString();
        }

        private void OnCountChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RebuildItemRows((int)SldCount.Value);
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            int count = _nameBoxes.Count;
            _parent.EnsureItemCount(count);
            for (int i = 0; i < count; i++)
            {
                _parent.ItemNames[i] = _nameBoxes[i].Text;
                _parent.ItemHotkeys[i] = _hotBoxes[i].Text.Trim();
            }

            _parent.Radius = SldRadius.Value;
            _parent.Behavior = (SelectionMode)CmbMode.SelectedIndex;
            _parent.DeadZoneRadius = SldDead.Value;
            _parent.ItemFontSize = SldFont.Value;
            _parent.ItemFontFamily = new FontFamily(TxtFont.Text);
            try
            {
                var brushObj = new BrushConverter().ConvertFromString(TxtColor.Text);
                if (brushObj is Brush brush)
                {
                    _parent.ItemColor = brush;
                }
            }
            catch
            {
                // ignore invalid color
            }

            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
