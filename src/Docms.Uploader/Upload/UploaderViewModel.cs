using Docms.Client;
using Docms.Client.Models;
using Docms.Uploader.Common;
using Docms.Uploader.FileWatch;
using Docms.Uploader.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Uploader.Upload
{
    public class UploaderViewModel : ViewModelBase
    {
        private CustomerResponse _Customer;
        private ProjectResponse _Project;
        private UserResponse _PersonInCharge;
        private string _CustomerText;
        private string _ProjectText;
        private string _PersonInChargeText;
        private bool _Loading;

        public class MediaFilesCollection : ObservableCollection<MediaFile> { }
        public class TagsCollection : ObservableCollection<Tag> { }

        public MediaFilesCollection SelectedMediaFiles { get; }

        public ObservableCollection<CustomerResponse> CustomerChoices { get; } = new ObservableCollection<CustomerResponse>();
        public ObservableCollection<ProjectResponse> ProjectChoices { get; } = new ObservableCollection<ProjectResponse>();
        public ObservableCollection<UserResponse> PersonInChargeChoices { get; } = new ObservableCollection<UserResponse>();

        public CustomerResponse Customer
        {
            get => _Customer;
            set
            {
                SetProperty(ref _Customer, value);
                Project = null;
                ProjectChoices.Clear();
                value.Projects.ToList().ForEach(p =>
                {
                    if (p.CustomerId == value.Id)
                    {
                        ProjectChoices.Add(p);
                    }
                });
            }
        }

        public ProjectResponse Project
        {
            get => _Project;
            set
            {
                SetProperty(ref _Project, value);
            }
        }

        public UserResponse PersonInCharge
        {
            get => _PersonInCharge;
            set
            {
                SetProperty(ref _PersonInCharge, value);
            }

        }
        public string CustomerText
        {
            get => _CustomerText;
            set
            {
                SetProperty(ref _CustomerText, value);
            }
        }

        public string ProjectText
        {
            get => _ProjectText;
            set
            {
                SetProperty(ref _ProjectText, value);
            }
        }

        public string PersonInChargeText
        {
            get => _PersonInChargeText;
            set
            {
                SetProperty(ref _PersonInChargeText, value);
            }

        }

        public TagsCollection TagChoices { get; } = new TagsCollection();
        public TagsCollection SelectedTags { get; set; }
        public bool Loading
        {
            get => _Loading;
            set
            {
                SetProperty(ref _Loading, value);
            }

        }

        public UploaderViewModel()
        {
            SelectedMediaFiles = new MediaFilesCollection();
            SelectedTags = new TagsCollection();
            Loading = true;
            InitializeAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    Loading = false;
                }
            });
        }

        private async Task InitializeAsync()
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var client = new DocmsClient(Settings.Default.DocmsWebEndpoint);
            await client.LoginAsync(Settings.Default.UserId, Settings.Default.PasswordHash).ConfigureAwait(false);
            var tasks = new List<Task>();
            tasks.Add(client.GetCustomersAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var c in t.Result)
                    {
                        CustomerChoices.Add(c);
                    }
                }
            }, scheduler));
            tasks.Add(client.GetUsersAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var u in t.Result)
                    {
                        PersonInChargeChoices.Add(u);
                    }
                }
            }, scheduler));
            tasks.Add(client.GetTagsAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var tag in t.Result)
                    {
                        TagChoices.Add(new Tag(tag.Id, tag.Name));
                    }
                }
            }, scheduler));
            await Task.WhenAll(tasks);
        }

        public void SelectFile(MediaFile file)
        {
            var index = SelectedMediaFiles.IndexOf(file);
            if (index < 0)
            {
                SelectedMediaFiles.Add(file);
            }
        }

        public async Task Upload()
        {
            var client = new DocmsClient(Settings.Default.DocmsWebEndpoint);
            await client.LoginAsync(Settings.Default.UserId, Settings.Default.PasswordHash).ConfigureAwait(false);
            var tags = SelectedTags.Select(t => t.Name).ToList();
            if (PersonInCharge != null || !string.IsNullOrEmpty(PersonInChargeText))
            {
                tags.Add(PersonInCharge?.Name ?? PersonInChargeText);
            }
            if (Customer != null || !string.IsNullOrEmpty(CustomerText))
            {
                tags.Add(Customer?.Name ?? CustomerText);
            }
            if (Project != null || !string.IsNullOrEmpty(ProjectText))
            {
                tags.Add(Project?.Name ?? ProjectText);
            }
            foreach (var f in SelectedMediaFiles)
            {
                await client.CreateDocumentAsync(f.FullPath, f.Name, tags.ToArray());
            }
        }

        public bool CanUpload()
        {
            return SelectedMediaFiles.Any();
        }
    }
}
