using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
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

            // 管理者権限チェック
            if (IsAdministrator())
            {
                MessageBox.Show("アプリケーションを管理者権限で実行しないでください。dllフォルダにある画像認識で使われる大量の依存関係が管理者権限で安全かどうか保証できません。この保護を無効化するには作者に問い合わせてください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1); // エラーメッセージを表示して終了
                return;
            }

            // AssemblyResolveイベントを設定
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form = new Form1());
        }

        // DLLフォルダからアセンブリを読み込むイベントハンドラ
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            // DLLのパスを指定 (例: "DLLフォルダ"がアプリケーションのルートディレクトリにある場合)
            string dllFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dll");

            // ロードが必要なアセンブリ名を取得
            var assemblyName = new AssemblyName(args.Name).Name;

            // DLLフォルダ内の該当DLLパスを取得
            string assemblyPath = Path.Combine(dllFolderPath, assemblyName + ".dll");

            // 該当DLLが存在する場合のみ読み込む
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null; // DLLが見つからない場合
        }


        // 管理者権限で実行されているか確認
        private static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            form.OnExit();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            form.OnExit();
        }
    }
}
