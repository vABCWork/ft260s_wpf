using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Colormaps;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace test_FT260
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    // 履歴(ヒストリ)データ　クラス
    // クラス名: HistoryData
    // メンバー:  double  data0　　SLG1_CH0
    //            double  data1    SLG1_CH1
    //            double  data2    SLG1_CH3
    //            double  data3    SLG2_CH0
    //            double  data4    SLG2_CH1
    //            double  data5    SLG3_CH0
    //            double  data6    SLG3_CH1
    //            double  data7    SLG3_CH3
    //            double  data8    SLG4_CH0
    //            double  data9    SLG4_CH1
    //            double  dt
    //

    public class HistoryData
    {
        public double data0 { get; set; }       // SLG1_CH0
        public double data1 { get; set; }       // SLG1_CH1
        public double data2 { get; set; }       // SLG1_CH3
        public double data3 { get; set; }       // SLG2_CH0
        public double data4 { get; set; }       // SLG2_CH1
        public double data5 { get; set; }       // SLG3_CH0
        public double data6 { get; set; }       // SLG3_CH1
        public double data7 { get; set; }       // SLG3_CH3
        public double data8 { get; set; }       // SLG4_CH0
        public double data9 { get; set; }       // SLG4_CH1

        public double dt { get; set; }         // 日時 (double型)
    }

    // 各SLGの各データ用のクラス定義
    public class SlgDataClass
    {
        public Byte I2c_address {  get; set; } // I2C アドレス(7bit)
        public UInt16 Buffer0 { get; set; }  // DataBuffer0 resultの内容
        public UInt16 Buffer1 { get; set; }  // DataBuffer1
        public UInt16 Buffer3 { get; set; }   // DataBuffer3

        public float Ch0_thermovolt { get; set; } // ch0 熱起電力[mV] (熱電対の熱起電力)
        public float Ch1_thermovolt { get; set; } // ch1 熱起電力[mV] (熱電対の熱起電力)
        public float Ch3_thermovolt { get; set; } // ch3 熱起電力[mV] (サーミスタでの測定温度を熱起電力に換算した値)

        public float Ch0_temp {  get; set; } // ch0 温度[℃]（熱電対による測定温度)
        public float Ch1_temp {  get; set; } // ch1 温度[℃] (熱電対による測定温度)
        public float Ch3_temp { get; set; }  // ch3 温度[℃] (サーミスタ測定温度)(冷接点補償用)

        // コンストラクタ
        public SlgDataClass(Byte i2c_address,UInt16 buffer0,UInt16 buffer1,UInt16 buffer3,
                            float ch0_thermovolt,float ch1_thermovolt,float ch3_thermovolt,
                            float ch0_temp,float ch1_temp,float ch3_temp)
        {
            I2c_address = i2c_address;
            Buffer0 = buffer0;
            Buffer1 = buffer1;
            Buffer3 = buffer3;
            Ch0_thermovolt = ch0_thermovolt;
            Ch1_thermovolt = ch1_thermovolt;
            Ch3_thermovolt = ch3_thermovolt;
            Ch0_temp = ch0_temp;
            Ch1_temp = ch1_temp;
            Ch3_temp = ch3_temp;
        }
    }



    public partial class MainWindow : Window
    {
        private enum FT260_STATUS
        {
            FT260_OK,
            FT260_INVALID_HANDLE,
            FT260_DEVICE_NOT_FOUND,
            FT260_DEVICE_NOT_OPENED,
            FT260_DEVICE_OPEN_FAIL,
            FT260_DEVICE_CLOSE_FAIL,
            FT260_INCORRECT_INTERFACE,
            FT260_INCORRECT_CHIP_MODE,
            FT260_DEVICE_MANAGER_ERROR,
            FT260_IO_ERROR,
            FT260_INVALID_PARAMETER,
            FT260_NULL_BUFFER_POINTER,
            FT260_BUFFER_SIZE_ERROR,
            FT260_UART_SET_FAIL,
            FT260_RX_NO_DATA,
            FT260_GPIO_WRONG_DIRECTION,
            FT260_INVALID_DEVICE,
            FT260_INVALID_OPEN_DRAIN_SET,
            FT260_INVALID_OPEN_DRAIN_RESET,
            FT260_I2C_READ_FAIL,
            FT260_OTHER_ERROR
        };

        private enum FT260_I2C_FLAG
        {
            FT260_I2C_NONE = 0,
            FT260_I2C_START = 0x02,
            FT260_I2C_REPEATED_START = 0x03,
            FT260_I2C_STOP = 0x04,
            FT260_I2C_START_AND_STOP = 0x06
        };

        // LIBFT260_API FT260_STATUS WINAPI FT260_CreateDeviceList(LPDWORD lpdwNumDevs);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_CreateDeviceList(ref UInt32 lpdwNumDevs);

        // LIBFT260_API FT260_STATUS WINAPI FT260_OpenByVidPid(WORD vid, WORD pid, DWORD deviceIndex, FT260_HANDLE* pFt260Handle);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_OpenByVidPid(ushort vid, ushort pid, UInt32 deviceIndex, ref IntPtr ft260handle);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_Init(FT260_HANDLE ft260Handle, uint32 kbps);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_Init(IntPtr ft260Handle, UInt32 kbps);

           // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_Write(FT260_HANDLE ft260Handle, uint8 deviceAddress, FT260_I2C_FLAG flag, LPVOID lpBuffer, DWORD dwBytesToWrite, LPDWORD lpdwBytesWritten);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_Write(IntPtr ft260Handle, uint deviceAddress, FT260_I2C_FLAG flag, byte[] wrBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_GetStatus(FT260_HANDLE ft260Handle, uint8* status);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_GetStatus(IntPtr ft260Handle, ref byte status);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_WriteAndMonitorStatus(FT260_HANDLE ft260Handle, uint8 deviceAddress, FT260_I2C_FLAG flag, LPVOID lpBuffer, DWORD dwBytesToWrite, LPDWORD lpdwBytesWritten, uint8* status);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_WriteAndMonitorStatus(IntPtr ft260Handle, uint deviceAddress, FT260_I2C_FLAG flag, byte[] wrBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten, ref byte statuis);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_Read(FT260_HANDLE ft260Handle, uint8 deviceAddress, FT260_I2C_FLAG flag, LPVOID lpBuffer, DWORD dwBytesToRead, LPDWORD lpdwBytesReturned, timeout);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_Read(IntPtr ft260Handle, uint deviceAddress, FT260_I2C_FLAG flag, byte[] rdBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned, UInt32 Timeout);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_ReadAndMonitorStatus(FT260_HANDLE handle, uint8 deviceAddress, FT260_I2C_FLAG flag, LPVOID lpBuffer, DWORD dwBytesToRead, LPDWORD lpdwBytesReturned, uint8* status, DWORD wait_timer = 5000);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_ReadAndMonitorStatus(IntPtr ft260Handle, uint deviceAddress, FT260_I2C_FLAG flag, byte[] rdBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesReturned, ref byte status, UInt32 Timeout);

        // LIBFT260_API FT260_STATUS WINAPI FT260_I2CMaster_Reset(FT260_HANDLE ft260Handle);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_I2CMaster_Reset(IntPtr ft260Handle);

        //LIBFT260_API FT260_STATUS WINAPI FT260_Close(FT260_HANDLE ft260Handle);
        [DllImport("LibFT260.dll")]
        private static extern FT260_STATUS FT260_Close(IntPtr ft260Handle);

        const int DEFAULT_VID = 0x0403;  // FT260のベンダーID
        const int DEFAULT_PID = 0x6030;  // FT240のプロダクトID

        const int BYTES_TO_WRITE = 2;
        const int BYTES_TO_READ = 2;
        static byte[] i2cTxData = new byte[BYTES_TO_WRITE];
        static byte[] i2cRxData = new byte[BYTES_TO_READ];

        IntPtr ft260handle = new IntPtr();
        FT260_STATUS Status = 0;

        uint trend_data_item_max;             // 各リアルタイム　トレンドデータの保持数 
        double[] trend_data0;                 // トレンドデータ 0  SLG1_CH0
        double[] trend_data1;                 // トレンドデータ 1  SLG1_CH1
        double[] trend_data2;                 // トレンドデータ 2  SLG1_CH3
        double[] trend_data3;                 // トレンドデータ 3  SLG2_CH0
        double[] trend_data4;                 // トレンドデータ 4  SLG2_CH1
        double[] trend_data5;                 // トレンドデータ 5  SLG3_CH0
        double[] trend_data6;                 // トレンドデータ 6  SLG3_CH1
        double[] trend_data7;                 // トレンドデータ 7 SLG3_CH3
        double[] trend_data8;                 // トレンドデータ 8 SLG4_CH0
        double[] trend_data9;                 // トレンドデータ 9 SLG4_CH1

        double[] trend_dt;                    // トレンドデータ　収集日時

        ScottPlot.Plottables.Scatter trend_scatter_0; // トレンドデータ0  
        ScottPlot.Plottables.Scatter trend_scatter_1; // トレンドデータ1  
        ScottPlot.Plottables.Scatter trend_scatter_2; // トレンドデータ2  
        ScottPlot.Plottables.Scatter trend_scatter_3; // トレンドデータ3  
        ScottPlot.Plottables.Scatter trend_scatter_4; // トレンドデータ4
        ScottPlot.Plottables.Scatter trend_scatter_5; // トレンドデータ5                                             //
        ScottPlot.Plottables.Scatter trend_scatter_6; // トレンドデータ6  
        ScottPlot.Plottables.Scatter trend_scatter_7; // トレンドデータ7  
        ScottPlot.Plottables.Scatter trend_scatter_8; // トレンドデータ8  
        ScottPlot.Plottables.Scatter trend_scatter_9; // トレンドデータ9


        public List<HistoryData> historyData_list;          // ヒストリデータ　データ収集時に使用


        double y_axis_top;                      // Y軸 温度目盛りの上限値
        double y_axis_bottom;                   // Y軸 温度目盛りの下限値

        DispatcherTimer SendIntervalTimer;  // タイマ　モニタ用　電文送信間隔   


        bool DLL_Loaded = false;
        DateTime sendDateTime;   // 送信日時
        DateTime receiveDateTime;   // 受信完了日時

        Byte slg_index;
      
        public SlgDataClass[] slgDatas;     // クラスを配列として扱う


        public MainWindow()
        {
            InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  // Shift-JISコードを使用する処理(フレームワーク .NET10) 

            Start_FT260();      // FT260の開始

            SendIntervalTimer = new System.Windows.Threading.DispatcherTimer();　　// タイマーの生成(定周期モニタ用)
            SendIntervalTimer.Tick += new EventHandler(SendIntervalTimer_Tick);  // タイマーイベント

            SendIntervalTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);         // タイマーイベント発生間隔 1sec(コマンド送信周期)

            //SendIntervalTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);         // タイマーイベント発生間隔 500msec(コマンド送信周期)

            historyData_list = new List<HistoryData>();     // モニタ時のトレンドデータ 記録用　

            Loaded += LoadEvent;      // LoadEvent実行


            slgDatas = new SlgDataClass[4];  // SLG4つ分の配列

            slgDatas[0] = new SlgDataClass(0x00, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f); // インスタンス生成
            slgDatas[1] = new SlgDataClass(0x00, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f); // 
            slgDatas[2] = new SlgDataClass(0x00, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f); // 
            slgDatas[3] = new SlgDataClass(0x00, 0, 0, 0, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f); // 

            slgDatas[0].I2c_address = 0x08; // SLG1のI2Cアドレス(7bit) 
            slgDatas[1].I2c_address = 0x20; // SLG2のI2Cアドレス(7bit) 
            slgDatas[2].I2c_address = 0x28; // SLG3のI2Cアドレス(7bit) 
            slgDatas[3].I2c_address = 0x30; // SLG4のI2Cアドレス(7bit) 

            SLG1_I2C_Address_TextBox.Text = "0x" + slgDatas[0].I2c_address.ToString("x2");  // アドレスの表示
            SLG2_I2C_Address_TextBox.Text = "0x" + slgDatas[1].I2c_address.ToString("x2");  // 
            SLG3_I2C_Address_TextBox.Text = "0x" + slgDatas[2].I2c_address.ToString("x2");  //
            SLG4_I2C_Address_TextBox.Text = "0x" + slgDatas[3].I2c_address.ToString("x2");  // 
        }

        //
        // 要素のレイアウトやレンダリングが完了し、操作を受け入れる準備が整ったときに発生
        //
        private void LoadEvent(object sender, EventArgs e)
        {

            Chart_Ini();    // チャートの初期表示
        }

        //
        // FT260 の開始
        //　 FT260とUSBで接続し、FT260を電源ONにして開始する。
        //
        private void Start_FT260()
        {
            UInt32 NumDev = 0;

            try
            {
                Status = FT260_CreateDeviceList(ref NumDev);   // HIDデバイスリストの作成とHIDのデバイス数を得る
            }
            catch (DllNotFoundException)
            {
                No_DLL();
            }

            if (Status != FT260_STATUS.FT260_OK)
            {
                Staus_Err_Msg();
            }

            HIDNumberTextBox.Text = NumDev.ToString();  // HIDの全デバイス数 (FT260以外も含む) 

            DLL_Loaded = true;

            Status = FT260_OpenByVidPid(DEFAULT_VID, DEFAULT_PID, 0, ref ft260handle);  // Open device

            if (Status != FT260_STATUS.FT260_OK)
            {
                Staus_Err_Msg();
            }

            Status = FT260_I2CMaster_Init(ft260handle, 400);   // I2C open, Clock=400[KHz]

            if (Status != FT260_STATUS.FT260_OK)
            {
                Staus_Err_Msg();
            }


        }


        //　通信テスト用
        private void Test_Send_Button_Click(object sender, RoutedEventArgs e)
        {
              slg_47011_buffer_rd(0);    //　SLG47011(1個目)から読み出し 14[msec]
              slg_temp_cal(0);           //  温度計算
              slg_temp_disp(0);          //  温度表示      

            //slg_47011_buffer_rd(1);    //　SLG47011(2個目)から読み出し
            //slg_temp_cal(1);           //  温度計算
            //slg_temp_disp(1);          //  温度表示      

            // slg_47011_buffer_rd(2);    //　SLG47011(3個目)から読み出し 
            // slg_47011_buffer_rd(3);    //　SLG47011(4個目)から読み出し
        }

        // 定周期モニタ用
        // 
        private void SendIntervalTimer_Tick(object sender, EventArgs e)
        {
            slg_47011_buffer_rd(0);    //　SLG-1 から読み出し 
            slg_temp_cal(0);           //  温度計算
            slg_temp_disp(0);          //  温度表示
                                       //  
            slg_47011_buffer_rd(1);    //　SLG-2 から読み出し 
            slg_temp_cal(1);           //  温度計算
            slg_temp_disp(1);          //  温度表示   

            slg_47011_buffer_rd(2);    //　SLG-3 から読み出し 
            slg_temp_cal(2);           //  温度計算
            slg_temp_disp(2);          //  温度表示
                                       //  
            slg_47011_buffer_rd(3);    //　SLG-4 から読み出し 
            slg_temp_cal(3);           //  温度計算
            slg_temp_disp(3);          //  温度表示   


            Store_History();          // ヒストリデータとして保持
            Chart_update();           // チャートの更新
        
        }
        // モニタの開始
        private void Start_Monitor_Button_Click(object sender, RoutedEventArgs e)
        {
            SendIntervalTimer.Start();   // 定周期　送信用タイマの開始
              
        }

        // モニタの停止
        private void Stop_Monitor_Button_Click(object sender, RoutedEventArgs e)
        {
            SendIntervalTimer.Stop();     // データ収集用コマンド送信タイマー停止
        }

        //
        //  ヒストリデータとして保持
        //
        // クラス名: HistoryData
        // メンバー:  double  data0　　SLG1_CH0
        //            double  data1    SLG1_CH1
        //            double  data2    SLG1_CH3
        //            double  data3    SLG2_CH0
        //            double  data4    SLG2_CH1
        //            double  data5    SLG3_CH0
        //            double  data6    SLG3_CH1
        //            double  data7    SLG3_CH3
        //            double  data8    SLG4_CH0
        //            double  data9    SLG4_CH1
        //            double  dt
        //
        private void Store_History()
        {

            HistoryData historyData = new HistoryData();     // 保存用ヒストリデータ

            historyData.data0 = slgDatas[0].Ch0_temp;
            historyData.data1 = slgDatas[0].Ch1_temp;
            historyData.data2 = slgDatas[0].Ch3_temp;
            historyData.data3 = slgDatas[1].Ch0_temp;
            historyData.data4 = slgDatas[1].Ch1_temp;
            historyData.data5 = slgDatas[2].Ch0_temp;
            historyData.data6 = slgDatas[2].Ch1_temp;
            historyData.data7 = slgDatas[2].Ch3_temp;
            historyData.data8 = slgDatas[3].Ch0_temp;
            historyData.data9 = slgDatas[3].Ch1_temp;

            historyData.dt = receiveDateTime.ToOADate();   // 受信日時を deouble型で格納

            historyData_list.Add(historyData);          // Listへ保持

        }



        //
        //   チャートの更新
        //
        //double[] trend_data0;   トレンドデータ 0  SLG1_CH0
        //double[] trend_data1;   トレンドデータ 1  SLG1_CH1
        //double[] trend_data2;   トレンドデータ 2  SLG1_CH3
        //double[] trend_data3;   トレンドデータ 3  SLG2_CH0
        //double[] trend_data4;   トレンドデータ 4  SLG2_CH1
        //double[] trend_data5;   トレンドデータ 5  SLG3_CH0
        //double[] trend_data6;   トレンドデータ 6  SLG3_CH1
        //double[] trend_data7;   トレンドデータ 7 SLG3_CH3
        //double[] trend_data8;   トレンドデータ 8 SLG4_CH0
        //double[] trend_data9;   トレンドデータ 9 SLG4_CH1
        private void Chart_update()
        {

            // 1スキャン前のデータを移動後、最新のデータを入れる
            Array.Copy(trend_data0, 1, trend_data0, 0, trend_data_item_max - 1);
            trend_data0[trend_data_item_max - 1] = slgDatas[0].Ch0_temp;

            Array.Copy(trend_data1, 1, trend_data1, 0, trend_data_item_max - 1);
            trend_data1[trend_data_item_max - 1] = slgDatas[0].Ch1_temp;

            Array.Copy(trend_data2, 1, trend_data2, 0, trend_data_item_max - 1);
            trend_data2[trend_data_item_max - 1] = slgDatas[0].Ch3_temp;

            Array.Copy(trend_data3, 1, trend_data3, 0, trend_data_item_max - 1);
            trend_data3[trend_data_item_max - 1] = slgDatas[1].Ch0_temp;

            Array.Copy(trend_data4, 1, trend_data4, 0, trend_data_item_max - 1);
            trend_data4[trend_data_item_max - 1] = slgDatas[1].Ch1_temp;

            Array.Copy(trend_data5, 1, trend_data5, 0, trend_data_item_max - 1);
            trend_data5[trend_data_item_max - 1] = slgDatas[2].Ch0_temp;

            Array.Copy(trend_data6, 1, trend_data6, 0, trend_data_item_max - 1);
            trend_data6[trend_data_item_max - 1] = slgDatas[2].Ch1_temp;

            Array.Copy(trend_data7, 1, trend_data7, 0, trend_data_item_max - 1);
            trend_data7[trend_data_item_max - 1] = slgDatas[2].Ch3_temp;

            Array.Copy(trend_data8, 1, trend_data8, 0, trend_data_item_max - 1);
            trend_data8[trend_data_item_max - 1] = slgDatas[3].Ch0_temp;

            Array.Copy(trend_data9, 1, trend_data9, 0, trend_data_item_max - 1);
            trend_data9[trend_data_item_max - 1] = slgDatas[3].Ch1_temp;


            Array.Copy(trend_dt, 1, trend_dt, 0, trend_data_item_max - 1);
            trend_dt[trend_data_item_max - 1] = receiveDateTime.ToOADate();    // 受信日時 double型に変換して、格納


            Axis_make();            // 軸の作成

            wpfPlot_Trend.Refresh();        // データ変更後のリフレッシュ (上のグラフ用)
            wpfPlot_Trend_AD.Refresh();     // データ変更後のリフレッシュ (下のグラフ用)

        }


        //  SLG データによる温度計算  
        //  1つのSLGで 2chの熱電対を持つ。SLGは4つある。(slg_index)
        //
        // 1) サーミスタによる温度を得る。(冷接点補償用)
        // 2)　サーミスタで得た温度に対応する、熱起電力を得る。
        // 3) 熱電対(K type)で発生している熱起電力を得る
        // 4) 2)と3)の熱起電力を足して、その熱起電力に対応する温度を求める。
        //
        // 　入力: slg_index : 0～3
        //
        private void slg_temp_cal(Byte slg_index)
        {
            if ((slg_index == 0) || (slg_index == 2))
            {
                Thermistor_temp_cal(slg_index); // サーミスタの温度計算(AD 14bit)

                Thermistor_thermo_volt(slg_index);  // サーミスタ測定温度の熱起電力
            }

            else if (slg_index == 1)
            {
                slgDatas[slg_index].Ch3_thermovolt = slgDatas[0].Ch3_thermovolt; // slg_index = 1は、slg_index = 0のサーミスタ測定温度を使用
            }
            else if (slg_index == 3)
            {
                slgDatas[slg_index].Ch3_thermovolt = slgDatas[2].Ch3_thermovolt; // slg_index = 3は、slg_index = 2のサーミスタ測定温度を使用
            }


            ThermoCouple_volt(slg_index);  // 熱電対の熱起電力を得る(AD 14bit) 	

            // ch0の処理
            if ( slgDatas[slg_index].Buffer0 < 16000)  // 正常時 (16000 = 0x3e80)
            {
                double temp =  ThermoCouple_Temp(slgDatas[slg_index].Ch0_thermovolt + slgDatas[slg_index].Ch3_thermovolt); //  SLG ch0 温度 [℃]
                slgDatas[slg_index].Ch0_temp = (float)temp;
            }
            else           // 断線エラー発生時、500℃としている。
            {
                slgDatas[slg_index].Ch0_temp = (float)500.0;
            }

            // ch1の処理
            if ( slgDatas[slg_index].Buffer1 < 16000)
            {
                double temp = ThermoCouple_Temp(slgDatas[slg_index].Ch1_thermovolt + slgDatas[slg_index].Ch3_thermovolt); //  SLG ch0 温度 [℃]
                slgDatas[slg_index].Ch1_temp = (float)temp;
            }
            else
            {
                slgDatas[slg_index].Ch1_temp = (float)500.0;
            }

        }




        //
        // サーミスタの温度計算
        //  使用サーミスタ:  NCP15XH103F03RC (村田製作所)
        //                   10K[Ω](at 25℃), B = 3380
        // SimSurfing NTサーミスタ動作シュミレーターで求めた、近似式(3次)を使用.
        //
        //   T = 101.61739* x^3 - 217.83684* x^2 + 206.25610* x - 54.752738
        //  
        //   T:温度[℃], x:測定電圧[V}
        private void Thermistor_temp_cal(Byte index)
        {
            double a;
            double x;
            
            a = slgDatas[index].Buffer3 / 16383.0;

            x = a * 1.62;       // x = 測定電圧

            slgDatas[index].Ch3_temp = (float)(101.61739 * Math.Pow(x, 3) - 217.83684 * Math.Pow(x, 2) + 206.25610 * x - 54.752738); // 温度

            float thermis_temp = slgDatas[index].Ch3_temp;

            if (index == 0)
            {
                SLG1_Ch3_TextBox.Text = thermis_temp.ToString("F1"); // 温度表示
            }
            else if (index == 2)
            {
                SLG3_Ch3_TextBox.Text = thermis_temp.ToString("F1");
            }
                      
        }

        //
        // サーミスタ測定温度から、熱起電力を得る
        //
        //  熱起電力は、熱電対 Kタイプ ( 0～1372[℃])用
        //
        //　多項式: JIS C1602 ( kikakurui.com/c1/C1602-2015-01.html )
        //
        //   E = b0 + b1*t^1 + b2*t^2 + b3*t^3 + b4*t^4 + b5*t^5 + b6*t^6 + b7*t^7 + b8*t^8 + b9*t^9 + c0*Exp(c1* (t - 126.9686)^2) 
        //   t: 温度 [℃]  
        //   E: 熱起電力 [uV]
        //
        //   b0 = -1.76004137E+01
        //   b1 = 3.89212050E+01
        //   b2 = 1.85587700E-02
        //   b3 = -9.94575929E-05
        //   b4 = 3.18409457E-07
        //   b5 = -5.60728449E-10
        //   b6 = 5.60750591E-13
        //   b7 = -3.20207200E-16
        //   b8 = 9.71511472E-20
        //   b9 = -1.21047213E-23
        //
        //   c0 = 1.185976E+02
        //   c1 = -1.183432E-04
        //
        private void Thermistor_thermo_volt(Byte index)
        {
            double t;
            double E;
            double[] b_const;

            b_const = new double[] { -1.76004137E+01, 3.89212050E+01, 1.85587700E-02, -9.94575929E-05, 3.18409457E-07,
                                     -5.60728449E-10, 5.60750591E-13, -3.20207200E-16, 9.71511472E-20,-1.21047213E-23};

            double c0 = 1.185976E+02;
            double c1 = -1.183432E-04;

            t = slgDatas[index].Ch3_temp;           // サーミスタ測定温度
            E = 0;

            for (int i = 0; i < 10; i++)
            {
                E = E + b_const[i] * Math.Pow(t, i);
            }

            double a = c1 * Math.Pow(t - 126.9686, 2);

            E = E + c0 * Math.Exp(a);

            slgDatas[index].Ch3_thermovolt =(float)(E * 0.001);   // [mV]

        }

        //
        // 熱電対の熱起電力を得る
        // AD値             測定電圧       温度
        //    0              -12.65 [mV]
        //  8192 (0x2000)        0 [mV]     0[℃]
        // 16383 (0x3fff)     12.65 [mV]　 310[℃]　
        //
        // 
        private void ThermoCouple_volt(Byte index)
        {
           double ch0_thermo_volt = ((double)(slgDatas[index].Buffer0 - 8192) / 8192) * 12.65625;

           double ch1_thermo_volt = ((double)(slgDatas[index].Buffer1 - 8192) / 8192) * 12.65625;

            slgDatas[index].Ch0_thermovolt = (float)(ch0_thermo_volt);
            slgDatas[index].Ch1_thermovolt = (float)(ch1_thermo_volt);
           
        }

        //  熱起電力から温度を得る
        //   入力: 熱起電力[mV] 
        //   出力: 温度[℃]    
        // 多項式:
        //   T = c0 + c1*E^1 + c2*E^2 + c3*E^3 + c4*E^4 + c5*E^5 + c6*E^6 + c7*E^7 + c8*E^8 + c9*E^9 
        //   T: 温度 [℃]  
        //   E: 熱起電力 [uV]
        //
        //   c0 = 0.0 
        //   c1 = 2.5083550E-02
        //   c2 = 7.8601060E-08
        //   c3 = -2.5031310E-10
        //   c4 = 8.3152700E-14
        //   c5 = -1.2280340E-17
        //   c6 = 9.8040360E-22
        //   c7 =-4.4130300E-26
        //   c8 =  1.0577340E-30
        //   c9 = -1.0527550E-35
        //
        private double ThermoCouple_Temp(double e_mv)
        {
            double t;
            double e;

            double[] c_const;

            c_const = new double[] {   0.0, 2.508355E-02, 7.860106E-08, -2.503131E-10, 8.315270E-14,
                                      -1.228034E-17, 9.804036E-22,-4.413030E-26, 1.057734E-30, -1.052755E-35};
            t = 0;
            e = e_mv * 1000;    // 熱起電力[uV]

            for (int i = 0; i < 10; i++)
            {
                t = t + c_const[i] * Math.Pow(e, i);
            }

            return t;
        }

        //  温度表示
        private void slg_temp_disp( Byte index)
        {
            float temp_ch0 = slgDatas[index].Ch0_temp;
            float temp_ch1 = slgDatas[index].Ch1_temp;

            if ( temp_ch0 < 500.0) {        // Ch0 温度表示
                if (index == 0)
                {
                    SLG1_Ch0_TextBox.Text = temp_ch0.ToString("F1");
                }
                else if ( index == 1)
                {
                    SLG2_Ch0_TextBox.Text = temp_ch0.ToString("F1");
                }
                else if ( index == 2)
                {
                    SLG3_Ch0_TextBox.Text = temp_ch0.ToString("F1");
                }
                else if ( index == 3)
                {
                    SLG4_Ch0_TextBox.Text = temp_ch0.ToString("F1");
                }
            }
            else　{
                if (index == 0)
                {
                    SLG1_Ch0_TextBox.Text = "error";
                }
                else if (index == 1)
                {
                    SLG2_Ch0_TextBox.Text = "error";
                }
                else if (index == 2)
                {
                    SLG3_Ch0_TextBox.Text = "error";
                }
                else if (index == 3)
                {
                    SLG4_Ch0_TextBox.Text = "error";
                }
             }

            if (temp_ch1 < 500.0)    // Ch1 温度表示
            {       
                if (index == 0)
                {
                    SLG1_Ch1_TextBox.Text = temp_ch1.ToString("F1");
                }
                else if (index == 1)
                {
                    SLG2_Ch1_TextBox.Text = temp_ch1.ToString("F1");
                }
                else if (index == 2)
                {
                    SLG3_Ch1_TextBox.Text = temp_ch1.ToString("F1");
                }
                else if (index == 3)
                {
                    SLG4_Ch1_TextBox.Text = temp_ch1.ToString("F1");
                }
            }
            else
            {
                if (index == 0)
                {
                    SLG1_Ch1_TextBox.Text = "error";
                }
                else if (index == 1)
                {
                    SLG2_Ch1_TextBox.Text = "error";
                }
                else if (index == 2)
                {
                    SLG3_Ch1_TextBox.Text = "error";
                }
                else if (index == 3)
                {
                    SLG4_Ch1_TextBox.Text = "error";
                }
            }

        }




        //  
        //    SLG47011の Buffer0 result, Buffer1 result, Buffer3 result からのデータ読み出し
        //   入力: slg_index ( 0～3 )
        //
        //slg-index  i2cアドレス　     読み出しバッファアドレス
        //   0        0x08        Buffer0 result=0x2212, Buffer1=0x2224, Buffer3=0x2248
        //   1        0x20        Buffer0 result=0x2212, Buffer1=0x2224
        //   2        0x28        Buffer0 result=0x2212, Buffer1=0x2224, Buffer3=0x2248
        //   3        0x30        Buffer0 result=0x2212, Buffer1=0x2224
        private void slg_47011_buffer_rd(Byte index)
        {
            UInt16 bd;

            i2c_rd_slg_47011(0x2212, index);  // Buffer0 result読み出し
            SndTextBox0.Text = get_send_str();  // 送信したSLG47011のバッファアドレス
            RcvTextBox0.Text = get_rcv_str();   // 受信データ表示
           
            bd = (UInt16)(i2cRxData[0] << 8);
            bd = (UInt16)( bd | i2cRxData[1]);
            slgDatas[index].Buffer0 = bd;       // DataBuff0の値を格納

       

            i2c_rd_slg_47011(0x2224, index);  // Buffer1 result読み出し
            SndTextBox1.Text = get_send_str();  // 送信したSLG47011のバッファアドレス
            RcvTextBox1.Text = get_rcv_str();   // 受信データ表示

            bd = (UInt16)(i2cRxData[0] << 8);
            bd = (UInt16)(bd | i2cRxData[1]);
            slgDatas[index].Buffer1 = bd;       // DataBuff1の値を格納


            if (index == 0 || index == 2)       // Buffer3 result 読み出し
            {
                i2c_rd_slg_47011(0x2248, index);  // Buffer3 result読み出し
                SndTextBox3.Text = get_send_str();  // 送信したSLG47011のバッファアドレス
                RcvTextBox3.Text = get_rcv_str();   // 受信データ表示

                bd = (UInt16)(i2cRxData[0] << 8);
                bd = (UInt16)(bd | i2cRxData[1]);
                slgDatas[index].Buffer3 = bd;       // DataBuff3の値を格納
            }
        }


        //  SLG47011からのデータ読み出し　

        // 入力: rd_adrs バッファアの読み出しアドレス　
        //               Buffer0 result = 0x2212
        //               Buffer1 result = 0x2224
        //               Buffer2 result = 0x2236
        //               Buffer3 result = 0x2248
        //
        //       index  読み出し対象のSLGを示す
        //
        // エラー時の表示
        // I2Cstatus:
        //  bit 0 = controller busy: all other status bits invalid
        //  bit 1 = error condition
        //  bit 2 = slave address was not acknowledged during last operation
        //  bit 3 = data not acknowledged during last operation
        //  bit 4 = arbitration lost during last operation
        //  bit 5 = controller idle
        //  bit 6 = bus busy
        //

        void i2c_rd_slg_47011(UInt16 rd_adrs, Byte index)
        {
            UInt32 writeLength = 0;
            UInt32 readLength = 0;

            i2cTxData[0] = (Byte)(rd_adrs >> 8);
            i2cTxData[1] = (Byte)(rd_adrs);

            Byte I2Cstatus = 0;

            Byte iic_slave_adrs = slgDatas[index].I2c_address; // 読み出し対象SLGのI2Cアドレス

            I2CAddressTextBox.Text = "0x" +  iic_slave_adrs.ToString("x2"); // 対象アドレスの表示

            Status = FT260_I2CMaster_WriteAndMonitorStatus(ft260handle, iic_slave_adrs, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, i2cTxData, BYTES_TO_WRITE, ref writeLength, ref I2Cstatus);

            sendDateTime = DateTime.Now;   // 送信時刻を得る

           
            if ((Status != FT260_STATUS.FT260_OK) || (writeLength != BYTES_TO_WRITE))
            {
                Staus_Err_Msg();
                I2C_Status_Msg(I2Cstatus);
            }

            Status = FT260_I2CMaster_ReadAndMonitorStatus(ft260handle, iic_slave_adrs, FT260_I2C_FLAG.FT260_I2C_START_AND_STOP, i2cRxData, BYTES_TO_READ, ref readLength, ref I2Cstatus, 5000);

            if ((Status != FT260_STATUS.FT260_OK) || (readLength != BYTES_TO_READ))
            {
                Staus_Err_Msg();
                I2C_Status_Msg(I2Cstatus);
            }

            receiveDateTime = DateTime.Now;   // 受信完了時刻を得る

            
        }


        //
        // 受信データの文字列を得る
        //
        private string get_rcv_str()
        {
            string rcv_str = "";

            for (int i = 0; i < BYTES_TO_READ; i++)   // 表示用の文字列作成
            {
                if ((i > 0) && (i % 16 == 0))    // 16バイト毎に1行空ける
                {
                    rcv_str = rcv_str + "\r\n";
                }

                rcv_str = rcv_str + i2cRxData[i].ToString("X2") + " ";
            }

            rcv_str = rcv_str + "(" + receiveDateTime.ToString("HH:mm:ss.fff") + ")";   // 受信データ文字列

            return rcv_str;
        }

        //
        // 送信データの文字列を得る
        // 　書き込みアドレスのみ、I2Cアドレスは含まない。
        //
        private string get_send_str()
        {
            string send_str = "";

            for (int i = 0; i < 2; i++)   //　SLG47011 バッファアドレス
            {
                send_str = send_str + i2cTxData[i].ToString("X2") + " ";
            }

            send_str = send_str + "(" + sendDateTime.ToString("HH:mm:ss.fff") + ")";   // 受信データ文字列

            return send_str;
        }


        // I2C Master Controll Status
        //  I2Cstatus:
        //  bit 0 = controller busy: all other status bits invalid
        //  bit 1 = error condition
        //  bit 2 = slave address was not acknowledged during last operation
        //  bit 3 = data not acknowledged during last operation
        //   bit 4 = arbitration lost during last operation
        //  bit 5 = controller idle
        //  bit 6 = bus busy

        private void I2C_Status_Msg(byte i2c_sta)
        {
            var msg = i2c_sta.ToString();

            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // メッセージボックスの表示
        }



        // DLLが無い
        private void No_DLL()
        {
            var msg = "DLLがありません。\r\n LibFT260.dll (amd64) is not present in the application folder.\r\n ";

            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // メッセージボックスの表示

            Close();            // アプリ終了  
        }

        // Stausの表示
        private void Staus_Err_Msg()
        {
            var msg = "";

            if (Status == FT260_STATUS.FT260_INVALID_HANDLE)
            {
                msg = "Invalid Handle";
            }
            else if (Status == FT260_STATUS.FT260_DEVICE_NOT_FOUND)
            {
                msg = "FT260 device not found";
            }
            else if (Status == FT260_STATUS.FT260_DEVICE_NOT_OPENED)
            {
                msg = "FT260 device not opened";
            }
            else if (Status == FT260_STATUS.FT260_DEVICE_OPEN_FAIL)
            {
                msg = "FT260 device open fail. Check cable, USB-Connector and power.";
            }
            else if (Status == FT260_STATUS.FT260_DEVICE_CLOSE_FAIL)
            {
                msg = "FT260 device close fail";
            }
            else if (Status == FT260_STATUS.FT260_INCORRECT_INTERFACE)
            {
                msg = "FT260 device incorrect interface";
            }
            else if (Status == FT260_STATUS.FT260_INCORRECT_CHIP_MODE)
            {
                msg = "FT260 device incorrect chip mode";
            }
            else if (Status == FT260_STATUS.FT260_DEVICE_MANAGER_ERROR)
            {
                msg = "FT260 device manager error";
            }
            else if (Status == FT260_STATUS.FT260_IO_ERROR)
            {
                msg = "FT260 device I/O error";
            }
            else if (Status == FT260_STATUS.FT260_INVALID_PARAMETER)
            {
                msg = "FT260 device invalid parameter";
            }
            else if (Status == FT260_STATUS.FT260_NULL_BUFFER_POINTER)
            {
                msg = "FT260 device null buffer pointer";
            }
            else if (Status == FT260_STATUS.FT260_BUFFER_SIZE_ERROR)
            {
                msg = "FT260 device buffer size error";
            }
            else if (Status == FT260_STATUS.FT260_UART_SET_FAIL)
            {
                msg = "FT260 device UART set fail";
            }
            else if (Status == FT260_STATUS.FT260_RX_NO_DATA)
            {
                msg = "FT260 device RX no data";
            }
            else if (Status == FT260_STATUS.FT260_GPIO_WRONG_DIRECTION)
            {
                msg = "FT260 device GPIO wrong direction";
            }
            else if (Status == FT260_STATUS.FT260_INVALID_DEVICE)
            {
                msg = "FT260 invalid device ";
            }
            else if (Status == FT260_STATUS.FT260_I2C_READ_FAIL)
            {
                msg = "FT260 I2C read fail";
            }
            else if (Status == FT260_STATUS.FT260_OTHER_ERROR)
            {
                msg = "FT260 other error, Send/Recive failure.";

            }

            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); // メッセージボックスの表示

            Close();     // アプリ終了

        }


        //
        // 　チャートの初期化(リアルタイム　チャート用)
        //
        //  ch0_data;  SLG1_CH0
        //  ch1_data;  SLG1_CH1
        //  ch2_data;  SLG1_CH3
        //  ch3_data;  SLG2_CH0
        //  ch4_data;  SLG2_CH1
        //  ch5_data;  SLG3_CH0
        //  ch6_data;  SLG3_CH1
        //  ch7_data;  SLG3_CH3
        //  ch8_data;  SLG4_CH0
        //  ch9_data;  SLG4_CH1
        //

        private void Chart_Ini()
        {
            trend_data_item_max = 30;             // 各リアルタイム　トレンドデータの保持数(=30 ) 1秒毎に収集すると、30秒分のデータ

            trend_data0 = new double[trend_data_item_max];      // トレンドデータ 0  SLG1のCH0 (上のグラフ)
            trend_data1 = new double[trend_data_item_max];      // トレンドデータ 1  SLG1のCH1 (上のグラフ)  
            trend_data2 = new double[trend_data_item_max];      // トレンドデータ 2  SLG1のCH3 (上のグラフ)
            trend_data3 = new double[trend_data_item_max];      // トレンドデータ 3  SLG2のCH0 (上のグラフ)
            trend_data4 = new double[trend_data_item_max];      // トレンドデータ 4  SLG2のCH1 (上のグラフ)  

            trend_data5 = new double[trend_data_item_max];      // トレンドデータ 5  SLG3のCH0 (下のグラフ)
            trend_data6 = new double[trend_data_item_max];      // トレンドデータ 6  SLG3のCH1 (下のグラフ)  
            trend_data7 = new double[trend_data_item_max];      // トレンドデータ 7  SLG3のCH3 (下のグラフ)
            trend_data8 = new double[trend_data_item_max];      // トレンドデータ 8  SLG4のCH0 (下のグラフ)
            trend_data9 = new double[trend_data_item_max];      // トレンドデータ 9  SLG4のCH1 (下のグラフ)  

            trend_dt = new double[trend_data_item_max];

            DateTime datetime = DateTime.Now;   // 現在の日時

            DateTime[] myDates = new DateTime[trend_data_item_max];  // 日時型

            for (int i = 0; i < trend_data_item_max; i++)  // 初期値の設定
            {
                trend_data0[i] = 20 + i;                  // SLG1 CH0
                trend_data1[i] = 30 + i;                  // SLG1 CH1
                trend_data2[i] = 40 + i;                  // SLG1 CH3
                trend_data3[i] = 50 + i;                  // SLG2 CH0
                trend_data4[i] = 60 + i;                  // SLG2 CH1

                trend_data5[i] = 120 + i;                 // SLG3 CH0
                trend_data6[i] = 130 + i;                 // SLG3 CH1
                trend_data7[i] = 140 + i;                 // SLG3 CH3
                trend_data8[i] = 150 + i;                 // SLG4 CH0
                trend_data9[i] = 160 + i;                 // SLG4 CH1

                myDates[i] = datetime + new TimeSpan(0, 0, i);  // i秒増やす

                trend_dt[i] = myDates[i].ToOADate();   // (現在の日時 + i 秒)をdouble型に変換
            }

            trend_scatter_0 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data0, ScottPlot.Colors.DarkCyan);  // SLG1 CH0 (上のグラフ)
            trend_scatter_1 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data1, ScottPlot.Colors.DarkRed);   // SLG1 CH1 (上のグラフ)
            trend_scatter_2 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data2, ScottPlot.Colors.Green);     // SLG1 CH3 (上のグラフ)
            trend_scatter_3 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data3, ScottPlot.Colors.DarkOrange);   // SLG2 CH0 (上のグラフ)
            trend_scatter_4 = wpfPlot_Trend.Plot.Add.Scatter(trend_dt, trend_data4, ScottPlot.Colors.DarkMagenta);  // SLG2 CH1 (上のグラフ)

            trend_scatter_5 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data5, ScottPlot.Colors.DarkCyan);  // SLG3 CH0 (下のグラフ)
            trend_scatter_6 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data6, ScottPlot.Colors.DarkRed);   // SLG3 CH1 (下のグラフ)
            trend_scatter_7 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data7, ScottPlot.Colors.Green);     // SLG3 CH3 (下のグラフ)
            trend_scatter_8 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data8, ScottPlot.Colors.DarkOrange);   // SLG4 CH0 (下のグラフ)
            trend_scatter_9 = wpfPlot_Trend_AD.Plot.Add.Scatter(trend_dt, trend_data9, ScottPlot.Colors.DarkMagenta);  // SLG4 CH1 (下のグラフ)

        
            trend_scatter_0.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH0は、左のY軸を使用)
            trend_scatter_1.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH1は、左のY軸を使用)
            trend_scatter_2.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG1 CH3は、左のY軸を使用)
            trend_scatter_3.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG2 CH0は、左のY軸を使用)
            trend_scatter_4.Axes.YAxis = wpfPlot_Trend.Plot.Axes.Left;  // 上のグラフ Y軸 左側 (SLG2 CH1は、左のY軸を使用)

            trend_scatter_5.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH0は、左のY軸)
            trend_scatter_6.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH1は、左のY軸)
            trend_scatter_7.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG3 CH3は、左のY軸)
            trend_scatter_8.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG4 CH0は、左のY軸)
            trend_scatter_9.Axes.YAxis = wpfPlot_Trend_AD.Plot.Axes.Left; // 下のグラフ Y軸 左側 (SLG4 CH1は、左のY軸)

            wpfPlot_Trend.UserInputProcessor.IsEnabled = false;     // マウスによるパン(グラフの移動)、ズーム(グラフの拡大、縮小)の操作禁止
            wpfPlot_Trend_AD.UserInputProcessor.IsEnabled = false;


            Axis_make();            // 軸の作成 

            // 凡例の表示
            // 参考:scottplot.net/cookbook/5.0/Legend/
            //
            wpfPlot_Trend.Plot.Legend.FontSize = 24;
            wpfPlot_Trend_AD.Plot.Legend.FontSize = 24;

            trend_scatter_0.LegendText = "SLG1-CH0";
            trend_scatter_1.LegendText = "SLG1-CH1";
            trend_scatter_2.LegendText = "SLG1-CH3";
            trend_scatter_3.LegendText = "SLG2-CH0";
            trend_scatter_4.LegendText = "SLG2-CH1";

            trend_scatter_5.LegendText = "SLG3-CH0";
            trend_scatter_6.LegendText = "SLG3-CH1";
            trend_scatter_7.LegendText = "SLG3-CH3";
            trend_scatter_8.LegendText = "SLG4-CH0";
            trend_scatter_9.LegendText = "SLG4-CH1";

            wpfPlot_Trend.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Vertical);
            wpfPlot_Trend_AD.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Vertical);

            wpfPlot_Trend.Refresh();        // データ変更後のリフレッシュ (上のグラフ用)
            wpfPlot_Trend_AD.Refresh();     // データ変更後のリフレッシュ (下のグラフ用)

        }

        //
        // 　軸の作成 
        //　上のグラフ: PV,SV,MV
        //  下のグラフ: 

        private void Axis_make()
        {

            // X軸の日時リミットを、最終日時+1秒にする
            DateTime dt_end = DateTime.FromOADate(trend_dt[trend_data_item_max - 1]); // double型を　DateTime型に変換
            TimeSpan dt_sec = new TimeSpan(0, 0, 1);    // 1 秒
            DateTime dt_limit = dt_end + dt_sec;      // DateTime型(最終日時+ 1秒) 
            double dt_ax_limt = dt_limit.ToOADate();   // double型(最終日時+ 1秒) 

            wpfPlot_Trend.Plot.Axes.SetLimitsX(trend_dt[0], dt_ax_limt);     // // 上のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒
            wpfPlot_Trend.Plot.Axes.SetLimitsY(0, 250, yAxis: wpfPlot_Trend.Plot.Axes.Left);      // PV,SV 上のグラフ Y軸 (左側)  下限=0, 上限=250[℃]

            wpfPlot_Trend_AD.Plot.Axes.SetLimitsX(trend_dt[0], dt_ax_limt);            // 下のグラフ X軸の最小=現在の時間 ,X軸の最大=最終日時+1秒
            wpfPlot_Trend_AD.Plot.Axes.SetLimitsY(0, 250, yAxis: wpfPlot_Trend_AD.Plot.Axes.Left);      // 下のグラフ Y軸 (左側)  下限=0, 上限=250


            custom_ticks();                             // X軸の目盛りのカスタマイズ
            set_y_axes_label();                         // 上のグラフ用 Y軸(左側、右側)のラベル.
            set_y_axes_label_ad();                      // 下のグラフ用 Y軸
        }


        // 上のグラフ用
        //  Y軸(左側)のラベル
        //　左|                |右
        //   C|                |
        //    |                |
        //    +----------------+
        //
        private void set_y_axes_label()
        {
            wpfPlot_Trend.Plot.Axes.Left.Label.FontName = "MS UI Gothic";      // Y軸(左側) ラベルのフォント名
            wpfPlot_Trend.Plot.Axes.Left.Label.FontSize = 24;               // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_Trend.Plot.Axes.Left.Label.Text = "C";                // Y軸(左側) ラベル (scottplot.net/cookbook/5.0/Styling/AxisCustom/)

        }

        // 下のグラフ用
        //  Y軸(左側)のラベル
        //　左|                |
        //  C |                |
        //    |                |
        //    +----------------+
        //
        private void set_y_axes_label_ad()
        {
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.FontName = "MS UI Gothic";      // Y軸(左側) ラベルのフォント名
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.FontSize = 24;               // Y軸(左側) ラベルのフォントサイズ変更  :
            wpfPlot_Trend_AD.Plot.Axes.Left.Label.Text = "C";           // Y軸(左側) ラベル (scottplot.net/cookbook/5.0/Styling/AxisCustom/)

        }


        //
        //  目盛りのカスタマイズ 
        // 参考: scottplot.net/cookbook/5.0/CustomizingTicks/
        //
        //       Custom Tick DateTimes
        // Users may define custom ticks using DateTime units
        // 
        private void custom_ticks()
        {
            DateTime dt;
            string label;

            // create a manual DateTime tick generator and add ticks
            ScottPlot.TickGenerators.DateTimeManual ticks = new ScottPlot.TickGenerators.DateTimeManual();

            //for (int i = 0; i < trend_data_item_max; i++)  // 1秒毎に目盛りのラベル表示
            //{
            //    DateTime dt = DateTime.FromOADate(trend_dt[i]);
            //    string label = dt.ToString("HH:mm:ss");
            //    ticks.AddMajor(dt, label);
            //}


            dt = DateTime.FromOADate(trend_dt[1]);  // 先頭 + 1の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);

            UInt16 t = (ushort)(trend_data_item_max / 2);
            dt = DateTime.FromOADate(trend_dt[t]);  // 中間の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);

            dt = DateTime.FromOADate(trend_dt[trend_data_item_max - 1]);  // 最後の時刻　目盛りのラベル表示
            label = dt.ToString("HH:mm:ss");
            ticks.AddMajor(dt, label);
            // 上のグラフ用
            wpfPlot_Trend.Plot.Axes.Bottom.TickGenerator = ticks;    　　　　// tell the horizontal axis to use the custom tick generator
          
            wpfPlot_Trend.Plot.Axes.Bottom.TickLabelStyle.FontName = "MS UI Gothic";  // X軸　目盛りのフォント名
            wpfPlot_Trend.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;     //  X軸　目盛りのフォントサイズ

            wpfPlot_Trend.Plot.Axes.Left.TickLabelStyle.FontName = "MS UI Gothic"; // Y軸(左側)　目盛りのフォント名
            wpfPlot_Trend.Plot.Axes.Left.TickLabelStyle.FontSize = 24;       //  Y軸(左側)　目盛りのフォントサイズ

            // 下のグラフ用
            wpfPlot_Trend_AD.Plot.Axes.Bottom.TickGenerator = ticks;       // tell the horizontal axis to use the custom tick generator

            wpfPlot_Trend_AD.Plot.Axes.Bottom.TickLabelStyle.FontName = "MS UI Gothic";  // X軸　目盛りのフォント名
            wpfPlot_Trend_AD.Plot.Axes.Bottom.TickLabelStyle.FontSize = 24;  //  X軸　目盛りのフォントサイズ

            wpfPlot_Trend.Plot.Axes.Left.TickLabelStyle.FontName = "MS UI Gothic"; // Y軸(左側)　目盛りのフォント名
            wpfPlot_Trend_AD.Plot.Axes.Left.TickLabelStyle.FontSize = 24;    //  Y軸(左側)　目盛りのフォントサイズ
        }


        // チェックボックスによるトレンド線の表示 
        //
        //  ch0_data;  SLG1_CH0
        //  ch1_data;  SLG1_CH1
        //  ch2_data;  SLG1_CH3
        //  ch3_data;  SLG2_CH0
        //  ch4_data;  SLG2_CH1
        //  ch5_data;  SLG3_CH0
        //  ch6_data;  SLG3_CH1
        //  ch7_data;  SLG3_CH3
        //  ch8_data;  SLG4_CH0
        //  ch9_data;  SLG4_CH1
        //
        private void CH_N_Show(object sender, RoutedEventArgs e)
        {

            if (trend_scatter_0 is null) return;
            if (trend_scatter_1 is null) return;
            if (trend_scatter_2 is null) return;
            if (trend_scatter_3 is null) return;
            if (trend_scatter_4 is null) return;
            if (trend_scatter_5 is null) return;
            if (trend_scatter_6 is null) return;
            if (trend_scatter_7 is null) return;
            if (trend_scatter_8 is null) return;
            if (trend_scatter_9 is null) return;

            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                trend_scatter_0.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                trend_scatter_1.IsVisible = true;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                trend_scatter_2.IsVisible = true;
            }
            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                trend_scatter_3.IsVisible = true;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                trend_scatter_4.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                trend_scatter_5.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                trend_scatter_6.IsVisible = true;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                trend_scatter_7.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                trend_scatter_8.IsVisible = true;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                trend_scatter_9.IsVisible = true;
            }


            wpfPlot_Trend.Refresh();   // グラフの更新 (上のグラフ)
            wpfPlot_Trend_AD.Refresh();  // グラフの更新 (下のグラフ)
        }

        // チェックボックスによるトレンド線の非表示
        private void CH_N_Hide(object sender, RoutedEventArgs e)
        {
            if (trend_scatter_0 is null) return;
            if (trend_scatter_1 is null) return;
            if (trend_scatter_2 is null) return;
            if (trend_scatter_3 is null) return;
            if (trend_scatter_4 is null) return;
            if (trend_scatter_5 is null) return;

            if (trend_scatter_6 is null) return;
            if (trend_scatter_7 is null) return;
            if (trend_scatter_8 is null) return;
            if (trend_scatter_9 is null) return;

            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Name == "SLG1_CH0_CheckBox")
            {
                trend_scatter_0.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH1_CheckBox")
            {
                trend_scatter_1.IsVisible = false;
            }
            else if (checkBox.Name == "SLG1_CH3_CheckBox")
            {
                trend_scatter_2.IsVisible = false;
            }
            else if (checkBox.Name == "SLG2_CH0_CheckBox")
            {
                trend_scatter_3.IsVisible = false;
            }
            else if (checkBox.Name == "SLG2_CH1_CheckBox")
            {
                trend_scatter_4.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH0_CheckBox")
            {
                trend_scatter_5.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH1_CheckBox")
            {
                trend_scatter_6.IsVisible = false;
            }
            else if (checkBox.Name == "SLG3_CH3_CheckBox")
            {
                trend_scatter_7.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH0_CheckBox")
            {
                trend_scatter_8.IsVisible = false;
            }
            else if (checkBox.Name == "SLG4_CH1_CheckBox")
            {
                trend_scatter_9.IsVisible = false;
            }

            wpfPlot_Trend.Refresh();   // グラフの更新 (上のグラフ)
            wpfPlot_Trend_AD.Refresh();  // グラフの更新 (下のグラフ)

        }

        // 保持しているデータをファイルへ保存
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
        //
        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            string path;

            string str_one_line;

            SaveFileDialog sfd = new SaveFileDialog();           //　SaveFileDialogクラスのインスタンスを作成 

            sfd.FileName = "temp_trend.csv";                              //「ファイル名」で表示される文字列を指定する

            sfd.Title = "保存先のファイルを選択してください。";        //タイトルを設定する 

            sfd.RestoreDirectory = true;                 //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする

            if (sfd.ShowDialog() == true)            //ダイアログを表示する
            {
                path = sfd.FileName;

                try
                {
                    System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.Default);

                    str_one_line = DataMemoTextBox.Text; // メモ欄
                    sw.WriteLine(str_one_line);         // 1行保存


                    str_one_line = "DateTime"  + "," +
                                    "SLG1_CH0" + "," + "SLG1_CH1" + "," + "SLG1_CH3" + "," +
                                    "SLG2_CH0" + "," + "SLG2_CH1" + "," +
                                    "SLG3_CH0" + "," + "SLG3_CH1" + "," + "SLG3_CH3" + "," +
                                    "SLG4_CH0" + "," + "SLG4_CH1";

                    sw.WriteLine(str_one_line);         // 1行保存

                    foreach (HistoryData historyData in historyData_list)         // historyData_listの内容を保存
                    {
                        DateTime dateTime = DateTime.FromOADate(historyData.dt); // 記録されている日時(double型)を　DateTime型に変換

                        string st_dateTime = dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff");             // DateTime型を文字型に変換　（2021/10/22 11:09:06.125 )

                        string st_dt0 = historyData.data0.ToString("F1");       // SLG1_CH0
                        string st_dt1 = historyData.data1.ToString("F1");       // SLG1_CH1
                        string st_dt2 = historyData.data2.ToString("F1");       // SLG1_CH3
                        string st_dt3 = historyData.data3.ToString("F1");       // SLG2_CH0
                        string st_dt4 = historyData.data4.ToString("F1");       // SLG2_CH1
                        string st_dt5 = historyData.data5.ToString("F1");       // SLG3_CH0
                        string st_dt6 = historyData.data6.ToString("F1");       // SLG3_CH1
                        string st_dt7 = historyData.data7.ToString("F1");       // SLG3_CH3
                        string st_dt8 = historyData.data8.ToString("F1");       // SLG4_CH0
                        string st_dt9 = historyData.data9.ToString("F1");       // SLG4_CH1


                        str_one_line = st_dateTime + "," + st_dt0 + "," + st_dt1 + "," + st_dt2 + "," + st_dt3 + "," + st_dt4 + "," + st_dt5 + "," +
                                       st_dt6 + "," + st_dt7 + "," + st_dt8 + "," + st_dt9;

                        sw.WriteLine(str_one_line);         // 1行保存
                    }

                    sw.Close();
                }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
        // 収集済みのデータをクリアの確認
        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "収集済みのデータがクリアされます。";
            string caption = "Check clear";

            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

            switch (result)
            {
                case MessageBoxResult.Yes:      // Yesを押した場合
                    historyData_list.Clear();   // 収集済みのデータのクリア
                    break;

                case MessageBoxResult.No:
                    break;

                case MessageBoxResult.Cancel:
                    break;
            }
        }

        // トレンド 履歴画面
        private void History_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new HistoryWindow();      // 注意メッセージのダイアログを開く
            window.Owner = this;
            window.Show();
        }
    }

}