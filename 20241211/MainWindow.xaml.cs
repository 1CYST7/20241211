﻿using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _20241211
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string aqiURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=e8dd42e6-9b8b-43f8-991e-b3dee723a52d&limit=1000&sort=ImportDate%20desc&format=JSON";
    
        public MainWindow()
        {
            InitializeComponent();
            urlTextBox.Text = aqiURL;  // 設定TextBox中的預設URL為`aqiURL`。
        }

        private async void btnGetAQI_Click(object sender, RoutedEventArgs e)
        {
            string url = urlTextBox.Text;  // 取得TextBox中的URL。
            ContentTextBox.Text = "抓取資料中...";  // 在ContentTextBox顯示"抓取資料中..."提示文字。

            string data = await GetAQIAsync(url);  // 異步呼叫GetAQIAsync方法來抓取AQI數據。
            ContentTextBox.Text = data;  // 將抓取到的AQI數據顯示在ContentTextBox中。
        }
        // 定義一個異步方法來從指定的URL抓取AQI數據。
        private async Task<string> GetAQIAsync(string url)
        {
            // 使用HttpClient發送GET請求
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);  // 發送GET請求，並等待響應。
                var content = await response.Content.ReadAsStringAsync();  // 以字串格式讀取響應內容。
                return content;  // 返回響應內容，即AQI數據。
            }
        }
    }
}