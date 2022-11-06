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
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace MyConverter
{
    public partial class Window1 : Window
    {

        private String dbFileName;
        private SQLiteConnection m_dbConn;
        List<decimal> course = new List<decimal>();
        List<string> names = new List<string>();
        List<int> nominal = new List<int>();
        List<string> charcode = new List<string>();
        string date;
        public Window1()
        {
            InitializeComponent();
            OpenBD();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Label.Content = date;
            List<MyTable> result = new List<MyTable>();
            for (int i = 0; i < names.Count; i++)
            {
                result.Add(new MyTable(names[i], nominal[i], charcode[i], course[i]));
            }
            DataGrid.ItemsSource = result;

        }

        private void OpenBD()
        {
            dbFileName = "BD.sqlite";
            m_dbConn = new SQLiteConnection("Data Source =" + dbFileName + ";Version = 3;");
            m_dbConn.Open();
            SQLiteCommand m_sqlCmd = new SQLiteCommand();
            m_sqlCmd.Connection = m_dbConn;
            m_sqlCmd.CommandText = "SELECT * FROM Catalog";
            SQLiteDataReader name = m_sqlCmd.ExecuteReader();
            if (name.HasRows)
            {
                while (name.Read())
                {
                    names.Add(name["Name"].ToString());
                    course.Add(Convert.ToDecimal(name["Course"]));
                    charcode.Add(name["Val"].ToString());
                    nominal.Add(Convert.ToInt32(name["Ed"]));
                    date = name["Date"].ToString();
                }
            }

        }
    }

    public class MyTable
    {
        public string Name { get; set; }
        public int Ed { get; set; }
        public string Val { get; set; }
        public decimal Course { get; set; }
        public MyTable(string Nam, int E, string Va, decimal Cours)
        {
            Name = Nam;
            Ed = E;
            Val = Va;
            Course = Cours;
        }
    }
}
