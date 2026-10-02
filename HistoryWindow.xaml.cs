using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//      SLG1_CH0
//      SLG1_CH1
//      SLG1_CH3
//      SLG2_CH0
//      SLG2_CH1
//      SLG3_CH0
//      SLG3_CH1
//      SLG3_CH3
//      SLG4_CH0
//      SLG4_CH1


namespace test_FT260
{
    /// <summary>
    /// HistoryWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryWindow : Window
    {

        public List<HistoryData> historyData_file_list;     // ヒストリデータ　ファイルからの読み出し時に使用

        ScottPlot.Plottables.Scatter history_scatter_0;   // ヒストリデータ 0   
        ScottPlot.Plottables.Scatter history_scatter_1;   // ヒストリデータ 1   
        ScottPlot.Plottables.Scatter history_scatter_2;   // ヒストリデータ 2   
        ScottPlot.Plottables.Scatter history_scatter_3;   // ヒストリデータ 3   
        ScottPlot.Plottables.Scatter history_scatter_4;   // ヒストリデータ 4  
        ScottPlot.Plottables.Scatter history_scatter_5;   // ヒストリデータ 5  
        ScottPlot.Plottables.Scatter history_scatter_6;   // ヒストリデータ 6
        ScottPlot.Plottables.Scatter history_scatter_7;   // ヒストリデータ 7   
        ScottPlot.Plottables.Scatter history_scatter_8;   // ヒストリデータ 8   
        ScottPlot.Plottables.Scatter history_scatter_9;   // ヒストリデータ 9                                          // 


        public HistoryWindow()
        {
            InitializeComponent();

            historyData_file_list = new List<HistoryData>(); // ファイルからの読み出し時に使用

        }

        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog();   // ダイアログのインスタンスを生成

            dialog.Filter = "csvファイル (*.csv)|*.csv|全てのファイル (*.*)|*.*";  //  // ファイルの種類を設定

            dialog.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする


            if (dialog.ShowDialog() == false)     // ダイアログを表示する
            {
                return;                          // キャンセルの場合、リターン
            }


            try
            {
                historyData_file_list.Clear();            // ヒストリデータのクリア

                StreamReader sr = new StreamReader(dialog.FileName, Encoding.GetEncoding("SHIFT_JIS"));    //  CSVファイルを読みだし
               
                FileNameTextBox.Text = dialog.FileName;                // ファイル名の表示

                DataMemoTextBox.Text = sr.ReadLine();           // 先頭行の Memoを読み出し、表示

                sr.ReadLine();              // 読み飛ばし (2行目は、日時、ch名の項目名のため)

                while (!sr.EndOfStream)     // ファイル最終行まで、繰り返し
                {
                    HistoryData historyData = new HistoryData();        // 読み出しデータを格納するクラス

                    string line = sr.ReadLine();        // 1行の読み出し

                    string[] items = line.Split(',');       // 1行を、,(カンマ)毎に items[]に格納 

                    DateTime dateTime;
                    DateTime.TryParse(items[0], out dateTime);  // 日付の文字列を DateTime型へ変換

                    historyData.dt = dateTime.ToOADate();       // DateTiem型を double型へ変換


                    double.TryParse(items[1], out double d0); // ch0の値　文字列を double型へ変換
                    historyData.data0 = d0;                   // クラスのメンバーへ格納

                    double.TryParse(items[2], out double d1); // ch1の値　文字列を double型へ変換
                    historyData.data1 = d1;                   // クラスのメンバーへ格納

                    double.TryParse(items[3], out double d2); // ch2の値　文字列を double型へ変換
                    historyData.data2 = d2;

                    double.TryParse(items[4], out double d3); // ch3の値　文字列を double型へ変換
                    historyData.data3 = d3;                   // クラスのメンバーへ格納

                    double.TryParse(items[5], out double d4); // ch4の値　文字列を double型へ変換
                    historyData.data4 = d4;

                    double.TryParse(items[6], out double d5); // ch5の値　文字列を double型へ変換
                    historyData.data5 = d5;                   // クラスのメンバーへ格納

                    double.TryParse(items[7], out double d6); // ch6の値　文字列を double型へ変換
                    historyData.data6 = d6;                   // クラスのメンバーへ格納

                    double.TryParse(items[8], out double d7); // ch7の値　文字列を double型へ変換
                    historyData.data7 = d7;                   // クラスのメンバーへ格納

                    double.TryParse(items[9], out double d8); // ch8の値　文字列を double型へ変換
                    historyData.data8 = d8;

                    double.TryParse(items[10], out double d9); // ch9の値　文字列を double型へ変換
                    historyData.data9 = d9;                   // クラスのメンバーへ格納

                  
                    historyData_file_list.Add(historyData);      // Listへ追加
                }

                disp_history_graph();       // ヒストリトレンドデータのグラフ表示

                set_check_box_true();       // チェックボックスをtrue
            }

            catch (Exception ex) when (ex is IOException || ex is IndexOutOfRangeException)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        //
        //  ヒストリトレンドデータのグラフ表示
        //
    
        //
        private void disp_history_graph()
        {
            wpfPlot_History.Plot.Clear();
            wpfPlot_History_AD.Plot.Clear();

            int cnt_max = historyData_file_list.Count;       // 行数分の配列

            double[] t_data0 = new double[cnt_max];   // データ 0  
            double[] t_data1 = new double[cnt_max];   // データ 1  
            double[] t_data2 = new double[cnt_max];   // データ 2  
            double[] t_data3 = new double[cnt_max];   // データ 3  
            double[] t_data4 = new double[cnt_max];   // データ 4  
            double[] t_data5 = new double[cnt_max];   // データ 5  
            double[] t_data6 = new double[cnt_max];   // データ 6  
            double[] t_data7 = new double[cnt_max];   // データ 7  
            double[] t_data8 = new double[cnt_max];   // データ 8  
            double[] t_data9 = new double[cnt_max];   // データ 9  

            double[] t_dt = new double[cnt_max];       //  date time


            for (int i = 0; i < cnt_max; i++)                   // List化された、historyDataクラスの情報をグラフ表示用の配列にコピー 
            {
                t_data0[i] = historyData_file_list[i].data0;       // SLG1_CH0
                t_data1[i] = historyData_file_list[i].data1;       // SLG1_CH1
                t_data2[i] = historyData_file_list[i].data2;       // SLG1_CH3
                t_data3[i] = historyData_file_list[i].data3;       // SLG2_CH0
                t_data4[i] = historyData_file_list[i].data4;       // SLG2_CH1
                t_data5[i] = historyData_file_list[i].data5;       // SLG3_CH0
                t_data6[i] = historyData_file_list[i].data6;       // SLG3_CH1
                t_data7[i] = historyData_file_list[i].data7;       // SLG4_CH0
                t_data8[i] = historyData_file_list[i].data8;       // SLG4_CH1
                t_data9[i] = historyData_file_list[i].data9;       // SLG4_CH3

                t_dt[i] = historyData_file_list[i].dt;           // data tiem
            }

            
            history_scatter_0 = wpfPlot_History.Plot.Add.Scatter(t_dt, t_data0, ScottPlot.Colors.DarkCyan); // SLG1 CH0 (上のグラフ)
            history_scatter_1 = wpfPlot_History.Plot.Add.Scatter(t_dt, t_data1, ScottPlot.Colors.DarkRed);  // SLG1 CH1 (上のグラフ)
            history_scatter_2 = wpfPlot_History.Plot.Add.Scatter(t_dt, t_data2, ScottPlot.Colors.Green);    // SLG1 CH3 (上のグラフ)
            history_scatter_3 = wpfPlot_History.Plot.Add.Scatter(t_dt, t_data3, ScottPlot.Colors.DarkOrange);  // SLG2 CH0 (上のグラフ)
            history_scatter_4 = wpfPlot_History.Plot.Add.Scatter(t_dt, t_data4, ScottPlot.Colors.DarkMagenta); // SLG2 CH1 (上のグラフ)

            history_scatter_5 = wpfPlot_History_AD.Plot.Add.Scatter(t_dt, t_data5, ScottPlot.Colors.DarkCyan); // SLG3 CH0 (上のグラフ)
            history_scatter_6 = wpfPlot_History_AD.Plot.Add.Scatter(t_dt, t_data6, ScottPlot.Colors.DarkRed);  // SLG3 CH1 (上のグラフ)
            history_scatter_7 = wpfPlot_History_AD.Plot.Add.Scatter(t_dt, t_data7, ScottPlot.Colors.Green);    // SLG3 CH3 (上のグラフ)
            history_scatter_8 = wpfPlot_History_AD.Plot.Add.Scatter(t_dt, t_data8, ScottPlot.Colors.DarkOrange);  // SLG4 CH0 (下のグラフ)
            history_scatter_9 = wpfPlot_History_AD.Plot.Add.Scatter(t_dt, t_data9, ScottPlot.Colors.DarkMagenta); // SLG4 CH1 (下のグラフ)


            wpfPlot_History.UserInputProcessor.IsEnabled = true;     // マウスによるパン(グラフの移動)、ズーム(グラフの拡大、縮小)の操作許可
            wpfPlot_History_AD.UserInputProcessor.IsEnabled = true;


            Axis_make_history();    // 軸の作成


            // 凡例の表示
            // 参考:scottplot.net/cookbook/5.0/Legend/
            //
            wpfPlot_History.Plot.Legend.FontSize = 24;
            wpfPlot_History_AD.Plot.Legend.FontSize = 24;

            history_scatter_0.LegendText = "SLG1-CH0";
            history_scatter_1.LegendText = "SLG1-CH1";
            history_scatter_2.LegendText = "SLG1-CH3";

            history_scatter_3.LegendText = "SLG2-CH0";
            history_scatter_4.LegendText = "SLG2-CH1";

            history_scatter_5.LegendText = "SLG3-CH0";
            history_scatter_6.LegendText = "SLG3-CH1";
            history_scatter_7.LegendText = "SLG3-CH3";

            history_scatter_8.LegendText = "SLG4-CH0";
            history_scatter_9.LegendText = "SLG4-CH1";


            wpfPlot_History.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Vertical);

            wpfPlot_History_AD.Plot.HideLegend();
            wpfPlot_History_AD.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Vertical);


