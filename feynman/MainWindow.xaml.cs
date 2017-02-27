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
        private static Color BackgroundColor = (Color)ColorConverter.ConvertFromString("#283593");
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

        enum MODE { VIEW, EDIT, CREATE, PASSWORD };

        MODE Mode = MODE.VIEW;

        bool SomethingChanged = false;

        public MainWindow()
        {
            InitializeComponent();

          //  CredManager.TestEncryption();

            this.grdMain.Background = Theme.BackgroundBrushColor;
            this.panCreds.Background = Theme.PanelBrushColor;
            this.panCreate.Background = Theme.PanelBrushColor;
            //this.Background = Brushes.Yellow;

            //CredManager.Load();

            RefreshInterface();

            cbxAccNames.SelectedIndex = 0;

            this.Width = panMain.Width + 40;

            SwitchToPasswordMode();
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
            
            if( acc == null)
            {
                return;
            }         

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
            SomethingChanged = false;

            if( cbxAccNames.SelectedItem == null )
            {
                return;
            }


            // Empty the panel and add text boxes for editing.
            tbAccountName.Text = cbxAccNames.SelectedItem.ToString();
            panEdit.Margin = panMain.Margin;
            panEdit.Width = panMain.Width;

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

        private void CreateTime()
        {
            Mode = MODE.CREATE;

            // Set interface stuff:
            tbAccountName.Text = "";
            panEdit.Margin = panMain.Margin;
            Panel editPanel = this.panCreate;
            editPanel.Children.Clear();

            // Add one entry panel:
            Panel panel = GetEntryPanel("","");
            editPanel.Children.Add(panel);      
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

            SomethingChanged = true;
        }

        private void btnSaveAccount_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private bool Save()
        {
            string accountName = tbAccountName.Text;
            Dictionary<string, string> details;

            if (Mode == MODE.CREATE)
            {
                // Check account name:
                if( String.IsNullOrEmpty( accountName ) )
                {
                    MessageBox.Show("You need to an account name, thank you.");
                    return false;
                }

                details = ExtractEntries();
                if( details.Count == 0 )
                {
                    MessageBox.Show("You need to enter some details, thank you.");
                    return false;
                }

                if( CredManager.CreateAccount(accountName, details) == false ){
                    return false;
                }
            }
            else if( Mode == MODE.EDIT )
            {
                details = ExtractEntries();
                CredManager.Replace(accountName, details);
            }          

            cbxAccNames.Items.Clear();

            RefreshInterface();

            cbxAccNames.SelectedItem = accountName;

            return true;

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

                // We'll only add details if the key exists in some formS
                if (!String.IsNullOrEmpty(key) )
                {
                    details.Add(key, val);
                }
            }

            return details;
        }

        private void btnEditAccount_Click(object sender, RoutedEventArgs e)
        {
            EditTime();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Does the user want to save the changes or cancel?

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Do you want to save your account?", "Save Confirmation", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes )
            {
                if ( Save() )
                {
                    SwitchToViewMode();
                }
            }
            else
            {
                SwitchToViewMode();
            }
        }

        private void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            CreateTime();
        }

        private void SwitchToViewMode()
        {
            // Move the panel out of the way to reveal the main screen.
            Thickness marg = panEdit.Margin;
            marg.Left = 500;

            panEdit.Margin = marg;
            panPassword.Margin = marg;

            Mode = MODE.VIEW;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string accountName = cbxAccNames.SelectedItem.ToString();

            if( Mode == MODE.VIEW )
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(String.Format("Are you sure you want to delete {0}", accountName), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    CredManager.RemoveAccount(accountName);
                }

                cbxAccNames.Items.Clear();
                RefreshInterface();
                cbxAccNames.SelectedIndex = 0;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string password = tbPassword.Text;

            if( CredManager.Save(password) )
            {

            }
        }

        private void SwitchToPasswordMode()
        {
            Mode = MODE.PASSWORD;

            panPassword.Margin = panMain.Margin;

            panPassword.Width = panMain.Width;
            panPassword.Height = panMain.Height;
           
        }

        private void btnPassword_Click(object sender, RoutedEventArgs e)
        {
            string password = tbPassword.Text;

            if( CredManager.LoadFile( password ) )
            {
                RefreshInterface();

                cbxAccNames.SelectedIndex = 0;

                SwitchToViewMode();
            }
        }
    }
}
