using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using EyeTribe.Controls.Calibration;
using EyeTribe.Controls.Cursor;
using EyeTribe.Controls.TrackBox;
using EyeTribe.ClientSdk.Data;
using System.Windows.Interop;
using EyeTribe.ClientSdk;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
using System.IO;

namespace Calibration
{
    public partial class MainWindow : IConnectionStateListener
    {
        private Screen activeScreen = Screen.PrimaryScreen;
        private CursorControl cursorControl;

        public MainWindow()
        {
            InitializeComponent();
            this.ContentRendered += (sender, args) => InitClient();
            this.KeyDown += MainWindow_KeyDown;
        }

        private void InitClient()
        {
            // Activate/connect client

            GazeManager.Instance.Activate();

            // Listen for changes in connection to server
            GazeManager.Instance.AddConnectionStateListener(this);

            // Fetch current status
            OnConnectionStateChanged(GazeManager.Instance.IsActivated);

            AddVisualInstanceTracker();

            UpdateState();
        }


        private void AddVisualInstanceTracker()
        {
            TrackingStatusGrid.Children.Clear();
            TrackingStatusGrid.Children.Add(new TrackBoxStatus());
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e == null)
                return;

            switch (e.Key)
            {
                // Start calibration on hitting "C"
                case Key.C:
                    ButtonCalibrateClicked(this, null);
                    break;

                // Toggle mouse redirect with "M"
                case Key.M:
                    ButtonMouseClicked(this, null);
                    break;

                // Turn cursor control off on hitting Escape
                case Key.Escape:
                    if (cursorControl != null)
                        cursorControl.Enabled = false;

                    UpdateState();
                    break;
            }
        }

        public void OnConnectionStateChanged(bool IsActivated)
        {
            // The connection state listener detects when the connection to the EyeTribe server changes
            if (btnCalibrate.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => OnConnectionStateChanged(IsActivated)));
                return;
            }

            if (!IsActivated)
                GazeManager.Instance.Deactivate();

            UpdateState();
        }

        private void ButtonCalibrateClicked(object sender, RoutedEventArgs e)
        {
            // Check connectivitiy status
            if (GazeManager.Instance.IsActivated == false)
                InitClient();

            // API needs to be active to start calibrating
            if (GazeManager.Instance.IsActivated)
                Calibrate();
            else
                UpdateState(); // show reconnect
        }

        private void ButtonMouseClicked(object sender, RoutedEventArgs e)
        {
            if (GazeManager.Instance.IsCalibrated == false) {

                return;
            }

            if (cursorControl == null)
                cursorControl = new MouseControll(activeScreen, true, true); // Lazy initialization
            else
                cursorControl.Enabled = !cursorControl.Enabled; // Toggle on/off

            UpdateState();
        }

        private void Calibrate()
        {
            // Update screen to calibrate where the window currently is
            activeScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);

            // Initialize and start the calibration
            CalibrationRunner calRunner = new CalibrationRunner(activeScreen, activeScreen.Bounds.Size, 9);
            calRunner.OnResult += calRunner_OnResult;
            calRunner.Start();
        }

        private void calRunner_OnResult(object sender, CalibrationRunnerEventArgs e)
        {
            // Invoke on UI thread since we are accessing UI elements
            if (RatingText.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new MethodInvoker(() => calRunner_OnResult(sender, e)));
                return;
            }

            // Show calibration results rating
            if (e.Result == CalibrationRunnerResult.Success)
            {
                UpdateState();
            }
            else
                MessageBox.Show(this, "Falha ao calibrar, por favor tente novamente");
        }

        private static void StartServerProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Minimized;
            psi.FileName = GetServerExecutablePath();
            psi.Arguments = "--framerate=60";
            if (psi.FileName == string.Empty || File.Exists(psi.FileName) == false)
                return;

            Process processServer = new Process();
            processServer.StartInfo = psi;
            processServer.Start();

            Thread.Sleep(5000); 
        }

        private static string GetServerExecutablePath()
        {
            // check default paths           
            const string x86 = @"C:\Program Files (x86)\EyeTribe\Server\EyeTribe.exe";
            if (File.Exists(x86))
                return x86;

            const string x64 = @"C:\Program Files\EyeTribe\Server\EyeTribe.exe";
            if (File.Exists(x64))
                return x64;

            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Title = "Por favor selecione o servidor Eye Tribe";
            dlg.Filter = "Executable Files (*.exe)|*.exe";
            

            return string.Empty;
        }

        private static bool IsServerProcessRunning()
        {
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.ProcessName.ToLower() == "eyetribe")
                        return true;
                }
            }
            catch (Exception)
            { }

            return false;
        }
        private void UpdateState()
        {
            // No connection
            if (GazeManager.Instance.IsActivated == false)
            {
                if (!IsServerProcessRunning())
                {
                    StartServerProcess();
                    InitClient();
                }

            }
                

            if (GazeManager.Instance.IsCalibrated == false)
            {
                btnCalibrate.Content = "Calibrar";
            }
            else
            {
                btnCalibrate.Content = "Recalibrar";

                // Set mouse-button label
                btnMouse.Content = "Ativar Controle";

                if (cursorControl != null && cursorControl.Enabled)
                    btnMouse.Content = "Desativar Controle";

                if (GazeManager.Instance.LastCalibrationResult != null)
                    RatingText.Text = RatingFunction(GazeManager.Instance.LastCalibrationResult);
            }
        }

   

        private string RatingFunction(CalibrationResult result)
        {
            if (result == null)
                return "";

            double accuracy = result.AverageErrorDegree;

            if (accuracy < 0.5)
                return "Qualidade da calibragem: PERFEITA";

            if (accuracy < 0.7)
                return "Qualidade da calibragem: BOA";

            if (accuracy < 1)
                return "Qualidade da calibragem: MODERADA";

            if (accuracy < 1.5)
                return "Qualidade da calibragem: BAIXA";

            return "Qualidade da calibragem: REFAZER";
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            GazeManager.Instance.Deactivate();
        }

        private void btnOpenKeyBoard_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("osk.exe");
        }
    }
}
