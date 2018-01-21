using System.Net;
using System.ComponentModel;
using TouchRemote.Utils;

namespace TouchRemote.Model
{
    public class Connection : INotifyPropertyChanged
    {
        public string Id { get; set; }

        public IPEndPoint RemoteEndpoint { get; set; }

        public IPEndPoint LocalEndpoint { get; set; }

        private int _ClientWidth;
        public int ClientWidth
        {
            get
            {
                return _ClientWidth;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _ClientWidth, value, () => ClientWidth);
            }
        }

        private int _ClientHeight;
        public int ClientHeight
        {
            get
            {
                return _ClientHeight;
            }
            set
            {
                PropertyChanged.ChangeAndNotify(ref _ClientHeight, value, () => ClientHeight);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
