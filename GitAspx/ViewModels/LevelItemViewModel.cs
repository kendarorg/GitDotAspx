using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace GitAspx.ViewModels
{
    public class LevelItemViewModel
    {
        private string _basePath;
        private string _name;
        public LevelItemViewModel(LevelViewModel parent)
        {
            _basePath = parent.BreadCrumb.Last();
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = Path.GetFileName(value);
            }
        }
        public string FullPath
        {
            get
            {
                return Path.Combine(_basePath, _name).Replace("/", "\\");

            }
        }
    }
}