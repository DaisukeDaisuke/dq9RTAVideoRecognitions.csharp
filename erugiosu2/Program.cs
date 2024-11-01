using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1;

namespace erugiosu2
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {

            // AssemblyResolveイベントを設定
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
    }
}
