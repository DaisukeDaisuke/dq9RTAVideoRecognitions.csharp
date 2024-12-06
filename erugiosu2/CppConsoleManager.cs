using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace erugiosu2
{
    internal class CppConsoleManager : IDisposable
    {
        private Process _process;
        private StreamWriter _standardInput;
        private Task _outputReaderTask;
        private bool _isDisposed = false;

        public bool IsRunning => _process != null && !_process.HasExited;
        
        public CppConsoleManager(string exePath)
        {
            if (!File.Exists(exePath))
            {
                MessageBox.Show($"'{exePath}' が見つかりません。操作を中止します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                _process.Start();
                _standardInput = _process.StandardInput;
                _outputReaderTask = Task.Run(() => ReadOutputAsync(_process.StandardOutput));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"C++プログラムの起動中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ReadOutputAsync(StreamReader standardOutput)
        {
            try
            {
                while (!_isDisposed && !_process.HasExited)
                {
                    string line = await standardOutput.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // 必要に応じて結果を処理する
                        OnOutputReceived?.Invoke(line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"C++プログラムの出力読み取り中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SendInput(string input)
        {
            if (!IsRunning) return;

            try
            {
                _standardInput.WriteLine(input);
                _standardInput.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"標準入力送信中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _process.Kill();
                _process.WaitForExit(5000);
            }
            catch
            {
                // プロセス終了時の例外は無視
            }

            _process?.Dispose();
            _standardInput?.Dispose();
        }

        public event Action<string> OnOutputReceived;
    }
}
