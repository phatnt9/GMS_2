using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateAccessControl
{
    public class CardType : INotifyPropertyChanged
    {
        private int _classId;
        private string _className;

        public int CLASS_ID
        {
            get
            {
                return _classId;
            }
            set
            {
                _classId = value;
                OnPropertyChanged("CLASS_ID");
            }
        }
        public string CLASS_NAME
        {
            get
            {
                return _className;
            }
            set
            {
                _className = value;
                OnPropertyChanged("CLASS_NAME");
            }
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
