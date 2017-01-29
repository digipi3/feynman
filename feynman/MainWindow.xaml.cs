using System;
using System.Collections.Generic;
using System.Linq;
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

namespace feynman
{
    public static class Theme
    {
        private static Color BackgroundColor = (Color)ColorConverter.ConvertFromString("#FF1D3B5F");
        public static SolidColorBrush BackgroundBrushColor = new SolidColorBrush(BackgroundColor);

        private static Color PanelColor = (Color)ColorConverter.ConvertFromString("#FF1374B8");
        public static SolidColorBrush PanelBrushColor = new SolidColorBrush(PanelColor);

        private static Color ButtonColor = (Color)ColorConverter.ConvertFromString("#FF8EA5F7");
        public static SolidColorBrush ButtonBrushColor = new SolidColorBrush(ButtonColor);
    }    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CredMan CredManager = new CredMan();

        enum MODE { VIEW, EDIT, CREATE };

        MODE Mode = MODE.VIEW;

        public MainWindow()
        {
            InitializeComponent();

            this.grdMain.Background = Theme.BackgroundBrushColor;
            this.panCreds.Background = Theme.PanelBrushColor;
            this.panCreate.Background = Theme.PanelBrushColor;
            //this.Background = Brushes.Yellow;

            CredManager.Load();

            RefreshInterface();

            cbxAccNames.SelectedIndex = 0;
        }

        private void cbxAccNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {    
            string accountName = (string) cbxAccNames.SelectedItem;

            if( string.IsNullOrEmpty(accountName) )
            {
                return;
            }

            Account acc = CredManager.GetAndSetCurrent(accountName);

            SetCredentials(acc);           
        }

        private void RefreshInterface()
        {
            // Get the current account from within the cred manager.
            Account currentAccount = CredManager.GetCurrentAccount();
            // Set the interface using the current account.
            SetCredentials(currentAccount);

            // Set the combo box wuth the names of the accounts
            List<Account> accs = CredManager.GetAccounts();
            foreach (Account ac in accs)
            {
                cbxAccNames.Items.Add(ac.Name);                
            }           
        }

        private void SetCredentials( Account acc )
        {
            this.panCreds.Children.Clear();            

            foreach (KeyValuePair<string, string> entry in acc.Credentials)
            {
                Button button = new Button();
                button.Content = entry.Key;

                Thickness margin = new Thickness();

                margin.Top = 10;
                margin.Bottom = 10;
                margin.Left = 10;
                margin.Right = 10;
                button.Margin = margin;

                Thickness thick = new Thickness();
                thick.Top = thick.Left = thick.Right = thick.Bottom = 3;

                button.BorderThickness = btnValueCopy.BorderThickness;
                button.Margin = btnValueCopy.Margin;
                button.FontFamily = btnValueCopy.FontFamily;
                button.FontStyle = btnValueCopy.FontStyle;
                button.FontSize = btnValueCopy.FontSize;
                button.Foreground = btnValueCopy.Foreground;
                button.Background = btnValueCopy.Background;
                button.BorderBrush = btnValueCopy.BorderBrush;
                button.Height = btnValueCopy.Height;
                button.Width = btnValueCopy.Width;

               // button.Background = Brush.OpacityProperty..FromRgb(0, 0, 0);

                button.Click += Button_Click;
                this.panCreds.Children.Add(button);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string key = ((Button)sender).Content.ToString();

            string value = CredManager.GetValueFromCurrent( key );

            Clipboard.SetText(value);

           // throw new NotImplementedException();
        }

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            CreateNew newWindow = new CreateNew();

            newWindow.ShowDialog();

            cbxAccNames.Items.Clear();

            RefreshInterface();

            // Set to the newly created or edited account:

            Account currentAcc = CredManager.GetCurrentAccount();

            foreach (string accountName in cbxAccNames.Items)
            {
                if( accountName == currentAcc.Name )
                {
                    cbxAccNames.SelectedItem = accountName;
                    break;
                }              
            }            
        }

        private TextBox GetEditTextBox(string keyOrValue)
        {
            TextBox textBox = new TextBox();

            textBox.Width = (panCreds.Width / 2) - 30;
            textBox.Height = 20;
            textBox.VerticalAlignment = VerticalAlignment.Top;
            textBox.Uid = keyOrValue;

            Thickness margin = textBox.Margin;
            margin.Left = margin.Top = margin.Right = margin.Bottom = 10;

            textBox.Margin = margin;

            return textBox;
        }

