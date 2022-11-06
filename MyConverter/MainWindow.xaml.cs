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
using System.Xml;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace MyConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Parse();
            CreateBD();
            AddBD();
        }
        private String dbFileName;
        private SQLiteConnection m_dbConn;
        private SQLiteCommand m_sqlCmd;

        public void CreateBD()
        {
            m_dbConn = new SQLiteConnection();
            m_sqlCmd = new SQLiteCommand();

            dbFileName = "BD.sqlite";
            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);
            try
            {
                m_dbConn = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                m_dbConn.Open();
                m_sqlCmd.Connection = m_dbConn;

                m_sqlCmd.CommandText = "CREATE TABLE IF NOT EXISTS Catalog (Name TEXT,Ed INTEGER, Val TEXT, Course FLOAT, Date TEXT)";
                m_sqlCmd.ExecuteNonQuery();

            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void AddBD()
        {
            if (m_dbConn.State != ConnectionState.Open)
            {
                MessageBox.Show("Open connection with database");
                return;
            }
            {
                try
                {
                    m_sqlCmd.CommandText = "DELETE FROM Catalog";
                    m_sqlCmd.ExecuteNonQuery();

                    for (int i = 0; i < names.Count; i++)

                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        m_sqlCmd.CommandText = "INSERT INTO Catalog ('Name', 'Ed','Val',Course, Date) values ('" +
                          charcode[i] + "' , '" +
                          nominal[i] + "' , '" + names[i] + "' , '" + course[i] + "' , '" + date + "')";
                        m_sqlCmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        List<string> htmls = new List<string>();
        List<decimal> course = new List<decimal>();
        List<string> names = new List<string>();
        List<int> nominal = new List<int>();
        List<string> charcode = new List<string>();
        string date;
        private void TextBoxWork()
        {
            TextBox4.Text = "";
            int index1 = -1, index2 = -1;
            double n;
            if (TextBox1.Text.Length > 0 && TextBox3.Text.Length > 0 && double.TryParse(TextBox2.Text, out n))
            {
                string charcodes1 = TextBox1.Text.ToUpper();
                string name1 = TextBox1.Text.ToLower();
                for (int i = 0; i < charcode.Count; i++)
                {
                    if (charcodes1 == charcode[i] || name1 == names[i].ToLower())
                    {
                        index1 = i;
                    }

                }

                string charcodes2 = TextBox3.Text.ToUpper();
                string name2 = TextBox3.Text.ToLower();
                for (int i = 0; i < charcode.Count; i++)
                {
                    if (charcodes2 == charcode[i] || name2 == names[i].ToLower())
                    {
                        index2 = i;
                    }

                }

                if (index1 > -1 && index2 > -1 && double.TryParse(TextBox2.Text, out n))
                {
                    double mathindex = Translate(index1, index2);
                    TextBox4.Text = Math.Round((mathindex * n), 5).ToString();
                }
            }
        }
        private void Parse()
        {
            try
            {
                //Инициализируем объекта типа XmlTextReader и
                //загружаем XML документ с сайта центрального банка
                XmlTextReader reader = new XmlTextReader("http://www.cbr.ru/scripts/XML_daily.asp");
                //В эти переменные будем сохранять куски XML
                //с определенными валютами (Euro, USD)
                //Перебираем все узлы в загруженном документе
                while (reader.Read())
                {
                    if (reader.Name == "ValCurs")
                    {
                        date = Convert.ToString(reader.GetAttribute("Date"));
                        Console.WriteLine(date);
                    }
                    //Проверяем тип текущего узла
                    switch (reader.NodeType)
                    {
                        //Если этого элемент Valute, то начинаем анализировать атрибуты
                        case XmlNodeType.Element:

                            if (reader.Name == "Valute")
                            {
                                if (reader.HasAttributes)
                                {
                                    //Метод передвигает указатель к следующему атрибуту
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name == "ID")
                                        {
                                            //Если значение атрибута равно R01235, то перед нами информация о курсах

                                            //Возвращаемся к элементу, содержащий текущий узел атрибута
                                            reader.MoveToElement();
                                            //Считываем содержимое дочерних узлом
                                            htmls.Add(reader.ReadOuterXml());
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }
                //Из выдернутых кусков XML кода создаем новые XML документы
                for (int i = 0; i < htmls.Count; i++)
                {
                    XmlDocument XmlDocument = new XmlDocument();
                    XmlDocument.LoadXml(htmls[i]);
                    //Метод возвращает узел, соответствующий выражению XPath
                    XmlNode xmlNode = XmlDocument.SelectSingleNode("Valute/Value");
                    //Считываем значение и конвертируем в decimal. Курс валют получен
                    course.Add(Convert.ToDecimal(xmlNode.InnerText));
                    //Названия
                    XmlNode xmlNames = XmlDocument.SelectSingleNode("Valute/Name");
                    names.Add(Convert.ToString(xmlNames.InnerText));
                    //Номинал
                    XmlNode xmlNominal = XmlDocument.SelectSingleNode("Valute/Nominal");
                    nominal.Add(Convert.ToInt32(xmlNominal.InnerText));
                    //CharCode
                    XmlNode xmlCharCode = XmlDocument.SelectSingleNode("Valute/CharCode");
                    charcode.Add(Convert.ToString(xmlCharCode.InnerText));
                }
                course.Add(1);
                names.Add("Российский рубль");
                nominal.Add(1);
                charcode.Add("RUB");
            }
            catch
            {
                try
                {
                    MessageBox.Show("Отстутсвует подключение к интернету. В приложении будут использоваться данные с предыдущего запуска.");
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
                catch
                {
                    MessageBox.Show("Необходимо подключение к интернету.");
                }
            }
        }


        private double Translate(int index1, int index2)
        {
            double course1 = Convert.ToDouble(course[index1]);
            double course2 = Convert.ToDouble(course[index2]);
            int nominal1 = nominal[index1];
            int nominal2 = nominal[index2];
            double mathindex = (course1 / nominal1) / (course2 / nominal2);
            return mathindex;
        }
        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxWork();

        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxWork();
        }

        private void TextBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxWork();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Конвертер валют - инструмент, который позволит вам рассчитать соотношения актуальных курсов денежных средств на текущий или прошлые дни. Курсы загружаются с сайта 'Центрального банка РФ'.");
        }

        

        private void TextBox2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
            
        }
    }
}
