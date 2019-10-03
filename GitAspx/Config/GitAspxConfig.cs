using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace GitAspx.Config
{
    public class GitAspxConfig : ConfigurationSection
    {
        [ConfigurationProperty("RepositoriesDirectory", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string RepositoriesDirectory
        {
            get { return ((string)(base["RepositoriesDirectory"])); }
            set { base["RepositoriesDirectory"] = value; }
        }

        [ConfigurationProperty("Admin", DefaultValue = "admin", IsKey = false, IsRequired = true)]
        public string Admin
        {
            get { return ((string)(base["Admin"])); }
            set { base["Admin"] = value.ToLowerInvariant(); }
        }

        [ConfigurationProperty("ReceivePack", DefaultValue = "false", IsKey = false, IsRequired = true)]
        public bool ReceivePack
        {
            get{ return (bool)(base["ReceivePack"]);}
            set { base["ReceivePack"] = value; }
        }

        [ConfigurationProperty("UploadPack", DefaultValue = "false", IsKey = false, IsRequired = true)]
        public bool UploadPack
        {
            get { return (bool)(base["UploadPack"]); }
            set { base["UploadPack"] = value; }
        }

        [ConfigurationProperty("Users")]
        [ConfigurationCollection(typeof(GitUsersCollection), AddItemName = "User")]
        public GitUsersCollection Users
        {
            get { return ((GitUsersCollection)(base["Users"])); }
        }

        [ConfigurationProperty("Repositories")]
        [ConfigurationCollection(typeof(GitRepoCollection), AddItemName = "Repository")]
        public GitRepoCollection Repositories
        {
            get { return ((GitRepoCollection)(base["Repositories"])); }
        }

    }

    [ConfigurationCollection(typeof(GitUser))]
    public class GitUsersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GitUser();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GitUser)(element)).UserId;
        }

        public List<GitUser> GetAll()
        {
            var plco = new List<GitUser>();
            foreach (var elementKey in BaseGetAllKeys())
            {
                var configItem = BaseGet(elementKey) as GitUser;
                if (configItem != null) plco.Add(configItem);
            }
            return plco;
        }
    }

    public class GitUser : ConfigurationElement
    {
        [ConfigurationProperty("UserId", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string UserId
        {
            get { return ((string)(base["UserId"])); }
            set { base["UserId"] = value.ToLowerInvariant(); }
        }

        [ConfigurationProperty("Password", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Password
        {
            get { return ((string)(base["Password"])); }
            set { base["Password"] = value; }
        }
    }

    [ConfigurationCollection(typeof(GitRepo))]
    public class GitRepoCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new GitRepo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GitRepo)(element)).RepoId;
        }

        public List<GitRepo> GetAll()
        {
            var plco = new List<GitRepo>();
            foreach (var elementKey in BaseGetAllKeys())
            {
                var configItem = BaseGet(elementKey) as GitRepo;
                if (configItem != null) plco.Add(configItem);
            }
            return plco;
        }
    }

    public class GitRepo : ConfigurationElement
    {
        [ConfigurationProperty("RepoId", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string RepoId
        {
            get { return ((string)(base["RepoId"])); }
            set { base["RepoId"] = value.ToLowerInvariant(); }
        }

        [ConfigurationProperty("Read", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Read
        {
            get { return ((string)(base["Read"])); }
            set { base["Read"] = value.ToLowerInvariant(); }
        }

        [ConfigurationProperty("Write", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Write
        {
            get { return ((string)(base["Write"])); }
            set { base["Write"] = value.ToLowerInvariant(); }
        }
    }
}