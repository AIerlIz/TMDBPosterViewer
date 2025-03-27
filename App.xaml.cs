using System.Windows;
using TMDBPosterViewer.Models;

namespace TMDBPosterViewer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 加载配置
        var config = AppConfig.Load();

        // 检查TMDB API密钥
        if (string.IsNullOrEmpty(config.TmdbApiKey))
        {
            var result = MessageBox.Show(
                "请先设置TMDB API密钥。\n\n是否现在设置？",
                "配置提示",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var key = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入TMDB API密钥：",
                    "设置API密钥",
                    "");

                if (!string.IsNullOrEmpty(key))
                {
                    config.TmdbApiKey = key;
                    config.Save();
                }
                else
                {
                    MessageBox.Show(
                        "未设置API密钥，应用程序将退出。",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Shutdown();
                }
            }
            else
            {
                Shutdown();
            }
        }

        // 设置全局异常处理
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show(
                $"发生未处理的错误：{args.Exception.Message}",
                "错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}

