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
using System.Windows.Shapes;

namespace feynman
{
    /// <summary>
    /// Interaction logic for CreateNew.xaml
    /// </summary>
    /// 

    public partial class CreateNew : Window
    {
        public CreateNew()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEntry();
        }

        private void AddEntry()
        {
            StackPanel pan = new StackPanel();
            pan.Height = this.panEntry.Height;
            pan.Width = this.panEntry.Width;
            pan.Background = this.panEntry.Background;
            pan.Orientation = this.panEntry.Orientation;
            pan.Margin = this.panEntry.Margin;

            TextBox tbNewKey = new TextBox();
            tbNewKey.Width = tbKey.Width;
            tbNewKey.Height = tbKey.Height;
            tbNewKey.Margin = tbKey.Margin;
            tbNewKey.VerticalAlignment = tbKey.VerticalAlignment;
            tbNewKey.Uid = tbKey.Uid;
            pan.Children.Add(tbNewKey);

            TextBox tbNewValue = new TextBox();
            tbNewValue.Width = tbValue.Width;
            tbNewValue.Height = tbValue.Height;
            tbNewValue.Margin = tbValue.Margin;
            tbNewValue.VerticalAlignment = tbValue.VerticalAlignment;
            tbNewValue.Uid = tbValue.Uid;
            pan.Children.Add(tbNewValue);

            Button btnNewRemove = new Button();
            btnNewRemove.Content = "-";
            btnNewRemove.Width = this.btnRemove.Width;
            btnNewRemove.Height = this.btnRemove.Height;
            btnNewRemove.Margin = this.btnRemove.Margin;
            btnNewRemove.VerticalAlignment = this.btnRemove.VerticalAlignment;
            btnNewRemove.Content = this.btnRemove.Content;
            btnNewRemove.Click += btnRemove_Click;  

            pan.Children.Add(btnNewRemove);

            this.panEntries.Children.Add(pan);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            Panel pan = (Panel) ((Button)sender).Parent;

            this.panEntries.Children.Remove(pan); 
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if( feynman.MainWindow.CredManager.DoesAccountNameExist( tbAccountName.Text ) )
            {
                MessageBox.Show("An account with that name already exists, please enter a unique accout name");
                return;
            }

            ExtractEntriesAndSave();
            Close();
        }

        public void ExtractEntriesAndSave()
        {
            Dictionary<string, string> details = new Dictionary<string, string>();
            string key;
            string val;

            foreach( Panel pan in panEntries.Children )
            {
                key = "";
                val = "";
                foreach( Control tb in pan.Children )
                {
                    if (tb is TextBox)
                    {
                        TextBox temp = (TextBox) tb;
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

            feynman.MainWindow.CredManager.CreateAccount(tbAccountName.Text, details);    
        }

        public void PopulateWithExisting()
        {

        }
    }
}
