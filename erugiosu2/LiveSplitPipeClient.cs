using System;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace erugiosu2
{
    public class LiveSplitPipeClient
    {
        private const string PipeName = @"\\.\pipe\LiveSplit";

        private static async Task<Task> WhenAnyWithTimeout(Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Task delayTask = Task.Delay(timeout, cancellationToken);
            return await Task.WhenAny(task, delayTask);
        }

        // タイマーの現在時間を取得するメソッド
        public static async Task GetCurrentTimeAsync(Func<string, Task> onComplete)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "LiveSplit", PipeDirection.InOut))
                {
                     
                    // LiveSplitに接続
                    await pipeClient.ConnectAsync(500);

                    // タイマー取得コマンドを送信
                    byte[] commandBytes = Encoding.UTF8.GetBytes("getcurrentrealtime\n");
                    Task task1 = pipeClient.WriteAsync(commandBytes, 0, commandBytes.Length);
                    Task complete1 = await WhenAnyWithTimeout(task1, TimeSpan.FromSeconds(0.5), cancellationToken.Token);

                    if (task1 == complete1)
                    {
                        // 応答を取得
                        using (StreamReader reader = new StreamReader(pipeClient))
                        {
                            Task<string> task2 = reader.ReadLineAsync();
                            Task complete2 = await WhenAnyWithTimeout(task2, TimeSpan.FromSeconds(0.5), cancellationToken.Token);
                            if (task2 == complete2) {
                                string result = await task2;

                               await onComplete(result);
                            }
                            else
                            {
                                // タイムアウト時にパイプを閉じて、`StreamReader` の読み取りをキャンセル
                                pipeClient.Close();
                                Console.WriteLine("LiveSplitの取得のタイムアウト（応答受信）");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("LiveSplitの取得のタイムアウト（コマンド送信）");
                    }
                }
            }
            catch (TimeoutException)
            {
                // LiveSplitが起動していない場合
                Console.WriteLine("LiveSplitが起動していないか、パイプが接続できませんでした。");
            }
            catch (IOException)
            {
                // パイプが開けなかった場合
                Console.WriteLine("LiveSplitが利用できない状態です。");
            }catch (Exception) {
                Console.WriteLine("LiveSplitの取得で未知の例外が発生しました。");
            }
            finally
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
                await Task.CompletedTask;
            }
        }
    }
}