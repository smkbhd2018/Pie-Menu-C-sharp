using System.Windows;

namespace PieOverlay
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow _parent;

        public SettingsWindow(MainWindow parent)
        {
            InitializeComponent();
            _parent = parent;

            // load existing values
            Txt0.Text = _parent.ItemNames[0];
            Txt1.Text = _parent.ItemNames[1];
            Txt2.Text = _parent.ItemNames[2];
            Txt3.Text = _parent.ItemNames[3];
            Txt4.Text = _parent.ItemNames[4];
            Txt5.Text = _parent.ItemNames[5];
            Txt6.Text = _parent.ItemNames[6];
            Txt7.Text = _parent.ItemNames[7];

            Hot0.Text = _parent.ItemHotkeys[0];
            Hot1.Text = _parent.ItemHotkeys[1];
            Hot2.Text = _parent.ItemHotkeys[2];
            Hot3.Text = _parent.ItemHotkeys[3];
            Hot4.Text = _parent.ItemHotkeys[4];
            Hot5.Text = _parent.ItemHotkeys[5];
            Hot6.Text = _parent.ItemHotkeys[6];
            Hot7.Text = _parent.ItemHotkeys[7];

            // load radius
            SldRadius.Value = _parent.Radius;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            // save back into parent
            _parent.ItemNames[0] = Txt0.Text;
            _parent.ItemNames[1] = Txt1.Text;
            _parent.ItemNames[2] = Txt2.Text;
            _parent.ItemNames[3] = Txt3.Text;
            _parent.ItemNames[4] = Txt4.Text;
            _parent.ItemNames[5] = Txt5.Text;
            _parent.ItemNames[6] = Txt6.Text;
            _parent.ItemNames[7] = Txt7.Text;

            _parent.ItemHotkeys[0] = Hot0.Text.Trim();
            _parent.ItemHotkeys[1] = Hot1.Text.Trim();
            _parent.ItemHotkeys[2] = Hot2.Text.Trim();
            _parent.ItemHotkeys[3] = Hot3.Text.Trim();
            _parent.ItemHotkeys[4] = Hot4.Text.Trim();
            _parent.ItemHotkeys[5] = Hot5.Text.Trim();
            _parent.ItemHotkeys[6] = Hot6.Text.Trim();
            _parent.ItemHotkeys[7] = Hot7.Text.Trim();

            _parent.Radius = SldRadius.Value;

            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
