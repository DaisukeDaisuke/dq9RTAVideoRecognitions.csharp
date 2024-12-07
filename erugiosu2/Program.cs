using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using WindowsFormsApp1;

namespace erugiosu2
{
   
    internal static class Program
    {
        private static Form1 form = null;
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {

            // OSバージョンチェック
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) // Windows 7 (6.1)以降かをチェック
            {
                MessageBox.Show("このアプリケーションはWindows 7以降でのみ動作します。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            // 管理者権限チェック
            if (IsAdministrator())
            {
                MessageBox.Show("アプリケーションを管理者権限で実行しないでください。dllフォルダにある画像認識で使われる大量の依存関係が管理者権限で安全かどうか保証できません。この保護を無効化するには作者に問い合わせてください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // エラーメッセージを表示して終了
                return;
            }

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form = new Form1());
        }


        // 管理者権限で実行されているか確認
        private static bool IsAdministrator()
        {
            Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(6, 1));
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            form?.OnExit();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // 例外オブジェクトを取得
                Exception exception = e.ExceptionObject as Exception;

                // エラーメッセージウィンドウを表示
                string message = $"未処理の例外が発生しました。\n\n詳細:\n{exception?.Message ?? "不明なエラー"}";
                string stackTrace = exception?.StackTrace ?? "スタックトレースはありません。";

                // ログファイルにエラーを書き込む
                File.WriteAllText("error_log.txt", $"{DateTime.Now}: {message}\n\n{stackTrace}");

                // エラーダイアログを表示
                MessageBox.Show(
                    $"{message}\n\nスタックトレース:\n{stackTrace}",
                    "アプリケーション エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                // エラーハンドラ内で別の例外が発生した場合の処理
                File.WriteAllText("critical_error_log.txt", $"例外ハンドラ内エラー: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                form?.OnExit();
                // アプリケーションを安全に終了
                Environment.Exit(1);
            }
        }
    }
}
