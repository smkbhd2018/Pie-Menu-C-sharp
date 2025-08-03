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

            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
