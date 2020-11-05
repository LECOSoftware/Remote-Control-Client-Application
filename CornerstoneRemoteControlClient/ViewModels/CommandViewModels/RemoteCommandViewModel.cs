// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Xml.Linq;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// Base view model for all commands.
    /// </summary>
    public class RemoteCommandViewModel : ViewModelBase
    {
        #region Constructor

        public RemoteCommandViewModel(String name, String description, String cookie = "")
        {
            Name = name;
            Description = description;

            if (String.IsNullOrEmpty(cookie))
                Cookie = name;
            else
                Cookie = cookie;
        }

        #endregion

        #region Public Interface

        public virtual XDocument GetCommand()
        {
            return XDocument.Parse(String.Format("<{0}/>", Name));
        }

        #endregion

        #region Public Properties

        private string _requiredFamily = string.Empty;

        public string RequiredFamily
        {
            get => _requiredFamily;
            set
            {
                _requiredFamily = value;
                RaisePropertyChanged(nameof(RequiredFamily));
            }
        }

        private String _name;
        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        private String _description;
        public String Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        private String _cookie;
        public String Cookie
        {
            get { return _cookie; }
            set
            {
                _cookie = value;
                RaisePropertyChanged("Cookie");
            }
        }

        public String CommandSyntax
        {
            get { return GetCommand().ToString(); }
        }

        #endregion
    }
}
