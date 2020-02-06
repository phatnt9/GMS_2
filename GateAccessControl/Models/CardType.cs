using System.ComponentModel;

namespace GateAccessControl
{
    public class CardType : INotifyPropertyChanged
    {
        private int _classId;
        private string _className;

        public int classId
        {
            get => _classId;
            set
            {
                _classId = value;
                OnPropertyChanged("classId");
            }
        }

        public string className
        {
            get => _className;
            set
            {
                _className = value;
                OnPropertyChanged("className");
            }
        }

        public CardType()
        {
        }

        public CardType(int classId, string className)
        {
            this.classId = classId;
            this.className = className;
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members
    }
}