using _2024_WpfApp7;
using LiveCharts;
using LiveCharts.Wpf;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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

        AQIdata aqiData = new AQIdata();  // 用於儲存AQI數據。
        List<Field> fields = new List<Field>();  // 儲存AQI數據的字段（可能是空氣質量的不同屬性）。
        List<Record> records = new List<Record>();  // 儲存AQI的記錄數據。
        SeriesCollection seriesCollection = new SeriesCollection();
        List<Record> selectedRecords = new List<Record>();  // 儲存選中的記錄數據。
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
            aqiData = JsonSerializer.Deserialize<AQIdata>(data);
            fields = aqiData.fields.ToList();
            records = aqiData.records.ToList();
            selectedRecords = records;
            statusBarText.Text = $"共有{records.Count}筆資料";

            DisplayAQIData();
        }
        private void DisplayAQIData()
        {
            RecordDataGrid.ItemsSource = records;  // 將`records`數據綁定到DataGrid顯示。

            Record record = records[0];  // 取出第一條記錄。
            DataWrapPanel.Children.Clear();  // 清空DataWrapPanel中的內容。

            foreach (Field field in fields)
            {
                var propertyInfo = record.GetType().GetProperty(field.id);  // 通過反射獲取字段屬性。
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(record) as string;  // 獲取該字段的值。
                    if (double.TryParse(value, out double v))  // 嘗試將值轉為數字。
                    {
                        CheckBox cb = new CheckBox
                        {
                            Content = field.info.label,  // 設定CheckBox顯示的內容為字段的label。
                            Tag = field.id,  // 設置CheckBox的Tag為字段的id。
                            Margin = new Thickness(3),
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Width = 120
                        };
                        cb.Checked += UpdateChart;
                        cb.Unchecked += UpdateChart;
                        DataWrapPanel.Children.Add(cb);  // 將CheckBox加入DataWrapPanel。
                    }
                }
            }
        }
        private void UpdateChart(object sender, RoutedEventArgs e)
        {
            seriesCollection.Clear();

            foreach (CheckBox cb in DataWrapPanel.Children)
            {
                if (cb.IsChecked == true)
                {
                    List<string> labels = new List<string>();
                    string tag = cb.Tag as string;
                    ColumnSeries columnSeries = new ColumnSeries();
                    ChartValues<double> values = new ChartValues<double>();

                    foreach (Record r in selectedRecords)
                    {
                        var propertyInfo = r.GetType().GetProperty(tag);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(r) as string;
                            if (double.TryParse(value, out double v))
                            {
                                labels.Add(r.sitename);
                                values.Add(v);
                            }
                        }
                    }
                    columnSeries.Values = values;
                    columnSeries.Title = tag;
                    columnSeries.LabelPoint = point => $"{labels[(int)point.X]}:{point.Y.ToString()}";
                    seriesCollection.Add(columnSeries);
                }
            }
            AQIChart.Series = seriesCollection;
        }
        // 定義一個異步方法來從指定的URL抓取AQI數據。
        private async Task<string> GetAQIAsync(string url)
        {
            // 使用HttpClient發送GET請求
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);  // 發送GET請求，並等待響應。
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    statusBarText.Text = $"Error: {response.StatusCode}";
                    return null;
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
            }
        
        }

        private void RecordDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void RecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecords = RecordDataGrid.SelectedItems.Cast<Record>().ToList();
            statusBarText.Text = $"共有{selectedRecords.Count}筆資料";
        }
    }
}