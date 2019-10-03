using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace GitAspx.ViewModels
{
    public class LevelViewModel
    {
        public LevelViewModel()
        {
            Directories = new List<LevelItemViewModel>();
            Files = new List<LevelItemViewModel>();
            BreadCrumb = new List<string>();
            TextFile = null;
            Ext = null;
            ImageFile = null;
            BinaryFile = null;
        }

        public void SetBreadCrumb(List<String> breadcrumbs)
        {
            for (int i = 0; i < breadcrumbs.Count; i++)
            {
                var result = string.Join(Path.DirectorySeparatorChar.ToString(),
                    breadcrumbs.Take(i+1));
                BreadCrumb.Add(result);
            }
        }

        public string Project { get; set; }
        public string TextFile { get; set; }
        public List<string> BreadCrumb { get; private set; }
        public List<LevelItemViewModel> Directories { get; set; }
        public List<LevelItemViewModel> Files { get; set; }

        public string Ext { get; set; }

        public string ImageFile { get; set; }

        public string BinaryFile { get; set; }

        public string FileName { get; set; }
    }
}