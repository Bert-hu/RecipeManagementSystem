using log4net.Appender;
using log4net.Core;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rms.UserClient.Utils
{
    internal class RichTextBoxAppender : RollingFileAppender
    {
        // 定义一个属性，用于存储RichTextBox控件的引用
        public UIRichTextBox RichTextBox { get; set; }

        // 重写Append方法，用于将日志信息输出到RichTextBox控件
        protected override void Append(LoggingEvent loggingEvent)
        {
            base.Append(loggingEvent);
            if (RichTextBox == null) return;

            // 根据日志级别设置不同的颜色
            Color color = Color.Black;
            switch (loggingEvent.Level.Name)
            {
                case "DEBUG":
                    color = Color.Gray;
                    break;
                case "INFO":
                    color = Color.Green;
                    break;
                case "WARN":
                    color = Color.Yellow;
                    break;
                case "ERROR":
                    color = Color.Red;
                    break;
                case "FATAL":
                    color = Color.Magenta;
                    break;
            }

            // 将日志信息格式化为字符串
            string message = RenderLoggingEvent(loggingEvent);

            // 在UI线程上执行委托，将日志信息追加到RichTextBox控件
            RichTextBox.Invoke(new Action(() =>
            {
                if (RichTextBox.Lines.Length >= 500)
                {
                    RichTextBox.Clear();
                }
                RichTextBox.SelectionStart = RichTextBox.TextLength; // 移动光标到文本末尾
                RichTextBox.SelectionLength = 0; // 清除选中的文本

                RichTextBox.SelectionColor = color; // 设置文本颜色
                RichTextBox.AppendText(message); // 追加文本

                RichTextBox.SelectionColor = RichTextBox.ForeColor; // 恢复默认颜色

                RichTextBox.ScrollToCaret(); // 滚动到光标位置
            }));
        }

    }
}
