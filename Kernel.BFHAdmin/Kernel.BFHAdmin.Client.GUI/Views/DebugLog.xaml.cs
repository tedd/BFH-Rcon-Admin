using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kernel.BFHAdmin.Common;
using Kernel.BFHAdmin.Common.Utils;
using NLog;

namespace Kernel.BFHAdmin.Client.GUI.Views
{
    /// <summary>
    /// Interaction logic for DebugLog.xaml
    /// </summary>
    public partial class DebugLog : UserControl, IDisposable
    {

        private struct LogEntryItem
        {
            public LogEventInfo LogEvent;
            public string Message;
        }

        private RconModel _rconModel;
        private NLogListener _nLogListener;
        private Paragraph paragraph = null;


        private StringBuilder _messageBuffer = new StringBuilder();
        private LogEventInfo _lastLogEventInfo;
        private Task _logWriteTimerTask;
        private CancellationTokenSource _cancellationTokenSource;
        private int _maxLogLineChunks = 100;
        private int _LogWriteFlushTimeMs = 2000;
        private Queue<LogEntryItem> _logEntryQueue = new Queue<LogEntryItem>();
        public DebugLog()
        {
            InitializeComponent();
        }

        public void Register(RconModel rconModel)
        {
            _rconModel = rconModel;

            _nLogListener = new NLogListener();
            _nLogListener.LogWrite += NLogListenerOnLogWrite;

            _cancellationTokenSource = new CancellationTokenSource();

            _logWriteTimerTask = PeriodicTaskFactory.Start(
                () =>
                {
                    ProcessQueue();
                },
                intervalInMilliseconds: _LogWriteFlushTimeMs,
                delayInMilliseconds: 1000,
                duration: -1,
                maxIterations: -1,
                synchronous: false,
                cancelToken: _cancellationTokenSource.Token,
                periodicTaskCreationOptions: TaskCreationOptions.None);

            //// Repeat every 1 second
            //_logWriteTimerAction = ignored1 =>
            //                           {
            //                               if (ignored1.IsCompleted)
            //                                   return;
            //                               FlushBuffer();
            //                               //Task.Delay(_LogWriteFlushTimeMs, _cancellationTokenSource.Token).ContinueWith(ignored2 => _logWriteTimerAction(ignored2), _cancellationTokenSource.Token);
            //                               _logWriteTimerTask = Task.Delay(_LogWriteFlushTimeMs, _cancellationTokenSource.Token).ContinueWith(_logWriteTimerAction, _cancellationTokenSource.Token);
            //                           };

            //// Wait 1 sec, then start above repeating task
            //_logWriteTimerTask = Task.Delay(1000, _cancellationTokenSource.Token).ContinueWith(_logWriteTimerAction, _cancellationTokenSource.Token);


        }

        private void ProcessQueue()
        {
            // Enqueue any remaining data in message buffer
            EnqueueMessageBuffer();

            // Do we have data?
            if (_logEntryQueue.Count > 0)
            {
                // Lock
                lock (_logEntryQueue)
                {
                    // Still have data? Flush it all to window
                    while (_logEntryQueue.Count > 0)
                    {
                        var logEntry = _logEntryQueue.Dequeue();
                        WriteText(logEntry.LogEvent.Level, logEntry.Message);
                    }
                }
            }
        }

        private void WriteText(LogLevel logLevel, string text)
        {
            Dispatcher.Invoke(
                () =>
                {

                    if (paragraph == null)
                    {
                        paragraph = new Paragraph();
                        paragraph.Margin = new Thickness(0, 0, 0, 0);

                        txtOut.Document.Blocks.Add(paragraph);
                    }

                    // Add this text
                    Brush color = LogLevelToColor(logLevel);
                    var run = new Run(text);
                    run.Foreground = color;
                    paragraph.Inlines.Add(run);

                    // If we have too much text we'll chop away from the beginning
                    if (paragraph.Inlines.Count > _maxLogLineChunks)
                        paragraph.Inlines.Remove(paragraph.Inlines.FirstInline);

                    //HwndSource lstHwnd = HwndSource.FromVisual(txtOut) as HwndSource;
                    //Win32API.SendMessage(txtOut., Win32API.WM_VSCROLL, Win32API.SB_BOTTOM, 0);
                    txtOut.ScrollToEnd();
                });
        }

        private void NLogListenerOnLogWrite(object sender, LogEventInfo logEvent, string message)
        {
            lock (_messageBuffer)
            {
                // New level? Process queue (will empty buffer)
                if (_lastLogEventInfo != null && _lastLogEventInfo.Level != logEvent.Level)
                    ProcessQueue();

                // Add to buffer
                AddToMessageBuffer(logEvent, message);
            }
        }

        private void AddToMessageBuffer(LogEventInfo logEvent, string message)
        {
            lock (_messageBuffer)
            {
                _lastLogEventInfo = logEvent;
                _messageBuffer.AppendLine(message);
            }
        }

        private void EnqueueMessageBuffer()
        {
            lock (_messageBuffer)
            {
                if (_lastLogEventInfo == null)
                    return;
                if (_messageBuffer.Length == 0)
                    return;
                // Enqueue line
                lock (_logEntryQueue)
                {
                    _logEntryQueue.Enqueue(
                        new LogEntryItem()
                            {
                                LogEvent = _lastLogEventInfo,
                                Message = _messageBuffer.ToString()
                            });
                    // Clear buffer
                    _messageBuffer.Clear();
                    _lastLogEventInfo = null;

                }
            }
        }


        private Brush LogLevelToColor(LogLevel level)
        {
            Brush color = Brushes.Black;
            if (level == LogLevel.Trace)
                color = Brushes.Gray;
            if (level == LogLevel.Debug)
                color = Brushes.Black;
            if (level == LogLevel.Info)
                color = Brushes.Green;
            if (level == LogLevel.Warn)
                color = Brushes.DarkOrange;
            if (level == LogLevel.Error)
                color = Brushes.Purple;
            if (level == LogLevel.Fatal)
                color = Brushes.Red;
            return color;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _logWriteTimerTask.Wait(500);
        }
    }
}
