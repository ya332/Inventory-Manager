﻿/*
 * Author: Ken Alparslan
 * Date: 03-12-2019
 * Description: This page has the logic for adding a new order to the database
 */

using System;
using System.Collections.Generic;
using System.Data;
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

namespace WallyWorld
{
    /// <summary>
    /// Interaction logic for Add_Order_Page.xaml
    /// </summary>
    public partial class Add_Order_Page : Page
    {
        static int cartItem = 0;
        private int quant;
        private decimal pr;
        private string productN;
        private string sku;
        private string customerN;
        private string totalP;
        private string branchN;
        public Add_Order_Page()
        {
            InitializeComponent();
        }

        private void Show_Products_Click(object sender, RoutedEventArgs e)
        {
            DBMS dbms = new DBMS();
            DataTable dt = new DataTable();

            if (Products.Columns.Count == 4 || Products.Columns.Count == 0)
            {
                DataGridTemplateColumn col1 = new DataGridTemplateColumn();
                col1.Header = "Select";

                FrameworkElementFactory factory1 = new FrameworkElementFactory(typeof(Button));
                factory1.SetValue(Button.ContentProperty, "Select");
                factory1.AddHandler(Button.ClickEvent, new RoutedEventHandler(SelectBtn_Click));
                DataTemplate cellTemplate1 = new DataTemplate();
                cellTemplate1.VisualTree = factory1;
                col1.CellTemplate = cellTemplate1;
                Products.Columns.Add(col1);
            }
            
            dt = dbms.DisplayProducts();
            Products.ItemsSource = dt.DefaultView;

        }
        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            stockCB.Items.Clear();
            DataRowView r = (DataRowView)((Button)e.Source).DataContext;
            int id = int.Parse(r["SKU"].ToString());
            string name = r["name"].ToString();
            int totalStock = int.Parse(r["stock"].ToString());
            decimal price = decimal.Parse(r["wPrice"].ToString(), System.Globalization.NumberStyles.Currency);
            List<string> branches = new List<string>();
            pr = price;
            productN = name;
            sku = id.ToString();
            for(int i=1; i<=totalStock; ++i)
            {
                stockCB.Items.Add(i);
            }

            if (BranchCB.Items.Count <  3 )
            {

                DBMS dbms = new DBMS();
                branches = dbms.GetBranches();
                for (int i = 0; i < branches.Count; ++i)
                {
                    BranchCB.Items.Add(branches.ElementAt(i));
                }
            }
            if(stockCB.Items.Count != 0)
            {
                stockCB.SelectedItem = 1;
            }
            NameTB.Text = name;
            CalculatedTotal.Text = "$"+(price * quant).ToString();
            Product_Border.Visibility = Visibility.Visible;
            totalP = CalculatedTotal.Text;
        }

        private void Search_CustomerBtn(object sender, RoutedEventArgs e)
        {
            string keyword = CustNameTB.Text.Trim();
            string name = "";
            bool isDigitPresent = keyword.Any(c => char.IsDigit(c));
            DBMS dbms = new DBMS();
            if(isDigitPresent == true) // telephone number
            {
                if(keyword.Length == 12)
                {
                    name = dbms.SearchCustomer(keyword, 0);
                }
            }
            else // lastName
            {
                name = dbms.SearchCustomer(keyword, 1);
            }
            if(name == "")
            {
                MessageBox.Show("No Customer found");
            }
            CustomerSearchResult.Text = name;
            customerN = name;
        }

        /*
         *  Function    : Add_To_Cart_Click
         *  Description : This function adds ptoducts to cart
         *      
         *
         *  Parameters  : object sender, RoutedEventArgs e
         *      
         *  Returns     : void
         *      
         */
        private void Add_To_Cart_Click(object sender, RoutedEventArgs e)
        {
            branchN = BranchCB.SelectedItem.ToString();

            if (customerN == "" || customerN == null)
            {
                MessageBox.Show("Cannot add to the Cart without Customer Name");
            }
            else if(stockCB.SelectedItem == null)
            {
                MessageBox.Show("Item is out of stock");
            }
            else
            {

                DBMS dbms = new DBMS();
                if (dbms.AddCart(sku, customerN, productN, branchN, quant, totalP) == 0)
                {
                    MessageBox.Show("Product added to Cart");
                    cartItem++;
                    Cart.Content = "Cart (" + cartItem + ")";
                }
                dbms.UpdateDatabaseQuantity(sku, quant, 0);
                Show_Products_Click(sender, e);
                stockCB.Items.Clear();
                stockCB.SelectedItem = "1";
                CalculatedTotal.Text = "";
            }
            
        }

        /*
         *  Function    : New_Customer_Click
         *  Description : This function adds new customer from the sales page
         *      
         *
         *  Parameters  : object sender, RoutedEventArgs e
         *      
         *  Returns     : void
         *      
         */
        private void New_Customer_Click(object sender, RoutedEventArgs e)
        {
            Window addCust = new Create_Customer();
            addCust.Show();
            
            
        }

        /*
         *  Function    : ComboBox_Selection_Changed
         *  Description : This function updates the calculated total cost
         *      
         *
         *  Parameters  : object sender, RoutedEventArgs e
         *      
         *  Returns     : void
         *      
         */
        private void ComboBox_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if(stockCB.SelectedItem != null)
            {
                quant = int.Parse(stockCB.SelectedItem.ToString());
                CalculatedTotal.Text = "$" + (pr * quant).ToString();
                totalP = CalculatedTotal.Text;
            }
            
        }

        /*
         *  Function    : Back_To_Main_Click
         *  Description : This function is used to navigate to the main page
         *      
         *
         *  Parameters  : object sender, RoutedEventArgs e
         *      
         *  Returns     : void
         *      
         */
        private void Back_To_Main_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Main_Page());
        }

        private void View_Cart_Btn(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Cart_Page());
        }
    }
}