            wpfPlot_History.Refresh();       // Refresh (上のグラフ)
            wpfPlot_History_AD.Refresh();    // Refresh (下のグラフ)

        }


        //
        // 　軸の作成 
        //
        private void Axis_make_history()
        {

            wpfPlot_History.Plot.Axes.SetLimits();             // 上のグラフ
            wpfPlot_History.Plot.Axes.DateTimeTicksBottom();  //  tell the plot to display dates on the bottom axis

            wpfPlot_History.Plot.Axes.Bottom.TickLabelStyle.FontName = "MS UI Gothic";  // X軸　目盛りのフォント名
            wpfPlot_History.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;      //  X軸　目盛りのフォントサイズ

            wpfPlot_History.Plot.Axes.Left.TickLabelStyle.FontName = "MS UI Gothic";  // X軸　目盛りのフォント名
            wpfPlot_History.Plot.Axes.Left.TickLabelStyle.FontSize = 24;        //  Y軸(左側)　目盛りのフォントサイズ

            wpfPlot_History_AD.Plot.Axes.SetLimits();          　// 下のグラフ
            wpfPlot_History_AD.Plot.Axes.DateTimeTicksBottom();  //  tell the plot to display dates on the bottom axis

            wpfPlot_History_AD.Plot.Axes.Bottom.TickLabelStyle.FontName = "MS UI Gothic";  // X軸　目盛りのフォント名
            wpfPlot_History_AD.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;      //  X軸　目盛りのフォントサイズ
            
            wpfPlot_History_AD.Plot.Axes.Left.TickLabelStyle.FontName = "MS UI Gothic"; // Y軸(左側)　目盛りのフォント名
            wpfPlot_History_AD.Plot.Axes.Left.TickLabelStyle.FontSize = 24;        //  Y軸(左側)　目盛りのフォントサイズ

            set_y_axes_label_history();                         // 上のグラフ用 Y軸(左側、右側)のラベル.
            set_y_axes_label_ad_history();                      // 下のグラフ用 Y軸

        }

        // 上のグラフ用
        //  Y軸(左)のラベル
        //　左|                |右
        // 　C|                |
        //    |                |
        //    +----------------+
        //
        private void set_y_axes_label_history()
        {
            wpfPlot_History.Plot.Axes.Left.Label.FontName = "MS UI Gothic";      // Y軸(左側) ラベルのフォント名
            wpfPlot_History.Plot.Axes.Left.Label.FontSize = 24;               // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_History.Plot.Axes.Left.Label.Text = "C";                // Y軸(左側) ラベル

        }

        // 下のグラフ用
        //  Y軸(左)のラベル
        //　左|                |右
        //   C|                |
        //    |                |
        //    +----------------+
        private void set_y_axes_label_ad_history()
        {
            wpfPlot_History_AD.Plot.Axes.Left.Label.FontName = "MS UI Gothic";    // Y軸(左側) ラベルのフォント名
            wpfPlot_History_AD.Plot.Axes.Left.Label.FontSize = 24;             // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_History_AD.Plot.Axes.Left.Label.Text = "C";         // Y軸(左側) ラベル
        }


        // チェックボックスによるトレンド線の表示 

        private void CH_N_Show(object sender, RoutedEventArgs e)
        {

            if (history_scatter_0 is null) return;
            if (history_scatter_1 is null) return;
            if (history_scatter_2 is null) return;
            if (history_scatter_3 is null) return;
            if (history_scatter_4 is null) return;
            if (history_scatter_5 is null) return;
            if (history_scatter_6 is null) return;
            if (history_scatter_7 is null) return;
            if (history_scatter_8 is null) return;
            if (history_scatter_9 is null) return;


            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                history_scatter_0.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                history_scatter_1.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                history_scatter_2.IsVisible = true;
            }

            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                history_scatter_3.IsVisible = true;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                history_scatter_4.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                history_scatter_5.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                history_scatter_6.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                history_scatter_7.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                history_scatter_8.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                history_scatter_9.IsVisible = true;
            }


            wpfPlot_History.Refresh();       // Refresh (上のグラフ)
            wpfPlot_History_AD.Refresh();    // Refresh (下のグラフ)

        }

        // チェックボックスによるトレンド線の非表示
        private void CH_N_Hide(object sender, RoutedEventArgs e)
        {
            if (history_scatter_0 is null) return;
            if (history_scatter_1 is null) return;
            if (history_scatter_2 is null) return;
            if (history_scatter_3 is null) return;
            if (history_scatter_4 is null) return;
            if (history_scatter_5 is null) return;
            if (history_scatter_6 is null) return;
            if (history_scatter_7 is null) return;
            if (history_scatter_8 is null) return;
            if (history_scatter_9 is null) return;


            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                history_scatter_0.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                history_scatter_1.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                history_scatter_2.IsVisible = false;
            }

            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                history_scatter_3.IsVisible = false;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                history_scatter_4.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                history_scatter_5.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                history_scatter_6.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                history_scatter_7.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                history_scatter_8.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                history_scatter_9.IsVisible = false;
            }
            

            wpfPlot_History.Refresh();       // Refresh (上のグラフ)
            wpfPlot_History_AD.Refresh();    // Refresh (下のグラフ)
        }


        //  グラフデータのオープン時には、
        //  表示するグラフのチェックボックスを全てチェック済みにする。
        private void set_check_box_true()
        {
           
            SLG1_CH0_CheckBox.IsChecked = true;
            SLG1_CH1_CheckBox.IsChecked = true;
            SLG1_CH3_CheckBox.IsChecked = true;
            SLG2_CH0_CheckBox.IsChecked = true;
            SLG2_CH1_CheckBox.IsChecked = true;

            SLG3_CH0_CheckBox.IsChecked = true;
            SLG3_CH1_CheckBox.IsChecked = true;
            SLG3_CH3_CheckBox.IsChecked = true;
            SLG4_CH0_CheckBox.IsChecked = true;
            SLG4_CH1_CheckBox.IsChecked = true;
        }



    }
}