        private Panel GetEntryPanel( string key, string value )
        {
            StackPanel panel = new StackPanel();
            panel.Width = panCreate.Width;
            panel.Orientation = panEntry.Orientation;
            panel.Height = this.panEntry.Height;

            TextBox keyTextBox = new TextBox();
            keyTextBox.Uid = "key";
            keyTextBox.Width = tbKey.Width;
            keyTextBox.Margin = tbKey.Margin;
            keyTextBox.Height = tbKey.Height;
            keyTextBox.FontFamily = tbKey.FontFamily;
            keyTextBox.FontSize = tbKey.FontSize;

            keyTextBox.Text = key;

            TextBox valTextBox = new TextBox();
            valTextBox.Uid = "value";
            valTextBox.Width = tbValue.Width;
            valTextBox.Height = tbValue.Height;
            valTextBox.Margin = tbValue.Margin;
            valTextBox.FontFamily = tbValue.FontFamily;
            valTextBox.Background = tbValue.Background;// Theme.ButtonBrushColor;// ButtonBrushColor;
            valTextBox.FontSize = tbValue.FontSize;
            valTextBox.Text = value;

            Button remove = new Button();
            remove.Content = btnRemove.Content;
            remove.Width = btnRemove.Width;
            remove.Height = btnRemove.Height;
            remove.Margin = btnRemove.Margin;
            remove.FontSize = btnRemove.FontSize;
            remove.FontFamily = btnRemove.FontFamily;
            remove.Background = Theme.ButtonBrushColor;// ButtonBrushColor;
            remove.Click += btnRemove_Click;

            panel.Children.Add(keyTextBox);
            panel.Children.Add(valTextBox);
            panel.Children.Add(remove);

           // panCreate.Children.Add(panel);

            return panel;
        }

        private void EditTime()
        {
            Mode = MODE.EDIT;

            // Empty the panel and add text boxes for editing.
            tbAccountName.Text = cbxAccNames.SelectedItem.ToString();
            panEdit.Margin = panMain.Margin;
            Panel editPanel = this.panCreate;
            editPanel.Children.Clear();           

            Account acc = CredManager.GetCurrentAccount();           

            foreach (KeyValuePair<String, String> pair in acc.Credentials)
            {
                string key = pair.Key;
                string value = pair.Value;

                Panel panel = GetEntryPanel(key, value);            

                editPanel.Children.Add(panel);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            RefreshInterface();
        }

        private void AddEntryComponents()
        {
            Panel pan = GetEntryPanel("","");
            //this.panCreds.Children.Clear();     
            this.panCreate.Children.Add(pan);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            this.panCreds.Children.Clear();
            AddEntryComponents();
        }

        private void btnAddEntry_Click(object sender, RoutedEventArgs e)
        {
            AddEntryComponents();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            Panel pan = (Panel)((Button)sender).Parent;

            this.panCreate.Children.Remove(pan);
        }

        private void btnSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            string accountName = tbAccountName.Text;

            if (Mode == MODE.CREATE)
            {
                if (CredManager.DoesAccountNameExist(accountName))
                {
                    MessageBox.Show("An account with that name already exists.");
                    return;
                }
            }

            // Get Entries:
            Dictionary<string, string> details = ExtractEntries();

            // If Editing then the existing account needs to go:
            // Replace

            if( Mode == MODE.EDIT )
            {
                CredManager.Replace(accountName, details);
            }
            else if( Mode == MODE.CREATE )
            {
                CredManager.AddAccount(accountName, details);
            }            

            //ExtractEntriesAndSave();

            cbxAccNames.Items.Clear();

            RefreshInterface();

            cbxAccNames.SelectedItem = accountName;

            Mode = MODE.VIEW;
       }

        public Dictionary<string,string> ExtractEntries()
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            string key;
            string val;

            foreach (Panel pan in panCreate.Children)
            {
                key = "";
                val = "";
                foreach (Control tb in pan.Children)
                {
                    if (tb is TextBox)
                    {
                        TextBox temp = (TextBox)tb;
                        if (temp.Uid == "key")
                        {
                            key = temp.Text;
                        }
                        else if (temp.Uid == "value")
                        {
                            val = temp.Text;
                        }
                    }
                }
                details.Add(key, val);
            }

            return details;
        }

        private void btnEditAccount_Click(object sender, RoutedEventArgs e)
        {
            EditTime();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Move the panel out of the way to reveal the main screen.
            Thickness marg = panEdit.Margin;
            marg.Left = 500;
            panEdit.Margin = marg;

            Save();
        }

        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            Mode = MODE.CREATE;

        }
    }
}
